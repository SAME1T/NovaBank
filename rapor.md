# NOVABANK - BANKA OTOMASYON SİSTEMİ
## PROJE RAPORU

---

## 1. GİRİŞ

### 1.1. Projenin Tanıtılması

NovaBank, modern bankacılık işlemlerini dijitalleştiren kapsamlı bir banka otomasyon sistemidir. Sistem, müşteri yönetiminden para transferlerine, kredi kartı işlemlerinden döviz alım-satımına kadar geniş bir yelpazede bankacılık hizmetleri sunmaktadır. Clean Architecture prensipleriyle geliştirilmiş olan sistem, .NET 9.0 teknolojisi kullanılarak Windows Forms tabanlı masaüstü uygulaması ve RESTful API servisleri içermektedir.

### 1.2. Projenin Amacı

Projenin temel amaçları:
- Modern bankacılık işlemlerinin dijital ortamda gerçekleştirilmesi
- Güvenli ve ölçeklenebilir bir bankacılık altyapısı oluşturulması
- Maker-Checker (Yapan-Kontrol Eden) onay mekanizması ile güvenli işlem yönetimi
- Çoklu para birimi desteği ile döviz işlemlerinin yönetimi
- Kapsamlı audit logging ile işlem takibi ve güvenlik
- Kullanıcı dostu arayüz ile kolay erişim sağlanması

### 1.3. Projenin Kapsamı

**Kapsam İçi Modüller:**
- ✅ Müşteri Yönetimi (Kayıt, Onay, KYC)
- ✅ Hesap Yönetimi (Vadesiz, Vadeli, Döviz Hesapları)
- ✅ Para İşlemleri (Para Yatırma, Çekme, Transfer)
- ✅ Transfer İşlemleri (Dahili Transfer, EFT)
- ✅ Kredi Kartı Yönetimi (Başvuru, Onay, Borç Yönetimi)
- ✅ Döviz İşlemleri (Alım, Satım, Pozisyon Takibi)
- ✅ Fatura Ödeme Sistemi
- ✅ Onay Akışları (Maker-Checker)
- ✅ Limit ve Komisyon Yönetimi
- ✅ Bildirim Sistemi
- ✅ Raporlama ve Ekstre İşlemleri
- ✅ Admin Paneli

**Kapsam Dışı:**
- Mobil uygulama geliştirilmesi
- SMS gateway entegrasyonu (sadece email bildirimleri)
- Gerçek zamanlı döviz kuru entegrasyonu (mock veriler)
- Fiziksel kart basımı ve dağıtımı

### 1.4. Kullanılacak Teknolojiler

**Backend Teknolojileri:**
- .NET 9.0 Framework
- ASP.NET Core 9.0 (REST API)
- Entity Framework Core 9.0 (ORM)
- PostgreSQL 14+ (Veritabanı)
- JWT Bearer Authentication
- FluentValidation (Validasyon)
- Mapster (Object Mapping)
- MailKit (Email Gönderimi)

**Frontend Teknolojileri:**
- Windows Forms (.NET 9.0)
- DevExpress WinForms 25.1.3 (UI Komponentleri)

**Mimari:**
- Clean Architecture Pattern
- Repository Pattern
- Unit of Work Pattern
- Domain-Driven Design (DDD) Prensipleri

**Geliştirme Araçları:**
- Visual Studio 2022
- PostgreSQL pgAdmin
- Git (Versiyon Kontrolü)

---

## 2. PROJE PLANI

### 2.1. Sistemin Kullanıcıları

**1. Admin (Yönetici)**
- Tüm sistem yönetimi
- Müşteri onayları
- Hesap durumu yönetimi
- Limit ve komisyon tanımları
- Audit log görüntüleme
- Kredi kartı başvuru onayları
- Şifre sıfırlama işlemleri

**2. Manager (Şube Müdürü)**
- Onay akışlarını yönetme
- Hesap açılış onayları
- Transfer onayları
- KYC doğrulama işlemleri

**3. Customer (Müşteri)**
- Hesap görüntüleme ve yönetimi
- Para yatırma/çekme
- Transfer işlemleri (Dahili/EFT)
- Kredi kartı başvurusu
- Döviz alım-satım
- Fatura ödeme
- Ekstre görüntüleme
- Bildirimleri görüntüleme

### 2.2. GANT İş Akış Diyagramı

**Not:** GANT diyagramı için aşağıdaki bilgiler kullanılmalıdır:

```
Faz 1: Analiz ve Tasarım (Hafta 1-2)
├── Gereksinim Analizi
├── Veritabanı Tasarımı
├── UML Diyagramları
└── Mimari Tasarım

Faz 2: Core Katmanı Geliştirme (Hafta 3-4)
├── Entity'lerin Oluşturulması
├── Value Object'lerin Tanımlanması
├── Enum'ların Oluşturulması
└── Domain Servislerinin Geliştirilmesi

Faz 3: Infrastructure Katmanı (Hafta 5-6)
├── EF Core DbContext
├── Repository Implementasyonları
├── Migration'ların Oluşturulması
└── Email Servisleri

Faz 4: Application Katmanı (Hafta 7-9)
├── Servis Katmanının Geliştirilmesi
├── Validator'ların Yazılması
├── Business Logic Implementasyonu
└── Unit of Work Pattern

Faz 5: API Katmanı (Hafta 10-11)
├── Endpoint'lerin Oluşturulması
├── Authentication/Authorization
├── Middleware'lerin Eklenmesi
└── Swagger Dokümantasyonu

Faz 6: WinForms Uygulaması (Hafta 12-14)
├── Form Tasarımları
├── API Entegrasyonu
├── UI/UX İyileştirmeleri
└── Test ve Hata Düzeltmeleri

Faz 7: Test ve Dokümantasyon (Hafta 15-16)
├── Unit Testler
├── Integration Testler
├── Kullanıcı Testleri
└── Dokümantasyon Hazırlığı
```

### 2.3. İşlevsel İhtiyaçlar (Olmazsa Olmazlar)

**Müşteri Yönetimi:**
- ✅ Müşteri kaydı ve onay süreci
- ✅ Giriş/Çıkış işlemleri
- ✅ Şifre sıfırlama
- ✅ KYC doğrulama

**Hesap Yönetimi:**
- ✅ Hesap açma (TRY, USD, EUR, GBP)
- ✅ Hesap durumu yönetimi (Aktif/Dondurulmuş/Kapalı)
- ✅ Bakiye sorgulama
- ✅ Ek hesap limiti yönetimi

**Para İşlemleri:**
- ✅ Para yatırma
- ✅ Para çekme
- ✅ Dahili transfer (Aynı banka içi)
- ✅ EFT (Dış bankaya transfer)
- ✅ Transfer iptali (30 dakika içinde)

**Kredi Kartı:**
- ✅ Kredi kartı başvurusu
- ✅ Başvuru onay/red işlemleri
- ✅ Borç görüntüleme
- ✅ Borç ödeme

**Döviz İşlemleri:**
- ✅ Döviz alımı
- ✅ Döviz satımı
- ✅ Pozisyon takibi
- ✅ Kâr/Zarar hesaplama

**Fatura Ödeme:**
- ✅ Fatura sorgulama
- ✅ Fatura ödeme (Hesap/Kart ile)
- ✅ Ödeme geçmişi

**Onay Akışları:**
- ✅ Maker-Checker mekanizması
- ✅ Onay beklemede işlemler
- ✅ Onay/Red işlemleri

**Admin İşlemleri:**
- ✅ Müşteri arama ve yönetimi
- ✅ Hesap durumu değiştirme
- ✅ Limit ve komisyon tanımları
- ✅ Audit log görüntüleme

### 2.4. İşlevsel Olmayan İhtiyaçlar (İlave Özellikler)

**Güvenlik:**
- ✅ JWT Token tabanlı kimlik doğrulama
- ✅ Şifre hash'leme (SHA256)
- ✅ Audit logging (Tüm kritik işlemler)
- ✅ Role-based access control (RBAC)
- ✅ Şifre sıfırlama token sistemi

**Performans:**
- ✅ Asenkron işlem desteği (async/await)
- ✅ Database transaction yönetimi
- ✅ Connection pooling
- ✅ Optimistic concurrency control

**Kullanılabilirlik:**
- ✅ Modern ve kullanıcı dostu arayüz (DevExpress)
- ✅ Hata mesajlarının Türkçe gösterilmesi
- ✅ Form validasyonları
- ✅ Responsive form tasarımları

**Ölçeklenebilirlik:**
- ✅ Clean Architecture ile modüler yapı
- ✅ Repository Pattern ile veri erişim soyutlaması
- ✅ Dependency Injection ile gevşek bağlılık

**Bakım Kolaylığı:**
- ✅ Kod dokümantasyonu (XML Comments)
- ✅ SOLID prensipleri
- ✅ Separation of Concerns

### 2.5. UML Diyagramları

**Not:** Aşağıdaki diyagramlar çizilmelidir:

#### 2.5.1. Use Case Diyagramı

**Aktörler:**
- Admin
- Manager
- Customer

**Use Case'ler:**

**Customer Use Cases:**
- Hesap Görüntüleme
- Para Yatırma
- Para Çekme
- Transfer Yapma (Dahili/EFT)
- Kredi Kartı Başvurusu
- Döviz Alım-Satım
- Fatura Ödeme
- Ekstre Görüntüleme
- Bildirim Görüntüleme
- Şifre Değiştirme

**Manager Use Cases:**
- Onay Akışlarını Görüntüleme
- İşlem Onaylama
- İşlem Reddetme
- KYC Doğrulama

**Admin Use Cases:**
- Müşteri Yönetimi
- Hesap Yönetimi
- Limit Tanımlama
- Komisyon Tanımlama
- Audit Log Görüntüleme
- Kredi Kartı Başvuru Onayı

#### 2.5.2. Class Diyagramı

**Ana Sınıflar:**

**Core Katmanı:**
- Entity (Base Class)
- Customer
- Account
- Transaction
- Transfer
- Card
- CreditCardApplication
- CurrencyTransaction
- CurrencyPosition
- BillPayment
- ApprovalWorkflow
- Loan
- Notification
- AuditLog

**Value Objects:**
- Money
- Iban
- AccountNo
- NationalId

**Application Katmanı:**
- AccountsService
- CustomersService
- TransfersService
- CreditCardService
- CurrencyExchangeService
- BillPaymentService
- AdminService

**Infrastructure Katmanı:**
- BankDbContext
- AccountRepository
- CustomerRepository
- TransferRepository
- EfUnitOfWork
- AuditLogger
- JwtTokenService

#### 2.5.3. Sequence Diyagramı

**Çizilmesi Gereken Senaryolar:**

1. **Müşteri Kayıt Senaryosu:**
   - Customer → CustomersService → CustomerRepository → BankDbContext → PostgreSQL

2. **Para Transfer Senaryosu:**
   - Customer → TransfersService → AccountRepository (FromAccount) → AccountRepository (ToAccount) → TransferRepository → TransactionRepository → BankDbContext

3. **Kredi Kartı Başvuru Senaryosu:**
   - Customer → CreditCardService → CreditCardApplicationRepository → BankDbContext

4. **Döviz Alım Senaryosu:**
   - Customer → CurrencyExchangeService → AccountRepository → CurrencyPositionRepository → CurrencyTransactionRepository → ExchangeRateRepository → BankDbContext

#### 2.5.4. Activity Diyagramı

**Çizilmesi Gereken Akışlar:**

