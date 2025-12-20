# NovaBank - Clean Architecture Analiz Raporu

## A) SORUNLAR

### 1. **Application Katmanı Boş**
- `NovaBank.Application` projesi hiçbir dosya içermiyor
- FluentValidation paketleri yüklü ama kullanılmıyor
- UseCase/Service katmanı eksik
- **Etki**: Business logic direkt olarak Api Endpoints'lerde yazılmış (AccountsEndpoints.cs'de görüldüğü gibi)

### 2. **Contracts Yanlış Yerde**
- `NovaBank.Api/Contracts/` klasöründe Request/Response modelleri var
- **Sorun**: Api projesi sadece host olmalı, Contracts ayrı proje olmalı
- **Etki**: WinForms Api'yi direkt referans ediyor (dependency violation)

### 3. **WinForms Yanlış Referanslar**
- `NovaBank.WinForms` → `NovaBank.Api` referansı var
- **Sorun**: WinForms sadece Contracts'ı bilmeli, Api'yi değil
- **Etki**: Tight coupling, test edilebilirlik düşük

### 4. **Api Endpoints Infrastructure'ı Direkt Kullanıyor**
- `AccountsEndpoints.cs` içinde `BankDbContext` direkt kullanılıyor
- **Sorun**: Application katmanı bypass edilmiş
- **Etki**: Business logic Api'de, Clean Architecture ihlali

### 5. **Infrastructure.Services Doğru Ama Eksik**
- `IbanGenerator` doğru yerde (Core'da interface, Infrastructure'da implementation)
- Ancak Repository pattern yok, direkt DbContext kullanılıyor

### 6. **WinForms.Services Karışık**
- `ApiClient.cs` → Doğru yerde (UI için HTTP client)
- `TcmbExchangeRateService.cs` → External service, Infrastructure'da olabilir ama UI-specific olduğu için burada da kalabilir

### 7. **WinForms.Dto Gereksiz**
- `DovizKurDto.cs` → Bu Api Contracts'a ait olmalı veya ayrı Contracts projesinde

### 8. **NovaBank.Modules Boş**
- Tüm alt klasörler boş (Cards, FX, Loans, Payments, Reporting, Risk)
- Ya kaldırılmalı ya da gelecekte kullanım planı yapılmalı

### 9. **Repository Pattern Eksik**
- Infrastructure'da Repository interface'leri yok
- Application katmanı için abstraction eksik

### 10. **Api → Infrastructure Referansı**
- Api direkt Infrastructure'ı referans ediyor
- **Sorun**: Api → Application → Infrastructure olmalı
- **Etki**: Dependency inversion ihlali

---

## B) HEDEF YAPI

```
src/
├── NovaBank.Core/                          # Domain Layer (En İç)
│   ├── Entities/
│   │   ├── Account.cs
│   │   ├── Card.cs
│   │   ├── Customer.cs
│   │   ├── Loan.cs
│   │   ├── PaymentOrder.cs
│   │   ├── Transaction.cs
│   │   └── Transfer.cs
│   ├── ValueObjects/
│   │   ├── AccountNo.cs
│   │   ├── Iban.cs
│   │   ├── Money.cs
│   │   └── NationalId.cs
│   ├── Enums/
│   │   ├── CardStatus.cs
│   │   ├── CardType.cs
│   │   ├── Currency.cs
│   │   ├── LoanStatus.cs
│   │   ├── PaymentStatus.cs
│   │   ├── TransactionDirection.cs
│   │   └── TransferChannel.cs
│   ├── Interfaces/                         # Domain Service Interfaces
│   │   ├── ICustomerDomainService.cs
│   │   ├── IRiskRules.cs
│   │   └── IIbanGenerator.cs
│   ├── Exceptions/
│   │   └── DomainException.cs
│   └── Abstractions/
│       └── Entity.cs
│
├── NovaBank.Application/                   # Use Cases / Business Logic
│   ├── Accounts/
│   │   ├── Commands/
│   │   │   ├── CreateAccount/
│   │   │   │   ├── CreateAccountCommand.cs
│   │   │   │   ├── CreateAccountCommandHandler.cs
│   │   │   │   └── CreateAccountCommandValidator.cs
│   │   │   └── ...
│   │   └── Queries/
│   │       ├── GetAccountById/
│   │       │   ├── GetAccountByIdQuery.cs
│   │       │   └── GetAccountByIdQueryHandler.cs
│   │       └── ...
│   ├── Transactions/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── Transfers/
│   ├── Customers/
│   ├── Common/
│   │   ├── Interfaces/                    # Application Service Interfaces
│   │   │   ├── IAccountRepository.cs
│   │   │   ├── ICustomerRepository.cs
│   │   │   └── ITransactionRepository.cs
│   │   ├── Mappings/                      # AutoMapper Profiles
│   │   └── Results/                       # Result<T> pattern
│   └── Validation/                        # FluentValidation validators
│
├── NovaBank.Infrastructure/                # External Concerns
│   ├── Persistence/
│   │   ├── BankDbContext.cs
│   │   ├── Configurations/
│   │   ├── Converters/
│   │   ├── DesignTime/
│   │   ├── Migrations/
│   │   └── Repositories/                   # Repository Implementations
│   │       ├── AccountRepository.cs
│   │       ├── CustomerRepository.cs
│   │       └── TransactionRepository.cs
│   ├── Services/                          # Infrastructure Services
│   │   ├── IbanGenerator.cs
│   │   └── TcmbExchangeRateService.cs     # External API client
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
├── NovaBank.Contracts/                     # YENİ: Shared DTOs
│   ├── Accounts/
│   │   ├── CreateAccountRequest.cs
│   │   ├── AccountResponse.cs
│   │   └── ...
│   ├── Customers/
│   ├── Transactions/
│   ├── Transfers/
│   ├── Cards/
│   ├── Loans/
│   ├── PaymentOrders/
│   └── Reports/
│
├── NovaBank.Api/                          # Presentation Layer (Host)
│   ├── Endpoints/
│   │   ├── AccountsEndpoints.cs
│   │   ├── CustomersEndpoints.cs
│   │   └── ...
│   ├── Program.cs
│   ├── Properties/
│   └── appsettings.json
│
└── NovaBank.WinForms/                     # UI Layer (Client)
    ├── Forms/
    │   ├── FrmAuth.cs
    │   ├── FrmAuth.Designer.cs
    │   ├── FrmMain.cs
    │   └── FrmMain.Designer.cs
    ├── Services/                          # UI Services
    │   └── ApiClient.cs                   # HTTP client wrapper
    ├── Program.cs
    └── appsettings.json
```

