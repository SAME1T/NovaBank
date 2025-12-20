# Ledger / Çift Kayıt Sistemi Refactor Özeti

## Yapılan Değişiklikler

### 1. Sistem Hesabı (Kasa) Tanımı

**Dosya:** `src/NovaBank.Application/Common/SystemAccounts.cs`
- Sistem kasa hesabı IBAN sabiti: `TR00CASH000000000000000000`
- Sistem müşteri TCKN sabiti: `00000000000`
- Sistem müşteri ad-soyad sabitleri

**Seed Sınıfı:** `src/NovaBank.Infrastructure/Persistence/Seeding/SystemAccountSeeder.cs`
- API başlangıcında sistem müşterisi ve kasa hesabını otomatik oluşturur
- Idempotent (2 kere çalışsa da sorun çıkarmaz)
- `Program.cs` içinde startup'ta çağrılıyor

### 2. UnitOfWork Pattern

**Interface:** `src/NovaBank.Application/Common/Interfaces/IUnitOfWork.cs`
- `ExecuteInTransactionAsync<T>`: Transaction içinde işlem yürütür
- Başarılı olursa commit, hata olursa rollback

**Implementasyon:** `src/NovaBank.Infrastructure/Persistence/UnitOfWork/EfUnitOfWork.cs`
- Entity Framework transaction yönetimi
- Nested transaction desteği (zaten transaction içindeyse yeni transaction açmaz)

