using NovaBank.WinForms.Services;
using NovaBank.Contracts.Accounts;
using NovaBank.Contracts.Customers;
using NovaBank.Contracts.Transactions;
using NovaBank.Contracts.Reports;
using NovaBank.Contracts.ExchangeRates;
using NovaBank.Contracts.Admin;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Windows.Forms;

namespace NovaBank.WinForms;

public partial class FrmMain : XtraForm
{
    private readonly ApiClient _api = new();
    private List<AccountResponse> _cachedAccounts = new();
    private AccountResponse? _selectedAccount;
    private bool _isLogoutFlow = false;
    
    public FrmMain(Guid? currentCustomerId = null) 
    { 
        if (currentCustomerId.HasValue)
            Session.CurrentCustomerId = currentCustomerId;
        
        InitializeComponent(); 
        this.Text = $"NovaBank Client  •  {_api.BaseUrl}" + (Session.CurrentCustomerId.HasValue ? $" • Müşteri: {Session.CurrentCustomerId}" : ""); 
    }

    private bool TryGuid(string text, out Guid id)
    {
        if (!Guid.TryParse(text, out id))
        {
            XtraMessageBox.Show("Geçerli bir GUID giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private bool TryGuidFromShort(string text, out Guid id)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            id = Guid.Empty;
            return false;
        }

        // Eğer kısa format ise, müşteri ID'sini tam GUID'e çevir
        if (Session.CurrentCustomerId.HasValue && text.Length <= 8)
        {
            id = Session.CurrentCustomerId.Value;
            return true;
        }

        // Tam GUID formatı
        return TryGuid(text, out id);
    }

    private bool TryAccountNo(string text, out long accountNo)
    {
        if (!long.TryParse(text, out accountNo))
        {
            XtraMessageBox.Show("Geçerli bir hesap numarası giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private bool TryDec(string? text, out decimal val, string alanAdi)
    {
        if (!decimal.TryParse(text, out val))
        {
            XtraMessageBox.Show($"{alanAdi} sayısal olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private async void FrmMain_Load(object sender, EventArgs e)
    {
        cmbCurrency.Properties.Items.AddRange(Enum.GetValues(typeof(NovaBank.Core.Enums.Currency)));
        cmbCurrency.EditValue = NovaBank.Core.Enums.Currency.TRY;

        // Currency dropdown'ları gizle (artık hesap currency'si kullanılacak)
        if (cmbDwCurrency != null)
        {
            cmbDwCurrency.Visible = false;
            cmbDwCurrency.Enabled = false;
        }
        if (cmbTransCurrency != null)
        {
            cmbTransCurrency.Visible = false;
            cmbTransCurrency.Enabled = false;
        }

        // Admin tab'ı sadece admin kullanıcılar için göster
        if (tabAdmin != null)
        {
            tabAdmin.Visible = Session.IsAdmin;
        }

        // Eğer giriş yapılmışsa müşteri bilgilerini prefill et
        if (Session.CurrentCustomerId.HasValue)
        {
            txtAccCustomerId.Text = Session.CurrentCustomerId.Value.ToString("N")[..8]; // İlk 8 karakter
            if (txtStmtAccountId != null)
                txtStmtAccountId.Text = "";

            // Müşteri bilgilerini yükle
            await LoadCustomerInfo();
            
            // Hesapları yükle
            await LoadAccounts();

            // Admin ise admin UI'ı yükle
            if (Session.IsAdmin)
            {
                await LoadAdminUI();
            }
        }
    }

    private async Task LoadCustomerInfo()
    {
        try
        {
            if (!Session.CurrentCustomerId.HasValue) return;
            
            var customer = await _api.GetAsync<CustomerResponse>($"/api/v1/customers/{Session.CurrentCustomerId.Value}");
            if (customer != null)
            {
                Session.CurrentCustomerName = $"{customer.FirstName} {customer.LastName}";
                lblWelcome.Text = $"👋 Hoş Geldiniz, {customer.FirstName} {customer.LastName}";
                lblStatus.Text = $"🔒 Giriş yapıldı: {customer.FirstName} {customer.LastName} | {DateTime.Now:dd.MM.yyyy HH:mm}";
                if (lblProfName != null)
                {
                    lblProfName.Text = $"👤 Ad Soyad: {customer.FirstName} {customer.LastName}";
                    lblProfNationalId.Text = $"🆔 TCKN: {customer.NationalId}";
                    lblProfEmail.Text = $"📧 E-posta: {customer.Email ?? "-"}";
                    lblProfPhone.Text = $"📱 Telefon: {customer.Phone ?? "-"}";
                }
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Müşteri bilgileri yüklenirken hata: {ex.Message}", "Uyarı");
        }
    }

    private async Task LoadAccounts()
    {
        try
        {
            if (!Session.CurrentCustomerId.HasValue) return;
            
            var list = await _api.GetAccountsByCustomerIdAsync(Session.CurrentCustomerId.Value);
            if (list != null && gridAccounts != null)
            {
                _cachedAccounts = list; // Cache'e kaydet
                gridAccounts.DataSource = list;
                
                // Gizlenecek kolonları ayarla
                if (gridAccountsView.Columns["Id"] != null) gridAccountsView.Columns["Id"].Visible = false;
                if (gridAccountsView.Columns["CustomerId"] != null) gridAccountsView.Columns["CustomerId"].Visible = false;
                if (gridAccountsView.Columns["AccountNo"] != null) gridAccountsView.Columns["AccountNo"].Visible = false;
                
                // Kolon genişliklerini ayarla
                if (gridAccountsView.Columns["Iban"] != null)
                {
                    gridAccountsView.Columns["Iban"].Width = 300;
                    gridAccountsView.Columns["Iban"].Caption = "IBAN";
                }
                if (gridAccountsView.Columns["Currency"] != null)
                {
                    gridAccountsView.Columns["Currency"].Width = 100;
                    gridAccountsView.Columns["Currency"].Caption = "Para Birimi";
                }
                if (gridAccountsView.Columns["Balance"] != null)
                {
                    gridAccountsView.Columns["Balance"].Width = 200;
                    gridAccountsView.Columns["Balance"].Caption = "Bakiye";
                    gridAccountsView.Columns["Balance"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridAccountsView.Columns["Balance"].DisplayFormat.FormatString = "N2";
                }
                if (gridAccountsView.Columns["OverdraftLimit"] != null)
                {
                    gridAccountsView.Columns["OverdraftLimit"].Width = 180;
                    gridAccountsView.Columns["OverdraftLimit"].Caption = "Ek Hesap Limiti";
                    gridAccountsView.Columns["OverdraftLimit"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridAccountsView.Columns["OverdraftLimit"].DisplayFormat.FormatString = "N2";
                }
                
                // Toplam bakiye hesapla
                var totalBalance = list.Sum(a => a.Balance);
                lblTotalBalance.Text = $"💰 Toplam Bakiye: {totalBalance:N2} TL";
                lblAccountCount.Text = $"📊 Hesap Sayısı: {list.Count}";

                // Transfer ComboBox'ını doldur
                if (cmbTransferAccount != null)
                {
                    cmbTransferAccount.Properties.Items.Clear();
                    foreach (var acc in list)
                    {
                        var displayText = $"{acc.Iban} - {acc.Currency} ({acc.Balance:N2})";
                        cmbTransferAccount.Properties.Items.Add(displayText);
                    }
                    
                    // Varsayılan seçili hesap
                    if (list.Count > 0)
                    {
                        _selectedAccount = list[0];
                        Session.SelectedAccountId = list[0].Id;
                        gridAccountsView.FocusedRowHandle = 0;
                        cmbTransferAccount.SelectedIndex = 0;
                        BindSenderSummary();
                        RefreshAccountDropdowns(); // Para işlemleri ve ekstre dropdown'larını güncelle
                    }
                }
                else
                {
                    // Varsayılan seçili hesap (ComboBox yoksa)
                    if (list.Count > 0)
                    {
                        _selectedAccount = list[0];
                        Session.SelectedAccountId = list[0].Id;
                        gridAccountsView.FocusedRowHandle = 0;
                        BindSenderSummary();
                        RefreshAccountDropdowns();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hesaplar yüklenirken hata: {ex.Message}", "Uyarı");
        }
    }

    private void BindSenderSummary()
    {
        if (_selectedAccount == null) return;
        // Designer'da oluşturulan label adı: lblSenderBind
        if (lblSenderBind != null)
        {
            var available = _selectedAccount.Balance + _selectedAccount.OverdraftLimit;
            lblSenderBind.Text = $"📤 {_selectedAccount.Iban} - {_selectedAccount.Currency} | Bakiye: {_selectedAccount.Balance:N2} | Kullanılabilir: {available:N2}";
        }
    }

    // Helper metodlar
    private AccountResponse? GetSelectedAccountForDw()
    {
        // Para işlemleri için seçili hesabı al (dropdown'dan veya _selectedAccount'tan)
        if (cmbDwAccount != null && cmbDwAccount.EditValue != null)
        {
            var accountId = (Guid)cmbDwAccount.EditValue;
            return _cachedAccounts.FirstOrDefault(a => a.Id == accountId);
        }
        return _selectedAccount;
    }

    private void RefreshAccountDropdowns()
    {
        // Para işlemleri dropdown'ını güncelle
        if (cmbDwAccount != null && _cachedAccounts.Count > 0)
        {
            cmbDwAccount.Properties.DataSource = _cachedAccounts;
            cmbDwAccount.Properties.DisplayMember = "Iban";
            cmbDwAccount.Properties.ValueMember = "Id";
            cmbDwAccount.Properties.Columns.Clear();
            cmbDwAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Iban", "IBAN", 200));
            cmbDwAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Currency", "Para Birimi", 80));
            cmbDwAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye", 120));
            cmbDwAccount.Properties.ShowHeader = true;
            cmbDwAccount.Properties.ShowFooter = false;
            
            if (Session.SelectedAccountId.HasValue)
            {
                cmbDwAccount.EditValue = Session.SelectedAccountId.Value;
            }
            else if (_cachedAccounts.Count > 0)
            {
                cmbDwAccount.EditValue = _cachedAccounts[0].Id;
            }
            
            cmbDwAccount.EditValueChanged += CmbDwAccount_EditValueChanged;
        }

        // Ekstre dropdown'ını güncelle
        if (cmbStmtAccount != null && _cachedAccounts.Count > 0)
        {
            cmbStmtAccount.Properties.DataSource = _cachedAccounts;
            cmbStmtAccount.Properties.DisplayMember = "Iban";
            cmbStmtAccount.Properties.ValueMember = "Id";
            cmbStmtAccount.Properties.Columns.Clear();
            cmbStmtAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Iban", "IBAN", 200));
            cmbStmtAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Currency", "Para Birimi", 80));
            cmbStmtAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye", 120));
            cmbStmtAccount.Properties.ShowHeader = true;
            cmbStmtAccount.Properties.ShowFooter = false;
            
            if (Session.SelectedAccountId.HasValue)
            {
                cmbStmtAccount.EditValue = Session.SelectedAccountId.Value;
            }
            else if (_cachedAccounts.Count > 0)
            {
                cmbStmtAccount.EditValue = _cachedAccounts[0].Id;
            }
            
            cmbStmtAccount.EditValueChanged += CmbStmtAccount_EditValueChanged;
        }
    }

    private void CmbDwAccount_EditValueChanged(object? sender, EventArgs e)
    {
        if (cmbDwAccount?.EditValue is Guid accountId)
        {
            var account = _cachedAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                RefreshAccountInfoForDw(accountId);
            }
        }
    }

    private void CmbStmtAccount_EditValueChanged(object? sender, EventArgs e)
    {
        if (cmbStmtAccount?.EditValue is Guid accountId)
        {
            var account = _cachedAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account != null && txtStmtAccountId != null)
            {
                txtStmtAccountId.Text = account.Iban;
            }
        }
    }

    private void RefreshAccountInfoForDw(Guid accountId)
    {
        var account = _cachedAccounts.FirstOrDefault(a => a.Id == accountId);
        if (account == null) return;

        // Hesap bilgilerini göster (lblDwIban, lblDwCurrency, lblDwBalance, lblDwOverdraft, lblDwAvailable)
        if (lblDwIban != null) lblDwIban.Text = $"IBAN: {account.Iban}";
        if (lblDwCurrency != null) lblDwCurrency.Text = $"Para Birimi: {account.Currency}";
        if (lblDwBalance != null) lblDwBalance.Text = $"Bakiye: {account.Balance:N2} {account.Currency}";
        if (lblDwOverdraft != null) lblDwOverdraft.Text = $"Ek Hesap Limiti: {account.OverdraftLimit:N2} {account.Currency}";
        if (lblDwAvailable != null)
        {
            var available = account.Balance + account.OverdraftLimit;
            lblDwAvailable.Text = $"Kullanılabilir: {available:N2} {account.Currency}";
        }
    }

    private void ShowErrorMessage(System.Net.HttpStatusCode statusCode, string message)
    {
        var title = statusCode switch
        {
            System.Net.HttpStatusCode.NotFound => "Bulunamadı",
            System.Net.HttpStatusCode.BadRequest => "Geçersiz İstek",
            System.Net.HttpStatusCode.Conflict => "Çakışma",
            _ => "Hata"
        };

        var icon = statusCode switch
        {
            System.Net.HttpStatusCode.NotFound => MessageBoxIcon.Warning,
            System.Net.HttpStatusCode.BadRequest => MessageBoxIcon.Error,
            System.Net.HttpStatusCode.Conflict => MessageBoxIcon.Warning,
            _ => MessageBoxIcon.Error
        };

        XtraMessageBox.Show(message, title, MessageBoxButtons.OK, icon);
    }

    private void CmbTransferAccount_EditValueChanged(object sender, EventArgs e)
    {
        try
        {
            if (cmbTransferAccount == null || cmbTransferAccount.SelectedIndex < 0) return;
            
            var selectedIndex = cmbTransferAccount.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _cachedAccounts.Count)
            {
                _selectedAccount = _cachedAccounts[selectedIndex];
                Session.SelectedAccountId = _selectedAccount.Id;
                BindSenderSummary();
                
                // Grid'de de seçili yap
                if (gridAccountsView != null)
                {
                    gridAccountsView.FocusedRowHandle = selectedIndex;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Hesap seçim hatası: {ex.Message}");
        }
    }

    private void GridAccounts_CellDoubleClick(object sender, EventArgs e)
    {
        if (gridAccountsView.FocusedRowHandle >= 0)
        {
            var account = gridAccountsView.GetRow(gridAccountsView.FocusedRowHandle) as AccountResponse;
            if (account != null)
            {
                // IBAN'ı panoya kopyala
                Clipboard.SetText(account.Iban);
                XtraMessageBox.Show($"IBAN kopyalandı: {account.Iban}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void GridAccounts_SelectionChanged(object sender, EventArgs e)
    {
        if (gridAccountsView.SelectedRowsCount > 0)
        {
            var row = gridAccountsView.GetSelectedRows()[0];
            _selectedAccount = gridAccountsView.GetRow(row) as AccountResponse;
            BindSenderSummary();
        }
    }

    private async void btnCreateAccount_Click(object sender, EventArgs e)
    {
        try
        {
            var custId = Session.CurrentCustomerId ?? Guid.Empty;
            if (custId == Guid.Empty) 
            { 
                XtraMessageBox.Show("Müşteri bulunamadı. Lütfen giriş yapın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            }
            
            // Para birimi kontrolü
            if (cmbCurrency.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen para birimi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Ek hesap limiti kontrolü
            if (string.IsNullOrWhiteSpace(txtOverdraft.Text))
            {
                XtraMessageBox.Show("Ek hesap limiti boş bırakılamaz. Minimum 0 girebilirsiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (!TryDec(txtOverdraft.Text, out var od, "Ek Hesap Limiti")) return;
            
            if (od < 0)
            {
                XtraMessageBox.Show("Ek hesap limiti negatif olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Hesap numarasını otomatik oluştur (rastgele)
            var random = new Random();
            var accNo = random.Next(100000, 999999);
            
            var currency = (NovaBank.Core.Enums.Currency)cmbCurrency.EditValue;
            var currencyName = currency.ToString();
            
            // Onay mesajı
            var confirmMsg = $"Yeni hesap oluşturulacak:\n\n" +
                           $"Para Birimi: {currencyName}\n" +
                           $"Ek Hesap Limiti: {od:N2} TL\n\n" +
                           $"Hesap numarası otomatik oluşturulacak ve IBAN atanacak.\n\n" +
                           $"Devam etmek istiyor musunuz?";
            
            var confirm = XtraMessageBox.Show(confirmMsg, "Hesap Oluşturma Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            
            btnCreateAccount.Enabled = false;
            this.UseWaitCursor = true;
            
            var req = new CreateAccountRequest(custId, accNo, currency, od);
            var resp = await _api.PostAsync("/api/v1/accounts", req);
            
            if (!resp.IsSuccessStatusCode) 
            { 
                var errorMsg = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Hesap oluşturulamadı:\n{errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                return; 
            }
            
            XtraMessageBox.Show(
                $"✓ Hesap başarıyla oluşturuldu!\n\n" +
                $"Hesap No: {accNo}\n" +
                $"Para Birimi: {currencyName}\n" +
                $"IBAN otomatik oluşturuldu.\n\n" +
                $"Hesap listeniz güncelleniyor...", 
                "Başarılı", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
            );
            
            // Form alanlarını temizle
            txtOverdraft.Text = "0,00";
            cmbCurrency.EditValue = NovaBank.Core.Enums.Currency.TRY;
            
            // Hesapları yenile
            await LoadAccounts();
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hesap oluşturulurken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
        finally
        {
            btnCreateAccount.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async void btnDeposit_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryDec(txtDepositAmount.Text, out var amt, "Tutar")) return;
            if (amt <= 0) { XtraMessageBox.Show("Tutar pozitif olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var account = GetSelectedAccountForDw();
            if (account == null) { XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var confirm = XtraMessageBox.Show($"{amt:N2} {account.Currency} yatırılacak.\nHesap: {account.Iban}\nOnaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            
            btnDeposit.Enabled = false;
            this.UseWaitCursor = true;
            
            var resp = await _api.DepositAsync(account.Id, amt, account.Currency, txtDepositDesc.Text);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"✓ Para yatırma işlemi başarılı!\nTutar: {amt:N2} {account.Currency}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
                RefreshAccountDropdowns(); // Dropdown'ları güncelle
                RefreshAccountInfoForDw(account.Id); // Hesap bilgilerini güncelle
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(resp);
                ShowErrorMessage(resp.StatusCode, errorMsg);
            }
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
        finally
        {
            btnDeposit.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async void btnWithdraw_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryDec(txtWithdrawAmount.Text, out var amt, "Tutar")) return;
            if (amt <= 0) { XtraMessageBox.Show("Tutar pozitif olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var account = GetSelectedAccountForDw();
            if (account == null) { XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var availableBalance = account.Balance + account.OverdraftLimit;
            if (amt > availableBalance)
            {
                XtraMessageBox.Show($"Yetersiz bakiye!\nMevcut bakiye: {account.Balance:N2} {account.Currency}\nEk hesap limiti: {account.OverdraftLimit:N2} {account.Currency}\nKullanılabilir: {availableBalance:N2} {account.Currency}", 
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var confirm = XtraMessageBox.Show($"{amt:N2} {account.Currency} çekilecek.\nHesap: {account.Iban}\nOnaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            
            btnWithdraw.Enabled = false;
            this.UseWaitCursor = true;
            
            var resp = await _api.WithdrawAsync(account.Id, amt, account.Currency, txtWithdrawDesc.Text);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"✓ Para çekme işlemi başarılı!\nTutar: {amt:N2} {account.Currency}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
                RefreshAccountDropdowns(); // Dropdown'ları güncelle
                RefreshAccountInfoForDw(account.Id); // Hesap bilgilerini güncelle
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(resp);
                ShowErrorMessage(resp.StatusCode, errorMsg);
            }
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
        finally
        {
            btnWithdraw.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private void btnSelectAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            // Kendi hesaplarımdan alıcı hesap seç (internal transfer için)
            if (_cachedAccounts.Count == 0)
            {
                XtraMessageBox.Show("Hesap bulunamadı. Lütfen önce hesaplarınızı yükleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Basit bir form ile hesap seçimi yapılabilir, şimdilik ilk hesabı kullan
            // TODO: Hesap seçim dialogu eklenebilir
            if (_cachedAccounts.Count > 0 && _selectedAccount != null)
            {
                // Alıcı hesabı seç (gönderen hesaptan farklı olmalı)
                var receiverAccount = _cachedAccounts.FirstOrDefault(a => a.Id != _selectedAccount.Id);
                if (receiverAccount != null && txtToIban != null)
                {
                    txtToIban.Text = receiverAccount.Iban;
                    if (lblRecipientName != null)
                        lblRecipientName.Text = $"Alıcı: {receiverAccount.Iban} - {receiverAccount.Currency}";
                    XtraMessageBox.Show($"Alıcı hesap seçildi: {receiverAccount.Iban}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    XtraMessageBox.Show("Transfer için en az 2 hesabınız olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
    }

    private async void btnExternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtToIban?.Text)) 
            { 
                XtraMessageBox.Show("Alıcı IBAN zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            }
            if (!TryDec(txtAmount?.Text, out var amt, "Tutar")) return;
            if (amt <= 0) { XtraMessageBox.Show("Tutar pozitif olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var fromAccount = _selectedAccount;
            if (fromAccount == null) 
            { 
                XtraMessageBox.Show("Lütfen gönderen hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            }
            
            if (fromAccount.Iban.Equals(txtToIban.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("Aynı hesaba transfer yapılamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var availableBalance = fromAccount.Balance + fromAccount.OverdraftLimit;
            if (amt > availableBalance)
            {
                XtraMessageBox.Show($"Yetersiz bakiye!\nKullanılabilir: {availableBalance:N2} {fromAccount.Currency}", 
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var confirm = XtraMessageBox.Show($"{amt:N2} {fromAccount.Currency} tutarında transfer yapılacak.\nGönderen: {fromAccount.Iban}\nAlıcı: {txtToIban.Text.Trim()}\nOnaylıyor musunuz?", 
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            
            btnExternalTransfer.Enabled = false;
            this.UseWaitCursor = true;
            
            var resp = await _api.TransferExternalAsync(fromAccount.Id, txtToIban.Text.Trim(), amt, fromAccount.Currency, txtTransDesc?.Text);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"✓ Transfer işlemi başarılı!\nTutar: {amt:N2} {fromAccount.Currency}\nAlıcı IBAN: {txtToIban.Text.Trim()}", 
                    "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
                RefreshAccountDropdowns();
                BindSenderSummary();
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(resp);
                ShowErrorMessage(resp.StatusCode, errorMsg);
            }
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
        finally
        {
            btnExternalTransfer.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async void btnGetStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            AccountResponse? account = null;
            
            // Ekstre için hesap seçimi (dropdown'dan veya _selectedAccount'tan)
            if (cmbStmtAccount != null && cmbStmtAccount.EditValue != null)
            {
                var accountId = (Guid)cmbStmtAccount.EditValue;
                account = _cachedAccounts.FirstOrDefault(a => a.Id == accountId);
            }
            else
            {
                account = _selectedAccount;
            }
            
            if (account == null) 
            { 
                XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            }
            
            var fromLocal = dtFrom.DateTime.Date;
            var toLocal   = dtTo.DateTime.Date.AddDays(1).AddTicks(-1);
            if (fromLocal > toLocal) 
            { 
                XtraMessageBox.Show("Bitiş tarihi başlangıçtan küçük olamaz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            }
            
            btnGetStatement.Enabled = false;
            this.UseWaitCursor = true;
            
            var from = DateTime.SpecifyKind(fromLocal, DateTimeKind.Local).ToUniversalTime();
            var to   = DateTime.SpecifyKind(toLocal, DateTimeKind.Local).ToUniversalTime();
            
            var stmt = await _api.GetStatementAsync(account.Id, from, to);
            if (stmt is null) 
            { 
                XtraMessageBox.Show("Ekstre alınamadı veya kayıt bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            }
            
            gridStatement.DataSource = stmt.Items.ToList();
            
            // IBAN textbox'ını güncelle (readonly)
            if (txtStmtAccountId != null)
                txtStmtAccountId.Text = account.Iban;
            
            // Kolon genişliklerini ayarla
            if (gridStatementView.Columns["Date"] != null)
            {
                gridStatementView.Columns["Date"].Width = 150;
                gridStatementView.Columns["Date"].Caption = "Tarih";
                gridStatementView.Columns["Date"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gridStatementView.Columns["Date"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            }
            if (gridStatementView.Columns["Description"] != null)
            {
                gridStatementView.Columns["Description"].Width = 400;
                gridStatementView.Columns["Description"].Caption = "Açıklama";
            }
            if (gridStatementView.Columns["Amount"] != null)
            {
                gridStatementView.Columns["Amount"].Width = 200;
                gridStatementView.Columns["Amount"].Caption = "Tutar";
                gridStatementView.Columns["Amount"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridStatementView.Columns["Amount"].DisplayFormat.FormatString = "N2";
            }
            if (gridStatementView.Columns["Balance"] != null)
            {
                gridStatementView.Columns["Balance"].Width = 200;
                gridStatementView.Columns["Balance"].Caption = "Bakiye";
                gridStatementView.Columns["Balance"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridStatementView.Columns["Balance"].DisplayFormat.FormatString = "N2";
            }
            if (gridStatementView.Columns["Direction"] != null)
            {
                gridStatementView.Columns["Direction"].Width = 120;
                gridStatementView.Columns["Direction"].Caption = "Yön";
            }
            
            var currency = account.Currency;
            lblTotals.Text = $"Açılış: {stmt.OpeningBalance:N2} {currency}  |  Alacak: {stmt.TotalCredit:N2} {currency}  |  Borç: {stmt.TotalDebit:N2} {currency}  |  Kapanış: {stmt.ClosingBalance:N2} {currency}";
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
        finally
        {
            btnGetStatement.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async Task<AccountResponse?> FindAccountByNumber(long accountNo)
    {
        try
        {
            // Hesap numarası ile direkt arama
            return await _api.GetAsync<AccountResponse>($"/api/v1/accounts/by-account-no/{accountNo}");
        }
        catch
        {
            return null;
        }
    }

    private async Task<AccountResponse?> FindAccountByIban(string iban)
    {
        try
        {
            // IBAN ile hesap arama
            return await _api.GetAsync<AccountResponse>($"/api/v1/accounts/by-iban/{iban}");
        }
        catch
        {
            return null;
        }
    }

    private void MnuLogout_Click(object sender, EventArgs e)
    {
        var result = XtraMessageBox.Show("Çıkış yapıp farklı kullanıcıyla giriş yapmak ister misiniz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        _isLogoutFlow = true;
        this.Hide();

        using var auth = new FrmAuth();
        var dialog = auth.ShowDialog();
        if (dialog == DialogResult.OK && auth.LoggedInCustomerId.HasValue)
        {
            var newMain = new FrmMain(auth.LoggedInCustomerId.Value);
            newMain.StartPosition = FormStartPosition.CenterScreen;
            // Yeni ana form kapanınca bu (eski) formu da kapat
            newMain.FormClosed += (s, args) => { this.Close(); };
            newMain.Show();
            return;
        }

        // Kullanıcı pencereyi X ile kapattı veya vazgeçtiyse uygulamayı önceki oturuma döndürmeden kapat
        this.Close();
    }

    private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Kullanıcı X ile kapatırsa uygulamayı tamamen kapat
        if (!_isLogoutFlow && e.CloseReason == CloseReason.UserClosing)
        {
            System.Windows.Forms.Application.Exit();
        }
    }

    private async void TxtToIban_Leave(object sender, EventArgs e)
    {
        try
        {
            lblRecipientName.Text = string.Empty;
            var iban = txtToIban.Text?.Trim();
            if (string.IsNullOrWhiteSpace(iban)) return;
            var ownerName = await _api.GetAsync<string>($"/api/v1/accounts/owner-by-iban/{iban}");
            if (!string.IsNullOrWhiteSpace(ownerName))
                lblRecipientName.Text = ownerName;
        }
        catch
        {
            // alıcı bulunamazsa sessiz geç
        }
    }

    private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
    {
        if (e.Page == tabExchangeRates)
        {
            LoadExchangeRatesAsync();
        }
    }

    private void BtnRefreshRates_Click(object sender, EventArgs e)
    {
        LoadExchangeRatesAsync();
    }

    private async void LoadExchangeRatesAsync()
    {
        try
        {
            this.UseWaitCursor = true;
            btnRefreshRates.Enabled = false;
            lblExchangeInfo.Text = "Kurlar yükleniyor...";
            
            var service = new TcmbExchangeRateService();
            var (date, rates) = await service.GetTodayAsync();
            
            if (rates == null || rates.Count == 0)
            {
                XtraMessageBox.Show("Kur bilgisi alınamadı. Lütfen internet bağlantınızı kontrol edin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lblExchangeInfo.Text = "Kur bilgisi alınamadı.";
                return;
            }
            
            // Tarih bilgisini göster
            var timeNote = DateTime.Now.Hour >= 15 && DateTime.Now.Minute >= 30 
                ? "✓ Güncel" 
                : "⚠ 15:30 sonrası güncellenir";
            lblExchangeInfo.Text = $"Tarih: {date:dd.MM.yyyy} | {timeNote} | Toplam {rates.Count} döviz";
            
            // DataGridView'e bağla
            dgvRates.DataSource = rates;
            
            // AutoSizeColumnsMode'u None yap (manuel genişlik kontrolü için)
            dgvRates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            // Kolon başlıklarını düzenle ve formatla
            if (dgvRates.Columns["CurrencyCode"] != null)
            {
                dgvRates.Columns["CurrencyCode"].HeaderText = "Kod";
                dgvRates.Columns["CurrencyCode"].Width = 100;
                dgvRates.Columns["CurrencyCode"].MinimumWidth = 80;
            }
            if (dgvRates.Columns["CurrencyName"] != null)
            {
                dgvRates.Columns["CurrencyName"].HeaderText = "Döviz";
                dgvRates.Columns["CurrencyName"].Width = 250;
                dgvRates.Columns["CurrencyName"].MinimumWidth = 200;
            }
            if (dgvRates.Columns["Unit"] != null)
            {
                dgvRates.Columns["Unit"].HeaderText = "Birim";
                dgvRates.Columns["Unit"].Width = 80;
                dgvRates.Columns["Unit"].MinimumWidth = 60;
                dgvRates.Columns["Unit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvRates.Columns["ForexBuying"] != null)
            {
                dgvRates.Columns["ForexBuying"].HeaderText = "Döviz Alış";
                dgvRates.Columns["ForexBuying"].Width = 180;
                dgvRates.Columns["ForexBuying"].MinimumWidth = 150;
                dgvRates.Columns["ForexBuying"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["ForexBuying"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvRates.Columns["ForexSelling"] != null)
            {
                dgvRates.Columns["ForexSelling"].HeaderText = "Döviz Satış";
                dgvRates.Columns["ForexSelling"].Width = 180;
                dgvRates.Columns["ForexSelling"].MinimumWidth = 150;
                dgvRates.Columns["ForexSelling"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["ForexSelling"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvRates.Columns["BanknoteBuying"] != null)
            {
                dgvRates.Columns["BanknoteBuying"].HeaderText = "Efektif Alış";
                dgvRates.Columns["BanknoteBuying"].Width = 180;
                dgvRates.Columns["BanknoteBuying"].MinimumWidth = 150;
                dgvRates.Columns["BanknoteBuying"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["BanknoteBuying"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvRates.Columns["BanknoteSelling"] != null)
            {
                dgvRates.Columns["BanknoteSelling"].HeaderText = "Efektif Satış";
                dgvRates.Columns["BanknoteSelling"].Width = 180;
                dgvRates.Columns["BanknoteSelling"].MinimumWidth = 150;
                dgvRates.Columns["BanknoteSelling"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["BanknoteSelling"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            
            // Başlık yazılarının tam görünmesi için yüksekliği ayarla
            dgvRates.ColumnHeadersHeight = 40;
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Kur çekilemedi:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblExchangeInfo.Text = "Hata oluştu.";
        }
        finally
        {
            this.UseWaitCursor = false;
            btnRefreshRates.Enabled = true;
        }
    }

    private async Task LoadAdminUI()
    {
        if (tabAdmin == null) return;

        // Panel: Müşteri Arama
        var pnlSearch = new PanelControl()
        {
            Location = new Point(20, 20),
            Size = new Size(1200, 80),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblSearch = new LabelControl()
        {
            Location = new Point(20, 25),
            Size = new Size(150, 22),
            Text = "Müşteri Ara:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };
        
        txtAdminSearch = new TextEdit()
        {
            Location = new Point(20, 50),
            Size = new Size(400, 38),
            Properties = { NullValuePrompt = "Ad, Soyad, TCKN veya Email ile ara..." }
        };
        
        btnAdminSearch = new SimpleButton()
        {
            Location = new Point(440, 50),
            Size = new Size(120, 38),
            Text = "🔍 Ara",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White }
        };
        btnAdminSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        btnAdminSearch.Click += BtnAdminSearch_Click;
        
        pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtAdminSearch, btnAdminSearch });
        
        // Grid: Müşteriler
        gridAdminCustomers = new GridControl()
        {
            Location = new Point(20, 110),
            Size = new Size(600, 400)
        };
        gridAdminCustomersView = new GridView();
        gridAdminCustomers.MainView = gridAdminCustomersView;
        gridAdminCustomersView.OptionsBehavior.Editable = false;
        gridAdminCustomersView.OptionsSelection.MultiSelect = false;
        gridAdminCustomersView.SelectionChanged += GridAdminCustomers_SelectionChanged;
        
        // Grid: Hesaplar
        gridAdminAccounts = new GridControl()
        {
            Location = new Point(640, 110),
            Size = new Size(580, 400)
        };
        gridAdminAccountsView = new GridView();
        gridAdminAccounts.MainView = gridAdminAccountsView;
        gridAdminAccountsView.OptionsBehavior.Editable = false;
        gridAdminAccountsView.OptionsSelection.MultiSelect = false;
        gridAdminAccountsView.SelectionChanged += GridAdminAccounts_SelectionChanged;
        
        // Panel: Hesap İşlemleri
        var pnlAccountActions = new PanelControl()
        {
            Location = new Point(20, 530),
            Size = new Size(1200, 150),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblOverdraft = new LabelControl()
        {
            Location = new Point(20, 20),
            Size = new Size(150, 22),
            Text = "Ek Hesap Limiti:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };
        
        txtAdminOverdraft = new TextEdit()
        {
            Location = new Point(20, 45),
            Size = new Size(200, 38),
            Properties = { Mask = { MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric, EditMask = "n2" } }
        };
        
        btnAdminUpdateOverdraft = new SimpleButton()
        {
            Location = new Point(240, 45),
            Size = new Size(150, 38),
            Text = "✓ Limit Güncelle",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White }
        };
        btnAdminUpdateOverdraft.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnAdminUpdateOverdraft.Click += BtnAdminUpdateOverdraft_Click;
        
        var lblStatus = new LabelControl()
        {
            Location = new Point(420, 20),
            Size = new Size(100, 22),
            Text = "Durum:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };
        
        cmbAdminStatus = new ComboBoxEdit()
        {
            Location = new Point(420, 45),
            Size = new Size(200, 38)
        };
        cmbAdminStatus.Properties.Items.AddRange(new[] { "Active", "Frozen", "Closed" });
        cmbAdminStatus.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        
        btnAdminUpdateStatus = new SimpleButton()
        {
            Location = new Point(640, 45),
            Size = new Size(150, 38),
            Text = "✓ Durum Güncelle",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White }
        };
        btnAdminUpdateStatus.Appearance.BackColor = Color.FromArgb(244, 67, 54);
        btnAdminUpdateStatus.Click += BtnAdminUpdateStatus_Click;
        
        pnlAccountActions.Controls.AddRange(new Control[] { 
            lblOverdraft, txtAdminOverdraft, btnAdminUpdateOverdraft,
            lblStatus, cmbAdminStatus, btnAdminUpdateStatus 
        });
        
        tabAdmin.Controls.AddRange(new Control[] { pnlSearch, gridAdminCustomers, gridAdminAccounts, pnlAccountActions });
        
        // İlk yükleme: Tüm müşterileri getir
        BtnAdminSearch_Click(null, EventArgs.Empty);
    }

    private async void BtnAdminSearch_Click(object? sender, EventArgs e)
    {
        try
        {
            var searchTerm = txtAdminSearch?.Text?.Trim();
            var customers = await _api.SearchCustomersAsync(searchTerm);
            if (customers != null && gridAdminCustomers != null)
            {
                gridAdminCustomers.DataSource = customers;
                
                // Grid kolonlarını ayarla
                if (gridAdminCustomersView != null)
                {
                    gridAdminCustomersView.Columns["CustomerId"].Visible = false;
                    gridAdminCustomersView.Columns["FullName"].Caption = "Ad Soyad";
                    gridAdminCustomersView.Columns["NationalIdMasked"].Caption = "TCKN";
                    gridAdminCustomersView.Columns["Role"].Caption = "Rol";
                }
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Müşteri arama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void GridAdminCustomers_SelectionChanged(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminCustomersView?.FocusedRowHandle < 0) return;
            
            var customer = gridAdminCustomersView.GetRow(gridAdminCustomersView.FocusedRowHandle) as NovaBank.Contracts.Admin.CustomerSummaryResponse;
            if (customer == null) return;
            
            var accounts = await _api.GetCustomerAccountsAsync(customer.CustomerId);
            if (accounts != null && gridAdminAccounts != null)
            {
                gridAdminAccounts.DataSource = accounts;
                
                // Grid kolonlarını ayarla
                if (gridAdminAccountsView != null)
                {
                    gridAdminAccountsView.Columns["AccountId"].Visible = false;
                    gridAdminAccountsView.Columns["Iban"].Caption = "IBAN";
                    gridAdminAccountsView.Columns["Currency"].Caption = "Para Birimi";
                    gridAdminAccountsView.Columns["Balance"].Caption = "Bakiye";
                    gridAdminAccountsView.Columns["Balance"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridAdminAccountsView.Columns["Balance"].DisplayFormat.FormatString = "N2";
                    gridAdminAccountsView.Columns["OverdraftLimit"].Caption = "Ek Hesap Limiti";
                    gridAdminAccountsView.Columns["OverdraftLimit"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridAdminAccountsView.Columns["OverdraftLimit"].DisplayFormat.FormatString = "N2";
                    gridAdminAccountsView.Columns["Status"].Caption = "Durum";
                }
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hesap yükleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void GridAdminAccounts_SelectionChanged(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminAccountsView?.FocusedRowHandle < 0) return;
            
            var account = gridAdminAccountsView.GetRow(gridAdminAccountsView.FocusedRowHandle) as AccountAdminResponse;
            if (account == null) return;
            
            // Seçili hesabın bilgilerini form alanlarına yükle
            if (txtAdminOverdraft != null)
                txtAdminOverdraft.Text = account.OverdraftLimit.ToString("N2");
            
            if (cmbAdminStatus != null)
                cmbAdminStatus.EditValue = account.Status;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Hesap seçim hatası: {ex.Message}");
        }
    }

    private async void BtnAdminUpdateOverdraft_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminAccountsView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var account = gridAdminAccountsView.GetRow(gridAdminAccountsView.FocusedRowHandle) as AccountAdminResponse;
            if (account == null) return;
            
            if (!decimal.TryParse(txtAdminOverdraft?.Text, out var limit) || limit < 0)
            {
                XtraMessageBox.Show("Geçerli bir limit giriniz (>= 0).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var resp = await _api.UpdateOverdraftLimitAsync(account.AccountId, limit);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Ek hesap limiti güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Hesapları yenile
                GridAdminCustomers_SelectionChanged(null, EventArgs.Empty);
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(resp);
                XtraMessageBox.Show($"Hata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnAdminUpdateStatus_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminAccountsView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var account = gridAdminAccountsView.GetRow(gridAdminAccountsView.FocusedRowHandle) as AccountAdminResponse;
            if (account == null) return;
            
            var status = cmbAdminStatus?.EditValue?.ToString();
            if (string.IsNullOrWhiteSpace(status))
            {
                XtraMessageBox.Show("Lütfen bir durum seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var resp = await _api.UpdateAccountStatusAsync(account.AccountId, status);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Hesap durumu güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Hesapları yenile
                GridAdminCustomers_SelectionChanged(null, EventArgs.Empty);
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(resp);
                XtraMessageBox.Show($"Hata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}



