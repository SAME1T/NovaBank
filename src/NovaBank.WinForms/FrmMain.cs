#nullable enable
using NovaBank.WinForms.Services;
using NovaBank.Contracts.Accounts;
using NovaBank.Contracts.Customers;
using NovaBank.Contracts.Transactions;
using NovaBank.Contracts.Reports;
using NovaBank.Contracts.ExchangeRates;
using NovaBank.Contracts.Admin;
using NovaBank.Core.Enums;
using NovaBank.Contracts.Bills;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Windows.Forms;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Microsoft.VisualBasic;

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
        var customerInfo = Session.CurrentCustomerId.HasValue ? $" • Müşteri: {Session.CurrentCustomerId}" : "";
        var roleInfo = Session.IsAdmin ? " • Yönetici" : (Session.CurrentRole == UserRole.Customer ? " • Müşteri" : "");
        this.Text = $"NovaBank Client  •  {_api.BaseUrl}" + customerInfo + roleInfo; 
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

    private void ApplyRoleBasedUI()
    {
        if (tabAdmin == null || tabs == null) return;

        if (Session.IsAdmin)
        {
            // Admin ise tab görünür olsun ve "Yönetim" adıyla gösterilsin
            if (!tabs.TabPages.Contains(tabAdmin))
            {
                tabs.TabPages.Add(tabAdmin);
            }
            tabAdmin.Text = "Yönetim";
            tabAdmin.Visible = true;
        }
        else
        {
            // Customer ise tab'ı koleksiyondan tamamen çıkar
            if (tabs.TabPages.Contains(tabAdmin))
            {
                tabs.TabPages.Remove(tabAdmin);
            }
        }
    }

    private async void FrmMain_Load(object sender, EventArgs e)
    {
        cmbCurrency.Properties.Items.AddRange(Enum.GetValues(typeof(NovaBank.Core.Enums.Currency)));
        cmbCurrency.EditValue = NovaBank.Core.Enums.Currency.TRY;

        if (cmbRecipientAccount != null)
        {
            cmbRecipientAccount.SelectedIndexChanged += CmbRecipientAccount_SelectedIndexChanged;
        }

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

        // Role-based UI ayarlarını uygula
        ApplyRoleBasedUI();

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
            
            // Kartlar sekmesini yükle
            LoadCardsUI();
            
            // Fatura sekmesini yükle
            LoadBillsUI();
            
            // Bildirim sayısını yükle
            await LoadNotificationCountAsync();

            // Admin ise admin UI'ı yükle
            if (Session.IsAdmin)
            {
                await LoadAdminUI();
            }
        }
    }

    private async Task LoadNotificationCountAsync()
    {
        try
        {
            var count = await _api.GetUnreadNotificationCountAsync();
            if (statusStrip != null)
            {
                var lblNotif = statusStrip.Items["lblNotifications"];
                if (lblNotif != null)
                {
                    lblNotif.Text = $"🔔 Bildirimler: {count}";
                    lblNotif.ForeColor = count > 0 ? Color.Yellow : Color.LightGray;
                }
            }
        }
        catch { }
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
                
                // Admin ise alıcı hesap listesini doldur
                if (Session.IsAdmin)
                {
                    await LoadRecipientsForAdminAsync();
                }

                // Kredi kartlarını da yükle ve göster
                var cards = await _api.GetMyCardsAsync();
                if (cards != null && gridMyCards != null)
                {
                    gridMyCards.DataSource = cards.Where(c => c.CreditLimit > 0).ToList();
                    
                    if (gridMyCardsView.Columns["CardId"] != null) gridMyCardsView.Columns["CardId"].Visible = false;
                    if (gridMyCardsView.Columns["Status"] != null) gridMyCardsView.Columns["Status"].Caption = "Durum";
                    if (gridMyCardsView.Columns["MaskedPan"] != null) gridMyCardsView.Columns["MaskedPan"].Caption = "Kart Numarası";
                    if (gridMyCardsView.Columns["CreditLimit"] != null) 
                    {
                        gridMyCardsView.Columns["CreditLimit"].Caption = "Limit";
                        gridMyCardsView.Columns["CreditLimit"].DisplayFormat.FormatString = "N2";
                        gridMyCardsView.Columns["CreditLimit"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    }
                    if (gridMyCardsView.Columns["AvailableLimit"] != null) 
                    {
                        gridMyCardsView.Columns["AvailableLimit"].Caption = "Kullanılabilir Limit";
                        gridMyCardsView.Columns["AvailableLimit"].DisplayFormat.FormatString = "N2";
                        gridMyCardsView.Columns["AvailableLimit"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    }
                    if (gridMyCardsView.Columns["CurrentDebt"] != null) 
                    {
                        gridMyCardsView.Columns["CurrentDebt"].Caption = "Mevcut Borç";
                        gridMyCardsView.Columns["CurrentDebt"].DisplayFormat.FormatString = "N2";
                        gridMyCardsView.Columns["CurrentDebt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    }
                    if (gridMyCardsView.Columns["MinPaymentDueDate"] != null) 
                    {
                        gridMyCardsView.Columns["MinPaymentDueDate"].Caption = "Son Ödeme Tarihi";
                        gridMyCardsView.Columns["MinPaymentDueDate"].DisplayFormat.FormatString = "dd.MM.yyyy";
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

    private void GridMyCards_CellDoubleClick(object sender, EventArgs e)
    {
        tabs.SelectedTabPage = tabCards;
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
            // Kullanıcı isteği üzerine bu buton işlevsiz hale getirildi.
            // Admin için ComboBox seçimi eklendi.
            // XtraMessageBox.Show("Bu fonksiyon kaldırıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) 
        { 
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
    }

    private async Task LoadRecipientsForAdminAsync()
    {
        if (!Session.IsAdmin) return;
        
        try
        {
            var accounts = await _api.GetAllAccountsAsync();
            if (accounts != null)
            {
                // UI thread safe - wait for handle if needed or invoke
                if (cmbRecipientAccount.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        cmbRecipientAccount.Properties.Items.Clear();
                        foreach (var acc in accounts)
                        {
                            cmbRecipientAccount.Properties.Items.Add(new AccountComboItem(acc));
                        }
                        cmbRecipientAccount.Visible = true;
                    });
                }
                else
                {
                     cmbRecipientAccount.Properties.Items.Clear();
                     foreach (var acc in accounts)
                     {
                         cmbRecipientAccount.Properties.Items.Add(new AccountComboItem(acc));
                     }
                     cmbRecipientAccount.Visible = true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Admin alıcıları yüklerken hata: {ex.Message}");
        }
    }

    private void CmbRecipientAccount_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbRecipientAccount.SelectedItem is AccountComboItem item)
        {
            txtToIban.Text = item.Account.Iban;
            // Admin ismini de label'a yazabiliriz
            // lblRecipientName.Text = ... (Servis çağrısı gerekebilir veya CustomerId'den bulunabilir ama şimdilik IBAN yeterli)
        }
    }

    public class AccountComboItem
    {
        public NovaBank.Contracts.Accounts.AccountResponse Account { get; }
        public AccountComboItem(NovaBank.Contracts.Accounts.AccountResponse account)
        {
            Account = account;
        }
        public override string ToString()
        {
            return $"{Account.Iban} - {Account.Currency} ({Account.Balance:N2})";
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

    private async void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
    {
        if (e.Page == tabMyAccounts)
        {
            await LoadAccounts();
        }
        else if (e.Page == tabExchangeRates)
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

        try
        {
            // Önceki kontrolleri temizle
            tabAdmin.Controls.Clear();

            // ===== BAŞLIK =====
            var lblAdminTitle = new LabelControl()
            {
                Location = new Point(20, 10),
                Size = new Size(500, 35),
                Text = "🏛️ Admin Yönetim Paneli",
                Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
            };

            // ===== ALT SEKMELER =====
            tabAdminSub = new XtraTabControl()
            {
                Location = new Point(20, 50),
                Size = new Size(1240, 740),
                HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top
            };
            
            tabAdminUsers = new XtraTabPage() { Text = "👥 Müşteri Yönetimi" };
            tabAdminCards = new XtraTabPage() { Text = "💳 Kredi Kartı Yönetimi" };
            tabAdminAudit = new XtraTabPage() { Text = "📜 Denetim Kayıtları" };
            tabAdminBills = new XtraTabPage() { Text = "📄 Fatura Kurumları" };
            
            tabAdminSub.TabPages.AddRange(new XtraTabPage[] { tabAdminUsers, tabAdminCards, tabAdminBills, tabAdminAudit });

            // ==========================================
            // TAB 1: MÜŞTERİ YÖNETİMİ
            // ==========================================
            
            // ===== ONAY BEKLEYENLER PANELİ =====
            var pnlPendingApprovals = new PanelControl()
            {
                Location = new Point(10, 10),
                Size = new Size(1200, 300),
                Appearance = { BackColor = Color.FromArgb(255, 248, 225), BorderColor = Color.FromArgb(255, 152, 0) }
            };

            lblPendingTitle = new LabelControl()
            {
                Location = new Point(20, 15),
                Size = new Size(400, 30),
                Text = "⏳ Onay Bekleyen Müşteri Kayıtları",
                Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(230, 81, 0) }
            };

            btnRefreshPending = new SimpleButton()
            {
                Location = new Point(20, 55),
                Size = new Size(140, 40),
                Text = "🔄 Yenile",
                Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
            };
            btnRefreshPending.Appearance.BackColor = Color.FromArgb(255, 152, 0);
            btnRefreshPending.Click += BtnRefreshPending_Click;

            btnApproveCustomer = new SimpleButton()
            {
                Location = new Point(170, 55),
                Size = new Size(140, 40),
                Text = "✓ Onayla",
                Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
            };
            btnApproveCustomer.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            btnApproveCustomer.Click += BtnApproveCustomer_Click;

            btnRejectCustomer = new SimpleButton()
            {
                Location = new Point(320, 55),
                Size = new Size(140, 40),
                Text = "✗ Reddet",
                Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
            };
            btnRejectCustomer.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            btnRejectCustomer.Click += BtnRejectCustomer_Click;

            // Grid: Onay Bekleyenler
            gridPendingApprovals = new GridControl()
            {
                Location = new Point(20, 110),
                Size = new Size(1150, 170)
            };
            gridPendingApprovalsView = new GridView();
            gridPendingApprovals.MainView = gridPendingApprovalsView;
            gridPendingApprovalsView.OptionsBehavior.Editable = false;
            gridPendingApprovalsView.OptionsSelection.MultiSelect = false;
            gridPendingApprovalsView.OptionsView.ShowGroupPanel = false;
            gridPendingApprovalsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(255, 152, 0);
            gridPendingApprovalsView.Appearance.HeaderPanel.ForeColor = Color.White;
            gridPendingApprovalsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            pnlPendingApprovals.Controls.AddRange(new Control[] { 
                lblPendingTitle, btnRefreshPending, btnApproveCustomer, btnRejectCustomer, gridPendingApprovals 
            });

            // ===== MÜŞTERİ ARAMA PANELİ =====
            var pnlSearch = new PanelControl()
            {
                Location = new Point(10, 320),
                Size = new Size(1200, 100),
                Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
            };

            var lblSearch = new LabelControl()
            {
                Location = new Point(20, 15),
                Size = new Size(200, 28),
                Text = "🔍 Müşteri Arama",
                Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
            };

            txtAdminSearch = new TextEdit()
            {
                Location = new Point(20, 50),
                Size = new Size(400, 38)
            };
            txtAdminSearch.Properties.NullValuePrompt = "Ad, Soyad, TCKN veya Email ile ara...";

            btnAdminSearch = new SimpleButton()
            {
                Location = new Point(440, 50),
                Size = new Size(120, 38),
                Text = "🔍 Ara",
                Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
            };
            btnAdminSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);
            btnAdminSearch.Click += BtnAdminSearch_Click;

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtAdminSearch, btnAdminSearch });

            // ===== MÜŞTERİ LİSTESİ =====
            gridAdminCustomers = new GridControl()
            {
                Location = new Point(10, 430),
                Size = new Size(580, 260)
            };
            gridAdminCustomersView = new GridView();
            gridAdminCustomers.MainView = gridAdminCustomersView;
            gridAdminCustomersView.OptionsBehavior.Editable = false;
            gridAdminCustomersView.OptionsSelection.MultiSelect = false;
            gridAdminCustomersView.OptionsView.ShowGroupPanel = false;
            gridAdminCustomersView.Appearance.HeaderPanel.BackColor = Color.FromArgb(25, 118, 210);
            gridAdminCustomersView.Appearance.HeaderPanel.ForeColor = Color.White;
            gridAdminCustomersView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gridAdminCustomersView.SelectionChanged += GridAdminCustomers_SelectionChanged;

            // ===== HESAP LİSTESİ =====
            gridAdminAccounts = new GridControl()
            {
                Location = new Point(610, 430),
                Size = new Size(600, 260)
            };
            gridAdminAccountsView = new GridView();
            gridAdminAccounts.MainView = gridAdminAccountsView;
            gridAdminAccountsView.OptionsBehavior.Editable = false;
            gridAdminAccountsView.OptionsSelection.MultiSelect = false;
            gridAdminAccountsView.OptionsView.ShowGroupPanel = false;
            gridAdminAccountsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(25, 118, 210);
            gridAdminAccountsView.Appearance.HeaderPanel.ForeColor = Color.White;
            gridAdminAccountsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gridAdminAccountsView.SelectionChanged += GridAdminAccounts_SelectionChanged;

            tabAdminUsers.Controls.AddRange(new Control[] { pnlPendingApprovals, pnlSearch, gridAdminCustomers, gridAdminAccounts });

            // ==========================================
            // TAB 2: KREDİ KARTI YÖNETİMİ
            // ==========================================
            LoadAdminCreditCardsUI();
            LoadAdminAuditUI();
            LoadAdminBillsUI();

            // Tüm kontrolleri tabAdmin'e ekle
            tabAdmin.Controls.AddRange(new Control[] { lblAdminTitle, tabAdminSub });

            // İlk yüklemeleri yap
            BtnRefreshPending_Click(null, EventArgs.Empty);
            BtnAdminSearch_Click(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Admin paneli yüklenirken hata oluştu:\n\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadAdminCreditCardsUI()
    {
        // Panel: Bekleyen Kart Başvuruları
        var pnlCardApps = new PanelControl()
        {
            Location = new Point(10, 10),
            Size = new Size(1200, 680),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };

        var lblCardTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "📋 Kredi Kartı Başvuruları ve Yönetimi",
            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(156, 39, 176) }
        };

        btnRefreshCardApps = new SimpleButton()
        {
            Location = new Point(20, 60),
            Size = new Size(140, 40),
            Text = "🔄 Yenile",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
        };
        btnRefreshCardApps.Appearance.BackColor = Color.FromArgb(156, 39, 176); // Purple
        btnRefreshCardApps.Click += BtnRefreshCardApps_Click;

        btnApproveCardApp = new SimpleButton()
        {
            Location = new Point(170, 60),
            Size = new Size(140, 40),
            Text = "✓ Onayla",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
        };
        btnApproveCardApp.Appearance.BackColor = Color.FromArgb(76, 175, 80); // Green
        btnApproveCardApp.Click += BtnApproveCardApp_Click;

        btnRejectCardApp = new SimpleButton()
        {
            Location = new Point(320, 60),
            Size = new Size(140, 40),
            Text = "✗ Reddet",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
        };
        btnRejectCardApp.Appearance.BackColor = Color.FromArgb(244, 67, 54); // Red
        btnRejectCardApp.Click += BtnRejectCardApp_Click;

        gridAdminCardApplications = new GridControl()
        {
            Location = new Point(20, 120),
            Size = new Size(1160, 540)
        };
        gridAdminCardApplicationsView = new GridView();
        gridAdminCardApplications.MainView = gridAdminCardApplicationsView;
        gridAdminCardApplicationsView.OptionsBehavior.Editable = false;
        gridAdminCardApplicationsView.OptionsSelection.MultiSelect = false;
        gridAdminCardApplicationsView.OptionsView.ShowGroupPanel = false;
        gridAdminCardApplicationsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(156, 39, 176);
        gridAdminCardApplicationsView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridAdminCardApplicationsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        pnlCardApps.Controls.AddRange(new Control[] { lblCardTitle, btnRefreshCardApps, btnApproveCardApp, btnRejectCardApp, gridAdminCardApplications });
        
        tabAdminCards.Controls.Add(pnlCardApps);
        
        // Load initial data
        BtnRefreshCardApps_Click(null, EventArgs.Empty);
    }

    private void LoadAdminAuditUI()
    {
        if (tabAdminAudit == null) return;

        var pnlAudit = new PanelControl()
        {
            Dock = DockStyle.Fill,
            Appearance = { BackColor = Color.White }
        };

        var lblAuditTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "📜 Sistem Denetim Kayıtları (Audit Logs)",
            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(69, 90, 100) }
        };

        // Filtreler Paneli
        var pnlFilters = new PanelControl()
        {
            Location = new Point(20, 60),
            Size = new Size(1200, 100),
            Appearance = { BackColor = Color.FromArgb(245, 247, 249) }
        };

        var lblFrom = new LabelControl() { Location = new Point(15, 15), Text = "Başlangıç:" };
        dtAuditFrom = new DateEdit() { Location = new Point(15, 35), Size = new Size(130, 30) };
        dtAuditFrom.EditValue = DateTime.Now.AddDays(-7);

        var lblTo = new LabelControl() { Location = new Point(155, 15), Text = "Bitiş:" };
        dtAuditTo = new DateEdit() { Location = new Point(155, 35), Size = new Size(130, 30) };
        dtAuditTo.EditValue = DateTime.Now;

        var lblAction = new LabelControl() { Location = new Point(295, 15), Text = "İşlem:" };
        cmbAuditAction = new ComboBoxEdit() { Location = new Point(295, 35), Size = new Size(150, 30) };
        cmbAuditAction.Properties.Items.AddRange(new object[] { "Hepsi", "Login", "Transfer", "AccountCreated", "CreditCardApplication", "Payment" });
        cmbAuditAction.SelectedIndex = 0;

        var lblSuccess = new LabelControl() { Location = new Point(455, 15), Text = "Durum:" };
        cmbAuditSuccess = new ComboBoxEdit() { Location = new Point(455, 35), Size = new Size(100, 30) };
        cmbAuditSuccess.Properties.Items.AddRange(new object[] { "Hepsi", "Başarılı", "Başarısız" });
        cmbAuditSuccess.SelectedIndex = 0;

        var lblSearch = new LabelControl() { Location = new Point(565, 15), Text = "Arama (Özet/ID):" };
        txtAuditSearch = new TextEdit() { Location = new Point(565, 35), Size = new Size(200, 30) };

        btnAuditLoad = new SimpleButton()
        {
            Location = new Point(780, 30),
            Size = new Size(120, 40),
            Text = "🔍 Yükle",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
        };
        btnAuditLoad.Appearance.BackColor = Color.FromArgb(69, 90, 100);
        btnAuditLoad.Click += BtnAuditLoad_Click;

        pnlFilters.Controls.AddRange(new Control[] { 
            lblFrom, dtAuditFrom, lblTo, dtAuditTo, lblAction, cmbAuditAction, 
            lblSuccess, cmbAuditSuccess, lblSearch, txtAuditSearch, btnAuditLoad 
        });

        gridAuditLogs = new GridControl() { Location = new Point(20, 170), Size = new Size(1200, 490) };
        gridAuditLogsView = new GridView();
        gridAuditLogs.MainView = gridAuditLogsView;
        gridAuditLogsView.OptionsBehavior.Editable = false;
        gridAuditLogsView.OptionsView.ShowGroupPanel = false;
        gridAuditLogsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(69, 90, 100);
        gridAuditLogsView.Appearance.HeaderPanel.ForeColor = Color.White;

        pnlAudit.Controls.AddRange(new Control[] { lblAuditTitle, pnlFilters, gridAuditLogs });
        tabAdminAudit.Controls.Add(pnlAudit);

        BtnAuditLoad_Click(null, EventArgs.Empty);
    }

    private void LoadAdminBillsUI()
    {
        if (tabAdminBills == null) return;
        tabAdminBills.Controls.Clear();

        var pnlMain = new PanelControl() { Dock = DockStyle.Fill, Appearance = { BackColor = Color.White } };

        var lblTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "🏢 Fatura Kurumları Yönetimi",
            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 121, 107) }
        };

        // Yeni Kurum Ekleme Paneli
        var pnlAdd = new PanelControl()
        {
            Location = new Point(20, 60),
            Size = new Size(400, 300),
            Appearance = { BackColor = Color.FromArgb(224, 242, 241), BorderColor = Color.FromArgb(0, 121, 107) }
        };

        var lblAddTitle = new LabelControl() { Location = new Point(20, 15), Text = "🆕 Yeni Kurum Ekle", Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold) } };
        
        var lblCode = new LabelControl() { Location = new Point(20, 50), Text = "Kurum Kodu (Örn: IGSDA):" };
        txtInstCode = new TextEdit() { Location = new Point(20, 70), Size = new Size(360, 30) };
        
        var lblName = new LabelControl() { Location = new Point(20, 105), Text = "Kurum Adı:" };
        txtInstName = new TextEdit() { Location = new Point(20, 125), Size = new Size(360, 30) };
        
        var lblCategory = new LabelControl() { Location = new Point(20, 160), Text = "Kategori:" };
        cmbInstCategory = new ComboBoxEdit() { Location = new Point(20, 180), Size = new Size(360, 30) };
        cmbInstCategory.Properties.Items.AddRange(Enum.GetNames(typeof(BillCategory)));
        cmbInstCategory.SelectedIndex = 0;

        btnAddInstitution = new SimpleButton()
        {
            Location = new Point(20, 230),
            Size = new Size(360, 40),
            Text = "💾 Kurumu Kaydet",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        btnAddInstitution.Appearance.BackColor = Color.FromArgb(0, 121, 107);
        btnAddInstitution.Click += BtnAddInstitution_Click;

        pnlAdd.Controls.AddRange(new Control[] { lblAddTitle, lblCode, txtInstCode, lblName, txtInstName, lblCategory, cmbInstCategory, btnAddInstitution });

        // Liste Paneli
        var pnlList = new PanelControl() { Location = new Point(440, 60), Size = new Size(780, 600) };
        
        btnRefreshInstitutions = new SimpleButton() { Location = new Point(20, 10), Size = new Size(120, 30), Text = "🔄 Yenile" };
        btnRefreshInstitutions.Click += (s, e) => LoadAdminInstitutionsAsync();

        btnDeleteInstitution = new SimpleButton() { Location = new Point(150, 10), Size = new Size(120, 30), Text = "✗ Sil", Appearance = { ForeColor = Color.Red } };
        btnDeleteInstitution.Click += BtnDeleteInstitution_Click;

        gridAdminInstitutions = new GridControl() { Location = new Point(20, 50), Size = new Size(740, 530) };
        gridAdminInstitutionsView = new GridView();
        gridAdminInstitutions.MainView = gridAdminInstitutionsView;
        gridAdminInstitutionsView.OptionsBehavior.Editable = false;
        gridAdminInstitutionsView.OptionsView.ShowGroupPanel = false;

        pnlList.Controls.AddRange(new Control[] { btnRefreshInstitutions, btnDeleteInstitution, gridAdminInstitutions });

        pnlMain.Controls.AddRange(new Control[] { lblTitle, pnlAdd, pnlList });
        tabAdminBills.Controls.Add(pnlMain);

        LoadAdminInstitutionsAsync();
    }

    private async void LoadAdminInstitutionsAsync()
    {
        try
        {
            var list = await _api.GetBillInstitutionsAsync();
            gridAdminInstitutions.DataSource = list;
        }
        catch (Exception ex) { XtraMessageBox.Show("Kurumlar yüklenemedi: " + ex.Message); }
    }

    private async void BtnAddInstitution_Click(object? sender, EventArgs e)
    {
        try
        {
            var code = txtInstCode?.Text?.Trim();
            var name = txtInstName?.Text?.Trim();
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name)) return;

            var category = (BillCategory)Enum.Parse(typeof(BillCategory), cmbInstCategory.Text);
            
            var req = new CreateBillInstitutionRequest(code, name, category);
            var resp = await _api.CreateBillInstitutionAsync(req);
            
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Kurum başarıyla eklendi.");
                txtInstCode!.Text = "";
                txtInstName!.Text = "";
                LoadAdminInstitutionsAsync();
                await LoadBillInstitutionsAsync(); // Kullanıcı tarafını da güncelle
            }
            else
            {
                var err = await ApiClient.GetErrorMessageAsync(resp);
                XtraMessageBox.Show("Hata: " + err);
            }
        }
        catch (Exception ex) { XtraMessageBox.Show("Hata: " + ex.Message); }
    }

    private async void BtnDeleteInstitution_Click(object? sender, EventArgs e)
    {
        var row = gridAdminInstitutionsView.GetFocusedRow() as BillInstitutionResponse;
        if (row == null) return;

        if (XtraMessageBox.Show($"{row.Name} kurumunu silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        try
        {
            var resp = await _api.DeleteBillInstitutionAsync(row.Id);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Kurum silindi.");
                LoadAdminInstitutionsAsync();
                await LoadBillInstitutionsAsync();
            }
        }
        catch (Exception ex) { XtraMessageBox.Show("Hata: " + ex.Message); }
    }


    private async void BtnRefreshCardApps_Click(object? sender, EventArgs e)
    {
        try
        {
            var apps = await _api.GetPendingCardApplicationsAsync();
            if (gridAdminCardApplications != null)
            {
                gridAdminCardApplications.DataSource = apps;
                if (gridAdminCardApplicationsView != null)
                {
                    // Kolonları düzenle
                    if (gridAdminCardApplicationsView.Columns["ApplicationId"] != null) gridAdminCardApplicationsView.Columns["ApplicationId"].Visible = false;
                    if (gridAdminCardApplicationsView.Columns["CustomerId"] != null) gridAdminCardApplicationsView.Columns["CustomerId"].Visible = false;
                    
                    if (gridAdminCardApplicationsView.Columns["CustomerName"] != null)
                    {
                        gridAdminCardApplicationsView.Columns["CustomerName"].Caption = "Müşteri Adı";
                        gridAdminCardApplicationsView.Columns["CustomerName"].VisibleIndex = 0;
                    }
                    if (gridAdminCardApplicationsView.Columns["RequestedLimit"] != null)
                    {
                        gridAdminCardApplicationsView.Columns["RequestedLimit"].Caption = "Talep Edilen Limit (₺)";
                        gridAdminCardApplicationsView.Columns["RequestedLimit"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridAdminCardApplicationsView.Columns["RequestedLimit"].DisplayFormat.FormatString = "N2";
                        gridAdminCardApplicationsView.Columns["RequestedLimit"].VisibleIndex = 1;
                    }
                    if (gridAdminCardApplicationsView.Columns["MonthlyIncome"] != null)
                    {
                        gridAdminCardApplicationsView.Columns["MonthlyIncome"].Caption = "Aylık Gelir (₺)";
                        gridAdminCardApplicationsView.Columns["MonthlyIncome"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridAdminCardApplicationsView.Columns["MonthlyIncome"].DisplayFormat.FormatString = "N2";
                        gridAdminCardApplicationsView.Columns["MonthlyIncome"].VisibleIndex = 2;
                    }
                    if (gridAdminCardApplicationsView.Columns["CreatedAt"] != null)
                    {
                        gridAdminCardApplicationsView.Columns["CreatedAt"].Caption = "Başvuru Tarihi";
                        gridAdminCardApplicationsView.Columns["CreatedAt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        gridAdminCardApplicationsView.Columns["CreatedAt"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
                        gridAdminCardApplicationsView.Columns["CreatedAt"].VisibleIndex = 3;
                    }
                     if (gridAdminCardApplicationsView.Columns["Status"] != null)
                    {
                        gridAdminCardApplicationsView.Columns["Status"].Caption = "Durum";
                        gridAdminCardApplicationsView.Columns["Status"].VisibleIndex = 4;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Başvurular yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnApproveCardApp_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminCardApplicationsView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir başvuru seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var app = gridAdminCardApplicationsView.GetRow(gridAdminCardApplicationsView.FocusedRowHandle) as NovaBank.WinForms.Services.CreditCardApplicationDto;
            if (app == null) return;

            // Onaylanan limit sor
            var approvedLimitStr = Microsoft.VisualBasic.Interaction.InputBox(
                $"'{app.CustomerName}' adlı müşterinin kredi kartı başvurusunu onaylıyorsunuz.\n\nTalep Edilen: {app.RequestedLimit:N2} TL\n\nOnaylanan limit miktarını giriniz:",
                "Limit Onayı",
                app.RequestedLimit.ToString("F0"));

            if (string.IsNullOrWhiteSpace(approvedLimitStr)) return; // İptal

            if (!decimal.TryParse(approvedLimitStr, out var approvedLimit) || approvedLimit <= 0)
            {
                XtraMessageBox.Show("Geçerli bir limit giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnApproveCardApp.Enabled = false;
            var response = await _api.ApproveCardApplicationAsync(app.ApplicationId, approvedLimit);
            
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("✓ Başvuru onaylandı ve kart oluşturuldu.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnRefreshCardApps_Click(null, EventArgs.Empty);
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"İşlem başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnApproveCardApp.Enabled = true;
        }
    }

    private async void BtnRejectCardApp_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminCardApplicationsView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir başvuru seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var app = gridAdminCardApplicationsView.GetRow(gridAdminCardApplicationsView.FocusedRowHandle) as NovaBank.WinForms.Services.CreditCardApplicationDto;
            if (app == null) return;

            // Red nedeni sor
            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                $"'{app.CustomerName}' adlı müşterinin başvurusunu REDDETMEK üzeresiniz.\n\nRed nedenini giriniz:",
                "Red Nedeni",
                "Uygun görülmedi");

            if (string.IsNullOrWhiteSpace(reason)) return; // İptal

            btnRejectCardApp.Enabled = false;
            var response = await _api.RejectCardApplicationAsync(app.ApplicationId, reason);
            
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Başvuru reddedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnRefreshCardApps_Click(null, EventArgs.Empty);
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"İşlem başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRejectCardApp.Enabled = true;
        }
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
                    gridAdminCustomersView.Columns["IsActive"].Caption = "Aktif";
                    if (gridAdminCustomersView.Columns["IsApproved"] != null)
                        gridAdminCustomersView.Columns["IsApproved"].Caption = "Onaylı";
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
            
            // Checkbox'ı güncelle
            if (chkAdminIsActive != null)
                chkAdminIsActive.Checked = customer.IsActive;
            
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

    private async void BtnAdminSaveActive_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminCustomersView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir müşteri seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var customer = gridAdminCustomersView.GetRow(gridAdminCustomersView.FocusedRowHandle) as NovaBank.Contracts.Admin.CustomerSummaryResponse;
            if (customer == null) return;
            
            if (chkAdminIsActive == null) return;
            
            var resp = await _api.UpdateCustomerActiveAsync(customer.CustomerId, chkAdminIsActive.Checked);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Müşteri aktiflik durumu güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Müşteri listesini yenile
                BtnAdminSearch_Click(null, EventArgs.Empty);
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

    private async void BtnAdminResetPassword_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminCustomersView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir müşteri seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var customer = gridAdminCustomersView.GetRow(gridAdminCustomersView.FocusedRowHandle) as NovaBank.Contracts.Admin.CustomerSummaryResponse;
            if (customer == null) return;
            
            var confirm = XtraMessageBox.Show(
                $"'{customer.FullName}' müşterisinin şifresini sıfırlamak istediğinize emin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (confirm != DialogResult.Yes) return;
            
            var result = await _api.ResetCustomerPasswordAsync(customer.CustomerId);
            if (result != null)
            {
                XtraMessageBox.Show(
                    $"Geçici Şifre: {result.TemporaryPassword}\n\nMüşteriye ilet.",
                    "Şifre Sıfırlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show("Şifre sıfırlama başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnAuditLoad_Click(object? sender, EventArgs e)
    {
        try
        {
            // Tarih filtreleri: Sadece DATE kısmını al (saat 00:00)
            DateTime? from = null;
            if (dtAuditFrom?.EditValue is DateTime fromDt)
            {
                from = fromDt.Date;
            }
            
            DateTime? to = null;
            if (dtAuditTo?.EditValue is DateTime toDt)
            {
                to = toDt.Date;
            }
            
            var search = txtAuditSearch?.Text?.Trim();
            
            // Action mapping: "Hepsi" veya boş ise null
            var action = cmbAuditAction?.EditValue?.ToString();
            if (string.IsNullOrWhiteSpace(action) || action == "Hepsi")
                action = null;

            // Success mapping: "Hepsi" => null, "Başarılı" => true, "Başarısız" => false
            bool? success = null;
            var successValue = cmbAuditSuccess?.EditValue?.ToString();
            if (successValue == "Başarılı")
                success = true;
            else if (successValue == "Başarısız")
                success = false;
            // "Hepsi" veya null ise success = null kalır

            btnAuditLoad.Enabled = false;
            this.UseWaitCursor = true;
            
            var logs = await _api.GetAuditLogsAsync(from, to, search, action, success, 200);
            if (logs != null && gridAuditLogs != null && gridAuditLogsView != null)
            {
                if (logs.Count == 0)
                {
                    XtraMessageBox.Show("Seçilen filtreye göre kayıt bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    gridAuditLogs.DataSource = null;
                    return;
                }

                gridAuditLogs.DataSource = logs;
                
                // Kolonları yapılandır (her seferinde yeniden yapılandır)
                gridAuditLogsView.PopulateColumns();
                
                if (gridAuditLogsView.Columns["Id"] != null)
                    gridAuditLogsView.Columns["Id"].Visible = false;
                if (gridAuditLogsView.Columns["ActorCustomerId"] != null)
                    gridAuditLogsView.Columns["ActorCustomerId"].Visible = false;
                    
                if (gridAuditLogsView.Columns["CreatedAt"] != null)
                {
                    gridAuditLogsView.Columns["CreatedAt"].Caption = "Tarih";
                    gridAuditLogsView.Columns["CreatedAt"].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
                    gridAuditLogsView.Columns["CreatedAt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                }
                if (gridAuditLogsView.Columns["ActorRole"] != null)
                    gridAuditLogsView.Columns["ActorRole"].Caption = "Rol";
                if (gridAuditLogsView.Columns["Action"] != null)
                    gridAuditLogsView.Columns["Action"].Caption = "Aksiyon";
                if (gridAuditLogsView.Columns["EntityType"] != null)
                    gridAuditLogsView.Columns["EntityType"].Caption = "Varlık Tipi";
                if (gridAuditLogsView.Columns["EntityId"] != null)
                    gridAuditLogsView.Columns["EntityId"].Caption = "Varlık ID";
                if (gridAuditLogsView.Columns["Success"] != null)
                {
                    gridAuditLogsView.Columns["Success"].Caption = "Başarılı";
                }
                if (gridAuditLogsView.Columns["ErrorCode"] != null)
                    gridAuditLogsView.Columns["ErrorCode"].Caption = "Hata Kodu";
                if (gridAuditLogsView.Columns["Summary"] != null)
                {
                    gridAuditLogsView.Columns["Summary"].Caption = "Özet";
                    gridAuditLogsView.Columns["Summary"].Width = 300;
                }
                
                // Grid'i yenile
                gridAuditLogsView.BestFitColumns();
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Denetim kayıtları yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnAuditLoad.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private void GridAuditLogs_DoubleClick(object? sender, EventArgs e)
    {
        try
        {
            if (gridAuditLogsView?.FocusedRowHandle < 0) return;

            var log = gridAuditLogsView.GetRow(gridAuditLogsView.FocusedRowHandle) as AuditLogResponse;
            if (log == null) return;

            var details = $"Özet: {log.Summary ?? "-"}\n\n" +
                         $"Varlık ID: {log.EntityId ?? "-"}\n" +
                         $"Varlık Tipi: {log.EntityType ?? "-"}\n" +
                         $"Aksiyon: {log.Action}\n" +
                         $"Rol: {log.ActorRole}\n" +
                         $"Başarılı: {(log.Success ? "Evet" : "Hayır")}\n" +
                         $"Hata Kodu: {log.ErrorCode ?? "-"}\n" +
                         $"Tarih: {log.CreatedAt:yyyy-MM-dd HH:mm:ss}";

            XtraMessageBox.Show(details, "Denetim Kaydı Detayları", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnRefreshPending_Click(object? sender, EventArgs e)
    {
        try
        {
            var pending = await _api.GetPendingApprovalsAsync();
            if (gridPendingApprovals != null)
            {
                gridPendingApprovals.DataSource = pending;
                
                // Bekleyen sayısını göster
                if (lblPendingTitle != null)
                {
                    var count = pending?.Count ?? 0;
                    if (count > 0)
                        lblPendingTitle.Text = $"⏳ Onay Bekleyen Müşteriler ({count})";
                    else
                        lblPendingTitle.Text = "✅ Onay Bekleyen Müşteri Yok";
                }
                
                if (gridPendingApprovalsView.Columns["CustomerId"] != null)
                    gridPendingApprovalsView.Columns["CustomerId"].Visible = false;
                if (gridPendingApprovalsView.Columns["FullName"] != null)
                    gridPendingApprovalsView.Columns["FullName"].Caption = "Ad Soyad";
                if (gridPendingApprovalsView.Columns["NationalId"] != null)
                    gridPendingApprovalsView.Columns["NationalId"].Caption = "TCKN";
                if (gridPendingApprovalsView.Columns["Email"] != null)
                    gridPendingApprovalsView.Columns["Email"].Caption = "E-posta";
                if (gridPendingApprovalsView.Columns["CreatedAt"] != null)
                {
                    gridPendingApprovalsView.Columns["CreatedAt"].Caption = "Kayıt Tarihi";
                    gridPendingApprovalsView.Columns["CreatedAt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    gridPendingApprovalsView.Columns["CreatedAt"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
                }
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Onay bekleyenler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnApproveCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridPendingApprovalsView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen onaylanacak müşteriyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var pending = gridPendingApprovalsView.GetRow(gridPendingApprovalsView.FocusedRowHandle) as NovaBank.Contracts.Admin.PendingApprovalResponse;
            if (pending == null) return;

            var confirm = XtraMessageBox.Show(
                $"'{pending.FullName}' adlı müşteriyi onaylamak istiyor musunuz?\n\nTCKN: {pending.NationalId}",
                "Müşteri Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm != DialogResult.Yes) return;

            btnApproveCustomer.Enabled = false;
            var response = await _api.ApproveCustomerAsync(pending.CustomerId);
            
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"✓ '{pending.FullName}' başarıyla onaylandı!\n\nArtık sisteme giriş yapabilir.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnRefreshPending_Click(null, EventArgs.Empty);
                BtnAdminSearch_Click(null, EventArgs.Empty);
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Onaylama başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnApproveCustomer.Enabled = true;
        }
    }

    private async void BtnRejectCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridPendingApprovalsView?.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen reddedilecek müşteriyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var pending = gridPendingApprovalsView.GetRow(gridPendingApprovalsView.FocusedRowHandle) as NovaBank.Contracts.Admin.PendingApprovalResponse;
            if (pending == null) return;

            var confirm = XtraMessageBox.Show(
                $"'{pending.FullName}' adlı müşterinin kaydını REDDETMEK istiyor musunuz?\n\nTCKN: {pending.NationalId}\n\n⚠️ Bu işlem müşteriyi pasif yapacaktır!",
                "Müşteri Reddi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            
            if (confirm != DialogResult.Yes) return;

            btnRejectCustomer.Enabled = false;
            var response = await _api.RejectCustomerAsync(pending.CustomerId);
            
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"'{pending.FullName}' reddedildi ve pasif yapıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnRefreshPending_Click(null, EventArgs.Empty);
                BtnAdminSearch_Click(null, EventArgs.Empty);
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Reddetme başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRejectCustomer.Enabled = true;
        }
    }

    // ===================== KREDİ KARTI MODÜLÜ =====================

    private void LoadCardsUI()
    {
        if (tabCards == null) return;

        tabCards.Controls.Clear();

        // Başlık
        var lblTitle = new LabelControl()
        {
            Location = new Point(20, 20),
            Size = new Size(400, 35),
            Text = "💳 Kredi Kartlarım",
            Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };

        // Kart Başvuru Paneli
        var pnlApply = new PanelControl()
        {
            Location = new Point(20, 70),
            Size = new Size(400, 160),
            Appearance = { BackColor = Color.FromArgb(232, 245, 253), BorderColor = Color.FromArgb(25, 118, 210) }
        };

        var lblApplyTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "📝 Yeni Kredi Kartı Başvurusu",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };

        var lblLimit = new LabelControl()
        {
            Location = new Point(20, 50),
            Text = "Talep Edilen Limit (₺):",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        txtCardLimit = new TextEdit()
        {
            Location = new Point(20, 75),
            Size = new Size(170, 35)
        };
        txtCardLimit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtCardLimit.Properties.Mask.EditMask = "n0";
        txtCardLimit.Properties.NullValuePrompt = "Örn: 10000";

        var lblIncome = new LabelControl()
        {
            Location = new Point(210, 50),
            Text = "Aylık Gelir (₺):",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        txtCardIncome = new TextEdit()
        {
            Location = new Point(210, 75),
            Size = new Size(170, 35)
        };
        txtCardIncome.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtCardIncome.Properties.Mask.EditMask = "n2";
        txtCardIncome.Properties.NullValuePrompt = "Örn: 25000";

        btnApplyCard = new SimpleButton()
        {
            Location = new Point(20, 115),
            Size = new Size(360, 35),
            Text = "✓ Başvur",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        btnApplyCard.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        btnApplyCard.Click += BtnApplyCard_Click;

        pnlApply.Controls.AddRange(new Control[] { lblApplyTitle, lblLimit, txtCardLimit, lblIncome, txtCardIncome, btnApplyCard });

        // Kart Listesi Paneli
        var pnlCards = new PanelControl()
        {
            Location = new Point(450, 70),
            Size = new Size(800, 300),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };

        var lblCardsTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "🏦 Mevcut Kartlarım",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80) }
        };

        btnRefreshCards = new SimpleButton()
        {
            Location = new Point(650, 10),
            Size = new Size(120, 30),
            Text = "🔄 Yenile",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White }
        };
        btnRefreshCards.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnRefreshCards.Click += BtnRefreshCards_Click;

        gridCardsMain = new GridControl()
        {
            Location = new Point(20, 50),
            Size = new Size(760, 230)
        };
        gridCardsMainView = new GridView();
        gridCardsMain.MainView = gridCardsMainView;
        gridCardsMainView.OptionsBehavior.Editable = false;
        gridCardsMainView.OptionsView.ShowGroupPanel = false;
        gridCardsMainView.Appearance.HeaderPanel.BackColor = Color.FromArgb(76, 175, 80);
        gridCardsMainView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridCardsMainView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);

        pnlCards.Controls.AddRange(new Control[] { lblCardsTitle, btnRefreshCards, gridCardsMain });

        // Borç Ödeme Paneli
        var pnlPayment = new PanelControl()
        {
            Location = new Point(20, 240),
            Size = new Size(400, 130),
            Appearance = { BackColor = Color.FromArgb(255, 243, 224), BorderColor = Color.FromArgb(255, 152, 0) }
        };

        var lblPayTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "💰 Kart Borcu Öde",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(255, 152, 0) }
        };

        var lblPayAmount = new LabelControl()
        {
            Location = new Point(20, 55),
            Text = "Ödenecek Tutar (₺):",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        txtCardPaymentAmount = new TextEdit()
        {
            Location = new Point(20, 80),
            Size = new Size(200, 35)
        };
        txtCardPaymentAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtCardPaymentAmount.Properties.Mask.EditMask = "n2";

        btnPayCardDebt = new SimpleButton()
        {
            Location = new Point(240, 80),
            Size = new Size(140, 35),
            Text = "💳 Öde",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        btnPayCardDebt.Appearance.BackColor = Color.FromArgb(255, 152, 0);
        btnPayCardDebt.Click += BtnPayCardDebt_Click;

        pnlPayment.Controls.AddRange(new Control[] { lblPayTitle, lblPayAmount, txtCardPaymentAmount, btnPayCardDebt });

        // Başvuru Durumu Paneli
        var pnlApplications = new PanelControl()
        {
            Location = new Point(20, 390),
            Size = new Size(1230, 250),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };

        var lblAppTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "📋 Başvuru Durumlarım",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(156, 39, 176) }
        };

        gridCardApplications = new GridControl()
        {
            Location = new Point(20, 50),
            Size = new Size(1190, 180)
        };
        gridCardApplicationsView = new GridView();
        gridCardApplications.MainView = gridCardApplicationsView;
        gridCardApplicationsView.OptionsBehavior.Editable = false;
        gridCardApplicationsView.OptionsView.ShowGroupPanel = false;
        gridCardApplicationsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(156, 39, 176);
        gridCardApplicationsView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridCardApplicationsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);

        pnlApplications.Controls.AddRange(new Control[] { lblAppTitle, gridCardApplications });

        // Kontrolleri ekle
        tabCards.Controls.AddRange(new Control[] { lblTitle, pnlApply, pnlCards, pnlPayment, pnlApplications });

        // İlk yükleme
        BtnRefreshCards_Click(null, EventArgs.Empty);
    }

    private async void BtnApplyCard_Click(object? sender, EventArgs e)
    {
        try
        {
            if (txtCardLimit == null || txtCardIncome == null) return;
            
            var limitText = txtCardLimit.EditValue?.ToString();
            var incomeText = txtCardIncome.EditValue?.ToString();

            if (string.IsNullOrWhiteSpace(limitText) || !decimal.TryParse(limitText, out var limit) || limit <= 0)
            {
                XtraMessageBox.Show("Geçerli bir limit tutarı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(incomeText) || !decimal.TryParse(incomeText, out var income) || income <= 0)
            {
                XtraMessageBox.Show("Geçerli bir aylık gelir giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnApplyCard.Enabled = false;
            var resp = await _api.ApplyCreditCardAsync(limit, income);
            
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Kredi kartı başvurunuz alındı!\nOnaylandığında bilgilendirileceksiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCardLimit.Text = "";
                txtCardIncome.Text = "";
                BtnRefreshCards_Click(null, EventArgs.Empty);
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Başvuru yapılamadı: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (btnApplyCard != null) btnApplyCard.Enabled = true;
        }
    }

    private async void BtnRefreshCards_Click(object? sender, EventArgs e)
    {
        try
        {
            // Kartları yükle
            var cards = await _api.GetMyCardsAsync();
            if (gridCardsMain != null)
            {
                gridCardsMain.DataSource = cards;
            }

            // Başvuruları yükle
            var applications = await _api.GetMyCardApplicationsAsync();
            if (gridCardApplications != null)
            {
                gridCardApplications.DataSource = applications;
            }
            
            // Hesaplarım sekmesindeki özeti de güncelle
            await LoadAccounts();
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Kartlar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnPayCardDebt_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridCardsMain == null || gridCardsMainView == null) return;

            var focusedRow = gridCardsMainView.GetFocusedRow();
            if (focusedRow == null)
            {
                XtraMessageBox.Show("Lütfen borç ödemek istediğiniz kartı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cardId = (Guid)gridCardsMainView.GetFocusedRowCellValue("CardId");
            var amountText = txtCardPaymentAmount?.EditValue?.ToString();
            
            if (string.IsNullOrWhiteSpace(amountText) || !decimal.TryParse(amountText, out var amount) || amount <= 0)
            {
                XtraMessageBox.Show("Geçerli bir ödeme tutarı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnPayCardDebt.Enabled = false;
            var resp = await _api.PayCardDebtAsync(cardId, amount);
            
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"₺{amount:N2} tutarında ödeme başarıyla yapıldı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCardPaymentAmount.Text = "";
                BtnRefreshCards_Click(null, EventArgs.Empty);
                await LoadAccounts(); // Bakiyeyi güncelle
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Ödeme yapılamadı: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (btnPayCardDebt != null) btnPayCardDebt.Enabled = true;
        }
    }

    // ===================== FATURA ÖDEME MODÜLÜ =====================

    private Guid? _currentBillInstitutionId;
    private decimal _currentBillAmount;
    private string? _currentInvoiceNo;

    private async void LoadBillsUI()
    {
        if (tabBills == null) return;

        tabBills.Controls.Clear();

        // Başlık
        var lblTitle = new LabelControl()
        {
            Location = new Point(20, 20),
            Size = new Size(400, 35),
            Text = "📄 Fatura Ödeme",
            Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };

        // Fatura Sorgulama Paneli
        var pnlInquiry = new PanelControl()
        {
            Location = new Point(20, 70),
            Size = new Size(500, 280),
            Appearance = { BackColor = Color.FromArgb(232, 245, 253), BorderColor = Color.FromArgb(25, 118, 210) }
        };

        var lblInquiryTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "🔍 Fatura Sorgula",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };

        var lblInstitution = new LabelControl()
        {
            Location = new Point(20, 55),
            Text = "Kurum:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        cmbBillInstitution = new ComboBoxEdit()
        {
            Location = new Point(20, 80),
            Size = new Size(450, 35)
        };
        cmbBillInstitution.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

        var lblSubscriber = new LabelControl()
        {
            Location = new Point(20, 125),
            Text = "Abone No:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        txtSubscriberNo = new TextEdit()
        {
            Location = new Point(20, 150),
            Size = new Size(300, 35)
        };
        txtSubscriberNo.Properties.NullValuePrompt = "Abone numaranızı giriniz";

        btnInquireBill = new SimpleButton()
        {
            Location = new Point(330, 150),
            Size = new Size(140, 35),
            Text = "🔍 Sorgula",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        btnInquireBill.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        btnInquireBill.Click += BtnInquireBill_Click;

        // Sonuç
        lblBillAmount = new LabelControl()
        {
            Location = new Point(20, 200),
            Size = new Size(300, 30),
            Text = "Fatura Tutarı: -",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(244, 67, 54) }
        };

        lblBillDueDate = new LabelControl()
        {
            Location = new Point(20, 235),
            Size = new Size(300, 25),
            Text = "Son Ödeme: -",
            Appearance = { Font = new Font("Segoe UI", 10) }
        };

        pnlInquiry.Controls.AddRange(new Control[] { lblInquiryTitle, lblInstitution, cmbBillInstitution, lblSubscriber, txtSubscriberNo, btnInquireBill, lblBillAmount, lblBillDueDate });

        // Ödeme Paneli
        var pnlPayment = new PanelControl()
        {
            Location = new Point(540, 70),
            Size = new Size(400, 280),
            Appearance = { BackColor = Color.FromArgb(232, 255, 232), BorderColor = Color.FromArgb(76, 175, 80) }
        };

        var lblPayTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "💳 Fatura Öde",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80) }
        };

        var lblPayAccount = new LabelControl()
        {
            Location = new Point(20, 60),
            Text = "Ödeme Kaynağı:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        var rgPaySource = new RadioGroup()
        {
            Location = new Point(20, 85),
            Size = new Size(350, 45),
            Properties = {
                Items = {
                    new RadioGroupItem(0, "💵 Banka Hesabı"),
                    new RadioGroupItem(1, "💳 Kredi Kartı")
                }
            }
        };
        rgPaySource.SelectedIndex = 0;
        rgPaySource.SelectedIndexChanged += async (s, e) => {
            if (rgPaySource.SelectedIndex == 0) await LoadBillAccountsAsync();
            else await LoadBillCardsAsync();
        };

        var lblSelect = new LabelControl()
        {
            Location = new Point(20, 140),
            Text = "Seçiniz:",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold) }
        };

        cmbBillAccount = new ComboBoxEdit()
        {
            Location = new Point(20, 160),
            Size = new Size(350, 35)
        };
        cmbBillAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

        btnPayBill = new SimpleButton()
        {
            Location = new Point(20, 210),
            Size = new Size(350, 45),
            Text = "💰 Faturayı Öde",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White }
        };
        btnPayBill.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnPayBill.Click += BtnPayBill_Click;
        btnPayBill.Tag = rgPaySource; // Referans olarak sakla

        pnlPayment.Controls.AddRange(new Control[] { lblPayTitle, lblPayAccount, rgPaySource, lblSelect, cmbBillAccount, btnPayBill });

        // Ödeme Geçmişi
        var pnlHistory = new PanelControl()
        {
            Location = new Point(20, 370),
            Size = new Size(920, 280),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };

        var lblHistoryTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Text = "📋 Fatura Ödeme Geçmişi",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(156, 39, 176) }
        };

        gridBillHistory = new GridControl()
        {
            Location = new Point(20, 50),
            Size = new Size(880, 210)
        };
        gridBillHistoryView = new GridView();
        gridBillHistory.MainView = gridBillHistoryView;
        gridBillHistoryView.OptionsBehavior.Editable = false;
        gridBillHistoryView.OptionsView.ShowGroupPanel = false;
        gridBillHistoryView.Appearance.HeaderPanel.BackColor = Color.FromArgb(156, 39, 176);
        gridBillHistoryView.Appearance.HeaderPanel.ForeColor = Color.White;

        pnlHistory.Controls.AddRange(new Control[] { lblHistoryTitle, gridBillHistory });

        // Kontrolleri ekle
        tabBills.Controls.AddRange(new Control[] { lblTitle, pnlInquiry, pnlPayment, pnlHistory });

        // Kurumları yükle
        await LoadBillInstitutionsAsync();
        await LoadBillAccountsAsync();
        await LoadBillHistoryAsync();
    }

    private async Task LoadBillHistoryAsync()
    {
        try
        {
            var history = await _api.GetMyBillHistoryAsync();
            gridBillHistory.DataSource = history;

            if (gridBillHistoryView.Columns["Id"] != null) gridBillHistoryView.Columns["Id"].Visible = false;
            if (gridBillHistoryView.Columns["AccountId"] != null) gridBillHistoryView.Columns["AccountId"].Visible = false;
            if (gridBillHistoryView.Columns["CardId"] != null) gridBillHistoryView.Columns["CardId"].Visible = false;
            if (gridBillHistoryView.Columns["InstitutionId"] != null) gridBillHistoryView.Columns["InstitutionId"].Visible = false;
            
            if (gridBillHistoryView.Columns["InstitutionName"] != null) gridBillHistoryView.Columns["InstitutionName"].Caption = "Kurum";
            if (gridBillHistoryView.Columns["SubscriberNo"] != null) gridBillHistoryView.Columns["SubscriberNo"].Caption = "Abone No";
            if (gridBillHistoryView.Columns["Amount"] != null) {
                gridBillHistoryView.Columns["Amount"].Caption = "Tutar";
                gridBillHistoryView.Columns["Amount"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridBillHistoryView.Columns["Amount"].DisplayFormat.FormatString = "N2";
            }
            if (gridBillHistoryView.Columns["Commission"] != null) {
                gridBillHistoryView.Columns["Commission"].Caption = "Komisyon";
                gridBillHistoryView.Columns["Commission"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridBillHistoryView.Columns["Commission"].DisplayFormat.FormatString = "N2";
            }
            if (gridBillHistoryView.Columns["TotalAmount"] != null) {
                gridBillHistoryView.Columns["TotalAmount"].Caption = "Toplam";
                gridBillHistoryView.Columns["TotalAmount"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridBillHistoryView.Columns["TotalAmount"].DisplayFormat.FormatString = "N2";
            }
            if (gridBillHistoryView.Columns["PaidAt"] != null) {
                gridBillHistoryView.Columns["PaidAt"].Caption = "Ödeme Tarihi";
                gridBillHistoryView.Columns["PaidAt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gridBillHistoryView.Columns["PaidAt"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            }
            if (gridBillHistoryView.Columns["ReferenceCode"] != null) gridBillHistoryView.Columns["ReferenceCode"].Caption = "Ref No";
            if (gridBillHistoryView.Columns["Status"] != null) gridBillHistoryView.Columns["Status"].Caption = "Durum";
        }
        catch { }
    }

    private async Task LoadBillInstitutionsAsync()
    {
        try
        {
            var institutions = await _api.GetBillInstitutionsAsync();
            cmbBillInstitution.Properties.Items.Clear();
            foreach (var inst in institutions)
            {
                cmbBillInstitution.Properties.Items.Add($"{inst.Name} ({inst.Category})");
            }
            // Store for later use
            cmbBillInstitution.Tag = institutions;
        }
        catch { }
    }

    private async Task LoadBillAccountsAsync()
    {
        try
        {
            if (!Session.CurrentCustomerId.HasValue) return;
            var accounts = await _api.GetAccountsByCustomerIdAsync(Session.CurrentCustomerId.Value);
            if (accounts == null || cmbBillAccount == null) return;
            
            cmbBillAccount.Properties.Items.Clear();
            foreach (var acc in accounts)
            {
                cmbBillAccount.Properties.Items.Add($"{acc.Iban} - ₺{acc.Balance:N2}");
            }
            cmbBillAccount.Tag = accounts;
            if (accounts.Count > 0) cmbBillAccount.SelectedIndex = 0;
        }
        catch { }
    }

    private async Task LoadBillCardsAsync()
    {
        try
        {
            var cards = await _api.GetMyCardsAsync();
            if (cards == null || cmbBillAccount == null) return;

            cmbBillAccount.Properties.Items.Clear();
            var creditCards = cards.Where(c => c.CreditLimit > 0).ToList(); // Sadece kredi kartları (limitli olanlar)
            foreach (var card in creditCards)
            {
                cmbBillAccount.Properties.Items.Add($"{card.MaskedPan} - Limit: ₺{card.AvailableLimit:N2}");
            }
            cmbBillAccount.Tag = creditCards;
            if (creditCards.Count > 0) cmbBillAccount.SelectedIndex = 0;
        }
        catch { }
    }

    private async void BtnInquireBill_Click(object? sender, EventArgs e)
    {
        try
        {
            if (cmbBillInstitution?.SelectedIndex < 0)
            {
                XtraMessageBox.Show("Lütfen bir kurum seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var subscriber = txtSubscriberNo?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(subscriber))
            {
                XtraMessageBox.Show("Lütfen abone numarasını giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var institutions = cmbBillInstitution.Tag as List<NovaBank.Contracts.Bills.BillInstitutionResponse>;
            if (institutions == null) return;

            var selectedInst = institutions[cmbBillInstitution.SelectedIndex];
            _currentBillInstitutionId = selectedInst.Id;

            btnInquireBill.Enabled = false;
            var result = await _api.InquireBillAsync(new NovaBank.Contracts.Bills.BillInquiryRequest(selectedInst.Id, subscriber));
            
            if (result != null && result.Amount > 0)
            {
                _currentBillAmount = result.Amount;
                _currentInvoiceNo = result.InvoiceNo;
                lblBillAmount.Text = $"Fatura Tutarı: ₺{result.Amount:N2}";
                lblBillDueDate.Text = $"Son Ödeme: {result.DueDate:dd.MM.yyyy}";
            }
            else
            {
                lblBillAmount.Text = "Fatura Tutarı: Borç bulunamadı";
                lblBillDueDate.Text = "Son Ödeme: -";
                _currentBillAmount = 0;
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Sorgulama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (btnInquireBill != null) btnInquireBill.Enabled = true;
        }
    }

    private async void BtnPayBill_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentBillAmount <= 0)
            {
                XtraMessageBox.Show("Önce fatura sorgulayınız.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbBillAccount?.SelectedIndex < 0)
            {
                XtraMessageBox.Show("Lütfen ödeme kaynağını seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rgSource = btnPayBill.Tag as RadioGroup;
            bool isCard = rgSource != null && rgSource.SelectedIndex == 1;

            Guid? accountId = null;
            Guid? cardId = null;

            if (isCard)
            {
                var cards = cmbBillAccount.Tag as List<NovaBank.WinForms.Services.CreditCardSummaryDto>;
                if (cards == null) return;
                cardId = cards[cmbBillAccount.SelectedIndex].CardId;
            }
            else
            {
                var accounts = cmbBillAccount.Tag as List<NovaBank.Contracts.Accounts.AccountResponse>;
                if (accounts == null) return;
                accountId = accounts[cmbBillAccount.SelectedIndex].Id;
            }

            var confirm = XtraMessageBox.Show(
                $"₺{_currentBillAmount:N2} tutarındaki fatura {(isCard ? "kredi kartı" : "hesap")} ile ödenecek.\n\nOnaylıyor musunuz?",
                "Fatura Ödeme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            btnPayBill.Enabled = false;
            var subscriberNo = txtSubscriberNo?.Text?.Trim() ?? "";
            var resp = await _api.PayBillAsync(new NovaBank.Contracts.Bills.PayBillRequest(
                accountId,
                cardId,
                _currentBillInstitutionId!.Value,
                subscriberNo,
                _currentBillAmount,
                _currentInvoiceNo
            ));

            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Fatura başarıyla ödendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblBillAmount.Text = "Fatura Tutarı: -";
                lblBillDueDate.Text = "Son Ödeme: -";
                _currentBillAmount = 0;
                await LoadAccounts();
                if (isCard) await LoadBillCardsAsync();
                else await LoadBillAccountsAsync();
                
                // Diğer sekmeleri ve geçmişi de güncelle
                BtnRefreshCards_Click(null, EventArgs.Empty);
                await LoadBillHistoryAsync();
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Ödeme yapılamadı: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (btnPayBill != null) btnPayBill.Enabled = true;
        }
    }
}