1. **Para Transfer Akışı:**
   - Başla → Hesap Kontrolü → Bakiye Kontrolü → Para Birimi Kontrolü → Transfer Kaydı → Transaction Kayıtları → Bakiye Güncelleme → Audit Log → Bitiş

2. **Kredi Kartı Başvuru Akışı:**
   - Başla → Müşteri Kontrolü → Bekleyen Başvuru Kontrolü → Başvuru Oluştur → Admin Onayı Bekle → Onay/Red → Kart Oluştur (Onay ise) → Bitiş

3. **Döviz Alım Akışı:**
   - Başla → Hesap Kontrolü → Kur Getir → Komisyon Hesapla → Bakiye Kontrolü → TL Hesaptan Çek → Döviz Hesaba Yatır → Pozisyon Güncelle → Transaction Kayıtları → Audit Log → Bitiş

#### 2.5.5. Component Diyagramı

**Bileşenler:**
- NovaBank.WinForms (UI Layer)
- NovaBank.Api (API Layer)
- NovaBank.Application (Business Logic Layer)
- NovaBank.Infrastructure (Data Access Layer)
- NovaBank.Core (Domain Layer)
- NovaBank.Contracts (DTO Layer)
- PostgreSQL Database

---

## 3. PROJE GERÇEKLEŞTİRİLMESİ

### 3.1. Modüllerin ve Tüm Formların Tasarımı

#### 3.1.1. FrmAuth (Giriş ve Kayıt Formu)

**Açıklama:** Kullanıcıların sisteme giriş yaptığı ve yeni hesap oluşturduğu ana form.

**Özellikler:**
- Tab kontrolü ile Giriş ve Kayıt sekmeleri
- TC Kimlik No ve şifre ile giriş
- Yeni müşteri kayıt formu
- Şifre görünürlük toggle butonu
- Şifre unuttum linki

**Form Elemanları:**
- txtLoginTc: Giriş için TC Kimlik No
- txtLoginPassword: Giriş şifresi
- btnLogin: Giriş butonu
- txtRegTc: Kayıt için TC Kimlik No
- txtRegAd: Ad
- txtRegSoyad: Soyad
- txtRegEmail: E-posta (opsiyonel)
- txtRegTel: Telefon (opsiyonel)
- txtRegPassword: Kayıt şifresi
- txtRegPasswordConfirm: Şifre tekrarı
- btnRegister: Kayıt butonu

**Not:** Form ekran görüntüsü eklenecek.

#### 3.1.2. FrmMain (Ana Form)

**Açıklama:** Müşterilerin tüm bankacılık işlemlerini gerçekleştirdiği ana form.

**Sekmeler:**

**1. Dashboard Sekmesi:**
- Hoş geldiniz mesajı
- Hesap kartları (Her hesap için kart görünümü)
- Toplam bakiye gösterimi (TRY, USD, EUR)
- Hesap sayısı bilgisi

**2. Hesaplar Sekmesi:**
- Hesap listesi (Grid görünümü)
- IBAN, Para Birimi, Bakiye, Ek Hesap Limiti kolonları
- Hesap seçimi ile detay görüntüleme

**3. Para İşlemleri Sekmesi:**
- Para Yatırma bölümü:
  - Hesap seçimi (Dropdown)
  - Tutar girişi
  - Açıklama alanı
  - Yatır butonu
- Para Çekme bölümü:
  - Hesap seçimi
  - Tutar girişi
  - Açıklama alanı
  - Çek butonu

**4. Transfer Sekmesi:**
- Gönderen hesap seçimi
- Alıcı hesap/IBAN girişi
- Tutar girişi
- Para birimi seçimi
- Açıklama alanı
- Transfer butonu (Dahili/EFT)

**5. Kredi Kartları Sekmesi:**
- Mevcut kartlar listesi
- Kredi kartı başvuru formu:
  - Talep edilen limit
  - Aylık gelir
  - Başvur butonu
- Başvuru durumu listesi
- Borç ödeme formu:
  - Kart seçimi
  - Ödeme tutarı
  - Kaynak hesap seçimi
  - Öde butonu

**6. Döviz İşlemleri Sekmesi:**
- Döviz Alım formu:
  - Döviz türü seçimi (USD, EUR, GBP)
  - Miktar girişi
  - TL hesap seçimi
  - Döviz hesap seçimi
  - Güncel kur bilgisi
  - Al butonu
- Döviz Satım formu:
  - Döviz türü seçimi
  - Miktar girişi
  - Döviz hesap seçimi
  - TL hesap seçimi
  - Kâr/Zarar gösterimi
  - Sat butonu
- Pozisyonlar listesi:
  - Döviz türü
  - Toplam miktar
  - Ortalama maliyet
  - Güncel değer
  - Gerçekleşmemiş K/Z

**7. Faturalar Sekmesi:**
- Fatura kurumları listesi
- Fatura sorgulama formu:
  - Kurum seçimi
  - Abone numarası
  - Sorgula butonu
- Fatura ödeme formu:
  - Hesap/Kart seçimi
  - Ödeme butonu
- Ödeme geçmişi listesi

**8. Ekstre Sekmesi:**
- Hesap seçimi
- Tarih aralığı seçimi (Başlangıç-Bitiş)
- Sorgula butonu
- Ekstre detayları (Grid):
  - Tarih
  - İşlem tipi (Credit/Debit)
  - Tutar
  - Açıklama
  - Referans kodu
- Açılış bakiyesi
- Kapanış bakiyesi
- Toplam alacak
- Toplam borç

**9. Bildirimler Sekmesi:**
- Bildirim listesi (Grid)
- Okunmamış bildirim sayısı (Status bar)
- Bildirim detayları
- Okundu işaretleme butonu

**10. Profil Sekmesi:**
- Müşteri bilgileri:
  - Ad Soyad
  - TC Kimlik No
  - E-posta
  - Telefon
- Hesap bilgileri özeti

**11. Admin Sekmesi (Sadece Admin için):**
- Müşteri Arama:
  - Arama kutusu
  - Müşteri listesi (Grid)
  - Müşteri seçimi ile hesap görüntüleme
- Hesap Yönetimi:
  - Hesap durumu değiştirme
  - Ek hesap limiti güncelleme
- Onay Bekleyen Müşteriler:
  - Müşteri listesi
  - Onayla/Reddet butonları
- Audit Loglar:
  - Tarih aralığı filtresi
  - Aksiyon filtresi
  - Başarı/Hata filtresi
  - Log listesi (Grid)
- Kredi Kartı Başvuruları:
  - Bekleyen başvurular listesi
  - Onayla/Reddet butonları
  - Limit girişi

**Not:** Her sekme için ekran görüntüsü eklenecek.

#### 3.1.3. FrmManager (Yönetici Formu)

**Açıklama:** Manager rolündeki kullanıcıların onay akışlarını yönettiği form.

**Özellikler:**
- Bekleyen onaylar listesi (Grid)
- Onay akışı detayları:
  - Entity tipi
  - Talep eden
  - Tutar
  - Durum
- Onayla butonu
- Reddet butonu (Neden girişi ile)
- Yenile butonu

**Not:** Form ekran görüntüsü eklenecek.

#### 3.1.4. FrmForgotPassword (Şifre Sıfırlama Formu)

**Açıklama:** Kullanıcıların şifrelerini sıfırlamak için kullandığı form.

**Adımlar:**
1. E-posta veya TC Kimlik No girişi → Kod gönder
2. Gönderilen kod girişi → Doğrula
3. Yeni şifre girişi → Tamamla

**Not:** Form ekran görüntüsü eklenecek.

### 3.2. Veritabanı Tasarımı (ER Diyagramı)

**Not:** ER diyagramı çizilmelidir. Aşağıdaki bilgiler kullanılmalıdır:

#### 3.2.1. Tablolar ve İlişkiler

**Ana Tablolar:**

1. **bank_customers**
   - Id (PK, Guid)
   - national_id (Unique)
   - first_name
   - last_name
   - email
   - phone
   - password_hash
   - role (Enum: Customer, Admin, Manager)
   - is_active
   - is_approved
   - branch_id (FK → branches)
   - risk_level
   - kyc_completed
   - created_at
   - updated_at

2. **bank_accounts**
   - Id (PK, Guid)
   - customer_id (FK → bank_customers)
   - account_no (Unique)
   - iban (Unique)
   - currency (Enum: TRY, USD, EUR, GBP)
   - balance (Decimal)
   - overdraft_limit
   - status (Enum: Active, Frozen, Closed)
   - account_type (Enum: Checking, Savings, Investment)
   - interest_rate
   - branch_id (FK → branches)
   - is_approved
   - approved_by_id (FK → bank_customers)
   - approved_at
   - created_at
   - updated_at

3. **bank_transactions**
   - Id (PK, Guid)
   - account_id (FK → bank_accounts)
   - amount (Decimal)
   - currency (Enum)
   - direction (Enum: Credit, Debit)
   - description
   - reference_code
   - transaction_date

4. **bank_transfers**
   - Id (PK, Guid)
   - from_account_id (FK → bank_accounts)
   - to_account_id (FK → bank_accounts, nullable)
   - external_iban (String, nullable)
   - amount (Decimal)
   - currency (Enum)
   - channel (Enum: Internal, EFT, FAST)
   - status (Enum: Scheduled, Executed, Failed, Canceled)
   - reversal_of_transfer_id (FK → bank_transfers, nullable)
   - reversed_by_transfer_id (FK → bank_transfers, nullable)
   - reversed_at
   - created_at

5. **bank_cards**
   - Id (PK, Guid)
   - account_id (FK → bank_accounts)
   - customer_id (FK → bank_customers, nullable)
   - card_type (Enum: Debit, Credit)
   - card_status (Enum: Active, Blocked, Closed)
   - masked_pan
   - expiry_month
   - expiry_year
   - credit_limit
   - available_limit
   - current_debt
   - min_payment_due_date
   - min_payment_amount
   - billing_cycle_day
   - is_approved
   - created_at
   - updated_at

6. **bank_credit_card_applications**
   - Id (PK, Guid)
   - customer_id (FK → bank_customers)
   - requested_limit
   - approved_limit
   - monthly_income
   - status (Enum: Pending, Approved, Rejected, Cancelled)
   - rejection_reason
   - processed_at
   - processed_by_admin_id (FK → bank_customers)
   - created_at
   - updated_at

7. **currency_transactions**
   - Id (PK, Guid)
   - customer_id (FK → bank_customers)
   - transaction_type (String: BUY, SELL)
   - currency (Enum)
   - amount
   - exchange_rate
   - rate_type (String: BUY, SELL)
   - rate_source
   - rate_date
   - try_amount
   - commission_try
   - net_try_amount
   - from_account_id (FK → bank_accounts)
   - to_account_id (FK → bank_accounts)
   - position_before_amount
   - position_after_amount
   - avg_cost_before
   - avg_cost_after
   - realized_pnl_try
   - realized_pnl_percent
   - description
   - reference_code
   - created_at

8. **currency_positions**
   - Id (PK, Guid)
   - customer_id (FK → bank_customers)
   - currency (Enum)
   - total_amount
   - average_cost_rate
   - total_cost_try
   - created_at
   - updated_at

9. **exchange_rates**
   - Id (PK, Guid)
   - base_currency (Enum)
   - target_currency (Enum)
   - buy_rate
   - sell_rate
   - effective_date
   - source
   - created_at
   - updated_at