### Referans Hiyerarşisi:
```
Core (hiçbir şey referans etmez)
  ↑
Application (sadece Core referans eder)
  ↑
Infrastructure (Core + Application referans eder)
  ↑
Contracts (sadece Core referans eder - DTOs için)
  ↑
Api (Application + Infrastructure + Contracts referans eder)
  ↑
WinForms (sadece Contracts referans eder)
```

---

## C) TAŞIMA PLANI

### Adım 1: NovaBank.Contracts Projesi Oluştur
- [ ] `src/NovaBank.Contracts/` klasörü oluştur
- [ ] `NovaBank.Contracts.csproj` oluştur
- [ ] `NovaBank.Core` referansı ekle (Enums için)
- [ ] Solution'a ekle

### Adım 2: Contracts Taşıma
- [ ] `NovaBank.Api/Contracts/Accounts.cs` → `NovaBank.Contracts/Accounts/AccountsContracts.cs`
- [ ] `NovaBank.Api/Contracts/Cards.cs` → `NovaBank.Contracts/Cards/CardsContracts.cs`
- [ ] `NovaBank.Api/Contracts/Customers.cs` → `NovaBank.Contracts/Customers/CustomersContracts.cs`
- [ ] `NovaBank.Api/Contracts/Loans.cs` → `NovaBank.Contracts/Loans/LoansContracts.cs`
- [ ] `NovaBank.Api/Contracts/PaymentOrders.cs` → `NovaBank.Contracts/PaymentOrders/PaymentOrdersContracts.cs`
- [ ] `NovaBank.Api/Contracts/Reports.cs` → `NovaBank.Contracts/Reports/ReportsContracts.cs`
- [ ] `NovaBank.Api/Contracts/Transactions.cs` → `NovaBank.Contracts/Transactions/TransactionsContracts.cs`
- [ ] Namespace'leri güncelle: `NovaBank.Api.Contracts` → `NovaBank.Contracts`

### Adım 3: WinForms Dto Taşıma
- [ ] `NovaBank.WinForms/Dto/DovizKurDto.cs` → `NovaBank.Contracts/ExchangeRates/DovizKurDto.cs`
- [ ] Namespace güncelle: `NovaBank.WinForms.Dto` → `NovaBank.Contracts.ExchangeRates`

### Adım 4: Application Katmanı Oluştur
- [ ] `NovaBank.Application/Accounts/Commands/CreateAccount/` klasör yapısı oluştur
- [ ] `CreateAccountCommand.cs` oluştur
- [ ] `CreateAccountCommandHandler.cs` oluştur (şu anki AccountsEndpoints.cs'deki logic'i taşı)
- [ ] `CreateAccountCommandValidator.cs` oluştur (FluentValidation)
- [ ] Diğer UseCase'ler için aynı pattern'i uygula

### Adım 5: Repository Pattern Ekle
- [ ] `NovaBank.Application/Common/Interfaces/IAccountRepository.cs` oluştur
- [ ] `NovaBank.Application/Common/Interfaces/ICustomerRepository.cs` oluştur
- [ ] `NovaBank.Application/Common/Interfaces/ITransactionRepository.cs` oluştur
- [ ] `NovaBank.Infrastructure/Persistence/Repositories/AccountRepository.cs` implement et
- [ ] `NovaBank.Infrastructure/Persistence/Repositories/CustomerRepository.cs` implement et
- [ ] `NovaBank.Infrastructure/Persistence/Repositories/TransactionRepository.cs` implement et
- [ ] `ServiceCollectionExtensions.cs`'e repository'leri register et

