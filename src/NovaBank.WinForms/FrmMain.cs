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
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;

namespace NovaBank.WinForms;

public partial class FrmMain : XtraForm

{

    private readonly ApiClient _api = new();
    private readonly Services.BankaciAiService _chatbotService = new();

    private List<AccountResponse> _cachedAccounts = new();

    private AccountResponse? _selectedAccount;

    private bool _isLogoutFlow = false;

    public FrmMain(Guid? currentCustomerId = null) 

    { 

        if (currentCustomerId.HasValue)

            Session.CurrentCustomerId = currentCustomerId;

        InitializeComponent(); 

        var customerInfo = Session.CurrentCustomerId.HasValue ? $" • Müşteri: {Session.CurrentCustomerId}" : "";

        var roleInfo = Session.IsAdmin ? " • Admin" : (Session.IsBranchManager ? " • Şube Yöneticisi" : (Session.CurrentRole == UserRole.Customer ? " • Müşteri" : ""));

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

        if (Session.IsAdminOrBranchManager)

        {

            // Admin veya BranchManager ise tab görünür olsun
            var tabText = Session.IsAdmin ? "Yönetim (Admin)" : "Yönetim (Şube)";

            if (!tabs.TabPages.Contains(tabAdmin))

            {

                tabs.TabPages.Add(tabAdmin);

            }

            tabAdmin.Text = tabText;

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
        
        // Sol sidebar'ı oluştur
        CreateSidebar();

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

            // Admin veya BranchManager ise admin UI'ı yükle

            if (Session.IsAdminOrBranchManager)

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

                lblStatus.Text = $"🔐 Giriş yapıldı: {customer.FirstName} {customer.LastName} | {DateTime.Now:dd.MM.yyyy HH:mm}";

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

                // Toplam bakiyeleri para birimine göre hesapla

                var totalTry = list.Where(a => a.Currency == "TRY").Sum(a => a.Balance);

                var totalUsd = list.Where(a => a.Currency == "USD").Sum(a => a.Balance);

                var totalEur = list.Where(a => a.Currency == "EUR").Sum(a => a.Balance);

                if (lblTotalTry != null) lblTotalTry.Text = $"₺ TRY: {totalTry:N2}";

                if (lblTotalUsd != null) lblTotalUsd.Text = $"$ USD: {totalUsd:N2}";

                if (lblTotalEur != null) lblTotalEur.Text = $"€ EUR: {totalEur:N2}";

                if (lblAccountCount != null) lblAccountCount.Text = $"📊 {list.Count} Hesap";

                if (lblTotalBalance != null) lblTotalBalance.Text = $"💰 Toplam: {totalTry:N2} TL";

                // Hesap kartlarını oluştur

                await RenderAccountCardsAsync(list);

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

                // Kendi hesaplarım arası transfer için alıcı listesini güncelle

                RefreshOwnRecipientAccounts();

            }

        }

        catch (Exception ex)