10. **bill_payments**
    - Id (PK, Guid)
    - account_id (FK → bank_accounts, nullable)
    - card_id (FK → bank_cards, nullable)
    - institution_id (FK → bill_institutions)
    - subscriber_no
    - amount
    - commission
    - reference_code
    - due_date
    - paid_at
    - status (Enum: Scheduled, Executed, Failed, Canceled)
    - created_at

11. **bill_institutions**
    - Id (PK, Guid)
    - code (Unique)
    - name
    - category (Enum: Electric, Water, Gas, Internet, Phone, TV, Insurance, Tax, Other)
    - logo_url
    - is_active
    - created_at
    - updated_at

12. **approval_workflows**
    - Id (PK, Guid)
    - entity_type (Enum: AccountOpening, Transfer, LoanApplication, CreditCardApplication, etc.)
    - entity_id (Guid)
    - requested_by_id (FK → bank_customers)
    - amount
    - currency (Enum, nullable)
    - status (Enum: Pending, Approved, Rejected, Cancelled, Expired)
    - required_role (Enum: Customer, Admin, Manager)
    - approved_by_id (FK → bank_customers, nullable)
    - approved_at
    - rejection_reason
    - expires_at
    - metadata_json
    - created_at
    - updated_at

13. **transaction_limits**
    - Id (PK, Guid)
    - limit_type (Enum: DailyTransfer, MonthlyTransfer, SingleTransaction, DailyAtm, DailyPos)
    - scope (Enum: Global, Role, Customer, Account)
    - scope_id (Guid, nullable)
    - scope_role (Enum, nullable)
    - currency (Enum)
    - min_amount
    - max_amount
    - requires_approval_above
    - is_active
    - created_at
    - updated_at

14. **commissions**
    - Id (PK, Guid)
    - commission_type (Enum: InternalTransfer, Eft, Swift, CurrencyExchange, etc.)
    - name
    - description
    - currency (Enum)
    - fixed_amount
    - percentage_rate
    - min_amount
    - max_amount
    - is_active
    - valid_from
    - valid_until
    - created_at
    - updated_at

15. **kyc_verifications**
    - Id (PK, Guid)
    - customer_id (FK → bank_customers)
    - verification_type (Enum: Identity, Address, Phone, Email, Income)
    - status (Enum: Pending, Verified, Rejected, Expired)
    - document_path
    - verified_by_id (FK → bank_customers, nullable)
    - verified_at
    - rejection_reason
    - expires_at
    - metadata_json
    - created_at
    - updated_at

16. **notifications**
    - Id (PK, Guid)
    - customer_id (FK → bank_customers)
    - notification_type (Enum: Sms, Email, Push, InApp)
    - title
    - message
    - status (Enum: Pending, Sent, Failed, Read)
    - sent_at
    - read_at
    - metadata_json
    - created_at
    - updated_at

17. **notification_preferences**
    - Id (PK, Guid)
    - customer_id (FK → bank_customers)
    - transaction_sms
    - transaction_email
    - login_sms
    - login_email
    - marketing_sms
    - marketing_email
    - security_alert_sms
    - security_alert_email
    - created_at
    - updated_at

18. **bank_loans**
    - Id (PK, Guid)
    - customer_id (FK → bank_customers)
    - principal_amount
    - principal_currency (Enum)
    - interest_rate_annual
    - term_months
    - start_date
    - status (Enum: Draft, Active, Closed, Defaulted)
    - is_approved
    - approved_by_id (FK → bank_customers, nullable)
    - approved_at
    - rejection_reason
    - disbursement_account_id (FK → bank_accounts, nullable)
    - remaining_principal
    - next_payment_date
    - next_payment_amount
    - paid_installments
    - created_at
    - updated_at

19. **bank_payment_orders**
    - Id (PK, Guid)
    - account_id (FK → bank_accounts)
    - payee_name
    - payee_iban
    - amount (Decimal)
    - currency (Enum)
    - cron_expr
    - status (Enum)
    - next_run_at
    - created_at
    - updated_at

20. **branches**
    - Id (PK, Guid)
    - code (Unique)
    - name
    - city
    - district
    - address
    - phone
    - manager_id (FK → bank_customers, nullable)
    - is_active
    - created_at
    - updated_at

21. **bank_audit_logs**
    - Id (PK, Guid)
    - actor_customer_id (FK → bank_customers, nullable)
    - actor_role
    - action
    - entity_type
    - entity_id
    - summary
    - metadata_json
    - ip_address
    - user_agent
    - success
    - error_code
    - created_at

22. **bank_password_reset_tokens**
    - Id (PK, Guid)
    - customer_id (FK → bank_customers)
    - target_email
    - code_hash
    - created_at
    - expires_at
    - attempt_count
    - is_used
    - used_at
    - requested_ip
    - requested_user_agent

**İlişkiler:**
- bank_customers (1) → (N) bank_accounts
- bank_customers (1) → (N) bank_transfers (from_account)
- bank_accounts (1) → (N) bank_transactions
- bank_accounts (1) → (N) bank_transfers (from_account)
- bank_accounts (1) → (N) bank_transfers (to_account)
- bank_accounts (1) → (N) bank_cards
- bank_customers (1) → (N) bank_credit_card_applications
- bank_customers (1) → (N) currency_transactions
- bank_customers (1) → (N) currency_positions
- bank_customers (1) → (N) approval_workflows (requested_by)
- bank_customers (1) → (N) approval_workflows (approved_by)
- bank_customers (1) → (N) kyc_verifications
- bank_customers (1) → (N) notifications
- bank_customers (1) → (1) notification_preferences
- bill_institutions (1) → (N) bill_payments
- bank_accounts (1) → (N) bill_payments
- bank_cards (1) → (N) bill_payments

**Not:** ER diyagramı çizilirken yukarıdaki ilişkiler gösterilmelidir.

### 3.3. Çıktılar & Raporlar

#### 3.3.1. Ekstre Raporu (.pdf)

**İçerik:**
- Müşteri bilgileri (Ad, Soyad, TCKN)
- Hesap bilgileri (IBAN, Hesap No, Para Birimi)
- Rapor tarih aralığı
- Açılış bakiyesi
- İşlem listesi:
  - Tarih
  - İşlem tipi (Alacak/Borç)
  - Tutar
  - Açıklama
  - Referans kodu
- Toplam alacak
- Toplam borç
- Kapanış bakiyesi
- Banka logosu ve bilgileri

**Not:** PDF çıktı örneği eklenecek.

#### 3.3.2. Hesap Özeti Raporu

**İçerik:**
- Müşteri bilgileri
- Tüm hesapların listesi
- Her hesap için:
  - IBAN
  - Para birimi
  - Bakiye
  - Ek hesap limiti
  - Durum
- Toplam bakiyeler (Para birimine göre)

#### 3.3.3. İşlem Geçmişi Raporu

**İçerik:**
- Seçilen tarih aralığı
- İşlem listesi (Tüm hesaplar için)
- Filtreleme seçenekleri:
  - İşlem tipi
  - Para birimi
  - Minimum/Maksimum tutar

---

## 4. PROJEDE ÖNGÖRÜLEN EKSİKLİKLER

### 4.1. Proje Planında Yapılması Planlanmış Ancak Eksik Kalan Modüller

1. **SMS Gateway Entegrasyonu**
   - Şu an sadece email bildirimleri gönderiliyor
   - SMS gönderimi için harici servis entegrasyonu gerekli

2. **Gerçek Zamanlı Döviz Kuru Entegrasyonu**
   - Şu an mock veriler kullanılıyor
   - TCMB API veya başka bir döviz kuru servisi entegrasyonu gerekli

3. **Mobil Uygulama**
   - Sadece Windows Forms masaüstü uygulaması mevcut
   - Android/iOS mobil uygulama geliştirilmesi planlanmıştı

4. **Çoklu Dil Desteği**
   - Şu an sadece Türkçe dil desteği var
   - İngilizce ve diğer diller için çeviri yapılması gerekli

5. **Gelişmiş Raporlama**
   - Temel ekstre raporu mevcut
   - Grafik ve analitik raporlar eklenebilir

6. **Fiziksel Kart Yönetimi**
   - Kredi kartı başvurusu ve onayı var
   - Fiziksel kart basımı ve dağıtım süreci yok

7. **SWIFT Transfer Desteği**
   - EFT transferi mevcut
   - Uluslararası SWIFT transferi eklenebilir

8. **Yatırım Ürünleri**
   - Temel bankacılık işlemleri mevcut
   - Fon, borsa, altın gibi yatırım ürünleri eklenebilir

### 4.2. Projeye Eklenmesi İçeriği Zenginleştirecek Modüller

1. **Biyometrik Kimlik Doğrulama**
   - Parmak izi veya yüz tanıma ile giriş
   - Güvenliği artırır

2. **Yapay Zeka Destekli Fraud Detection**
   - Şüpheli işlem tespiti
   - Otomatik risk analizi

3. **Chatbot Desteği**
   - Müşteri hizmetleri chatbot'u
   - 7/24 destek sağlar

4. **Blockchain Entegrasyonu**
   - Kripto para desteği
   - Blockchain tabanlı transferler

5. **Open Banking API**
   - Üçüncü parti uygulamalara API erişimi
   - Fintech entegrasyonları

6. **Gelişmiş Analitik Dashboard**
   - Müşteri davranış analizi
   - Harcama trendleri
   - Kişiselleştirilmiş öneriler

7. **Sosyal Medya Entegrasyonu**
   - Sosyal medya ile giriş
   - Paylaşım özellikleri

8. **QR Kod ile Ödeme**
   - QR kod oluşturma ve okuma
   - Hızlı ödeme çözümü

---

## 5. PROJE TESLİM

### 5.1. Kurulum Adımları

**Gereksinimler:**
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime
- PostgreSQL 14+ (Veritabanı sunucusu)
- En az 4 GB RAM
- 500 MB disk alanı

**Kurulum Adımları:**

1. **PostgreSQL Kurulumu:**
   - PostgreSQL 14+ indirilip kurulur
   - Veritabanı oluşturulur: `CREATE DATABASE novabank;`
   - Connection string ayarlanır

2. **API Kurulumu:**
   - `src/NovaBank.Api` klasörüne gidilir
   - `appsettings.json` dosyasında connection string güncellenir
   - `dotnet restore` komutu çalıştırılır
   - `dotnet ef database update` komutu ile migration'lar uygulanır
   - `dotnet run` komutu ile API başlatılır
   - API `http://localhost:5221` adresinde çalışır

3. **WinForms Uygulaması Kurulumu:**
   - `src/NovaBank.WinForms` klasörüne gidilir
   - `appsettings.json` dosyasında API URL'i kontrol edilir
   - `dotnet restore` komutu çalıştırılır
   - `dotnet run` komutu ile uygulama başlatılır

4. **Setup Dosyası Oluşturma:**
   - Visual Studio'da Publish seçeneği kullanılır
   - Self-contained deployment seçilir
   - Setup.exe dosyası oluşturulur

**Not:** Her adım için ekran görüntüsü eklenecek.

### 5.2. Varsayılan Kullanıcılar

**Admin:**
- TC: 11111111111
- Şifre: Admin123!

**Manager:**
- TC: 98765432101
- Şifre: Manager123

**Not:** İlk çalıştırmada admin kullanıcısı otomatik oluşturulur.

### 5.3. Test Senaryoları