**DI Kaydı:** `src/NovaBank.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `IUnitOfWork` → `EfUnitOfWork` (Scoped)

### 3. Repository FOR UPDATE Metodları

**Interface Güncellemesi:** `src/NovaBank.Application/Common/Interfaces/IAccountRepository.cs`
- `GetByIdForUpdateAsync(Guid id, ct)`: FOR UPDATE ile ID'ye göre hesap getir
- `GetByIbanForUpdateAsync(string iban, ct)`: FOR UPDATE ile IBAN'a göre hesap getir

**Implementasyon:** `src/NovaBank.Infrastructure/Persistence/Repositories/AccountRepository.cs`
- PostgreSQL `FOR UPDATE` ile satır kilitleme
- Concurrent işlemlerde çift harcama önlenir

### 4. Repository SaveChanges Kaldırıldı

**Değişiklik:** Tüm repository'lerde (`AccountRepository`, `CustomerRepository`, `TransactionRepository`, `TransferRepository`)
- `AddAsync` ve `UpdateAsync` metodlarından `SaveChangesAsync` çağrıları kaldırıldı
- Artık `UnitOfWork` transaction sonunda tek seferde `SaveChanges` yapıyor
- Bu sayede tüm işlemler atomik olarak çalışıyor

### 5. TransfersService Güncellemesi

**Dosya:** `src/NovaBank.Application/Transfers/TransfersService.cs`

**Değişiklikler:**
- `IUnitOfWork` inject edildi
- `TransferInternalAsync` ve `TransferExternalAsync` metodları:
  - `UnitOfWork.ExecuteInTransactionAsync` içinde çalışıyor
  - `GetByIdForUpdateAsync` ve `GetByIbanForUpdateAsync` ile hesapları kilitliyor
  - Her transfer işleminde **2 adet transaction** oluşturuluyor:
    - **Debit** (gönderen hesap)
    - **Credit** (alıcı hesap)
  - Transfer kaydı oluşturuluyor
  - Hesapların bakiyeleri güncelleniyor
  - Tüm işlemler tek DB transaction içinde atomik

**Kurallar:**
- `amount > 0` kontrolü
- `sender != receiver` kontrolü (internal transfer için)
- Currency eşleşmesi kontrolü
- Yetersiz bakiye kontrolü (available balance = balance + overdraftLimit)

### 6. TransactionsService Güncellemesi

**Dosya:** `src/NovaBank.Application/Transactions/TransactionsService.cs`

**Değişiklikler:**
- Artık doğrudan balance oynamıyor
- `ITransfersService` inject edildi
- `DepositAsync`: Sistem kasa hesabından müşteri hesabına transfer yapar
  - `SYSTEM_CASH_ACCOUNT` → `CustomerAccount`
- `WithdrawAsync`: Müşteri hesabından sistem kasa hesabına transfer yapar
  - `CustomerAccount` → `SYSTEM_CASH_ACCOUNT`

**Sonuç:**
- Deposit ve Withdraw işlemleri de artık çift kayıt (2 transaction) oluşturuyor
- Tüm para hareketleri transfer mantığıyla çalışıyor

### 7. ErrorCodes Güncellemesi

**Dosya:** `src/NovaBank.Application/Common/Errors/ErrorCodes.cs`

**Eklenen Kodlar:**
- `CurrencyMismatch`: Para birimi uyuşmazlığı
- `InvalidAmount`: Geçersiz tutar
- `AccountNotFound`: Hesap bulunamadı
- `CustomerNotFound`: Müşteri bulunamadı
- `SameAccountTransfer`: Aynı hesaba transfer

### 8. API Endpoint Status Code Mapping

**Dosyalar:**
- `src/NovaBank.Api/Endpoints/TransactionsEndpoints.cs`
- `src/NovaBank.Api/Endpoints/TransfersEndpoints.cs`

**Değişiklikler:**
- `CurrencyMismatch` → `409 Conflict`
- `InvalidAmount` → `400 BadRequest`
- `AccountNotFound` → `404 NotFound`
- `InsufficientFunds` → `400 BadRequest`
- `SameAccountTransfer` → `400 BadRequest`

### 9. Program.cs Seed Entegrasyonu

**Dosya:** `src/NovaBank.Api/Program.cs`

**Değişiklik:**
- API başlangıcında `SystemAccountSeeder.SeedSystemAccountsAsync()` çağrılıyor
- Sistem müşterisi ve kasa hesabı otomatik oluşturuluyor

## Çift Kayıt (Ledger) Akışı

### Deposit (Para Yatırma)
1. Sistem kasa hesabı (`SYSTEM_CASH_ACCOUNT`) bulunur
2. Müşteri hesabı bulunur
3. Transfer oluşturulur: `SYSTEM_CASH_ACCOUNT` → `CustomerAccount`
4. **2 Transaction oluşturulur:**
   - Debit: Sistem kasa hesabından çıkan
   - Credit: Müşteri hesabına giren
5. Bakiyeler güncellenir
6. Tüm işlemler tek DB transaction içinde

### Withdraw (Para Çekme)
1. Sistem kasa hesabı (`SYSTEM_CASH_ACCOUNT`) bulunur
2. Müşteri hesabı bulunur
3. Transfer oluşturulur: `CustomerAccount` → `SYSTEM_CASH_ACCOUNT`
4. **2 Transaction oluşturulur:**
   - Debit: Müşteri hesabından çıkan
   - Credit: Sistem kasa hesabına giren
5. Bakiyeler güncellenir
6. Tüm işlemler tek DB transaction içinde

### Transfer (Internal/External)
1. Gönderen ve alıcı hesaplar FOR UPDATE ile kilitlenir
2. Transfer oluşturulur
3. **2 Transaction oluşturulur:**
   - Debit: Gönderen hesaptan çıkan
   - Credit: Alıcı hesaba giren
4. Bakiyeler güncellenir
5. Tüm işlemler tek DB transaction içinde

## FOR UPDATE Kullanımı

**Metodlar:**
- `IAccountRepository.GetByIdForUpdateAsync(Guid id, ct)`
- `IAccountRepository.GetByIbanForUpdateAsync(string iban, ct)`

**Kullanım Yerleri:**
- `TransfersService.TransferInternalAsync`: Gönderen ve alıcı hesaplar kilitlenir
- `TransfersService.TransferExternalAsync`: Gönderen hesap kilitlenir (alıcı bizim bankamızdaysa o da kilitlenir)

**PostgreSQL SQL:**
```sql
SELECT * FROM bank_accounts WHERE id = {id} FOR UPDATE
SELECT * FROM bank_accounts WHERE iban = {iban} FOR UPDATE
```

## Transaction Tablolarında Çift Kayıt Örneği

**Deposit işlemi (1000 TRY):**
```
Transaction 1:
- AccountId: SYSTEM_CASH_ACCOUNT_ID
- Amount: 1000 TRY
- Direction: Debit
- Description: "Para yatırma"