        {

            System.Diagnostics.Debug.WriteLine($"Hesap seçim hatası: {ex.Message}");

        }

    }

    /// <summary>

    /// Kendi hesaplarım arası transfer için alıcı hesap dropdown'ını günceller.

    /// Seçili gönderen hesabını hariç tutar ve sadece aynı para birimindeki hesapları gösterir.

    /// </summary>

    private void RefreshOwnRecipientAccounts()

    {

        if (cmbOwnRecipientAccount == null || _selectedAccount == null) return;

        cmbOwnRecipientAccount.Properties.Items.Clear();

        // Sadece aynı para birimi ve farklı hesapları listele

        var eligibleAccounts = _cachedAccounts

            .Where(a => a.Id != _selectedAccount.Id && a.Currency == _selectedAccount.Currency)

            .ToList();

        foreach (var acc in eligibleAccounts)

        {

            var displayText = $"{acc.Iban} - {acc.Currency} ({acc.Balance:N2})";

            cmbOwnRecipientAccount.Properties.Items.Add(displayText);

        }

        // Bilgilendirme mesajı

        if (eligibleAccounts.Count == 0)

        {

            var otherCurrencyAccounts = _cachedAccounts.Where(a => a.Id != _selectedAccount.Id && a.Currency != _selectedAccount.Currency).ToList();

            if (otherCurrencyAccounts.Count > 0)

            {

                lblOwnRecipientInfo.Text = $"⚠️ Aynı para biriminde başka hesabınız yok. Döviz Al/Sat modülünü kullanın.";

                lblOwnRecipientInfo.Appearance.ForeColor = Color.FromArgb(255, 152, 0);

            }

            else

            {

                lblOwnRecipientInfo.Text = "ℹ️ Transfer yapabileceğiniz başka hesabınız yok.";

                lblOwnRecipientInfo.Appearance.ForeColor = Color.FromArgb(100, 100, 100);

            }

        }

        else

        {

            lblOwnRecipientInfo.Text = "📥 Alıcı hesabınızı seçin";

            lblOwnRecipientInfo.Appearance.ForeColor = Color.FromArgb(100, 100, 100);

        }

        // Komisyon bilgisini güncelle (kendi hesaplar arası = ücretsiz)

        if (lblCommissionInfo != null)

        {

            lblCommissionInfo.Text = "💰 Komisyon: 0,00 TL (Kendi hesaplar arası ücretsiz)";

            lblCommissionInfo.Appearance.ForeColor = Color.FromArgb(76, 175, 80);

        }

    }

    /// <summary>

    /// Transfer tipi radio button değiştiğinde UI'ı günceller.

    /// </summary>

    private void RdoTransferType_CheckedChanged(object? sender, EventArgs e)

    {

        if (rdoOwnAccounts == null || rdoExternalIban == null) return;

        bool isOwnAccounts = rdoOwnAccounts.Checked;

        // Kendi hesaplarım arası kontrolleri

        if (lblOwnRecipientAccount != null) lblOwnRecipientAccount.Visible = isOwnAccounts;

        if (cmbOwnRecipientAccount != null) cmbOwnRecipientAccount.Visible = isOwnAccounts;

        if (lblOwnRecipientInfo != null) lblOwnRecipientInfo.Visible = isOwnAccounts;

        if (btnOwnAccountTransfer != null) btnOwnAccountTransfer.Visible = isOwnAccounts;

        // IBAN'a transfer kontrolleri

        if (lblIban != null) lblIban.Visible = !isOwnAccounts;

        if (txtToIban != null) txtToIban.Visible = !isOwnAccounts;

        if (lblRecipientName != null) lblRecipientName.Visible = !isOwnAccounts;

        if (btnExternalTransfer != null) btnExternalTransfer.Visible = !isOwnAccounts;

        // Kendi hesaplarım seçildiyse alıcı listesini güncelle

        if (isOwnAccounts)

        {

            RefreshOwnRecipientAccounts();

        }

    }

    /// <summary>

    /// Kendi hesaplarım arası alıcı hesap seçimi değiştiğinde.

    /// </summary>

    private void CmbOwnRecipientAccount_EditValueChanged(object? sender, EventArgs e)

    {

        if (cmbOwnRecipientAccount == null || cmbOwnRecipientAccount.SelectedIndex < 0) return;

        try

        {

            // Seçili alıcı hesabın bilgilerini göster

            var eligibleAccounts = _cachedAccounts

                .Where(a => a.Id != _selectedAccount?.Id && a.Currency == _selectedAccount?.Currency)

                .ToList();

            if (cmbOwnRecipientAccount.SelectedIndex < eligibleAccounts.Count)

            {

                var recipientAccount = eligibleAccounts[cmbOwnRecipientAccount.SelectedIndex];

                var available = recipientAccount.Balance + recipientAccount.OverdraftLimit;

                lblOwnRecipientInfo.Text = $"💳 {recipientAccount.Iban} | Bakiye: {recipientAccount.Balance:N2} {recipientAccount.Currency}";

                lblOwnRecipientInfo.Appearance.ForeColor = Color.FromArgb(76, 175, 80);

            }

        }

        catch (Exception ex)

        {

            System.Diagnostics.Debug.WriteLine($"Alıcı hesap seçim hatası: {ex.Message}");

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

                XtraMessageBox.Show($"✅ Para yatırma işlemi başarılı!\nTutar: {amt:N2} {account.Currency}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

                XtraMessageBox.Show($"✅ Para çekme işlemi başarılı!\nTutar: {amt:N2} {account.Currency}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

    /// <summary>

    /// Kendi Hesabıma Transfer butonu click event handler.

    /// Aynı müşterinin hesapları arasında, aynı para biriminde transfer yapar.

    /// </summary>

    private async void BtnOwnAccountTransfer_Click(object? sender, EventArgs e)

    {

        try

        {

            // Gönderen hesap kontrolü

            if (_selectedAccount == null)

            {

                XtraMessageBox.Show("Lütfen gönderen hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            // Alıcı hesap kontrolü

            if (cmbOwnRecipientAccount == null || cmbOwnRecipientAccount.SelectedIndex < 0)

            {

                XtraMessageBox.Show("Lütfen alıcı hesabınızı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            // Tutar kontrolü

            if (!TryDec(txtAmount?.Text, out var amt, "Tutar")) return;

            if (amt <= 0)

            {

                XtraMessageBox.Show("Tutar pozitif olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            // Alıcı hesabı bul

            var eligibleAccounts = _cachedAccounts

                .Where(a => a.Id != _selectedAccount.Id && a.Currency == _selectedAccount.Currency)

                .ToList();

            if (cmbOwnRecipientAccount.SelectedIndex >= eligibleAccounts.Count)

            {

                XtraMessageBox.Show("Geçersiz alıcı hesap seçimi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;

            }

            var toAccount = eligibleAccounts[cmbOwnRecipientAccount.SelectedIndex];

            // Aynı hesap kontrolü

            if (_selectedAccount.Id == toAccount.Id)

            {

                XtraMessageBox.Show("Aynı hesaba transfer yapılamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            // Para birimi kontrolü - farklı ise döviz modülüne yönlendir

            if (_selectedAccount.Currency != toAccount.Currency)

            {

                var result = XtraMessageBox.Show(

                    $"Farklı para birimleri arasında transfer için döviz al/sat modülünü kullanmanız gerekmektedir.\n\n" +

                    $"Gönderen: {_selectedAccount.Currency}\n" +

                    $"Alıcı: {toAccount.Currency}\n\n" +

                    $"Döviz Kurları sekmesine gitmek istiyor musunuz?",

                    "Para Birimi Uyuşmazlığı",

                    MessageBoxButtons.YesNo,

                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes && tabs != null && tabExchangeRates != null)

                {

                    tabs.SelectedTabPage = tabExchangeRates;

                }

                return;

            }

            // Bakiye kontrolü

            var availableBalance = _selectedAccount.Balance + _selectedAccount.OverdraftLimit;

            if (amt > availableBalance)

            {

                XtraMessageBox.Show(

                    $"Yetersiz bakiye!\n\n" +

                    $"İstenen tutar: {amt:N2} {_selectedAccount.Currency}\n" +

                    $"Mevcut bakiye: {_selectedAccount.Balance:N2} {_selectedAccount.Currency}\n" +

                    $"Ek hesap limiti: {_selectedAccount.OverdraftLimit:N2} {_selectedAccount.Currency}\n" +

                    $"Kullanılabilir: {availableBalance:N2} {_selectedAccount.Currency}",

                    "Yetersiz Bakiye",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning);

                return;

            }

            // Onay mesajı

            var confirmMsg = $"Kendi hesaplarınız arasında transfer yapılacak:\n\n" +

                           $"Gönderen: {_selectedAccount.Iban}\n" +

                           $"Alıcı: {toAccount.Iban}\n" +

                           $"Tutar: {amt:N2} {_selectedAccount.Currency}\n" +

                           $"Komisyon: 0,00 TL (Kendi hesaplar arası ücretsiz)\n\n" +

                           $"Onaylıyor musunuz?";

            var confirm = XtraMessageBox.Show(confirmMsg, "Transfer Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Butonları disable et

            if (btnOwnAccountTransfer != null) btnOwnAccountTransfer.Enabled = false;

            this.UseWaitCursor = true;

            // API çağrısı - Internal Transfer kullanıyoruz (aynı banka içi)

            var resp = await _api.TransferInternalAsync(

                _selectedAccount.Id,

                toAccount.Id,

                amt,

                _selectedAccount.Currency,

                txtTransDesc?.Text ?? "Kendi hesaplarım arası transfer"

            );

            if (resp.IsSuccessStatusCode)

            {

                XtraMessageBox.Show(

                    $"✓ Transfer başarıyla tamamlandı!\n\n" +

                    $"Gönderen: {_selectedAccount.Iban}\n" +

                    $"Alıcı: {toAccount.Iban}\n" +

                    $"Tutar: {amt:N2} {_selectedAccount.Currency}",

                    "Başarılı",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);

                // Form alanlarını temizle

                if (txtAmount != null) txtAmount.Text = "";

                if (txtTransDesc != null) txtTransDesc.Text = "";

                if (cmbOwnRecipientAccount != null) cmbOwnRecipientAccount.SelectedIndex = -1;

                // Hesapları yenile

                await LoadAccounts();

                RefreshAccountDropdowns();

                RefreshOwnRecipientAccounts();

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

            XtraMessageBox.Show($"Transfer sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        finally

        {

            if (btnOwnAccountTransfer != null) btnOwnAccountTransfer.Enabled = true;

            this.UseWaitCursor = false;

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
    
    private void FrmMain_Resize(object sender, EventArgs e)
    {
        // Form resize olduğunda sidebar ve içerik pozisyonunu güncelle
        if (pnlSidebar != null && tabs != null)
        {
            // Sabit sidebar genişliği kullan
            UpdateMainContentPosition(SIDEBAR_WIDTH);
            
            // Sidebar yüksekliğini güncelle
            if (statusStrip != null)
            {
                pnlSidebar.Height = this.Height - statusStrip.Height;
            }
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

        // Aktif sidebar butonunu güncelle
        UpdateActiveSidebarButtonForTab(e.Page);

        if (e.Page == tabMyAccounts)

        {

            await LoadAccounts();

        }

        else if (e.Page == tabExchangeRates)

        {

            LoadExchangeRatesWithFxAsync();

        }

    }
    
    private void UpdateActiveSidebarButtonForTab(XtraTabPage? activeTab)
    {
        if (activeTab == null) return;
        
        SimpleButton? activeBtn = null;
        
        if (activeTab == tabMyAccounts) activeBtn = btnSidebarAccounts;
        else if (activeTab == tabDw) activeBtn = btnSidebarMoneyOps;
        else if (activeTab == tabTransfer) activeBtn = btnSidebarTransfer;
        else if (activeTab == tabCards) activeBtn = btnSidebarCards;
        else if (activeTab == tabBills) activeBtn = btnSidebarBills;
        else if (activeTab == tabReports) activeBtn = btnSidebarStatements;
        else if (activeTab == tabExchangeRates) activeBtn = btnSidebarFx;
        else if (activeTab == tabSettings) activeBtn = btnSidebarSettings;
        else if (activeTab == tabAdmin) activeBtn = btnSidebarAdmin;
        
        if (activeBtn != null)
            UpdateActiveSidebarButton(activeBtn);
    }

    private void BtnRefreshRates_Click(object sender, EventArgs e)

    {

        LoadExchangeRatesWithFxAsync();

    }

    private async void LoadExchangeRatesWithFxAsync()

    {

        // Önce hesapları yükle (her zaman yükle, güncel olsun)
        await LoadAccounts();

        await LoadExchangeRatesAsync_Internal();

        await LoadFxAccountDropdowns();

        await LoadFxPositionsAsync();

    }

    private async Task LoadExchangeRatesAsync_Internal()

    {

        try

        {

            this.UseWaitCursor = true;

            if (btnRefreshRates != null) btnRefreshRates.Enabled = false;

            if (lblExchangeInfo != null) lblExchangeInfo.Text = "Kurlar yükleniyor...";

            var service = new TcmbExchangeRateService();

            var (date, rates) = await service.GetTodayAsync();

            if (rates == null || rates.Count == 0)

            {

                XtraMessageBox.Show("Kur bilgisi alınamadı. Lütfen internet bağlantınızı kontrol edin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (lblExchangeInfo != null) lblExchangeInfo.Text = "Kur bilgisi alınamadı.";

                return;

            }

            // Cache rates for FX operations - Tüm dövizleri cache'le (TRY hariç)

            _cachedRates.Clear();

            var ratesForApi = new List<ExchangeRateItemDto>();

            foreach (var rate in rates)

            {

                // TRY hariç tüm dövizleri cache'le
                if (!string.IsNullOrEmpty(rate.CurrencyCode) && rate.CurrencyCode != "TRY" && rate.ForexBuying.HasValue && rate.ForexSelling.HasValue)

                {

                    _cachedRates[rate.CurrencyCode] = new CurrencyRateDto(rate.ForexBuying.Value, rate.ForexSelling.Value, date);

                    // API'ye göndermek için listeye ekle
                    ratesForApi.Add(new ExchangeRateItemDto(rate.CurrencyCode, rate.ForexBuying.Value, rate.ForexSelling.Value));

                }

            }

            // Kurları sunucuya kaydet
            try
            {
                var saveResp = await _api.SaveExchangeRatesAsync(date, ratesForApi);
                if (!saveResp.IsSuccessStatusCode)
                {
                    // Hata olsa bile devam et, sadece uyarı göster
                    System.Diagnostics.Debug.WriteLine($"Kurlar sunucuya kaydedilemedi: {saveResp.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Kurlar sunucuya kaydedilemedi: {ex.Message}");
            }

            // Döviz dropdown'larını güncelle
            UpdateCurrencyDropdowns(rates);

            // Tarih bilgisini göster

            var timeNote = DateTime.Now.Hour >= 15 && DateTime.Now.Minute >= 30 

                ? "✓ Güncel" 

                : "⚠ 15:30 sonrası güncellenir";

            if (lblExchangeInfo != null) lblExchangeInfo.Text = $"Tarih: {date:dd.MM.yyyy} | {timeNote} | Toplam {rates.Count} döviz";

            // DataGridView'e bağla

            if (dgvRates != null)

            {

                dgvRates.DataSource = rates;

                // AutoSizeColumnsMode'u None yap (manuel genişlik kontrolü için)

                dgvRates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                // Kolon başlıklarını düzenle ve formatla

                if (dgvRates.Columns["CurrencyCode"] != null)

                {

                    dgvRates.Columns["CurrencyCode"].HeaderText = "Kod";

                    dgvRates.Columns["CurrencyCode"].Width = 60;

                }

                if (dgvRates.Columns["CurrencyName"] != null)

                {

                    dgvRates.Columns["CurrencyName"].HeaderText = "Döviz";

                    dgvRates.Columns["CurrencyName"].Width = 120;

                }

                if (dgvRates.Columns["Unit"] != null)

                {

                    dgvRates.Columns["Unit"].HeaderText = "Birim";

                    dgvRates.Columns["Unit"].Width = 50;

                }

                if (dgvRates.Columns["ForexBuying"] != null)

                {

                    dgvRates.Columns["ForexBuying"].HeaderText = "Alış";

                    dgvRates.Columns["ForexBuying"].Width = 70;

                    dgvRates.Columns["ForexBuying"].DefaultCellStyle.Format = "N4";

                }

                if (dgvRates.Columns["ForexSelling"] != null)

                {

                    dgvRates.Columns["ForexSelling"].HeaderText = "Satış";

                    dgvRates.Columns["ForexSelling"].Width = 70;

                    dgvRates.Columns["ForexSelling"].DefaultCellStyle.Format = "N4";

                }

                if (dgvRates.Columns["BanknoteBuying"] != null)

                {

                    dgvRates.Columns["BanknoteBuying"].Visible = false;

                }

                if (dgvRates.Columns["BanknoteSelling"] != null)

                {

                    dgvRates.Columns["BanknoteSelling"].Visible = false;

                }

                // Başlık yazılarının tam görünmesi için yüksekliği ayarla

                dgvRates.ColumnHeadersHeight = 35;

            }

            // Update FX rate labels

            UpdateFxRateLabels();

        }

        catch (Exception ex)

        {

            XtraMessageBox.Show($"Kur çekilemedi:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (lblExchangeInfo != null) lblExchangeInfo.Text = "Hata oluştu.";

        }

        finally

        {

            this.UseWaitCursor = false;

            if (btnRefreshRates != null) btnRefreshRates.Enabled = true;

        }

    }

    private async void LoadExchangeRatesAsync()

    {

        await LoadExchangeRatesAsync_Internal();

    }

    private async Task LoadAdminUI()

    {

        if (tabAdmin == null) return;

        try

        {

            // Önceki kontrolleri temizle

            tabAdmin.Controls.Clear();

            // ===== BAÅLIK =====

            var lblAdminTitle = new LabelControl()

            {

                Location = new Point(20, 10),

                Size = new Size(500, 35),

                Text = Session.IsAdmin ? "🔧 Admin Yönetim Paneli" : "👔 Şube Yönetim Paneli",

                Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Session.IsAdmin ? Color.FromArgb(25, 118, 210) : Color.FromArgb(63, 81, 181) }

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

            tabAdminAudit = new XtraTabPage() { Text = "📋 Denetim Kayıtları" };

            tabAdminBills = new XtraTabPage() { Text = "📄 Fatura Kurumları" };

            tabAdminBranchManager = new XtraTabPage() { Text = "👔 Şube Yönetici Yönetimi" };

            // Şube Yönetici tab'ını sadece Admin görebilir (BranchManager göremez)
            if (Session.IsAdmin)
            {
                tabAdminSub.TabPages.AddRange(new XtraTabPage[] { tabAdminUsers, tabAdminCards, tabAdminBills, tabAdminBranchManager, tabAdminAudit });
            }
            else
            {
                tabAdminSub.TabPages.AddRange(new XtraTabPage[] { tabAdminUsers, tabAdminCards, tabAdminBills, tabAdminAudit });
            }

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

            // Tip sütunu için görselleştirme event'i (bir kez ekle)
            gridPendingApprovalsView.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column?.FieldName == "ItemType")
                {
                    if (e.Value?.ToString() == "Customer")
                        e.DisplayText = "👤 Müşteri";
                    else if (e.Value?.ToString() == "Account")
                        e.DisplayText = "💳 Hesap";
                }
            };

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

                Text = "ğŸ” Müşteri Arama",

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

                Text = "ğŸ” Ara",

                Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }

            };

            btnAdminSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);

            btnAdminSearch.Click += BtnAdminSearch_Click;

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtAdminSearch, btnAdminSearch });

            // ===== MÜŞTERİ LİSTESİ =====

            gridAdminCustomers = new GridControl()

            {

                Location = new Point(10, 430),

                Size = new Size(580, 140)

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

                Size = new Size(600, 140)

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

            // ===== SİLME BUTONLARI (SADECE ADMİN İÇİN) =====
            if (Session.IsAdmin)
            {
                btnDeleteCustomer = new SimpleButton()
                {
                    Location = new Point(20, 580),
                    Size = new Size(180, 40),
                    Text = "🗑️ Müşteri Sil",
                    Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
                };
                btnDeleteCustomer.Appearance.BackColor = Color.FromArgb(211, 47, 47);
                btnDeleteCustomer.AppearanceHovered.BackColor = Color.FromArgb(183, 28, 28);
                btnDeleteCustomer.Appearance.Options.UseBackColor = true;
                btnDeleteCustomer.Appearance.Options.UseForeColor = true;
                btnDeleteCustomer.Appearance.Options.UseFont = true;
                btnDeleteCustomer.LookAndFeel.UseDefaultLookAndFeel = false;
                btnDeleteCustomer.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
                btnDeleteCustomer.Click += BtnDeleteCustomer_Click;

                btnDeleteAccount = new SimpleButton()
                {
                    Location = new Point(610, 580),
                    Size = new Size(180, 40),
                    Text = "🗑️ Hesap Sil",
                    Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
                };
                btnDeleteAccount.Appearance.BackColor = Color.FromArgb(198, 40, 40);
                btnDeleteAccount.AppearanceHovered.BackColor = Color.FromArgb(183, 28, 28);
                btnDeleteAccount.Appearance.Options.UseBackColor = true;
                btnDeleteAccount.Appearance.Options.UseForeColor = true;
                btnDeleteAccount.Appearance.Options.UseFont = true;
                btnDeleteAccount.LookAndFeel.UseDefaultLookAndFeel = false;
                btnDeleteAccount.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
                btnDeleteAccount.Click += BtnDeleteAccount_Click;

                tabAdminUsers.Controls.AddRange(new Control[] { pnlPendingApprovals, pnlSearch, gridAdminCustomers, gridAdminAccounts, btnDeleteCustomer, btnDeleteAccount });
            }
            else
            {
                tabAdminUsers.Controls.AddRange(new Control[] { pnlPendingApprovals, pnlSearch, gridAdminCustomers, gridAdminAccounts });
            }

            // ==========================================

            // TAB 2: KREDİ KARTI YÖNETİMİ

            // ==========================================

            LoadAdminCreditCardsUI();

            LoadAdminAuditUI();

            LoadAdminBillsUI();

            // Şube Yönetici Yönetimi UI'ı sadece Admin için yükle
            if (Session.IsAdmin)
            {
                LoadAdminBranchManagerUI();
            }

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

            Text = "💳 Kredi Kartı Başvuruları ve Yönetimi",

            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(156, 39, 176) }

        };

        btnRefreshCardApps = new SimpleButton()

        {

            Location = new Point(20, 60),

            Size = new Size(140, 40),

            Text = "ğŸ”„ Yenile",

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

            Text = "📋 Sistem Denetim Kayıtları (Audit Logs)",

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

        var lblAddTitle = new LabelControl() { Location = new Point(20, 15), Text = "➕ Yeni Kurum Ekle", Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold) } };

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

    private void LoadAdminBranchManagerUI()
    {
        if (tabAdminBranchManager == null) return;

        var pnlMain = new PanelControl()
        {
            Dock = DockStyle.Fill,
            Appearance = { BackColor = Color.White }
        };

        var lblTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Size = new Size(600, 35),
            Text = "👔 Şube Yönetici (Branch Manager) Yönetimi",
            Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(63, 81, 181) }
        };

        // ===== YENİ ŞUBE YÖNETİCİ OLUŞTURMA PANELİ =====
        var pnlCreate = new PanelControl()
        {
            Location = new Point(20, 60),
            Size = new Size(500, 420),
            Appearance = { BackColor = Color.FromArgb(232, 234, 246), BorderColor = Color.FromArgb(63, 81, 181) }
        };

        var lblCreateTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Size = new Size(400, 30),
            Text = "➕ Yeni Şube Yöneticisi Oluştur",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(63, 81, 181) }
        };

        var lblInfo = new LabelControl()
        {
            Location = new Point(20, 50),
            Size = new Size(460, 40),
            Text = "⚠️ Şube Yöneticileri kayıt ekranından kaydolamaz.\nSadece Admin tarafından oluşturulabilir.",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(100, 100, 100) }
        };

        var lblNationalId = new LabelControl()
        {
            Location = new Point(20, 100),
            Size = new Size(150, 22),
            Text = "🆔 TC Kimlik No:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };

        txtBmNationalId = new TextEdit()
        {
            Location = new Point(20, 125),
            Size = new Size(220, 35)
        };
        txtBmNationalId.Properties.MaxLength = 11;
        txtBmNationalId.Properties.NullValuePrompt = "11 haneli TC";
        txtBmNationalId.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtBmNationalId.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };

        var lblFirstName = new LabelControl()
        {
            Location = new Point(260, 100),
            Size = new Size(80, 22),
            Text = "👤 Ad:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };

        txtBmFirstName = new TextEdit()
        {
            Location = new Point(260, 125),
            Size = new Size(200, 35)
        };
        txtBmFirstName.Properties.NullValuePrompt = "Ad";
        txtBmFirstName.Properties.Appearance.Font = new Font("Segoe UI", 10);

        var lblLastName = new LabelControl()
        {
            Location = new Point(20, 170),
            Size = new Size(100, 22),
            Text = "👤 Soyad:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };

        txtBmLastName = new TextEdit()
        {
            Location = new Point(20, 195),
            Size = new Size(200, 35)
        };
        txtBmLastName.Properties.NullValuePrompt = "Soyad";
        txtBmLastName.Properties.Appearance.Font = new Font("Segoe UI", 10);

        var lblEmail = new LabelControl()
        {
            Location = new Point(260, 170),
            Size = new Size(100, 22),
            Text = "📧 E-posta:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };

        txtBmEmail = new TextEdit()
        {
            Location = new Point(260, 195),
            Size = new Size(200, 35)
        };
        txtBmEmail.Properties.NullValuePrompt = "email@domain.com";
        txtBmEmail.Properties.Appearance.Font = new Font("Segoe UI", 10);

        var lblPhone = new LabelControl()
        {
            Location = new Point(20, 240),
            Size = new Size(100, 22),
            Text = "📱 Telefon:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };

        txtBmPhone = new TextEdit()
        {
            Location = new Point(20, 265),
            Size = new Size(200, 35)
        };
        txtBmPhone.Properties.MaxLength = 10;
        txtBmPhone.Properties.NullValuePrompt = "5xxxxxxxxx";
        txtBmPhone.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtBmPhone.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };

        var lblPassword = new LabelControl()
        {
            Location = new Point(260, 240),
            Size = new Size(100, 22),
            Text = "🔒 Şifre:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };

        txtBmPassword = new TextEdit()
        {
            Location = new Point(260, 265),
            Size = new Size(200, 35)
        };
        txtBmPassword.Properties.PasswordChar = '●';
        txtBmPassword.Properties.NullValuePrompt = "Min. 6 karakter";
        txtBmPassword.Properties.Appearance.Font = new Font("Segoe UI", 10);

        var lblYetkiler = new LabelControl()
        {
            Location = new Point(20, 315),
            Size = new Size(440, 50),
            Text = "✅ Yetkiler: Müşteri/Hesap yönetimi, Onay/Red, Şifre sıfırlama, Audit log\n❌ Kısıtlamalar: Hesap kapatma ve kullanıcı deaktive etme yapamaz",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80) }
        };

        btnCreateBranchManager = new SimpleButton()
        {
            Location = new Point(20, 370),
            Size = new Size(440, 40),
            Text = "✓ Şube Yöneticisi Oluştur",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White }
        };
        btnCreateBranchManager.Appearance.BackColor = Color.FromArgb(63, 81, 181);
        btnCreateBranchManager.AppearanceHovered.BackColor = Color.FromArgb(48, 63, 159);
        btnCreateBranchManager.Click += BtnCreateBranchManager_Click;

        pnlCreate.Controls.AddRange(new Control[] {
            lblCreateTitle, lblInfo,
            lblNationalId, txtBmNationalId,
            lblFirstName, txtBmFirstName,
            lblLastName, txtBmLastName,
            lblEmail, txtBmEmail,
            lblPhone, txtBmPhone,
            lblPassword, txtBmPassword,
            lblYetkiler, btnCreateBranchManager
        });

        // ===== MEVCUT ŞUBE YÖNETİCİLERİ LİSTESİ =====
        var pnlList = new PanelControl()
        {
            Location = new Point(540, 60),
            Size = new Size(680, 420),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };

        var lblListTitle = new LabelControl()
        {
            Location = new Point(20, 15),
            Size = new Size(400, 28),
            Text = "📋 Mevcut Şube Yöneticileri",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(63, 81, 181) }
        };

        var btnRefreshBm = new SimpleButton()
        {
            Location = new Point(20, 50),
            Size = new Size(140, 35),
            Text = "🔄 Yenile",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        btnRefreshBm.Appearance.BackColor = Color.FromArgb(63, 81, 181);
        btnRefreshBm.Click += async (s, e) => await LoadBranchManagersAsync();

        gridBranchManagers = new GridControl()
        {
            Location = new Point(20, 95),
            Size = new Size(640, 310)
        };
        gridBranchManagersView = new GridView();
        gridBranchManagers.MainView = gridBranchManagersView;
        gridBranchManagersView.OptionsBehavior.Editable = false;
        gridBranchManagersView.OptionsSelection.MultiSelect = false;
        gridBranchManagersView.OptionsView.ShowGroupPanel = false;
        gridBranchManagersView.Appearance.HeaderPanel.BackColor = Color.FromArgb(63, 81, 181);
        gridBranchManagersView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridBranchManagersView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        pnlList.Controls.AddRange(new Control[] { lblListTitle, btnRefreshBm, gridBranchManagers });

        pnlMain.Controls.AddRange(new Control[] { lblTitle, pnlCreate, pnlList });
        tabAdminBranchManager.Controls.Add(pnlMain);

        // İlk yükleme
        _ = LoadBranchManagersAsync();
    }

    private async Task LoadBranchManagersAsync()
    {
        try
        {
            // Şube yöneticilerini listele (Role = BranchManager olanlar)
            var customers = await _api.SearchCustomersAsync("");
            if (customers != null && gridBranchManagers != null)
            {
                var branchManagers = customers.Where(c => c.Role == "BranchManager").ToList();
                gridBranchManagers.DataSource = branchManagers;

                if (gridBranchManagersView != null)
                {
                    gridBranchManagersView.Columns["CustomerId"].Visible = false;
                    if (gridBranchManagersView.Columns["FullName"] != null)
                        gridBranchManagersView.Columns["FullName"].Caption = "Ad Soyad";
                    if (gridBranchManagersView.Columns["NationalIdMasked"] != null)
                        gridBranchManagersView.Columns["NationalIdMasked"].Caption = "TCKN";
                    if (gridBranchManagersView.Columns["Role"] != null)
                        gridBranchManagersView.Columns["Role"].Caption = "Rol";
                    if (gridBranchManagersView.Columns["IsActive"] != null)
                        gridBranchManagersView.Columns["IsActive"].Caption = "Aktif";
                    if (gridBranchManagersView.Columns["IsApproved"] != null)
                        gridBranchManagersView.Columns["IsApproved"].Caption = "Onaylı";
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Şube yöneticileri yüklenirken hata: {ex.Message}");
        }
    }

    private async void BtnCreateBranchManager_Click(object? sender, EventArgs e)
    {
        try
        {
            var nationalId = txtBmNationalId?.Text?.Trim();
            var firstName = txtBmFirstName?.Text?.Trim();
            var lastName = txtBmLastName?.Text?.Trim();
            var email = txtBmEmail?.Text?.Trim();
            var phone = txtBmPhone?.Text?.Trim();
            var password = txtBmPassword?.Text?.Trim();

            // Validasyonlar
            if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 11)
            {
                XtraMessageBox.Show("TC Kimlik No 11 haneli olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                XtraMessageBox.Show("Ad ve Soyad alanları zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                XtraMessageBox.Show("Geçerli bir e-posta adresi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                XtraMessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnCreateBranchManager!.Enabled = false;
            this.UseWaitCursor = true;

            var request = new NovaBank.Contracts.Admin.CreateBranchManagerRequest(
                nationalId,
                firstName,
                lastName,
                email,
                phone ?? "",
                password
            );

            var response = await _api.CreateBranchManagerAsync(request);

            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show(
                    $"✅ Şube Yöneticisi başarıyla oluşturuldu!\n\n" +
                    $"Ad Soyad: {firstName} {lastName}\n" +
                    $"TC: {nationalId}\n" +
                    $"E-posta: {email}\n\n" +
                    $"Kullanıcı artık TC ve şifresi ile giriş yapabilir.",
                    "Başarılı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Formu temizle
                txtBmNationalId!.Text = "";
                txtBmFirstName!.Text = "";
                txtBmLastName!.Text = "";
                txtBmEmail!.Text = "";
                txtBmPhone!.Text = "";
                txtBmPassword!.Text = "";

                // Listeyi yenile
                await LoadBranchManagersAsync();
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Şube Yöneticisi oluşturulamadı:\n\n{error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (btnCreateBranchManager != null) btnCreateBranchManager.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async void BtnDeleteCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminCustomersView == null) return;
            var row = gridAdminCustomersView.GetFocusedRow();
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen silmek istediğiniz müşteriyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customerId = (Guid)gridAdminCustomersView.GetFocusedRowCellValue("CustomerId");
            var customerName = gridAdminCustomersView.GetFocusedRowCellValue("FullName")?.ToString() ?? "Bilinmiyor";
            var role = gridAdminCustomersView.GetFocusedRowCellValue("Role")?.ToString();

            // Admin silinmesini engelle
            if (role == "Admin")
            {
                XtraMessageBox.Show("Admin kullanıcısı silinemez!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = XtraMessageBox.Show(
                $"⚠️ DİKKAT - KALICI SİLME!\n\n\"{customerName}\" müşterisini ve TÜM hesaplarını veritabanından kalıcı olarak silmek istediğinizden emin misiniz?\n\n⚠️ Bu işlem geri alınamaz!\n⚠️ Log kayıtları korunacaktır.",
                "Müşteri Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            this.UseWaitCursor = true;

            // Müşteriyi veritabanından sil
            var response = await _api.DeleteCustomerAsync(customerId);
            
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"✅ \"{customerName}\" müşterisi ve tüm hesapları başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Müşteri listesini yenile
                BtnAdminSearch_Click(null, EventArgs.Empty);
                // Hesap listesini temizle
                if (gridAdminAccounts != null)
                    gridAdminAccounts.DataSource = null;
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Müşteri silinemedi:\n{error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.UseWaitCursor = false;
        }
    }

    private async void BtnDeleteAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            if (gridAdminAccountsView == null) return;
            var row = gridAdminAccountsView.GetFocusedRow();
            if (row == null)
            {
                XtraMessageBox.Show("Lütfen silmek istediğiniz hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var accountId = (Guid)gridAdminAccountsView.GetFocusedRowCellValue("AccountId");
            var iban = gridAdminAccountsView.GetFocusedRowCellValue("Iban")?.ToString() ?? "Bilinmiyor";
            var currency = gridAdminAccountsView.GetFocusedRowCellValue("Currency")?.ToString() ?? "";
            var balance = gridAdminAccountsView.GetFocusedRowCellValue("Balance");
            
            var balanceStr = balance != null ? $"{Convert.ToDecimal(balance):N2} {currency}" : "0";

            var result = XtraMessageBox.Show(
                $"⚠️ DİKKAT - KALICI SİLME!\n\nIBAN: {iban}\nBakiye: {balanceStr}\n\nBu hesabı veritabanından kalıcı olarak silmek istediğinizden emin misiniz?\n\n⚠️ Bu işlem geri alınamaz!\n⚠️ Log kayıtları korunacaktır.",
                "Hesap Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            this.UseWaitCursor = true;

            var response = await _api.DeleteAccountAsync(accountId);
            
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"✅ Hesap başarıyla silindi.\n\nIBAN: {iban}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Hesap listesini yenile
                if (gridAdminCustomersView != null)
                {
                    var customerId = gridAdminCustomersView.GetFocusedRowCellValue("CustomerId");
                    if (customerId != null)
                    {
                        var accounts = await _api.GetCustomerAccountsAsync((Guid)customerId);
                        if (accounts != null && gridAdminAccounts != null)
                        {
                            gridAdminAccounts.DataSource = accounts;
                        }
                    }
                }
            }
            else
            {
                var error = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Hesap silinemedi:\n{error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.UseWaitCursor = false;
        }
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

            // Action mapping: "Hepsi" veya boÅŸ ise null

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

                        lblPendingTitle.Text = $"⏳ Onay Bekleyen ({count})";

                    else

                        lblPendingTitle.Text = "✅ Onay Bekleyen Yok";

                }

                // Sütun görünürlüğü ve başlıkları
                if (gridPendingApprovalsView.Columns["ItemId"] != null)
                    gridPendingApprovalsView.Columns["ItemId"].Visible = false;

                if (gridPendingApprovalsView.Columns["ItemType"] != null)
                {
                    gridPendingApprovalsView.Columns["ItemType"].Caption = "Tip";
                    gridPendingApprovalsView.Columns["ItemType"].VisibleIndex = 0;
                }

                if (gridPendingApprovalsView.Columns["FullName"] != null)
                {
                    gridPendingApprovalsView.Columns["FullName"].Caption = "Ad Soyad";
                    gridPendingApprovalsView.Columns["FullName"].VisibleIndex = 1;
                }

                if (gridPendingApprovalsView.Columns["NationalId"] != null)
                {
                    gridPendingApprovalsView.Columns["NationalId"].Caption = "TCKN";
                    gridPendingApprovalsView.Columns["NationalId"].VisibleIndex = 2;
                }

                if (gridPendingApprovalsView.Columns["Email"] != null)
                {
                    gridPendingApprovalsView.Columns["Email"].Caption = "E-posta";
                    gridPendingApprovalsView.Columns["Email"].VisibleIndex = 3;
                }

                if (gridPendingApprovalsView.Columns["Iban"] != null)
                {
                    gridPendingApprovalsView.Columns["Iban"].Caption = "IBAN";
                    gridPendingApprovalsView.Columns["Iban"].VisibleIndex = 4;
                }

                if (gridPendingApprovalsView.Columns["Currency"] != null)
                {
                    gridPendingApprovalsView.Columns["Currency"].Caption = "Para Birimi";
                    gridPendingApprovalsView.Columns["Currency"].VisibleIndex = 5;
                }

                if (gridPendingApprovalsView.Columns["CreatedAt"] != null)

                {

                    gridPendingApprovalsView.Columns["CreatedAt"].Caption = "Kayıt Tarihi";
                    gridPendingApprovalsView.Columns["CreatedAt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    gridPendingApprovalsView.Columns["CreatedAt"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
                    gridPendingApprovalsView.Columns["CreatedAt"].VisibleIndex = 6;

                }

                if (gridPendingApprovalsView.Columns["AccountId"] != null)
                    gridPendingApprovalsView.Columns["AccountId"].Visible = false;

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

                XtraMessageBox.Show("Lütfen onaylanacak öğeyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var pending = gridPendingApprovalsView.GetRow(gridPendingApprovalsView.FocusedRowHandle) as NovaBank.Contracts.Admin.PendingApprovalResponse;

            if (pending == null) return;

            btnApproveCustomer.Enabled = false;

            HttpResponseMessage response;

            if (pending.ItemType == NovaBank.Contracts.Admin.PendingItemType.Account)
            {
                // Hesap onayı
                if (!pending.AccountId.HasValue)
                {
                    XtraMessageBox.Show("Hesap bilgisi bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnApproveCustomer.Enabled = true;
                    return;
                }

                var confirm = XtraMessageBox.Show(
                    $"'{pending.FullName}' adlı müşterinin {pending.Currency} hesabını onaylamak istiyor musunuz?\n\n" +
                    $"IBAN: {pending.Iban}\n" +
                    $"Para Birimi: {pending.Currency}",
                    "Hesap Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    btnApproveCustomer.Enabled = true;
                    return;
                }

                response = await _api.UpdateAccountStatusAsync(pending.AccountId.Value, "Active");

                if (response.IsSuccessStatusCode)
                {
                    XtraMessageBox.Show(
                        $"✓ {pending.Currency} hesabı başarıyla onaylandı!\n\n" +
                        $"Müşteri: {pending.FullName}\n" +
                        $"IBAN: {pending.Iban}",
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var error = await ApiClient.GetErrorMessageAsync(response);
                    XtraMessageBox.Show($"Hesap onaylama başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnApproveCustomer.Enabled = true;
                    return;
                }
            }
            else
            {
                // Müşteri onayı
                var confirm = XtraMessageBox.Show(
                    $"'{pending.FullName}' adlı müşteriyi onaylamak istiyor musunuz?\n\nTCKN: {pending.NationalId}",
                    "Müşteri Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    btnApproveCustomer.Enabled = true;
                    return;
                }

                response = await _api.ApproveCustomerAsync(pending.ItemId);

                if (response.IsSuccessStatusCode)
                {
                    XtraMessageBox.Show($"✓ '{pending.FullName}' başarıyla onaylandı!\n\nArtık sisteme giriş yapabilir.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var error = await ApiClient.GetErrorMessageAsync(response);
                    XtraMessageBox.Show($"Onaylama başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnApproveCustomer.Enabled = true;
                    return;
                }
            }

            BtnRefreshPending_Click(null, EventArgs.Empty);

            BtnAdminSearch_Click(null, EventArgs.Empty);

            btnApproveCustomer.Enabled = true;

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

                XtraMessageBox.Show("Lütfen reddedilecek öğeyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var pending = gridPendingApprovalsView.GetRow(gridPendingApprovalsView.FocusedRowHandle) as NovaBank.Contracts.Admin.PendingApprovalResponse;

            if (pending == null) return;

            btnRejectCustomer.Enabled = false;

            HttpResponseMessage response;

            if (pending.ItemType == NovaBank.Contracts.Admin.PendingItemType.Account)
            {
                // Hesap reddi
                if (!pending.AccountId.HasValue)
                {
                    XtraMessageBox.Show("Hesap bilgisi bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnRejectCustomer.Enabled = true;
                    return;
                }

                var confirm = XtraMessageBox.Show(
                    $"'{pending.FullName}' adlı müşterinin {pending.Currency} hesabını REDDETMEK istiyor musunuz?\n\n" +
                    $"IBAN: {pending.Iban}\n" +
                    $"Para Birimi: {pending.Currency}\n\n" +
                    $"⚠️ Bu işlem hesabı kapatacaktır!",
                    "Hesap Reddi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                {
                    btnRejectCustomer.Enabled = true;
                    return;
                }

                response = await _api.UpdateAccountStatusAsync(pending.AccountId.Value, "Closed");

                if (response.IsSuccessStatusCode)
                {
                    XtraMessageBox.Show(
                        $"'{pending.FullName}' adlı müşterinin {pending.Currency} hesabı reddedildi ve kapatıldı.\n\n" +
                        $"IBAN: {pending.Iban}",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var error = await ApiClient.GetErrorMessageAsync(response);
                    XtraMessageBox.Show($"Hesap reddi başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnRejectCustomer.Enabled = true;
                    return;
                }
            }
            else
            {
                // Müşteri reddi
                var confirm = XtraMessageBox.Show(
                    $"'{pending.FullName}' adlı müşterinin kaydını REDDETMEK istiyor musunuz?\n\nTCKN: {pending.NationalId}\n\n⚠️ Bu işlem müşteriyi pasif yapacaktır!",
                    "Müşteri Reddi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                {
                    btnRejectCustomer.Enabled = true;
                    return;
                }

                response = await _api.RejectCustomerAsync(pending.ItemId);

                if (response.IsSuccessStatusCode)
                {
                    XtraMessageBox.Show($"'{pending.FullName}' reddedildi ve pasif yapıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var error = await ApiClient.GetErrorMessageAsync(response);
                    XtraMessageBox.Show($"Reddetme başarısız: {error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnRejectCustomer.Enabled = true;
                    return;
                }
            }

            BtnRefreshPending_Click(null, EventArgs.Empty);

            BtnAdminSearch_Click(null, EventArgs.Empty);

            btnRejectCustomer.Enabled = true;

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

            Text = "ğŸ”„ Yenile",

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

            Size = new Size(400, 140),

            Appearance = { BackColor = Color.FromArgb(255, 243, 224), BorderColor = Color.FromArgb(255, 152, 0) }

        };

        var lblPayTitle = new LabelControl()

        {

            Location = new Point(15, 10),

            Text = "💳 Borç Öde",

            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(255, 152, 0) }

        };

        var lblSourceAcc = new LabelControl()

        {

            Location = new Point(15, 42),

            Text = "Ödeyecek Hesap:",

            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold) }

        };

        cmbCardPayAccount = new LookUpEdit()

        {

            Location = new Point(15, 62),

            Size = new Size(200, 30)

        };

        cmbCardPayAccount.Properties.NullText = "Hesap seçin...";

        var lblPayAmountSpan = new LabelControl()

        {

            Location = new Point(230, 42),

            Text = "Tutar (₺):",

            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold) }

        };

        txtCardPaymentAmount = new TextEdit()

        {

            Location = new Point(230, 62),

            Size = new Size(150, 30)

        };

        txtCardPaymentAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;

        txtCardPaymentAmount.Properties.Mask.EditMask = "n2";

        btnPayCardDebt = new SimpleButton()

        {

            Location = new Point(15, 100),

            Size = new Size(365, 32),

            Text = "💳 Borcu Öde",

            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }

        };

        btnPayCardDebt.Appearance.BackColor = Color.FromArgb(255, 152, 0);

        btnPayCardDebt.Click += BtnPayCardDebt_Click;

        pnlPayment.Controls.AddRange(new Control[] { lblPayTitle, lblSourceAcc, cmbCardPayAccount, lblPayAmountSpan, txtCardPaymentAmount, btnPayCardDebt });

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

            Text = "💳 Başvuru Durumlarım",

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

            if (gridCardsMain != null && cards != null)

            {

                gridCardsMain.DataSource = cards;

                // Kolonları ayarla

                if (gridCardsMainView.Columns["CardId"] != null) gridCardsMainView.Columns["CardId"].Visible = false;

                if (gridCardsMainView.Columns["MaskedPan"] != null) gridCardsMainView.Columns["MaskedPan"].Caption = "Kart Numarası";

                if (gridCardsMainView.Columns["CreditLimit"] != null) 

                {

                    gridCardsMainView.Columns["CreditLimit"].Caption = "Limit";

                    gridCardsMainView.Columns["CreditLimit"].DisplayFormat.FormatString = "N2";

                }

                if (gridCardsMainView.Columns["AvailableLimit"] != null)

                {

                    gridCardsMainView.Columns["AvailableLimit"].Caption = "Kullanılabilir Limit";

                    gridCardsMainView.Columns["AvailableLimit"].DisplayFormat.FormatString = "N2";

                }

                if (gridCardsMainView.Columns["CurrentDebt"] != null)

                {

                    gridCardsMainView.Columns["CurrentDebt"].Caption = "Dönem Borcu";

                    gridCardsMainView.Columns["CurrentDebt"].DisplayFormat.FormatString = "N2";

                }

                if (gridCardsMainView.Columns["MinPaymentAmount"] != null)

                {

                    gridCardsMainView.Columns["MinPaymentAmount"].Caption = "Min. Ödeme";

                    gridCardsMainView.Columns["MinPaymentAmount"].DisplayFormat.FormatString = "N2";

                }

                if (gridCardsMainView.Columns["MinPaymentDueDate"] != null)

                {

                    gridCardsMainView.Columns["MinPaymentDueDate"].Caption = "Son Ödeme Tarihi";

                    gridCardsMainView.Columns["MinPaymentDueDate"].DisplayFormat.FormatString = "dd.MM.yyyy";

                }

                if (gridCardsMainView.Columns["Status"] != null) gridCardsMainView.Columns["Status"].Caption = "Durum";

            }

            // Başvuruları yükle

            var applications = await _api.GetMyCardApplicationsAsync();

            if (gridCardApplications != null)

            {

                gridCardApplications.DataSource = applications;

            }

            // Kaynak hesapları doldur (TL hesapları)

            if (cmbCardPayAccount != null && _cachedAccounts != null)

            {

                var tryAccounts = _cachedAccounts.Where(a => a.Currency == "TRY").ToList();

                cmbCardPayAccount.Properties.DataSource = tryAccounts;

                cmbCardPayAccount.Properties.DisplayMember = "Iban";

                cmbCardPayAccount.Properties.ValueMember = "Id";

                cmbCardPayAccount.Properties.Columns.Clear();

                cmbCardPayAccount.Properties.Columns.Add(new LookUpColumnInfo("Iban", "Hesap / IBAN", 200));

                cmbCardPayAccount.Properties.Columns.Add(new LookUpColumnInfo("Balance", "Bakiye", 100));

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

            var fromAccountId = cmbCardPayAccount?.EditValue as Guid?;

            if (!fromAccountId.HasValue)

            {

                XtraMessageBox.Show("Lütfen ödemenin yapılacağı hesabı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var amountText = txtCardPaymentAmount?.EditValue?.ToString();

            if (string.IsNullOrWhiteSpace(amountText) || !decimal.TryParse(amountText, out var amount) || amount <= 0)

            {

                XtraMessageBox.Show("Geçerli bir ödeme tutarı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            btnPayCardDebt.Enabled = false;

            var resp = await _api.PayCardDebtAsync(cardId, amount, fromAccountId.Value);

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

            Text = "ğŸ” Fatura Sorgula",

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

            Text = "ğŸ” Sorgula",

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

                    new RadioGroupItem(0, "💰 Banka Hesabı"),

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

    // ========== DÖVİZ AL/SAT ==========

    private Dictionary<string, CurrencyRateDto> _cachedRates = new();
    private List<NovaBank.Contracts.ExchangeRates.DovizKurDto>? _allRates = null;

    private void UpdateCurrencyDropdowns(List<NovaBank.Contracts.ExchangeRates.DovizKurDto> rates)
    {
        _allRates = rates;
        
        // Tüm dövizleri al (TRY hariç, sadece ForexBuying ve ForexSelling olanlar)
        var availableCurrencies = rates
            .Where(r => !string.IsNullOrEmpty(r.CurrencyCode) && 
                       r.CurrencyCode != "TRY" && 
                       r.ForexBuying.HasValue && 
                       r.ForexSelling.HasValue)
            .Select(r => r.CurrencyCode!)
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        if (cmbFxBuyCurrency != null)
        {
            cmbFxBuyCurrency.Properties.Items.Clear();
            cmbFxBuyCurrency.Properties.Items.AddRange(availableCurrencies);
            if (availableCurrencies.Length > 0 && cmbFxBuyCurrency.EditValue == null)
            {
                cmbFxBuyCurrency.EditValue = availableCurrencies[0];
            }
        }

        if (cmbFxSellCurrency != null)
        {
            cmbFxSellCurrency.Properties.Items.Clear();
            cmbFxSellCurrency.Properties.Items.AddRange(availableCurrencies);
            if (availableCurrencies.Length > 0 && cmbFxSellCurrency.EditValue == null)
            {
                cmbFxSellCurrency.EditValue = availableCurrencies[0];
            }
        }
    }

    private void UpdateFxRateLabels()

    {

        // Alım için kur label

        if (cmbFxBuyCurrency != null && lblFxBuyRate != null)

        {

            var currency = cmbFxBuyCurrency.EditValue?.ToString();

            if (!string.IsNullOrEmpty(currency) && _cachedRates.TryGetValue(currency, out var rate))

            {

                lblFxBuyRate.Text = $"Kur: {rate.SellRate:N4} TL (Banka Satış)";

            }

            else

            {

                lblFxBuyRate.Text = "Kur: -- TL";

            }

        }

        // Satım için kur label

        if (cmbFxSellCurrency != null && lblFxSellRate != null)

        {

            var currency = cmbFxSellCurrency.EditValue?.ToString();

            if (!string.IsNullOrEmpty(currency) && _cachedRates.TryGetValue(currency, out var rate))

            {

                lblFxSellRate.Text = $"Kur: {rate.BuyRate:N4} TL (Banka Alış)";

            }

            else

            {

                lblFxSellRate.Text = "Kur: -- TL";

            }

        }

    }

    private async Task LoadFxAccountDropdowns()

    {

        // Eğer hesaplar yüklenmemişse, yükle
        if (_cachedAccounts == null || _cachedAccounts.Count == 0)
        {
            await LoadAccounts();
        }

        // Tekrar kontrol et
        if (_cachedAccounts == null || _cachedAccounts.Count == 0)
        {
            // Hesaplar yüklenemedi, dropdown'ları temizle
            if (cmbFxBuyFromTry != null)
            {
                cmbFxBuyFromTry.Properties.Items.Clear();
                cmbFxBuyFromTry.Properties.Items.Add("⚠️ Hesaplar yüklenemedi");
                cmbFxBuyFromTry.Tag = null;
            }
            if (cmbFxSellToTry != null)
            {
                cmbFxSellToTry.Properties.Items.Clear();
                cmbFxSellToTry.Properties.Items.Add("⚠️ Hesaplar yüklenemedi");
                cmbFxSellToTry.Tag = null;
            }
            return;
        }

        // TL Hesapları - Sadece Active durumundaki hesapları filtrele
        var tryAccounts = _cachedAccounts.Where(a => 
            a.Currency != null && 
            a.Currency.Equals("TRY", StringComparison.OrdinalIgnoreCase) && 
            a.Status != null && 
            a.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();

        if (cmbFxBuyFromTry != null)

        {

            cmbFxBuyFromTry.Properties.Items.Clear();

            if (tryAccounts.Count > 0)
            {
                foreach (var acc in tryAccounts)

                {

                    cmbFxBuyFromTry.Properties.Items.Add($"{acc.Iban} - ₺{acc.Balance:N2}");

                }

                cmbFxBuyFromTry.Tag = tryAccounts;

                // Varsayılan seçim - EditValue kullan
                if (cmbFxBuyFromTry.EditValue == null && tryAccounts.Count > 0)
                {
                    cmbFxBuyFromTry.EditValue = cmbFxBuyFromTry.Properties.Items[0];
                }
            }
            else
            {
                cmbFxBuyFromTry.Properties.Items.Add("⚠️ Aktif TRY hesabınız yok");
                cmbFxBuyFromTry.Tag = null;
            }

        }

        if (cmbFxSellToTry != null)

        {

            cmbFxSellToTry.Properties.Items.Clear();

            if (tryAccounts.Count > 0)
            {
                foreach (var acc in tryAccounts)

                {

                    cmbFxSellToTry.Properties.Items.Add($"{acc.Iban} - ₺{acc.Balance:N2}");

                }

                cmbFxSellToTry.Tag = tryAccounts;

                // Varsayılan seçim - EditValue kullan
                if (cmbFxSellToTry.EditValue == null && tryAccounts.Count > 0)
                {
                    cmbFxSellToTry.EditValue = cmbFxSellToTry.Properties.Items[0];
                }
            }
            else
            {
                cmbFxSellToTry.Properties.Items.Add("⚠️ Aktif TRY hesabınız yok");
                cmbFxSellToTry.Tag = null;
            }

        }

        // Varsayılan seçimi kur dropdown'ları için

        UpdateFxForeignAccountDropdowns();

    }

    private void UpdateFxForeignAccountDropdowns()

    {

        // Hesaplar yüklenmemişse çık
        if (_cachedAccounts == null || _cachedAccounts.Count == 0)
            return;

        // Tüm döviz hesapları - Sadece Active durumundaki hesapları göster

        var buyCurrency = cmbFxBuyCurrency?.EditValue?.ToString() ?? "USD";

        var sellCurrency = cmbFxSellCurrency?.EditValue?.ToString() ?? "USD";

        var buyForeignAccounts = _cachedAccounts.Where(a => 
            a.Currency != null && 
            a.Currency.Equals(buyCurrency, StringComparison.OrdinalIgnoreCase) && 
            a.Status != null && 
            a.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();

        var sellForeignAccounts = _cachedAccounts.Where(a => 
            a.Currency != null && 
            a.Currency.Equals(sellCurrency, StringComparison.OrdinalIgnoreCase) && 
            a.Status != null && 
            a.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();
        
        // Onay bekleyen hesap var mı kontrol et
        var pendingBuyAccounts = _cachedAccounts.Where(a => a.Currency == buyCurrency && a.Status == "PendingApproval").ToList();
        var pendingSellAccounts = _cachedAccounts.Where(a => a.Currency == sellCurrency && a.Status == "PendingApproval").ToList();

        if (cmbFxBuyToForeign != null)

        {

            cmbFxBuyToForeign.Properties.Items.Clear();

            foreach (var acc in buyForeignAccounts)

            {

                cmbFxBuyToForeign.Properties.Items.Add($"{acc.Iban} - {acc.Balance:N2} {acc.Currency}");

            }

            cmbFxBuyToForeign.Tag = buyForeignAccounts;

            // Döviz hesabı yoksa buton devre dışı ve uyarı göster

            if (buyForeignAccounts.Count == 0)

            {

                if (pendingBuyAccounts.Count > 0)
                {
                    cmbFxBuyToForeign.Properties.Items.Add($"⏳ {buyCurrency} hesabınız onay bekliyor");
                    if (lblFxBuyCalc != null) lblFxBuyCalc.Text = $"⏳ {buyCurrency} hesabınız admin onayı bekliyor. Onaylandıktan sonra işlem yapabilirsiniz.";
                }
                else
                {
                    cmbFxBuyToForeign.Properties.Items.Add($"⚠️ {buyCurrency} hesabınız yok - Önce hesap açın");
                    if (lblFxBuyCalc != null) lblFxBuyCalc.Text = $"⚠️ {buyCurrency} döviz hesabınız yok. Hesaplarım > Yeni Hesap Aç'tan döviz hesabı açın.";
                }

                if (btnFxBuy != null) btnFxBuy.Enabled = false;

            }

            else

            {

                if (btnFxBuy != null) btnFxBuy.Enabled = true;

                // Varsayılan seçim - EditValue kullan
                if (cmbFxBuyToForeign.EditValue == null && buyForeignAccounts.Count > 0)
                {
                    cmbFxBuyToForeign.EditValue = cmbFxBuyToForeign.Properties.Items[0];
                }

            }

        }

        if (cmbFxSellFromForeign != null)

        {

            cmbFxSellFromForeign.Properties.Items.Clear();

            foreach (var acc in sellForeignAccounts)

            {

                cmbFxSellFromForeign.Properties.Items.Add($"{acc.Iban} - {acc.Balance:N2} {acc.Currency}");

            }

            cmbFxSellFromForeign.Tag = sellForeignAccounts;

            // Döviz hesabı yoksa buton devre dışı ve uyarı göster

            if (sellForeignAccounts.Count == 0)

            {

                if (pendingSellAccounts.Count > 0)
                {
                    cmbFxSellFromForeign.Properties.Items.Add($"⏳ {sellCurrency} hesabınız onay bekliyor");
                    if (lblFxSellCalc != null) lblFxSellCalc.Text = $"⏳ {sellCurrency} hesabınız admin onayı bekliyor. Onaylandıktan sonra işlem yapabilirsiniz.";
                }
                else
                {
                    cmbFxSellFromForeign.Properties.Items.Add($"⚠️ {sellCurrency} hesabınız yok - Önce hesap açın");
                    if (lblFxSellCalc != null) lblFxSellCalc.Text = $"⚠️ {sellCurrency} döviz hesabınız yok. Hesaplarım > Yeni Hesap Aç'tan döviz hesabı açın.";
                }

                if (btnFxSell != null) btnFxSell.Enabled = false;

            }

            else

            {

                if (btnFxSell != null) btnFxSell.Enabled = true;

                // Varsayılan seçim - EditValue kullan
                if (cmbFxSellFromForeign.EditValue == null && sellForeignAccounts.Count > 0)
                {
                    cmbFxSellFromForeign.EditValue = cmbFxSellFromForeign.Properties.Items[0];
                }

            }

        }

    }

    private void CmbFxBuyCurrency_EditValueChanged(object? sender, EventArgs e)

    {

        UpdateFxRateLabels();

        UpdateFxForeignAccountDropdowns();

        CalculateFxBuy();

    }

    private void CmbFxSellCurrency_EditValueChanged(object? sender, EventArgs e)

    {

        UpdateFxRateLabels();

        UpdateFxForeignAccountDropdowns();

        CalculateFxSell();

    }

    private void TxtFxBuyAmount_EditValueChanged(object? sender, EventArgs e)

    {

        CalculateFxBuy();

    }

    private void TxtFxSellAmount_EditValueChanged(object? sender, EventArgs e)

    {

        CalculateFxSell();

    }

    private void CalculateFxBuy()

    {

        if (lblFxBuyCalc == null || txtFxBuyAmount == null || cmbFxBuyCurrency == null) return;

        var currency = cmbFxBuyCurrency.EditValue?.ToString();

        if (string.IsNullOrEmpty(currency) || !_cachedRates.TryGetValue(currency, out var rate))

        {

            lblFxBuyCalc.Text = "💰 Döviz seçin";

            return;

        }

        if (!decimal.TryParse(txtFxBuyAmount.Text, out var amount) || amount <= 0)

        {

            lblFxBuyCalc.Text = "💰 Ödenecek: 0,00 TL";

            return;

        }

        var tryAmount = amount * rate.SellRate;

        var commission = tryAmount * 0.001m; // %0.1 komisyon

        var total = tryAmount + commission;

        lblFxBuyCalc.Text = $"💰 Ödenecek: {total:N2} TL (Kom: {commission:N2})";

    }

    private void CalculateFxSell()

    {

        if (lblFxSellCalc == null || txtFxSellAmount == null || cmbFxSellCurrency == null) return;

        var currency = cmbFxSellCurrency.EditValue?.ToString();

        if (string.IsNullOrEmpty(currency) || !_cachedRates.TryGetValue(currency, out var rate))

        {

            lblFxSellCalc.Text = "💰 Döviz seçin";

            return;

        }

        if (!decimal.TryParse(txtFxSellAmount.Text, out var amount) || amount <= 0)

        {

            lblFxSellCalc.Text = "💰 Alınacak: 0,00 TL";

            return;

        }

        var tryAmount = amount * rate.BuyRate;

        var commission = tryAmount * 0.001m; // %0.1 komisyon

        var net = tryAmount - commission;

        lblFxSellCalc.Text = $"💰 Alınacak: {net:N2} TL (Kom: {commission:N2})";

    }

    private async void BtnFxBuy_Click(object? sender, EventArgs e)

    {

        try

        {

            // Validasyonlar

            var currency = cmbFxBuyCurrency?.EditValue?.ToString();

            if (string.IsNullOrEmpty(currency))

            {

                XtraMessageBox.Show("Lütfen döviz seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            if (!decimal.TryParse(txtFxBuyAmount?.Text, out var amount) || amount <= 0)

            {

                XtraMessageBox.Show("Geçerli bir miktar girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var tryAccounts = cmbFxBuyFromTry?.Tag as List<AccountResponse>;

            var foreignAccounts = cmbFxBuyToForeign?.Tag as List<AccountResponse>;

            if (tryAccounts == null || tryAccounts.Count == 0 || cmbFxBuyFromTry?.EditValue == null)

            {

                XtraMessageBox.Show("TL hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            if (foreignAccounts == null || foreignAccounts.Count == 0 || cmbFxBuyToForeign?.EditValue == null)

            {

                XtraMessageBox.Show($"{currency} hesabınız yok. Önce bir {currency} hesabı açın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            // EditValue'dan seçili item'ı bul
            var selectedTryText = cmbFxBuyFromTry.EditValue.ToString();
            var fromAccount = tryAccounts.FirstOrDefault(a => selectedTryText?.Contains(a.Iban) == true);
            
            var selectedForeignText = cmbFxBuyToForeign.EditValue.ToString();
            var toAccount = foreignAccounts.FirstOrDefault(a => selectedForeignText?.Contains(a.Iban) == true);

            if (fromAccount == null)
            {
                XtraMessageBox.Show("TL hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (toAccount == null)
            {
                XtraMessageBox.Show($"{currency} hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_cachedRates.TryGetValue(currency, out var rate))

            {

                XtraMessageBox.Show("Kur bilgisi bulunamadı. Kurları yenileyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var tryAmount = amount * rate.SellRate;

            var commission = tryAmount * 0.001m;

            var total = tryAmount + commission;

            // Onay

            var confirm = XtraMessageBox.Show(

                $"{amount:N2} {currency} alınacak\n\n" +

                $"Kur: {rate.SellRate:N4} TL/{currency}\n" +

                $"TL Tutarı: {tryAmount:N2} TL\n" +

                $"Komisyon: {commission:N2} TL\n" +

                $"────────────────────────\n" +

                $"TOPLAM: {total:N2} TL\n\n" +

                $"Kaynak: {fromAccount.Iban}\n" +

                $"Hedef: {toAccount.Iban}\n\n" +

                $"Onaylıyor musunuz?",

                "Döviz Alım Onayı",

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            btnFxBuy!.Enabled = false;

            this.UseWaitCursor = true;

            var resp = await _api.BuyCurrencyAsync(currency, amount, fromAccount.Id, toAccount.Id, "Döviz alımı");

            if (resp.IsSuccessStatusCode)

            {

                XtraMessageBox.Show(

                    $"✅ Döviz alımı başarılı!\n\n" +

                    $"{amount:N2} {currency} alındı\n" +

                    $"Ödenen: {total:N2} TL",

                    "Başarılı",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);

                txtFxBuyAmount!.Text = "";

                await LoadAccounts();

                await LoadFxAccountDropdowns();

                await LoadFxPositionsAsync();

            }

            else

            {

                var error = await ApiClient.GetErrorMessageAsync(resp);

                XtraMessageBox.Show($"Döviz alımı başarısız:\n{error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        catch (Exception ex)

        {

            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        finally

        {

            if (btnFxBuy != null) btnFxBuy.Enabled = true;

            this.UseWaitCursor = false;

        }

    }

    private async void BtnFxSell_Click(object? sender, EventArgs e)

    {

        try

        {

            // Validasyonlar

            var currency = cmbFxSellCurrency?.EditValue?.ToString();

            if (string.IsNullOrEmpty(currency))

            {

                XtraMessageBox.Show("Lütfen döviz seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            if (!decimal.TryParse(txtFxSellAmount?.Text, out var amount) || amount <= 0)

            {

                XtraMessageBox.Show("Geçerli bir miktar girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var foreignAccounts = cmbFxSellFromForeign?.Tag as List<AccountResponse>;

            var tryAccounts = cmbFxSellToTry?.Tag as List<AccountResponse>;

            if (foreignAccounts == null || foreignAccounts.Count == 0 || cmbFxSellFromForeign?.EditValue == null)

            {

                XtraMessageBox.Show($"{currency} hesabınız yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            if (tryAccounts == null || tryAccounts.Count == 0 || cmbFxSellToTry?.EditValue == null)

            {

                XtraMessageBox.Show("TL hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            // EditValue'dan seçili item'ı bul
            var selectedForeignText = cmbFxSellFromForeign.EditValue.ToString();
            var fromAccount = foreignAccounts.FirstOrDefault(a => selectedForeignText?.Contains(a.Iban) == true);
            
            var selectedTryText = cmbFxSellToTry.EditValue.ToString();
            var toAccount = tryAccounts.FirstOrDefault(a => selectedTryText?.Contains(a.Iban) == true);

            if (fromAccount == null)
            {
                XtraMessageBox.Show($"{currency} hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (toAccount == null)
            {
                XtraMessageBox.Show("TL hesabı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_cachedRates.TryGetValue(currency, out var rate))

            {

                XtraMessageBox.Show("Kur bilgisi bulunamadı. Kurları yenileyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }

            var tryAmount = amount * rate.BuyRate;

            var commission = tryAmount * 0.001m;

            var net = tryAmount - commission;

            // Onay

            var confirm = XtraMessageBox.Show(

                $"{amount:N2} {currency} satılacak\n\n" +

                $"Kur: {rate.BuyRate:N4} TL/{currency}\n" +

                $"TL Tutarı: {tryAmount:N2} TL\n" +

                $"Komisyon: {commission:N2} TL\n" +

                $"────────────────────────\n" +

                $"NET ALINACAK: {net:N2} TL\n\n" +

                $"Kaynak: {fromAccount.Iban}\n" +

                $"Hedef: {toAccount.Iban}\n\n" +

                $"Onaylıyor musunuz?",

                "Döviz Satım Onayı",

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            btnFxSell!.Enabled = false;

            this.UseWaitCursor = true;

            var resp = await _api.SellCurrencyAsync(currency, amount, fromAccount.Id, toAccount.Id, "Döviz satımı");

            if (resp.IsSuccessStatusCode)

            {

                XtraMessageBox.Show(

                    $"✅ Döviz satımı başarılı!\n\n" +

                    $"{amount:N2} {currency} satıldı\n" +

                    $"Alınan: {net:N2} TL",

                    "Başarılı",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);

                txtFxSellAmount!.Text = "";

                await LoadAccounts();

                await LoadFxAccountDropdowns();

                await LoadFxPositionsAsync();

            }

            else

            {

                var error = await ApiClient.GetErrorMessageAsync(resp);

                XtraMessageBox.Show($"Döviz satımı başarısız:\n{error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        catch (Exception ex)

        {

            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        finally

        {

            if (btnFxSell != null) btnFxSell.Enabled = true;

            this.UseWaitCursor = false;

        }

    }

    private async Task LoadFxPositionsAsync()

    {

        try

        {

            var positions = await _api.GetCurrencyPositionsAsync();

            if (positions != null && gridFxPositions != null)

            {

                gridFxPositions.DataSource = positions.Positions?.Select(p => new

                {

                    Döviz = p.Currency,

                    Miktar = p.TotalAmount,

                    OrtMaliyet = p.AverageCostRate,

                    ToplamTL = p.TotalCostTry,

                    GüncelKur = p.CurrentRate,

                    GüncelDeğer = p.CurrentValue,

                    KarZarar = p.UnrealizedPnlTry,

                    KarZararYüzde = p.UnrealizedPnlPercent

                }).ToList();

                if (lblFxPositionsSummary != null)

                {

                    var pnlColor = positions.TotalUnrealizedPnlTry >= 0 ? "📈" : "📉";

                    lblFxPositionsSummary.Text = 

                        $"📊 Toplam Maliyet: {positions.TotalCostTry:N2} TL\n" +

                        $"💰 Güncel Değer: {positions.TotalCurrentValue:N2} TL\n" +

                        $"{pnlColor} K/Z: {positions.TotalUnrealizedPnlTry:+#,##0.00;-#,##0.00;0} TL ({positions.TotalUnrealizedPnlPercent:+#0.00;-#0.00;0}%)";

                    lblFxPositionsSummary.Appearance.ForeColor = positions.TotalUnrealizedPnlTry >= 0 

                        ? Color.FromArgb(76, 175, 80) 

                        : Color.FromArgb(244, 67, 54);

                }

            }

        }

        catch (Exception ex)

        {

            System.Diagnostics.Debug.WriteLine($"Pozisyon yükleme hatası: {ex.Message}");

        }

    }

    // ========== HESAPLARIM - KART GÖRÜNÜMÜ ==========

    private Guid? _selectedCardAccountId;

    private async Task RenderAccountCardsAsync(List<AccountResponse> accounts)

    {

        if (pnlAccountCards == null) return;

        pnlAccountCards.SuspendLayout();

        pnlAccountCards.Controls.Clear();

        if (accounts == null || accounts.Count == 0)

        {

            // Boş state - Hesap yok

            var emptyLabel = new Label

            {

                Text = "📭 Henüz hesabınız bulunmuyor.\n\n'+ Yeni Hesap Aç' butonunu kullanarak\nilk hesabınızı oluşturabilirsiniz.",

                Font = new Font("Segoe UI", 12, FontStyle.Regular),

                ForeColor = Color.FromArgb(100, 100, 100),

                TextAlign = ContentAlignment.MiddleCenter,

                Dock = DockStyle.Fill

            };

            pnlAccountCards.Controls.Add(emptyLabel);

        }

        else

        {

            foreach (var acc in accounts)

            {

                var card = CreateAccountCard(acc);

                pnlAccountCards.Controls.Add(card);

            }

        }

        pnlAccountCards.ResumeLayout();

        // Son işlemleri yükle

        await LoadRecentTransactionsAsync();

    }

    private System.Windows.Forms.Panel CreateAccountCard(AccountResponse account)

    {

        var currencyColor = account.Currency switch

        {

            "TRY" => Color.FromArgb(25, 118, 210),

            "USD" => Color.FromArgb(76, 175, 80),

            "EUR" => Color.FromArgb(156, 39, 176),

            "GBP" => Color.FromArgb(244, 67, 54),

            _ => Color.FromArgb(100, 100, 100)

        };

        var currencySymbol = account.Currency switch

        {

            "TRY" => "₺",

            "USD" => "$",

            "EUR" => "€",

            "GBP" => "Â£",

            _ => account.Currency

        };

        var card = new System.Windows.Forms.Panel

        {

            Size = new Size(250, 145),

            Margin = new Padding(8),

            BackColor = Color.White,

            BorderStyle = BorderStyle.FixedSingle,

            Cursor = Cursors.Hand,

            Tag = account

        };

        // Üst renk şeridi

        var topStrip = new System.Windows.Forms.Panel

        {

            Dock = DockStyle.Top,

            Height = 6,

            BackColor = currencyColor

        };

        // Para birimi başlık

        var lblCurrency = new Label

        {

            Text = $"{currencySymbol} {account.Currency}",

            Font = new Font("Segoe UI", 11, FontStyle.Bold),

            ForeColor = currencyColor,

            Location = new Point(12, 14),

            AutoSize = true

        };

        // Bakiye

        var lblBalance = new Label

        {

            Text = $"{account.Balance:N2}",

            Font = new Font("Segoe UI", 18, FontStyle.Bold),

            ForeColor = Color.FromArgb(40, 40, 40),

            Location = new Point(12, 38),

            AutoSize = true

        };

        // Kullanılabilir Bakiye

        var available = account.Balance + account.OverdraftLimit;

        var lblAvailable = new Label

        {

            Text = $"Kullanılabilir: {available:N2}",

            Font = new Font("Segoe UI", 9),

            ForeColor = Color.FromArgb(120, 120, 120),

            Location = new Point(12, 70),

            AutoSize = true

        };

        // IBAN (kısa)

        var shortIban = account.Iban.Length > 20 ? account.Iban.Substring(0, 12) + "..." + account.Iban.Substring(account.Iban.Length - 6) : account.Iban;

        var lblIbanShort = new Label

        {

            Text = shortIban,

            Font = new Font("Consolas", 8.5F),

            ForeColor = Color.FromArgb(100, 100, 100),

            Location = new Point(12, 92),

            AutoSize = true

        };

        // Kopyala butonu

        var btnCopy = new Button

        {

            Text = "ğŸ“‹",

            Font = new Font("Segoe UI", 9),

            Size = new Size(30, 24),

            Location = new Point(205, 90),

            FlatStyle = FlatStyle.Flat,

            Cursor = Cursors.Hand,

            Tag = account.Iban

        };

        btnCopy.FlatAppearance.BorderSize = 0;

        btnCopy.Click += (s, e) =>

        {

            Clipboard.SetText(account.Iban);

            XtraMessageBox.Show("IBAN kopyalandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

        };

        // Durum - Status'a göre renk ve metin belirle
        var (statusText, statusColor) = account.Status switch
        {
            "Active" => ("● Aktif", Color.FromArgb(76, 175, 80)),
            "PendingApproval" => ("⏳ Onay Bekliyor", Color.FromArgb(255, 152, 0)),
            "Frozen" => ("❄️ Dondurulmuş", Color.FromArgb(33, 150, 243)),
            "Closed" => ("⛔ Kapalı", Color.FromArgb(244, 67, 54)),
            _ => ("● Aktif", Color.FromArgb(76, 175, 80))
        };

        var lblStatus = new Label

        {

            Text = statusText,

            Font = new Font("Segoe UI", 8),

            ForeColor = statusColor,

            Location = new Point(12, 118),

            AutoSize = true

        };

        card.Controls.AddRange(new Control[] { topStrip, lblCurrency, lblBalance, lblAvailable, lblIbanShort, btnCopy, lblStatus });

        // Kart seçimi

        card.Click += (s, e) => SelectAccountCard(account, card);

        foreach (Control c in card.Controls)

        {

            if (c != btnCopy)

            {

                c.Click += (s, e) => SelectAccountCard(account, card);

            }

        }

        return card;

    }

    private void SelectAccountCard(AccountResponse account, System.Windows.Forms.Panel card)

    {

        _selectedAccount = account;

        _selectedCardAccountId = account.Id;

        Session.SelectedAccountId = account.Id;

        // Tüm kartların UI'ını sıfırla

        if (pnlAccountCards != null)

        {

            foreach (Control c in pnlAccountCards.Controls)

            {

                if (c is System.Windows.Forms.Panel p)

                {

                    p.BackColor = Color.White;

                }

            }

        }

        // Seçili kartı vurgula

        card.BackColor = Color.FromArgb(232, 245, 253);

        // Dropdown'ları güncelle

        RefreshAccountDropdowns();

    }

    private async Task LoadRecentTransactionsAsync()

    {

        if (gridRecentTransactions == null) return;

        try

        {

            var transactions = await _api.GetTransactionsAsync(5);

            if (transactions != null && transactions.Any())

            {

                gridRecentTransactions.DataSource = transactions.Take(5).Select(t => new

                {

                    Tarih = t.CreatedAt.ToString("dd.MM HH:mm"),

                    Tür = t.Type,

                    Tutar = $"{(t.Amount >= 0 ? "+" : "")}{t.Amount:N2}"

                }).ToList();

            }

            else

            {

                // Boş state

                gridRecentTransactions.DataSource = new[] { new { Tarih = "-", Tür = "Henüz işlem yok", Tutar = "-" } };

            }

        }

        catch

        {

            gridRecentTransactions.DataSource = new[] { new { Tarih = "-", Tür = "Yüklenemedi", Tutar = "-" } };

        }

    }

    // ========== HIZLI AKSİYON BUTONLARI ==========

    private void BtnQuickDeposit_Click(object? sender, EventArgs e)

    {

        tabs.SelectedTabPage = tabDw;

        if (_selectedCardAccountId.HasValue && cmbDwAccount != null)

        {

            cmbDwAccount.EditValue = _selectedCardAccountId.Value;

        }

    }

    private void BtnQuickWithdraw_Click(object? sender, EventArgs e)

    {

        tabs.SelectedTabPage = tabDw;

        if (_selectedCardAccountId.HasValue && cmbDwAccount != null)

        {

            cmbDwAccount.EditValue = _selectedCardAccountId.Value;

        }

    }

    private void BtnQuickTransfer_Click(object? sender, EventArgs e)

    {

        tabs.SelectedTabPage = tabTransfer;

        if (_selectedCardAccountId.HasValue && cmbTransferAccount != null)

        {

            var index = _cachedAccounts.FindIndex(a => a.Id == _selectedCardAccountId.Value);

            if (index >= 0) cmbTransferAccount.SelectedIndex = index;

        }

    }

    private void BtnQuickFx_Click(object? sender, EventArgs e)

    {

        tabs.SelectedTabPage = tabExchangeRates;

    }

    // ========== YENİ HESAP AÇMA DİALOGU ==========

    private async void btnCreateAccount_Click(object? sender, EventArgs e)

    {

        try

        {

            // Hesap tipi seçenekleri: TRY (Vadesiz) veya Döviz Hesabı

            var accountTypes = new[] { "TRY - Türk Lirası (Vadesiz Hesap)", "Döviz Hesabı (USD/EUR/GBP)" };

            var foreignCurrencies = new[] { "USD - Amerikan Doları", "EUR - Euro", "GBP - İngiliz Sterlini" };

            using var form = new System.Windows.Forms.Form

            {

                Text = "Yeni Hesap Aç",

                Size = new System.Drawing.Size(400, 320),

                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,

                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,

                MaximizeBox = false,

                MinimizeBox = false

            };

            var lblTitle = new System.Windows.Forms.Label

            {

                Text = "💳 Yeni Hesap Oluştur",

                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),

                ForeColor = System.Drawing.Color.FromArgb(25, 118, 210),

                Location = new System.Drawing.Point(20, 20),

                AutoSize = true

            };

            var lblType = new System.Windows.Forms.Label

            {

                Text = "Hesap Tipi:",

                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),

                Location = new System.Drawing.Point(20, 60),

                AutoSize = true

            };

            var cmbType = new System.Windows.Forms.ComboBox

            {

                Location = new System.Drawing.Point(20, 85),

                Size = new System.Drawing.Size(340, 30),

                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,

                Font = new System.Drawing.Font("Segoe UI", 10)

            };

            cmbType.Items.AddRange(accountTypes);

            cmbType.SelectedIndex = 0;

            var lblCur = new System.Windows.Forms.Label

            {

                Text = "Döviz Cinsi:",

                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),

                Location = new System.Drawing.Point(20, 125),

                AutoSize = true,

                Visible = false

            };

            var cmbCur = new System.Windows.Forms.ComboBox

            {

                Location = new System.Drawing.Point(20, 150),

                Size = new System.Drawing.Size(340, 30),

                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,

                Font = new System.Drawing.Font("Segoe UI", 10),

                Visible = false

            };

            cmbCur.Items.AddRange(foreignCurrencies);

            cmbCur.SelectedIndex = 0;

            var lblOd = new System.Windows.Forms.Label

            {

                Text = "Ek Hesap Limiti (Opsiyonel):",

                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),

                Location = new System.Drawing.Point(20, 125),

                AutoSize = true

            };

            var txtOd = new System.Windows.Forms.TextBox

            {

                Location = new System.Drawing.Point(20, 150),

                Size = new System.Drawing.Size(340, 30),

                Font = new System.Drawing.Font("Segoe UI", 10),

                Text = "0"

            };

            var btnOk = new System.Windows.Forms.Button

            {

                Text = "✓ Hesap Oluştur",

                Location = new System.Drawing.Point(20, 195),

                Size = new System.Drawing.Size(160, 35),

                BackColor = System.Drawing.Color.FromArgb(76, 175, 80),

                ForeColor = System.Drawing.Color.White,

                FlatStyle = System.Windows.Forms.FlatStyle.Flat,

                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),

                DialogResult = System.Windows.Forms.DialogResult.OK

            };

            var btnCancel = new System.Windows.Forms.Button

            {

                Text = "İptal",

                Location = new System.Drawing.Point(200, 195),

                Size = new System.Drawing.Size(160, 35),

                FlatStyle = System.Windows.Forms.FlatStyle.Flat,

                Font = new System.Drawing.Font("Segoe UI", 10),

                DialogResult = System.Windows.Forms.DialogResult.Cancel

            };

            // Hesap tipi değiştiğinde döviz seçimini göster/gizle

            cmbType.SelectedIndexChanged += (s, ev) =>

            {

                var isDoviz = cmbType.SelectedIndex == 1;

                lblCur.Visible = isDoviz;

                cmbCur.Visible = isDoviz;

                lblOd.Location = new System.Drawing.Point(20, isDoviz ? 190 : 125);

                txtOd.Location = new System.Drawing.Point(20, isDoviz ? 215 : 150);

                btnOk.Location = new System.Drawing.Point(20, isDoviz ? 260 : 195);

                btnCancel.Location = new System.Drawing.Point(200, isDoviz ? 260 : 195);

                form.Size = new System.Drawing.Size(400, isDoviz ? 320 : 260);

            };

            form.Controls.AddRange(new System.Windows.Forms.Control[] { lblTitle, lblType, cmbType, lblCur, cmbCur, lblOd, txtOd, btnOk, btnCancel });

            form.AcceptButton = btnOk;

            form.CancelButton = btnCancel;

            // Başlangıç boyutunu ayarla

            form.Size = new System.Drawing.Size(400, 260);

            if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)

            {

                string currencyCode;

                if (cmbType.SelectedIndex == 0)

                {

                    // TRY Hesabı

                    currencyCode = "TRY";

                }

                else

                {

                    // Döviz Hesabı

                    currencyCode = cmbCur.SelectedItem?.ToString()?.Split('-')[0].Trim() ?? "USD";

                }

                decimal.TryParse(txtOd.Text, out var overdraft);

                var custId = Session.CurrentCustomerId ?? Guid.Empty;

                var random = new Random();

                var accNo = random.Next(100000, 999999);

                var currency = Enum.Parse<NovaBank.Core.Enums.Currency>(currencyCode);

                var request = new CreateAccountRequest(custId, accNo, currency, overdraft);

                var resp = await _api.CreateAccountAsync(request);

                if (resp.IsSuccessStatusCode)

                {

                    string msg;
                    string title;
                    MessageBoxIcon icon;

                    if (cmbType.SelectedIndex == 0)
                    {
                        // TRY hesabı - hemen aktif
                        msg = $"✓ TL vadesiz hesabınız başarıyla oluşturuldu!";
                        title = "Başarılı";
                        icon = MessageBoxIcon.Information;
                    }
                    else
                    {
                        // Döviz hesabı - onay bekliyor
                        msg = $"⏳ {currencyCode} döviz hesap talebiniz alındı!\n\n" +
                              $"Hesabınız admin onayından sonra aktif olacaktır.\n" +
                              $"Onaylandığında Döviz Kurları ekranından {currencyCode} alım/satım yapabileceksiniz.";
                        title = "Onay Bekleniyor";
                        icon = MessageBoxIcon.Information;
                    }

                    XtraMessageBox.Show(msg, title, System.Windows.Forms.MessageBoxButtons.OK, icon);

                    await LoadAccounts();

                    // Döviz hesabı açıldıysa döviz kurları dropdown'larını da güncelle

                    if (cmbType.SelectedIndex == 1)

                    {

                        await LoadFxAccountDropdowns();

                    }

                }

                else

                {

                    var error = await ApiClient.GetErrorMessageAsync(resp);

                    XtraMessageBox.Show($"Hesap oluşturulamadı:\n{error}", "Hata", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                }

            }

        }

        catch (Exception ex)

        {

            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

        }

    }

    // ========== SOL SIDEBAR ==========
    
    private const int SIDEBAR_WIDTH = 180; // Sabit sidebar genişliği
    
    private void CreateSidebar()
    {
        // Sidebar panel - Sabit genişlik, koyu mavi renk
        pnlSidebar = new PanelControl()
        {
            Location = new Point(0, 0),
            Size = new Size(SIDEBAR_WIDTH, this.Height - (statusStrip?.Height ?? 0)),
            Appearance = { BackColor = Color.FromArgb(25, 55, 95), BorderColor = Color.FromArgb(35, 70, 120) }
        };
        pnlSidebar.LookAndFeel.UseDefaultLookAndFeel = false;
        pnlSidebar.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        
        // Tooltip artık kullanılmayacak ama null hatası vermesin diye oluşturalım
        lblSidebarTooltip = new LabelControl() { Visible = false };
        this.Controls.Add(lblSidebarTooltip);
        
        // Menü öğeleri
        int yPos = 15;
        const int itemHeight = 45;
        const int spacing = 5;
        
        btnSidebarAccounts = CreateSidebarButton("🏦", "Hesaplarım", yPos, tabMyAccounts);
        yPos += itemHeight + spacing;
        
        btnSidebarMoneyOps = CreateSidebarButton("💰", "Para İşlemleri", yPos, tabDw);
        yPos += itemHeight + spacing;
        
        btnSidebarTransfer = CreateSidebarButton("↔", "Transfer", yPos, tabTransfer);
        yPos += itemHeight + spacing;
        
        btnSidebarCards = CreateSidebarButton("💳", "Kartlarım", yPos, tabCards);
        yPos += itemHeight + spacing;
        
        btnSidebarBills = CreateSidebarButton("📄", "Fatura Öde", yPos, tabBills);
        yPos += itemHeight + spacing;
        
        btnSidebarStatements = CreateSidebarButton("📊", "Ekstreler", yPos, tabReports);
        yPos += itemHeight + spacing;
        
        btnSidebarFx = CreateSidebarButton("💱", "Döviz Kurları", yPos, tabExchangeRates);
        yPos += itemHeight + spacing;
        
        btnSidebarSettings = CreateSidebarButton("⚙", "Ayarlar", yPos, tabSettings);
        
        // Admin/BranchManager butonu (Admin veya Şube Yöneticisi için)
        if (Session.IsAdminOrBranchManager)
        {
            yPos += itemHeight + spacing;
            var adminLabel = Session.IsAdmin ? "Yönetim" : "Şube Yönetim";
            btnSidebarAdmin = CreateSidebarButton("🔧", adminLabel, yPos, tabAdmin);
        }
        
        this.Controls.Add(pnlSidebar);
        pnlSidebar.BringToFront();
        
        // Ana içerik alanını ayarla
        UpdateMainContentPosition(SIDEBAR_WIDTH);
    }
    
    private SimpleButton CreateSidebarButton(string icon, string text, int yPos, XtraTabPage targetTab)
    {
        var btn = new SimpleButton()
        {
            Location = new Point(5, yPos),
            Size = new Size(SIDEBAR_WIDTH - 10, 42),
            Text = $"{icon}  {text}",
            Appearance = { 
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Near },
                Options = { UseForeColor = true, UseBackColor = true }
            },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat },
            Padding = new Padding(10, 0, 0, 0)
        };
        btn.AppearanceHovered.BackColor = Color.FromArgb(40, 80, 140);
        btn.AppearanceHovered.ForeColor = Color.White;
        btn.AppearancePressed.BackColor = Color.FromArgb(20, 60, 110);
        
        // Click event - tab değiştir
        btn.Click += (s, e) => {
            if (targetTab != null && tabs != null)
            {
                tabs.SelectedTabPage = targetTab;
                UpdateActiveSidebarButton(btn);
            }
        };
        
        pnlSidebar.Controls.Add(btn);
        return btn;
    }
    
    // Artık kullanılmayan ExpandSidebar ve CollapseSidebar metodları kaldırıldı
    
    private void UpdateActiveSidebarButton(SimpleButton? activeBtn)
    {
        // pnlSidebar null ise çık
        if (pnlSidebar == null) return;
        
        // Tüm butonları normal yap
        foreach (Control ctrl in pnlSidebar.Controls)
        {
            if (ctrl is SimpleButton btn)
            {
                btn.Appearance.BackColor = Color.Transparent;
            }
        }
        
        // Aktif butonu vurgula
        if (activeBtn != null)
        {
            activeBtn.Appearance.BackColor = Color.FromArgb(13, 71, 161);
        }
    }
    
    private void UpdateMainContentPosition(int sidebarWidth)
    {
        if (tabs != null)
        {
            tabs.Location = new Point(sidebarWidth, 0);
            tabs.Size = new Size(this.Width - sidebarWidth, this.Height - statusStrip.Height);
        }
    }
    
    // InitializeChatbot artık gerekli değil - ChatbotPanel kendi içinde AI servisini yönetiyor

}