**Test Senaryoları:**
1. Müşteri kaydı ve girişi
2. Hesap açma işlemi
3. Para yatırma/çekme
4. Transfer işlemleri
5. Kredi kartı başvurusu ve onayı
6. Döviz alım-satım
7. Fatura ödeme
8. Ekstre görüntüleme

**Not:** Test senaryoları için ekran görüntüleri eklenecek.

---

## 6. SONUÇ

### 6.1. Projenin Genel Değerlendirmesi

**Artıları:**
- ✅ Clean Architecture ile modüler ve bakımı kolay kod yapısı
- ✅ Kapsamlı bankacılık işlemleri desteği
- ✅ Güvenli authentication ve authorization mekanizması
- ✅ Detaylı audit logging ile işlem takibi
- ✅ Modern ve kullanıcı dostu arayüz (DevExpress)
- ✅ Çoklu para birimi desteği
- ✅ Maker-Checker onay mekanizması
- ✅ Transaction yönetimi ile veri bütünlüğü
- ✅ Asenkron işlem desteği ile performans
- ✅ Kapsamlı hata yönetimi

**Eksileri:**
- ❌ SMS gateway entegrasyonu yok
- ❌ Gerçek zamanlı döviz kuru entegrasyonu yok
- ❌ Mobil uygulama desteği yok
- ❌ Çoklu dil desteği yok
- ❌ Gelişmiş raporlama özellikleri sınırlı
- ❌ Unit test kapsamı yetersiz

**Tercih Edilme Sebepleri:**
- Modern teknolojiler kullanılarak geliştirilmiş
- Ölçeklenebilir mimari yapı
- Güvenlik odaklı tasarım
- Kapsamlı özellik seti
- Gerçek dünya bankacılık senaryolarını karşılayan yapı

### 6.2. Projenin Geliştirme Süresi Boyunca Size Katkısı

**Teknik Beceriler:**
- Clean Architecture ve DDD prensipleri hakkında derinlemesine bilgi
- .NET 9.0 ve Entity Framework Core ile gelişmiş veritabanı yönetimi
- RESTful API tasarımı ve geliştirme
- Windows Forms ile masaüstü uygulama geliştirme
- JWT authentication ve authorization
- Transaction yönetimi ve concurrency control
- Repository Pattern ve Unit of Work Pattern uygulaması

**Yazılım Mühendisliği:**
- Gereksinim analizi ve sistem tasarımı
- UML diyagramları ile sistem modelleme
- Veritabanı tasarımı ve normalizasyon
- Kod organizasyonu ve modülerlik
- Dokümantasyon yazma

**Problem Çözme:**
- Karmaşık iş mantıklarının çözümü
- Performans optimizasyonu
- Güvenlik sorunlarının çözümü
- Hata yönetimi ve debugging

**Takım Çalışması:**
- Versiyon kontrolü (Git) kullanımı
- Kod review süreçleri
- Dokümantasyon paylaşımı

---

## 7. KAYNAKLAR

### 7.1. Teknik Dokümantasyonlar

1. **Microsoft .NET Dokümantasyonu**
   - https://learn.microsoft.com/dotnet/
   - .NET 9.0 API referansları ve örnekler

2. **Entity Framework Core Dokümantasyonu**
   - https://learn.microsoft.com/ef/core/
   - EF Core migration ve repository pattern örnekleri

3. **ASP.NET Core Dokümantasyonu**
   - https://learn.microsoft.com/aspnet/core/
   - RESTful API geliştirme rehberi

4. **PostgreSQL Dokümantasyonu**
   - https://www.postgresql.org/docs/
   - Veritabanı yönetimi ve SQL referansları

5. **DevExpress WinForms Dokümantasyonu**
   - https://docs.devexpress.com/WindowsForms/
   - UI komponentleri ve kullanım örnekleri

### 7.2. Mimari ve Tasarım Desenleri

6. **Clean Architecture**
   - https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
   - Robert C. Martin'in Clean Architecture makalesi

7. **Domain-Driven Design**
   - https://martinfowler.com/bliki/DomainDrivenDesign.html
   - DDD prensipleri ve uygulamaları

8. **Repository Pattern**
   - https://martinfowler.com/eaaCatalog/repository.html
   - Repository Pattern açıklaması ve örnekleri

### 7.3. GitHub Projeleri

9. **Clean Architecture Örnekleri**
   - https://github.com/jasontaylordev/CleanArchitecture
   - Clean Architecture template ve örnekleri

10. **Banking System Örnekleri**
    - Çeşitli açık kaynak bankacılık projeleri

### 7.4. Eğitim Videoları

11. **.NET Core Eğitim Serileri**
    - YouTube .NET Core tutorial videoları
    - Entity Framework Core eğitim içerikleri

12. **Clean Architecture Eğitimleri**
    - Clean Architecture uygulamalı eğitim videoları

### 7.5. Makaleler ve Blog Yazıları

13. **JWT Authentication**
    - JWT token tabanlı authentication makaleleri

14. **Transaction Management**
    - Database transaction yönetimi ve concurrency control makaleleri

15. **Banking System Design**
    - Bankacılık sistemleri tasarım prensipleri

---

## EK: PROJE MALİYET KESTİRİMİ

### Ölçüm Parametreleri

| Ölçüm Parametresi | Sayı | Ağırlık Faktörü | Toplam |
|-------------------|------|----------------|--------|
| Kullanıcı Girdi Sayısı | 45 | 3 | 135 |
| Kullanıcı Çıktı Sayısı | 38 | 4 | 152 |
| Kullanıcı Sorgu Sayısı | 52 | 3 | 156 |
| Veri Tabanındaki Tablo Sayısı | 24 | 7 | 168 |
| Arayüz Sayısı | 15 | 5 | 75 |
| **Ana İşlev Nokta Sayısı (AİN)** | - | - | **686** |

### Teknik Karmaşıklık Soruları

| Soru | Puan |
|------|------|
| 1. Uygulama, güvenilir yedekleme ve kurtarma gerektiriyor mu? | 4 |
| 2. Veri iletişimi gerekiyor mu? | 5 |
| 3. Dağıtık işlem işlevleri var mı? | 3 |
| 4. Performans kritik mi? | 4 |
| 5. Sistem mevcut ve ağır yükü olan bir işletim ortamında mı çalışacak? | 3 |
| 6. Sistem, çevrim içi veri girişi gerektiriyor mu? | 5 |
| 7. Çevrim içi veri girişi, bir ara işlem için birden çok ekran gerektiriyor mu? | 3 |
| 8. Ana kütükler çevrim-içi olarak mı günleniyor? | 5 |
| 9. Girdiler, çıktılar, kütükler ya da sorgular karmaşık mı? | 4 |
| 10. İçsel işlemler karmaşık mı? | 4 |
| 11. Tasarlanacak kod, yeniden kullanılabilir mi olacak? | 4 |
| 12. Dönüştürme ve kurulum, tasarımda dikkate alınacak mı? | 3 |
| 13. Sistem birden çok yerde yerleşik farklı kurumlar için mi geliştiriliyor? | 2 |
| 14. Tasarlanan uygulama, kolay kullanılabilir ve kullanıcı tarafından kolayca değiştirilebilir mi olacak? | 3 |
| **Toplam (TKF)** | **52** |

### Hesaplamalar

**İşlev Noktası (İN) Hesaplama:**
- İN = AİN × (0.65 + 0.01 × TKF)
- İN = 686 × (0.65 + 0.01 × 52)
- İN = 686 × 1.17
- **İN = 802.62**

**Satır Sayısı Tahmini:**
- Satır Sayısı = İN × 30
- Satır Sayısı = 802.62 × 30
- **Satır Sayısı ≈ 24,079 satır**

**Not:** Gerçek proje satır sayısı yaklaşık 25,000+ satırdır ve tahmin ile uyumludur.

---

---

## EK: DİYAGRAMLAR İÇİN DETAYLI AKIŞ ŞEMALARI

### EK 1: USE CASE DİYAGRAMI - DETAYLI AÇIKLAMA

#### Aktörler ve İlişkileri:

**1. Customer (Müşteri)**
- Sistemin ana kullanıcısı
- Bankacılık işlemlerini gerçekleştirir

**2. Manager (Şube Müdürü)**
- Customer'dan türetilmiş (inheritance)
- Onay işlemlerini yönetir

**3. Admin (Yönetici)**
- Manager'dan türetilmiş (inheritance)
- Tüm sistem yönetim yetkilerine sahip

#### Use Case'ler ve Aktör İlişkileri:

**Customer Use Cases:**
```
Customer
├── Hesap Görüntüleme
├── Para Yatırma
├── Para Çekme
├── Transfer Yapma (Dahili)
├── Transfer Yapma (EFT)
├── Kredi Kartı Başvurusu
├── Kredi Kartı Borç Görüntüleme
├── Kredi Kartı Borç Ödeme
├── Döviz Alımı
├── Döviz Satımı
├── Döviz Pozisyon Görüntüleme
├── Fatura Sorgulama
├── Fatura Ödeme
├── Ekstre Görüntüleme
├── Bildirim Görüntüleme
├── Bildirim Okundu İşaretleme
├── Şifre Değiştirme
├── Şifre Sıfırlama
└── Profil Görüntüleme
```