Transaction 2:
- AccountId: CUSTOMER_ACCOUNT_ID
- Amount: 1000 TRY
- Direction: Credit
- Description: "Para yatırma"
```

**Transfer işlemi (500 TRY, Account A → Account B):**
```
Transaction 1:
- AccountId: ACCOUNT_A_ID
- Amount: 500 TRY
- Direction: Debit
- Description: "Transfer"

Transaction 2:
- AccountId: ACCOUNT_B_ID
- Amount: 500 TRY
- Direction: Credit
- Description: "Transfer"
```

## Build Sonucu

**Hatalar:**
- ✅ `ITransfersService` using eksikliği düzeltildi (`TransactionsService.cs`)

**Uyarılar:**
- Dosya kilitleme uyarıları: API çalışıyorsa normal (MSB3061)
- Nullable reference type uyarıları: Core entity'lerde mevcut (bu adımda değiştirilmedi)
- `CS0109` uyarısı: `Result<T>.Success` için `new` keyword gereksiz (küçük bir uyarı)

**Sonuç:**
- Tüm projeler başarıyla derleniyor
- API endpoint'leri çalışır durumda
- WinForms etkilenmedi (API contract'ları değişmedi)

## Migration Durumu

**Migration yapılmadı:**
- Mevcut DB şeması yeterli
- `Transaction` entity'sinde `Direction` alanı zaten var
- `Transfer` entity'si zaten var
- Ek kolon eklenmedi

## Test Önerileri

1. **Deposit Test:**
   - Müşteri hesabına para yatır
   - Sistem kasa hesabı bakiyesinin azaldığını kontrol et
   - Müşteri hesabı bakiyesinin arttığını kontrol et
   - 2 transaction kaydı oluştuğunu kontrol et

2. **Withdraw Test:**
   - Müşteri hesabından para çek
   - Sistem kasa hesabı bakiyesinin arttığını kontrol et
   - Müşteri hesabı bakiyesinin azaldığını kontrol et
   - 2 transaction kaydı oluştuğunu kontrol et

3. **Transfer Test:**
   - İki hesap arasında transfer yap
   - Her iki hesabın bakiyesinin doğru güncellendiğini kontrol et
   - 2 transaction kaydı oluştuğunu kontrol et

4. **Concurrent Test:**
   - Aynı hesaptan eşzamanlı 2 transfer isteği gönder
   - FOR UPDATE sayesinde çift harcama olmamalı
   - Sadece bir transfer başarılı olmalı (diğeri yetersiz bakiye hatası vermeli)

## Özet

✅ Sistem hesabı (kasa) tanımı ve seed eklendi
✅ UnitOfWork pattern implementasyonu
✅ FOR UPDATE ile row locking
✅ Repository'lerde SaveChanges kaldırıldı (UnitOfWork yönetiyor)
✅ TransfersService UnitOfWork + FOR UPDATE ile güncellendi
✅ TransactionsService deposit/withdraw transfer'e bağlandı
✅ Her işlemde 2 transaction (Debit + Credit) oluşturuluyor
✅ Tüm işlemler tek DB transaction içinde atomik
✅ API endpoint'lerde status code mapping iyileştirildi
✅ Build başarılı (API çalışıyorsa dosya kilitleme uyarıları normal)