### Adım 6: Api Endpoints Refactor
- [ ] `AccountsEndpoints.cs` içindeki business logic'i `CreateAccountCommandHandler`'a taşı
- [ ] Endpoints sadece HTTP mapping yapsın, Application'ı çağırsın
- [ ] `BankDbContext` kullanımını kaldır, Repository kullan
- [ ] Tüm Endpoints dosyalarını aynı şekilde refactor et

### Adım 7: WinForms Referansları Düzelt
- [ ] `NovaBank.WinForms.csproj` içinden `NovaBank.Api` referansını kaldır
- [ ] `NovaBank.Contracts` referansı ekle
- [ ] `using NovaBank.Api.Contracts;` → `using NovaBank.Contracts;` değiştir
- [ ] `FrmMain.cs`, `FrmAuth.cs`, `FrmMain.Designer.cs` içindeki namespace'leri güncelle

### Adım 8: Api Referansları Düzelt
- [ ] `NovaBank.Api.csproj` içine `NovaBank.Contracts` referansı ekle
- [ ] `NovaBank.Api.csproj` içinden `NovaBank.Infrastructure` referansını kaldır
- [ ] `NovaBank.Application` referansı ekle (zaten var mı kontrol et)
- [ ] `Program.cs` içinde `AddInfrastructure` çağrısı kalır (DI için gerekli)

### Adım 9: Infrastructure.Services Genişlet
- [ ] `TcmbExchangeRateService.cs` → `NovaBank.Infrastructure/Services/TcmbExchangeRateService.cs` taşı (opsiyonel, UI-specific olduğu için WinForms'ta da kalabilir)
- [ ] Eğer taşınırsa, `IExchangeRateService` interface'i `Core/Interfaces/` içine ekle

### Adım 10: Modules Klasörü
- [ ] `NovaBank.Modules/` klasörünü kaldır VEYA
- [ ] Gelecekte kullanılacaksa boş bırak, dokümante et

### Adım 11: Namespace Güncellemeleri
- [ ] Tüm dosyalarda `NovaBank.Api.Contracts` → `NovaBank.Contracts`
- [ ] Application katmanında namespace'ler: `NovaBank.Application.{Feature}.{Command/Query}`
- [ ] Infrastructure Repository'ler: `NovaBank.Infrastructure.Persistence.Repositories`

### Adım 12: csproj Referans Kontrolü
- [ ] **Core**: Hiçbir referans yok ✓
- [ ] **Application**: Sadece Core referansı var ✓
- [ ] **Infrastructure**: Core + Application referansları var ✓
- [ ] **Contracts**: Sadece Core referansı var ✓
- [ ] **Api**: Application + Infrastructure + Contracts referansları var ✓
- [ ] **WinForms**: Sadece Contracts referansı var ✓

### Adım 13: Test ve Doğrulama
- [ ] Tüm projeler build ediliyor mu?
- [ ] Api çalışıyor mu?
- [ ] WinForms Api'ye bağlanabiliyor mu?
- [ ] Endpoints'ler Application katmanını kullanıyor mu?

### Adım 14: Gitignore Güncelle
- [ ] `.gitignore` dosyasına `bin/` ve `obj/` ekle (yoksa)
- [ ] `*.user` dosyalarını ignore et

---

## ÖNEMLİ NOTLAR

1. **TcmbExchangeRateService**: UI-specific bir servis olduğu için WinForms'ta kalabilir. Eğer başka client'lar da kullanacaksa Infrastructure'a taşınmalı.

2. **Modules Klasörü**: Şu an boş. Gelecekte feature module pattern kullanılacaksa bırakılabilir, yoksa kaldırılmalı.

3. **Migration Stratejisi**: 
   - Önce Contracts projesini oluştur ve taşı
   - Sonra Application katmanını oluştur
   - En son Api ve WinForms'u refactor et
   - Her adımda test et

4. **Breaking Changes**: 
   - Namespace değişiklikleri tüm solution'ı etkileyecek
   - Git'te feature branch kullan
   - Adım adım commit yap

---

## ÖNCELİK SIRASI

1. **Yüksek Öncelik**: Contracts projesi oluştur ve taşı (WinForms bağımlılığı için kritik)
2. **Yüksek Öncelik**: Application katmanını oluştur (Business logic'in doğru yerde olması için)
3. **Orta Öncelik**: Repository pattern ekle (Test edilebilirlik için)
4. **Orta Öncelik**: Api Endpoints refactor (Clean Architecture için)
5. **Düşük Öncelik**: Modules klasörü temizliği
6. **Düşük Öncelik**: TcmbExchangeRateService taşıma (opsiyonel)