**Manager Use Cases (Customer'ın tümü + ek olarak):**
```
Manager (extends Customer)
├── [Customer'ın tüm use case'leri]
├── Onay Akışlarını Görüntüleme
├── İşlem Onaylama
├── İşlem Reddetme
├── KYC Doğrulama
└── KYC Reddetme
```

**Admin Use Cases (Manager'ın tümü + ek olarak):**
```
Admin (extends Manager)
├── [Manager'ın tüm use case'leri]
├── Müşteri Arama
├── Müşteri Yönetimi
├── Müşteri Onaylama
├── Müşteri Reddetme
├── Müşteri Aktif/Pasif Yapma
├── Hesap Yönetimi
├── Hesap Durumu Değiştirme
├── Ek Hesap Limiti Güncelleme
├── Limit Tanımlama
├── Komisyon Tanımlama
├── Audit Log Görüntüleme
├── Kredi Kartı Başvuru Onayı
├── Kredi Kartı Başvuru Reddi
├── Fatura Kurumu Ekleme
├── Fatura Kurumu Silme
└── Şifre Sıfırlama (Müşteri için)
```

#### Include/Extend İlişkileri:

- **"Audit Log Kaydı"** → Tüm kritik işlemlerden sonra include edilir
- **"Bildirim Gönderme"** → İşlem başarılı olduğunda extend edilir
- **"Onay Kontrolü"** → Limit üzeri işlemlerde include edilir
- **"Bakiye Kontrolü"** → Para çekme ve transfer işlemlerinde include edilir

#### Çizim Talimatları:

1. **Aktörler:** Sol tarafta dikey olarak (Customer en üstte, Admin en altta)
2. **Use Case'ler:** Oval şekillerde, sistem sınırı içinde
3. **İlişkiler:** 
   - Düz çizgi: Normal ilişki
   - Ok işareti: Include/Extend ilişkisi
   - Inheritance: Üçgen ok (Customer → Manager → Admin)
4. **Sistem Sınırı:** Dikdörtgen kutu içinde tüm use case'ler

---

### EK 2: SEQUENCE DİYAGRAMI - DETAYLI AKIŞLAR

#### Senaryo 1: Müşteri Kayıt İşlemi

```
Customer (FrmAuth) → CustomersService → CustomerRepository → AccountRepository → BankDbContext → PostgreSQL

1. Customer: btnRegister_Click()
2. Customer → CustomersService: CreateCustomerAsync(request)
3. CustomersService → CustomerRepository: ExistsByTcknAsync(tckn)
4. CustomerRepository → BankDbContext: Query
5. BankDbContext → PostgreSQL: SELECT
6. PostgreSQL → BankDbContext: Result
7. BankDbContext → CustomerRepository: false (yoksa)
8. CustomerRepository → CustomersService: false
9. CustomersService: new Customer(...)
10. CustomersService → CustomerRepository: AddAsync(customer)
11. CustomerRepository → BankDbContext: Add
12. CustomersService → AccountRepository: CreateAccountAsync()
13. AccountRepository → BankDbContext: Add
14. CustomersService → IUnitOfWork: SaveChangesAsync()
15. IUnitOfWork → BankDbContext: SaveChanges()
16. BankDbContext → PostgreSQL: INSERT (customer)
17. BankDbContext → PostgreSQL: INSERT (account)
18. PostgreSQL → BankDbContext: Success
19. BankDbContext → IUnitOfWork: Success
20. IUnitOfWork → CustomersService: Success
21. CustomersService → Customer: Result<CustomerResponse>
22. Customer: ShowSuccessMessage()
```

#### Senaryo 2: Para Transfer İşlemi (Dahili)

```
Customer (FrmMain) → TransfersService → AccountRepository → TransferRepository → TransactionRepository → BankDbContext → PostgreSQL

1. Customer: btnTransfer_Click()
2. Customer → TransfersService: TransferInternalAsync(request)
3. TransfersService → IUnitOfWork: ExecuteInTransactionAsync()
4. IUnitOfWork → BankDbContext: BeginTransaction()
5. TransfersService → AccountRepository: GetByIdForUpdateAsync(fromAccountId)
6. AccountRepository → BankDbContext: FromSqlInterpolated (FOR UPDATE)
7. BankDbContext → PostgreSQL: SELECT ... FOR UPDATE
8. PostgreSQL → BankDbContext: Account (locked)
9. TransfersService → AccountRepository: GetByIdForUpdateAsync(toAccountId)
10. AccountRepository → BankDbContext: FromSqlInterpolated (FOR UPDATE)
11. BankDbContext → PostgreSQL: SELECT ... FOR UPDATE
12. PostgreSQL → BankDbContext: Account (locked)
13. TransfersService: Validate (fromAccount, toAccount, amount, currency)
14. TransfersService: fromAccount.CanWithdraw(amount) → true
15. TransfersService: fromAccount.Withdraw(amount)
16. TransfersService: toAccount.Deposit(amount)
17. TransfersService → TransferRepository: AddAsync(transfer)
18. TransferRepository → BankDbContext: Add
19. TransfersService → TransactionRepository: AddAsync(fromTransaction)
20. TransactionRepository → BankDbContext: Add
21. TransfersService → TransactionRepository: AddAsync(toTransaction)
22. TransactionRepository → BankDbContext: Add
23. TransfersService → AccountRepository: UpdateAsync(fromAccount)
24. TransfersService → AccountRepository: UpdateAsync(toAccount)
25. IUnitOfWork → BankDbContext: SaveChanges()
26. BankDbContext → PostgreSQL: UPDATE (fromAccount)
27. BankDbContext → PostgreSQL: UPDATE (toAccount)
28. BankDbContext → PostgreSQL: INSERT (transfer)
29. BankDbContext → PostgreSQL: INSERT (transaction x2)
30. IUnitOfWork → BankDbContext: CommitTransaction()
31. BankDbContext → PostgreSQL: COMMIT
32. TransfersService → IAuditLogger: LogAsync()
33. IAuditLogger → BankDbContext: Add (audit log)
34. IAuditLogger → BankDbContext: SaveChanges()
35. TransfersService → Customer: Result<TransferResponse>
36. Customer: ShowSuccessMessage()
```

#### Senaryo 3: Kredi Kartı Başvuru ve Onay İşlemi

**3.1. Başvuru Aşaması:**
```
Customer (FrmMain) → CreditCardService → CreditCardApplicationRepository → BankDbContext → PostgreSQL

1. Customer: btnApplyCreditCard_Click()
2. Customer → CreditCardService: ApplyForCreditCardAsync(request)
3. CreditCardService → ICustomerRepository: GetByIdAsync(customerId)
4. ICustomerRepository → BankDbContext: FindAsync
5. CreditCardService: Validate (isApproved, hasPendingApplication)
6. CreditCardService → CreditCardApplicationRepository: HasPendingApplicationAsync()
7. CreditCardApplicationRepository → BankDbContext: Query
8. CreditCardService: new CreditCardApplication(...)
9. CreditCardService → CreditCardApplicationRepository: AddAsync(application)
10. CreditCardApplicationRepository → BankDbContext: Add
11. CreditCardService → IUnitOfWork: SaveChangesAsync()
12. IUnitOfWork → BankDbContext: SaveChanges()
13. BankDbContext → PostgreSQL: INSERT
14. CreditCardService → IAuditLogger: LogAsync()
15. CreditCardService → Customer: Result.Success()
```

**3.2. Onay Aşaması:**
```
Admin (FrmMain) → CreditCardService → CreditCardApplicationRepository → CardRepository → BankDbContext → PostgreSQL

1. Admin: btnApproveCardApplication_Click()
2. Admin → CreditCardService: ApproveApplicationAsync(applicationId, approvedLimit)
3. CreditCardService → CreditCardApplicationRepository: GetByIdAsync(applicationId)
4. CreditCardApplicationRepository → BankDbContext: FindAsync
5. CreditCardService → ICustomerRepository: GetByIdAsync(customerId)
6. CreditCardService → IAccountRepository: GetByCustomerIdAsync(customerId)
7. CreditCardService: Find TRY Account
8. CreditCardService: application.Approve(approvedLimit, adminId)
9. CreditCardService: Card.CreateCreditCard(...)
10. CreditCardService → CardRepository: AddAsync(card)
11. CardRepository → BankDbContext: Add
12. CreditCardService → IUnitOfWork: SaveChangesAsync()
13. IUnitOfWork → BankDbContext: SaveChanges()
14. BankDbContext → PostgreSQL: UPDATE (application)
15. BankDbContext → PostgreSQL: INSERT (card)
16. CreditCardService → IAuditLogger: LogAsync()
17. CreditCardService → Admin: Result.Success()
```

#### Senaryo 4: Döviz Alım İşlemi

```
Customer (FrmMain) → CurrencyExchangeService → AccountRepository → CurrencyPositionRepository → CurrencyTransactionRepository → ExchangeRateRepository → BankDbContext → PostgreSQL

1. Customer: btnBuyCurrency_Click()
2. Customer → CurrencyExchangeService: BuyCurrencyAsync(request)
3. CurrencyExchangeService → IUnitOfWork: ExecuteInTransactionAsync()
4. IUnitOfWork → BankDbContext: BeginTransaction()
5. CurrencyExchangeService → AccountRepository: GetByIdForUpdateAsync(tryAccountId)
6. AccountRepository → BankDbContext: FromSqlInterpolated (FOR UPDATE)
7. BankDbContext → PostgreSQL: SELECT ... FOR UPDATE
8. CurrencyExchangeService → AccountRepository: GetByIdForUpdateAsync(foreignAccountId)
9. AccountRepository → BankDbContext: FromSqlInterpolated (FOR UPDATE)
10. BankDbContext → PostgreSQL: SELECT ... FOR UPDATE
11. CurrencyExchangeService → ExchangeRateRepository: GetLatestAsync(TRY, currency)
12. ExchangeRateRepository → BankDbContext: Query
13. BankDbContext → PostgreSQL: SELECT
14. CurrencyExchangeService: rate.CalculateBuy(amount) → tryAmount
15. CurrencyExchangeService → ICommissionService: CalculateCommissionAsync()
16. ICommissionService → ICommissionRepository: GetActiveCommissionsAsync()
17. CurrencyExchangeService: totalTry = tryAmount + commission
18. CurrencyExchangeService: Validate balance
19. CurrencyExchangeService → CurrencyPositionRepository: GetByCustomerAndCurrencyAsync()
20. CurrencyPositionRepository → BankDbContext: Query
21. CurrencyExchangeService: Create or Update Position
22. CurrencyExchangeService: tryAccount.Withdraw(totalTry)
23. CurrencyExchangeService: foreignAccount.Deposit(amount)
24. CurrencyExchangeService: position.AddPosition(amount, totalTry)
25. CurrencyExchangeService → TransactionRepository: AddAsync(tryTransaction)
26. CurrencyExchangeService → TransactionRepository: AddAsync(foreignTransaction)
27. CurrencyExchangeService → CurrencyTransactionRepository: AddAsync(fxTransaction)
28. IUnitOfWork → BankDbContext: SaveChanges()
29. BankDbContext → PostgreSQL: UPDATE (tryAccount)
30. BankDbContext → PostgreSQL: UPDATE (foreignAccount)
31. BankDbContext → PostgreSQL: UPDATE/INSERT (position)
32. BankDbContext → PostgreSQL: INSERT (transactions x3)
33. IUnitOfWork → BankDbContext: CommitTransaction()
34. CurrencyExchangeService → IAuditLogger: LogAsync()
35. CurrencyExchangeService → Customer: Result<CurrencyExchangeResponse>
```

#### Çizim Talimatları:

1. **Lifeline'lar:** Dikey çizgiler, her katman için
2. **Mesajlar:** Yatay oklar (→) mesaj yönünü gösterir
3. **Activation Box:** Her lifeline üzerinde aktif olduğu süre boyunca
4. **Return Mesajları:** Kesikli çizgiler (opsiyonel)
5. **Loop/Alt:** Transaction içindeki işlemler için alt diyagram

---

### EK 3: ACTIVITY DİYAGRAMI - DETAYLI AKIŞLAR

#### Akış 1: Para Transfer İşlemi

```
[Başla]
  ↓
[Transfer Formu Aç]
  ↓
[Gönderen Hesap Seç]
  ↓
[Alıcı Hesap/IBAN Gir]
  ↓
[Tutar Gir]
  ↓
[Para Birimi Seç]
  ↓
[Açıklama Gir (Opsiyonel)]
  ↓
[Transfer Butonuna Tıkla]
  ↓
{Kontrol: Aynı Hesap mı?}
  ├─ Evet → [Hata: Aynı hesaba transfer yapılamaz] → [Bitiş]
  └─ Hayır → ↓
{Kontrol: Tutar > 0 mı?}
  ├─ Hayır → [Hata: Tutar pozitif olmalı] → [Bitiş]
  └─ Evet → ↓
[Transaction Başlat]
  ↓
[Gönderen Hesabı Kilitle (FOR UPDATE)]
  ↓
[Alıcı Hesabı Kilitle (FOR UPDATE)]
  ↓
{Kontrol: Gönderen Hesap Bulundu mu?}
  ├─ Hayır → [Hata: Hesap bulunamadı] → [Transaction Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Alıcı Hesap Bulundu mu?}
  ├─ Hayır → [Hata: Hesap bulunamadı] → [Transaction Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Hesap Durumu Aktif mi?}
  ├─ Hayır → [Hata: Hesap aktif değil] → [Transaction Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Para Birimi Uyuşuyor mu?}
  ├─ Hayır → [Hata: Para birimi uyuşmuyor] → [Transaction Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Bakiye Yeterli mi?}
  ├─ Hayır → [Hata: Yetersiz bakiye] → [Transaction Rollback] → [Bitiş]
  └─ Evet → ↓
[Gönderen Hesaptan Para Çek]
  ↓
[Alıcı Hesaba Para Yatır]
  ↓
[Transfer Kaydı Oluştur]
  ↓
[Debit Transaction Kaydı Oluştur]
  ↓
[Credit Transaction Kaydı Oluştur]
  ↓
[Hesapları Güncelle]
  ↓
[Transaction Commit]
  ↓
[Audit Log Kaydet]
  ↓
[Başarı Mesajı Göster]
  ↓
[Formu Yenile]
  ↓
[Bitiş]
```

#### Akış 2: Kredi Kartı Başvuru ve Onay Süreci

```
[Başla]
  ↓
[Kredi Kartı Sekmesine Git]
  ↓
[Talep Edilen Limit Gir]
  ↓
[Aylık Gelir Gir]
  ↓
[Başvur Butonuna Tıkla]
  ↓
{Kontrol: Müşteri Onaylı mı?}
  ├─ Hayır → [Hata: Hesabınız onaylanmadı] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Bekleyen Başvuru Var mı?}
  ├─ Evet → [Hata: Zaten bekleyen başvurunuz var] → [Bitiş]
  └─ Hayır → ↓
{Kontrol: Limit ve Gelir > 0 mı?}
  ├─ Hayır → [Hata: Limit ve gelir pozitif olmalı] → [Bitiş]
  └─ Evet → ↓
[CreditCardApplication Oluştur]
  ↓
[Veritabanına Kaydet]
  ↓
[Audit Log Kaydet]
  ↓
[Başarı Mesajı: Başvurunuz alındı, onay bekleniyor]
  ↓
[Formu Yenile]
  ↓
--- [ONAY SÜRECİ] ---
  ↓
[Admin: Kredi Kartı Başvuruları Sekmesine Git]
  ↓
[Bekleyen Başvuruları Listele]
  ↓
[Başvuru Seç]
  ↓
{Admin Seçimi}
  ├─ Onayla → ↓
  │   [Onaylanan Limit Gir]
  │   ↓
  │   {Kontrol: Limit > 0 mı?}
  │   ├─ Hayır → [Hata] → [Bitiş]
  │   └─ Evet → ↓
  │   {Kontrol: Müşterinin TRY Hesabı Var mı?}
  │   ├─ Hayır → [Hata: TRY hesabı bulunamadı] → [Bitiş]
  │   └─ Evet → ↓
  │   [Başvuruyu Onayla]
  │   ↓
  │   [Kredi Kartı Oluştur]
  │   ↓
  │   [Veritabanına Kaydet]
  │   ↓
  │   [Audit Log Kaydet]
  │   ↓
  │   [Başarı: Kart oluşturuldu]
  │   ↓
  │   [Bitiş]
  │
  └─ Reddet → ↓
      [Red Nedeni Gir]
      ↓
      {Kontrol: Neden Boş mu?}
      ├─ Evet → [Hata: Red nedeni gerekli] → [Bitiş]
      └─ Hayır → ↓
      [Başvuruyu Reddet]
      ↓
      [Veritabanına Kaydet]
      ↓
      [Audit Log Kaydet]
      ↓
      [Başarı: Başvuru reddedildi]
      ↓
      [Bitiş]
```

#### Akış 3: Döviz Alım İşlemi

```
[Başla]
  ↓
[Döviz İşlemleri Sekmesine Git]
  ↓
[Döviz Türü Seç (USD/EUR/GBP)]
  ↓
[Alınacak Miktar Gir]
  ↓
[TL Hesap Seç]
  ↓
[Döviz Hesap Seç]
  ↓
[Güncel Kur Sorgula]
  ↓
{Kontrol: Döviz = TRY mi?}
  ├─ Evet → [Hata: TL alınamaz] → [Bitiş]
  └─ Hayır → ↓
{Kontrol: Miktar >= 10 mu?}
  ├─ Hayır → [Hata: Minimum 10 döviz birimi] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Kur Bulundu mu?}
  ├─ Hayır → [Hata: Kur bulunamadı] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Kur Güncel mi? (1 gün içinde)}
  ├─ Hayır → [Hata: Kur güncel değil] → [Bitiş]
  └─ Evet → ↓
[TL Tutarını Hesapla (amount × SellRate)]
  ↓
[Komisyon Hesapla]
  ↓
[Toplam TL Tutarını Hesapla]
  ↓
[Transaction Başlat]
  ↓
[TL Hesabı Kilitle (FOR UPDATE)]
  ↓
[Döviz Hesabı Kilitle (FOR UPDATE)]
  ↓
{Kontrol: Hesaplar Bulundu mu?}
  ├─ Hayır → [Hata] → [Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Hesap Durumları Aktif mi?}
  ├─ Hayır → [Hata] → [Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Para Birimleri Doğru mu?}
  ├─ Hayır → [Hata] → [Rollback] → [Bitiş]
  └─ Evet → ↓
{Kontrol: TL Bakiye Yeterli mi?}
  ├─ Hayır → [Hata: Yetersiz TL bakiyesi] → [Rollback] → [Bitiş]
  └─ Evet → ↓
[Pozisyon Getir veya Oluştur]
  ↓
[Pozisyon Öncesi Snapshot Al]
  ↓
[TL Hesaptan Para Çek (totalTry)]
  ↓
[Döviz Hesaba Para Yatır (amount)]
  ↓
[Pozisyonu Güncelle (AddPosition)]
  ↓
[Pozisyon Sonrası Snapshot Al]
  ↓
[TL Transaction Kaydı Oluştur]
  ↓
[Döviz Transaction Kaydı Oluştur]
  ↓
[CurrencyTransaction Kaydı Oluştur]
  ↓
[Hesapları Güncelle]
  ↓
[Transaction Commit]
  ↓
[Audit Log Kaydet]
  ↓
[Başarı Mesajı Göster (Kur, Komisyon, Toplam)]
  ↓
[Formu Yenile]
  ↓
[Bitiş]
```

#### Akış 4: Şifre Sıfırlama İşlemi

```
[Başla]
  ↓
[Şifremi Unuttum Butonuna Tıkla]
  ↓
[FrmForgotPassword Aç]
  ↓
[E-posta veya TC Kimlik No Gir]
  ↓
[Kod Gönder Butonuna Tıkla]
  ↓
{Kontrol: Kullanıcı Bulundu mu?}
  ├─ Hayır → [Başarı Mesajı: Eğer hesap varsa kod gönderildi] → [Bitiş]
  └─ Evet → ↓
[6 Haneli Kod Üret]
  ↓
[Kod Hash'le]
  ↓
[PasswordResetToken Oluştur (10 dakika geçerli)]
  ↓
[Veritabanına Kaydet]
  ↓
[E-posta Gönder (Kod ile)]
  ↓
{Kontrol: E-posta Gönderildi mi?}
  ├─ Hayır → [Audit Log: E-posta gönderilemedi] → [Bitiş]
  └─ Evet → ↓
[Başarı Mesajı: Kod gönderildi]
  ↓
--- [DOĞRULAMA AŞAMASI] ---
  ↓
[Gönderilen Kodu Gir]
  ↓
[Doğrula Butonuna Tıkla]
  ↓
{Kontrol: Token Bulundu mu?}
  ├─ Hayır → [Hata: Kod hatalı veya süresi dolmuş] → [Bitiş]
  └─ Evet → ↓
{Kontrol: Token Süresi Dolmuş mu?}
  ├─ Evet → [Hata: Kod süresi dolmuş] → [Bitiş]
  └─ Hayır → ↓
{Kontrol: Token Kullanılmış mı?}
  ├─ Evet → [Hata: Kod zaten kullanılmış] → [Bitiş]
  └─ Hayır → ↓
{Kontrol: Deneme Sayısı >= 5 mi?}
  ├─ Evet → [Hata: Çok fazla deneme] → [Bitiş]
  └─ Hayır → ↓
{Kontrol: Kod Doğru mu?}
  ├─ Hayır → [Deneme Sayısını Artır] → [Hata: Kod hatalı] → [Bitiş]
  └─ Evet → ↓
[Audit Log: Kod doğrulandı]
  ↓
[Başarı: Yeni şifre girebilirsiniz]
  ↓
--- [ŞİFRE GÜNCELLEME AŞAMASI] ---
  ↓
[Yeni Şifre Gir]
  ↓
[Yeni Şifre Tekrar Gir]
  ↓
{Kontrol: Şifreler Eşleşiyor mu?}
  ├─ Hayır → [Hata: Şifreler eşleşmiyor] → [Bitiş]
  └─ Evet → ↓
[Kod Tekrar Kontrol Et]
  ↓
[Transaction Başlat]
  ↓
[Müşteri Şifresini Güncelle (Hash'le)]
  ↓
[Token'ı Kullanıldı Olarak İşaretle]
  ↓
[Transaction Commit]
  ↓
[Audit Log: Şifre başarıyla sıfırlandı]
  ↓
[Başarı Mesajı: Şifre güncellendi]
  ↓
[Formu Kapat]
  ↓
[Bitiş]
```

#### Çizim Talimatları:

1. **Başlangıç:** Siyah dolu daire
2. **Bitiş:** Daire içinde siyah nokta
3. **Aktiviteler:** Yuvarlatılmış dikdörtgen
4. **Karar Noktaları:** Elmas şekli
5. **Fork/Join:** Kalın yatay çizgi (paralel işlemler için)
6. **Swimlanes:** Aktörlere göre dikey bölümler (opsiyonel)

---

### EK 4: CLASS DİYAGRAMI - DETAYLI YAPILAR

#### Core Katmanı Sınıfları:

```
┌─────────────────────────────────┐
│         Entity (Abstract)       │
├─────────────────────────────────┤
│ +Id: Guid                       │
│ +CreatedAt: DateTime            │
│ +UpdatedAt: DateTime?           │
│ +RowVersion: byte[]?            │
│ #TouchUpdated(): void           │
└─────────────────────────────────┘
              ▲
              │ (inherits)
              │
    ┌─────────┴─────────┬──────────────┬──────────────┐
    │                   │              │              │
┌───┴────┐    ┌─────────┴──┐  ┌────────┴────┐  ┌─────┴──────┐
│Customer│    │   Account  │  │  Transfer   │  │   Card    │
├────────┤    ├────────────┤  ├─────────────┤  ├───────────┤
│-NationalId  │-AccountNo  │  │-FromAccountId│  │-AccountId │
│-FirstName   │-Iban       │  │-ToAccountId  │  │-CardType  │
│-LastName    │-Currency   │  │-Amount       │  │-CardStatus│
│-Email       │-Balance    │  │-Channel      │  │-MaskedPan │
│-PasswordHash│-Status     │  │-Status       │  │-CreditLimit│
│-Role        │+Deposit()  │  │+MarkExecuted()│ │+Block()   │
│-IsApproved  │+Withdraw() │  │+MarkFailed() │  │+Unblock() │
│+Approve()   │+Freeze()   │  │+MarkReversed()│ │+AddSpending()│
│+Reject()    │+Activate() │  └─────────────┘  │+MakePayment()│
└────────────┘ └────────────┘                    └─────────────┘
```

#### Value Objects:

```
┌──────────────────┐
│      Money       │
├──────────────────┤
│ -Amount: decimal │
│ +Currency: Enum  │
│ +Add(): Money    │
│ +Subtract(): Money│
│ +Multiply(): Money│
└──────────────────┘

┌──────────────────┐
│       Iban       │
├──────────────────┤
│ -Value: string   │
│ +Value: string   │
└──────────────────┘

┌──────────────────┐
│    AccountNo      │
├──────────────────┤
│ -Value: long      │
│ +Value: long      │
└──────────────────┘

┌──────────────────┐
│   NationalId     │
├──────────────────┤
│ -Value: string   │
│ +Value: string   │
└──────────────────┘
```

#### Application Katmanı:

```
┌─────────────────────────────────┐
│      IAccountsService            │
├─────────────────────────────────┤
│ +CreateAccountAsync()            │
│ +GetByIdAsync()                 │
│ +GetByCustomerIdAsync()         │
└─────────────────────────────────┘
              ▲
              │ (implements)
              │
┌─────────────┴─────────────┐
│     AccountsService        │
├────────────────────────────┤
│ -_accountRepository        │
│ -_customerRepository       │
│ -_ibanGenerator           │
│ -_unitOfWork              │
│ +CreateAccountAsync()     │
│ +GetByIdAsync()           │
└───────────────────────────┘

┌─────────────────────────────────┐
│      ITransfersService           │
├─────────────────────────────────┤
│ +TransferInternalAsync()         │
│ +TransferExternalAsync()        │
│ +ReverseTransferAsync()         │
└─────────────────────────────────┘
              ▲
              │ (implements)
              │
┌─────────────┴─────────────┐
│     TransfersService      │
├────────────────────────────┤
│ -_accountRepository        │
│ -_transferRepository       │
│ -_transactionRepository    │
│ -_unitOfWork              │
│ -_auditLogger             │
│ +TransferInternalAsync()  │
│ +TransferExternalAsync() │
└───────────────────────────┘
```

#### Infrastructure Katmanı:

```
┌─────────────────────────────────┐
│      IAccountRepository         │
├─────────────────────────────────┤
│ +GetByIdAsync()                 │
│ +GetByIbanAsync()              │
│ +AddAsync()                     │
│ +UpdateAsync()                  │
│ +GetByIdForUpdateAsync()       │
└─────────────────────────────────┘
              ▲
              │ (implements)
              │
┌─────────────┴─────────────┐
│    AccountRepository      │
├────────────────────────────┤
│ -_context: BankDbContext   │
│ +GetByIdAsync()           │
│ +GetByIbanAsync()         │
│ +AddAsync()               │
│ +UpdateAsync()            │
└───────────────────────────┘

┌─────────────────────────────────┐
│      IUnitOfWork                │
├─────────────────────────────────┤
│ +SaveChangesAsync()             │
│ +ExecuteInTransactionAsync()    │
└─────────────────────────────────┘
              ▲
              │ (implements)
              │
┌─────────────┴─────────────┐
│      EfUnitOfWork        │
├───────────────────────────┤
│ -_context: BankDbContext  │
│ +SaveChangesAsync()       │
│ +ExecuteInTransactionAsync()│
└───────────────────────────┘
```

#### İlişki Türleri:

- **Inheritance:** Üçgen ok (Entity → Customer, Account, etc.)
- **Implementation:** Kesikli üçgen ok (IAccountsService → AccountsService)
- **Dependency:** Kesikli ok (AccountsService → IAccountRepository)
- **Composition:** Dolu elmas (BankDbContext → Account, Customer)
- **Aggregation:** Boş elmas (Account → Money, Iban)

#### Çizim Talimatları:

1. **Sınıflar:** Üç bölümlü kutu (İsim, Özellikler, Metodlar)
2. **Interface'ler:** <<interface>> etiketi veya I prefix
3. **Abstract Sınıflar:** <<abstract>> etiketi veya italik yazı
4. **Erişim Modifier'lar:**
   - + Public
   - - Private
   - # Protected
   - ~ Internal
5. **İlişkiler:** Yukarıdaki sembollerle gösterilir

---

### EK 5: COMPONENT DİYAGRAMI - DETAYLI YAPILAR

```
┌─────────────────────────────────────────────────────────┐
│              NovaBank.WinForms                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  FrmMain     │  │  FrmAuth     │  │  FrmManager  │ │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘ │
│         │                 │                  │          │
│  ┌──────┴─────────────────┴──────────────────┴──────┐ │
│  │            ApiClient                              │ │
│  └───────────────────────┬───────────────────────────┘ │
└──────────────────────────┼─────────────────────────────┘
                           │ HTTP/REST
                           │
┌──────────────────────────┼─────────────────────────────┐
│              NovaBank.Api                               │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Endpoints (Minimal API)                         │ │
│  │  - AccountsEndpoints                             │ │
│  │  - CustomersEndpoints                            │ │
│  │  - TransfersEndpoints                            │ │
│  │  - CreditCardEndpoints                           │ │
│  │  - CurrencyExchangeEndpoints                     │ │
│  └───────────────────────┬──────────────────────────┘ │
│                          │                             │
│  ┌───────────────────────┴──────────────────────────┐ │
│  │  Middleware                                       │ │
│  │  - CurrentUserMiddleware                          │ │
│  │  - Authentication                                │ │
│  └───────────────────────┬──────────────────────────┘ │
└──────────────────────────┼─────────────────────────────┘
                           │ (uses)
                           │
┌──────────────────────────┼─────────────────────────────┐
│         NovaBank.Application                            │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Services                                         │ │
│  │  ┌──────────────┐  ┌──────────────┐            │ │
│  │  │AccountsService│  │TransfersService│          │ │
│  │  └──────────────┘  └──────────────┘            │ │
│  │  ┌──────────────┐  ┌──────────────┐            │ │
│  │  │CreditCardService│CurrencyExchangeService│   │ │
│  │  └──────────────┘  └──────────────┘            │ │
│  └───────────────────────┬──────────────────────────┘ │
│                          │                             │
│  ┌───────────────────────┴──────────────────────────┐ │
│  │  Common                                           │ │
│  │  - CurrentUser                                    │ │
│  │  - Result<T>                                      │ │
│  │  - ErrorCodes                                    │ │
│  └───────────────────────┬──────────────────────────┘ │
└──────────────────────────┼─────────────────────────────┘
                           │ (depends on)
                           │
┌──────────────────────────┼─────────────────────────────┐
│         NovaBank.Infrastructure                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Persistence                                     │ │
│  │  ┌──────────────┐  ┌──────────────┐            │ │
│  │  │BankDbContext│  │Repositories   │            │ │
│  │  └──────┬──────┘  └──────┬───────┘            │ │
│  │         │                 │                     │ │
│  │  ┌──────┴─────────────────┴───────┐           │ │
│  │  │    EfUnitOfWork                 │           │ │
│  │  └─────────────────────────────────┘           │ │
│  └───────────────────────┬──────────────────────────┘ │
│                          │                             │
│  ┌───────────────────────┴──────────────────────────┐ │
│  │  Services                                        │ │
│  │  - AuditLogger                                   │ │
│  │  - JwtTokenService                               │ │
│  │  - MailKitEmailSender                            │ │
│  └───────────────────────┬──────────────────────────┘ │
└──────────────────────────┼─────────────────────────────┘
                           │ (uses)
                           │
┌──────────────────────────┼─────────────────────────────┐
│         NovaBank.Core                                   │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Entities                                        │ │
│  │  - Customer, Account, Transfer, Card, etc.       │ │
│  └──────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────┐ │
│  │  ValueObjects                                    │ │
│  │  - Money, Iban, AccountNo, NationalId            │ │
│  └──────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Enums                                            │ │
│  │  - Currency, UserRole, AccountStatus, etc.        │ │
│  └──────────────────────────────────────────────────┘ │
└──────────────────────────┼─────────────────────────────┘
                           │
┌──────────────────────────┼─────────────────────────────┐
│         NovaBank.Contracts                              │
│  ┌──────────────────────────────────────────────────┐ │
│  │  DTOs & Request/Response Models                   │ │
│  │  - AccountResponse, TransferRequest, etc.        │ │
│  └──────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                           │
                           │
┌──────────────────────────┼─────────────────────────────┐
│         PostgreSQL Database                             │
│  ┌──────────────────────────────────────────────────┐ │
│  │  24 Tables                                       │ │
│  │  - bank_customers                                │ │
│  │  - bank_accounts                                 │ │
│  │  - bank_transactions                             │ │
│  │  - ... (21 more tables)                         │ │
│  └──────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

#### Port ve Interface Tanımlamaları:

**NovaBank.Api:**
- `<<provided>>` HTTP REST API
- `<<required>>` Application Services

**NovaBank.Application:**
- `<<provided>>` Business Logic Services
- `<<required>>` Repository Interfaces, Domain Entities

**NovaBank.Infrastructure:**
- `<<provided>>` Repository Implementations, DbContext
- `<<required>>` Database Connection

#### Çizim Talimatları:

1. **Component'ler:** Dikdörtgen kutular
2. **Interface'ler:** Küçük daireler (lollipop)
3. **Bağımlılıklar:** Kesikli oklar
4. **Port'lar:** Küçük kareler component kenarında
5. **Stereotype'lar:** << >> içinde

---

### EK 6: ER DİYAGRAMI - DETAYLI İLİŞKİLER

#### Ana Entity'ler ve İlişkileri:

```
┌─────────────────────┐
│  bank_customers     │
├─────────────────────┤
│ PK: Id (Guid)       │
│     national_id (UK)│
│     first_name      │
│     last_name       │
│     email           │
│     password_hash    │
│     role            │
│     is_active       │
│     is_approved     │
│ FK: branch_id       │
└──────────┬──────────┘
           │
           │ 1:N
           │
    ┌──────┴──────────┬──────────────┬──────────────┬──────────────┐
    │                 │              │              │              │
┌───┴────┐  ┌─────────┴──┐  ┌────────┴────┐  ┌─────┴──────┐  ┌──┴──────┐
│bank_   │  │bank_credit │  │currency_    │  │approval_    │  │kyc_     │
│accounts│  │card_       │  │transactions │  │workflows     │  │verifications│
│        │  │applications│  │             │  │              │  │          │
├────────┤  ├────────────┤  ├─────────────┤  ├─────────────┤  ├──────────┤
│PK: Id  │  │PK: Id      │  │PK: Id       │  │PK: Id       │  │PK: Id    │
│FK:     │  │FK:         │  │FK:          │  │FK:          │  │FK:       │
│customer│  │customer_id │  │customer_id  │  │requested_by │  │customer_id│
│_id     │  │            │  │             │  │_id          │  │          │
│        │  │            │  │             │  │FK:          │  │          │
│        │  │            │  │             │  │approved_by  │  │          │
│        │  │            │  │             │  │_id          │  │          │
└───┬────┘  └────────────┘  └─────────────┘  └─────────────┘  └──────────┘
    │
    │ 1:N
    │
┌───┴──────────┬──────────────┬──────────────┬──────────────┐
│              │              │              │              │
┌──────┴──┐  ┌─┴────────┐  ┌───┴──────┐  ┌───┴──────┐
│bank_    │  │bank_     │  │bill_     │  │bank_     │
│transfers│  │transactions│ │payments  │  │cards     │
├─────────┤  ├──────────┤  ├──────────┤  ├──────────┤
│PK: Id   │  │PK: Id    │  │PK: Id    │  │PK: Id    │
│FK:      │  │FK:       │  │FK:       │  │FK:       │
│from_    │  │account_id│  │account_id│  │account_id│
│account_ │  │          │  │FK:       │  │FK:       │
│id       │  │          │  │card_id   │  │customer_id│
│FK:      │  │          │  │FK:       │  │          │
│to_      │  │          │  │institution│ │          │
│account_ │  │          │  │_id       │  │          │
│id       │  │          │  │          │  │          │
│FK:      │  │          │  │          │  │          │
│reversal_│  │          │  │          │  │          │
│of_      │  │          │  │          │  │          │
│transfer │  │          │  │          │  │          │
│_id      │  │          │  │          │  │          │
│(self)   │  │          │  │          │  │          │
└─────────┘  └──────────┘  └──────────┘  └──────────┘
```

#### İlişki Detayları:

**1. bank_customers → bank_accounts (1:N)**
- Bir müşteri birden fazla hesaba sahip olabilir
- İlişki: customer_id (FK)

**2. bank_accounts → bank_transactions (1:N)**
- Bir hesapta birden fazla işlem olabilir
- İlişki: account_id (FK)

**3. bank_accounts → bank_transfers (1:N) - FromAccount**
- Bir hesap birden fazla transfer gönderebilir
- İlişki: from_account_id (FK)

**4. bank_accounts → bank_transfers (1:N) - ToAccount**
- Bir hesap birden fazla transfer alabilir
- İlişki: to_account_id (FK, nullable)

**5. bank_transfers → bank_transfers (1:1) - Self Referencing**
- Bir transfer bir reversal transfer'e sahip olabilir
- İlişki: reversal_of_transfer_id (FK, nullable)

**6. bank_customers → bank_credit_card_applications (1:N)**
- Bir müşteri birden fazla kredi kartı başvurusu yapabilir
- İlişki: customer_id (FK)

**7. bank_customers → currency_transactions (1:N)**
- Bir müşteri birden fazla döviz işlemi yapabilir
- İlişki: customer_id (FK)

**8. bank_customers → currency_positions (1:N)**
- Bir müşteri birden fazla döviz pozisyonuna sahip olabilir
- İlişki: customer_id (FK)

**9. bank_accounts → bank_cards (1:N)**
- Bir hesaba birden fazla kart bağlanabilir
- İlişki: account_id (FK)

**10. bill_institutions → bill_payments (1:N)**
- Bir kurum birden fazla fatura ödemesine sahip olabilir
- İlişki: institution_id (FK)

**11. bank_accounts → bill_payments (1:N)**
- Bir hesaptan birden fazla fatura ödemesi yapılabilir
- İlişki: account_id (FK, nullable)

**12. bank_cards → bill_payments (1:N)**
- Bir karttan birden fazla fatura ödemesi yapılabilir
- İlişki: card_id (FK, nullable)

**13. bank_customers → approval_workflows (1:N) - RequestedBy**
- Bir müşteri birden fazla onay talebi oluşturabilir
- İlişki: requested_by_id (FK)

**14. bank_customers → approval_workflows (1:N) - ApprovedBy**
- Bir müşteri (admin/manager) birden fazla onay verebilir
- İlişki: approved_by_id (FK, nullable)

**15. bank_customers → kyc_verifications (1:N)**
- Bir müşteri birden fazla KYC doğrulaması yapabilir
- İlişki: customer_id (FK)

**16. bank_customers → notifications (1:N)**
- Bir müşteri birden fazla bildirim alabilir
- İlişki: customer_id (FK)

**17. bank_customers → notification_preferences (1:1)**
- Bir müşterinin bir bildirim tercihi vardır
- İlişki: customer_id (FK, unique)

**18. bank_customers → bank_audit_logs (1:N)**
- Bir müşteri birden fazla audit log kaydına sahip olabilir
- İlişki: actor_customer_id (FK, nullable)

**19. bank_customers → bank_password_reset_tokens (1:N)**
- Bir müşteri birden fazla şifre sıfırlama token'ı alabilir
- İlişki: customer_id (FK)

**20. branches → bank_customers (1:N)**
- Bir şubede birden fazla müşteri olabilir
- İlişki: branch_id (FK, nullable)

**21. branches → bank_accounts (1:N)**
- Bir şubede birden fazla hesap açılabilir
- İlişki: branch_id (FK, nullable)

#### Özel İlişkiler:

**Self-Referencing:**
- `bank_transfers.reversal_of_transfer_id` → `bank_transfers.id`
- `bank_transfers.reversed_by_transfer_id` → `bank_transfers.id`

**Many-to-Many (Ara Tablo ile):**
- Bu projede doğrudan many-to-many ilişki yok, tüm ilişkiler 1:N veya 1:1

#### Çizim Talimatları:

1. **Entity'ler:** Dikdörtgen kutular
2. **Özellikler:** Entity içinde listelenir
   - PK: Primary Key (altı çizili)
   - FK: Foreign Key (italik)
   - UK: Unique Key
3. **İlişkiler:**
   - 1:N: Tek çizgi, N tarafında çatallı
   - 1:1: Tek çizgi, her iki tarafta tek
   - N:M: Çift çatallı (bu projede yok)
4. **Kardinalite:**
   - 1: Tek
   - N: Çok (0 veya daha fazla)
   - 0..1: Sıfır veya bir
   - 1..*: Bir veya daha fazla
5. **İlişki İsimleri:** İlişki çizgisi üzerinde yazılır

---

### EK 7: GANT DİYAGRAMI - DETAYLI ZAMAN PLANI

#### Fazlar ve Süreler:

```
Faz 1: Analiz ve Tasarım
├── Gereksinim Analizi (3 gün)
├── Veritabanı Tasarımı (4 gün)
├── UML Diyagramları (5 gün)
└── Mimari Tasarım (3 gün)
Toplam: 15 gün (3 hafta)

Faz 2: Core Katmanı Geliştirme
├── Entity'lerin Oluşturulması (5 gün)
├── Value Object'lerin Tanımlanması (2 gün)
├── Enum'ların Oluşturulması (2 gün)
└── Domain Servislerinin Geliştirilmesi (3 gün)
Toplam: 12 gün (2.5 hafta)

Faz 3: Infrastructure Katmanı
├── EF Core DbContext (3 gün)
├── Repository Implementasyonları (8 gün)
├── Migration'ların Oluşturulması (4 gün)
└── Email Servisleri (2 gün)
Toplam: 17 gün (3.5 hafta)

Faz 4: Application Katmanı
├── Servis Katmanının Geliştirilmesi (15 gün)
├── Validator'ların Yazılması (3 gün)
├── Business Logic Implementasyonu (10 gün)
└── Unit of Work Pattern (2 gün)
Toplam: 30 gün (6 hafta)

Faz 5: API Katmanı
├── Endpoint'lerin Oluşturulması (8 gün)
├── Authentication/Authorization (4 gün)
├── Middleware'lerin Eklenmesi (2 gün)
└── Swagger Dokümantasyonu (1 gün)
Toplam: 15 gün (3 hafta)

Faz 6: WinForms Uygulaması
├── Form Tasarımları (10 gün)
├── API Entegrasyonu (8 gün)
├── UI/UX İyileştirmeleri (5 gün)
└── Test ve Hata Düzeltmeleri (7 gün)
Toplam: 30 gün (6 hafta)

Faz 7: Test ve Dokümantasyon
├── Unit Testler (5 gün)
├── Integration Testler (4 gün)
├── Kullanıcı Testleri (3 gün)
└── Dokümantasyon Hazırlığı (3 gün)
Toplam: 15 gün (3 hafta)

TOPLAM SÜRE: 134 gün (≈ 27 hafta / 6.5 ay)
```

#### Milestone'lar:

- **M1:** Analiz ve Tasarım Tamamlandı (15. gün)
- **M2:** Core Katmanı Tamamlandı (27. gün)
- **M3:** Infrastructure Tamamlandı (44. gün)
- **M4:** Application Katmanı Tamamlandı (74. gün)
- **M5:** API Tamamlandı (89. gün)
- **M6:** WinForms Tamamlandı (119. gün)
- **M7:** Proje Teslim (134. gün)

#### Bağımlılıklar:

- Faz 2, Faz 1'den sonra başlar
- Faz 3, Faz 2'den sonra başlar
- Faz 4, Faz 3'ten sonra başlar (kısmen paralel)
- Faz 5, Faz 4'ten sonra başlar
- Faz 6, Faz 5 ile paralel başlayabilir
- Faz 7, Faz 6'dan sonra başlar

#### Çizim Talimatları:

1. **Yatay Eksen:** Zaman (günler veya haftalar)
2. **Dikey Eksen:** Görevler/Fazlar
3. **Çubuklar:** Her görevin başlangıç ve bitiş zamanı
4. **Milestone'lar:** Elmas şekli
5. **Bağımlılıklar:** Oklar ile gösterilir
6. **Kritik Yol:** En uzun yol (kırmızı ile işaretlenir)

---

### EK 8: STATE DİYAGRAMI - DETAYLI DURUM GEÇİŞLERİ

#### Account Durum Diyagramı:

```
[Initial State]
    ↓
[Active] ──────┐
    │          │
    │ Freeze() │
    ↓          │
[Frozen]       │
    │          │
    │ Activate()│
    ↓          │
[Active] ←─────┘
    │
    │ Close()
    ↓
[Closed] ──────→ [Final State]
```

#### CreditCardApplication Durum Diyagramı:

```
[Initial State]
    ↓
[Pending] ──────┐
    │           │
    │ Approve() │
    │           │
    ↓           │
[Approved]      │
    │           │
    │           │ Reject()
    │           │
    ↓           │
[Rejected]      │
    │           │
    │           │ Cancel()
    │           │
    ↓           │
[Cancelled] ←───┘
    │
    ↓
[Final State]
```

#### Transfer Durum Diyagramı:

```
[Initial State]
    ↓
[Scheduled]
    │
    │ Execute()
    ↓
[Executed] ──────┐
    │            │
    │            │ (30 dakika içinde)
    │            │ Reverse()
    │            │
    ↓            │
[Reversed] ←─────┘
    │
    │ (Hata durumunda)
    │ MarkFailed()
    ↓
[Failed]
    │
    │ Cancel()
    ↓
[Canceled]
    ↓
[Final State]
```

#### ApprovalWorkflow Durum Diyagramı:

```
[Initial State]
    ↓
[Pending] ──────┐
    │           │
    │ Approve() │
    │           │
    ↓           │
[Approved]      │
    │           │
    │           │ Reject()
    │           │
    ↓           │
[Rejected]      │
    │           │
    │           │ Cancel()
    │           │
    ↓           │
[Cancelled]     │
    │           │
    │           │ (Süre doldu)
    │           │ MarkExpired()
    │           │
    ↓           │
[Expired] ←─────┘
    │
    ↓
[Final State]
```

#### Çizim Talimatları:

1. **Durumlar:** Yuvarlatılmış dikdörtgen
2. **Geçişler:** Oklar, üzerinde event/action yazılır
3. **Başlangıç:** Siyah dolu daire
4. **Bitiş:** Daire içinde siyah nokta
5. **Guard Conditions:** [ ] içinde yazılır
6. **Actions:** / ile ayrılır

---

**Not:** Tüm diyagramlar yukarıdaki açıklamalara göre çizilmelidir. Her diyagram için detaylı akış şemaları ve ilişkiler belirtilmiştir.

**Rapor Tarihi:** [Tarih]  
**Hazırlayan:** [İsim]  
**Proje Adı:** NovaBank - Banka Otomasyon Sistemi

