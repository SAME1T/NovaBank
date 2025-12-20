using NovaBank.WinForms.Services;
using NovaBank.WinForms.Dto;
using NovaBank.Api.Contracts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System.Windows.Forms;

namespace NovaBank.WinForms;

public partial class FrmMain : XtraForm
{
    private readonly ApiClient _api = new();
    private readonly Guid? _currentCustomerId;
    private AccountResponse? _selectedAccount;
    private bool _isLogoutFlow = false;
    public FrmMain(Guid? currentCustomerId = null) 
    { 
        _currentCustomerId = currentCustomerId;
        InitializeComponent(); 
        this.Text = $"NovaBank Client  •  {_api.BaseUrl}" + (currentCustomerId.HasValue ? $" • Müşteri: {currentCustomerId}" : ""); 
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
        if (_currentCustomerId.HasValue && text.Length <= 8)
        {
            id = _currentCustomerId.Value;
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
        cmbDwCurrency.Properties.Items.AddRange(Enum.GetValues(typeof(NovaBank.Core.Enums.Currency)));
        cmbTransCurrency.Properties.Items.AddRange(Enum.GetValues(typeof(NovaBank.Core.Enums.Currency)));
        cmbCurrency.EditValue = NovaBank.Core.Enums.Currency.TRY;
        cmbDwCurrency.EditValue = NovaBank.Core.Enums.Currency.TRY;
        cmbTransCurrency.EditValue = NovaBank.Core.Enums.Currency.TRY;

        // Eğer giriş yapılmışsa müşteri bilgilerini prefill et
        if (_currentCustomerId.HasValue)
        {
            txtAccCustomerId.Text = _currentCustomerId.Value.ToString("N")[..8]; // İlk 8 karakter
            txtStmtAccountId.Text = "";

            // Müşteri bilgilerini yükle
            await LoadCustomerInfo();
            
            // Hesapları yükle
            await LoadAccounts();
        }
    }

    private async Task LoadCustomerInfo()
    {
        try
        {
            var customer = await _api.GetAsync<CustomerResponse>($"/api/v1/customers/{_currentCustomerId.Value}");
            if (customer != null)
            {
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
            var list = await _api.GetAsync<List<AccountResponse>>($"/api/v1/accounts/by-customer/{_currentCustomerId.Value}");
            if (list != null && gridAccounts != null)
            {
                gridAccounts.DataSource = list;
                
                // Gizlenecek kolonları ayarla
                if (gridAccountsView.Columns["Id"] != null) gridAccountsView.Columns["Id"].Visible = false;
                if (gridAccountsView.Columns["CustomerId"] != null) gridAccountsView.Columns["CustomerId"].Visible = false;
                if (gridAccountsView.Columns["AccountNo"] != null) gridAccountsView.Columns["AccountNo"].Visible = false;
                
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
                        gridAccountsView.FocusedRowHandle = 0;
                        cmbTransferAccount.SelectedIndex = 0;
                        BindSenderSummary();
                    }
                }
                else
                {
                    // Varsayılan seçili hesap (ComboBox yoksa)
                    if (list.Count > 0)
                    {
                        _selectedAccount = list[0];
                        gridAccountsView.FocusedRowHandle = 0;
                        BindSenderSummary();
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
            lblSenderBind.Text = $"📤 {_selectedAccount.Iban} - {_selectedAccount.Currency} | Bakiye: {_selectedAccount.Balance:N2}";
    }

    private void CmbTransferAccount_EditValueChanged(object sender, EventArgs e)
    {
        try
        {
            if (cmbTransferAccount == null || cmbTransferAccount.SelectedIndex < 0) return;
            
            // Hesapları tekrar al (veya cache'den kullan)
            var list = gridAccounts.DataSource as List<AccountResponse>;
            if (list == null || list.Count == 0) return;
            
            var selectedIndex = cmbTransferAccount.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < list.Count)
            {
                _selectedAccount = list[selectedIndex];
                BindSenderSummary();
                
                // Para birimini otomatik ayarla
                if (_selectedAccount != null && cmbTransCurrency != null)
                {
                    cmbTransCurrency.EditValue = _selectedAccount.Currency;
                }
                
                // Grid'de de seçili yap
                if (gridAccountsView != null)
                {
                    gridAccountsView.FocusedRowHandle = selectedIndex;
                }
            }
        }
        catch (Exception ex)
        {
            // Hata durumunda sessizce devam et
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
            var custId = _currentCustomerId ?? Guid.Empty;
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
            
            var account = _selectedAccount;
            if (account == null) { XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var req = new DepositRequest(account.Id, amt, (NovaBank.Core.Enums.Currency)cmbDwCurrency.EditValue!, txtDepositDesc.Text ?? "");
            var confirm = XtraMessageBox.Show($"{amt:N2} {cmbDwCurrency.EditValue} yatırılacak. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            var resp = await _api.PostAsync("/api/v1/transactions/deposit", req);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"Para yatırma işlemi başarılı!\nTutar: {amt:N2} {cmbDwCurrency.EditValue}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Para yatırma işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { XtraMessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnWithdraw_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryDec(txtWithdrawAmount.Text, out var amt2, "Tutar")) return;
            
            var account = _selectedAccount;
            if (account == null) { XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var req = new WithdrawRequest(account.Id, amt2, (NovaBank.Core.Enums.Currency)cmbDwCurrency.EditValue!, txtWithdrawDesc.Text ?? "");
            var confirm = XtraMessageBox.Show($"{amt2:N2} {cmbDwCurrency.EditValue} çekilecek. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            var resp = await _api.PostAsync("/api/v1/transactions/withdraw", req);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"Para çekme işlemi başarılı!\nTutar: {amt2:N2} {cmbDwCurrency.EditValue}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Para çekme işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { XtraMessageBox.Show(ex.Message, "Error"); }
    }

    private void btnSelectAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            // Hesaplarım sayfasındaki hesaplardan birini seç
            if (gridAccountsView.SelectedRowsCount > 0)
            {
                var row = gridAccountsView.GetSelectedRows()[0];
                _selectedAccount = gridAccountsView.GetRow(row) as AccountResponse;
                BindSenderSummary();
                if (_selectedAccount != null)
                    XtraMessageBox.Show($"Gönderen hesap seçildi: {_selectedAccount.Iban}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show("Lütfen hesaplarım sayfasından bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex) { XtraMessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnExternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtToIban.Text)) { XtraMessageBox.Show("Alıcı IBAN zorunludur.", "Uyarı"); return; }
            if (!TryDec(txtAmount.Text, out var tamt2, "Tutar")) return;
            
            var fromAccount = _selectedAccount;
            if (fromAccount == null) { XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var req = new TransferExternalRequest(fromAccount.Id, txtToIban.Text.Trim(), tamt2, (NovaBank.Core.Enums.Currency)cmbTransCurrency.EditValue!, txtTransDesc.Text ?? "");
            var confirm = XtraMessageBox.Show($"{tamt2:N2} {cmbTransCurrency.EditValue} tutarında transfer yapılacak. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            var resp = await _api.PostAsync("/api/v1/transfers/external", req);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show($"Transfer işlemi başarılı!\nTutar: {tamt2:N2} {cmbTransCurrency.EditValue}\nAlıcı IBAN: {txtToIban.Text}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                XtraMessageBox.Show($"Transfer işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { XtraMessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnGetStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            var account = _selectedAccount;
            if (account == null) { XtraMessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var fromLocal = dtFrom.DateTime.Date;
            var toLocal   = dtTo.DateTime.Date.AddDays(1).AddTicks(-1);
            if (fromLocal > toLocal) { XtraMessageBox.Show("Bitiş tarihi başlangıçtan küçük olamaz", "Uyarı"); return; }
            var from = DateTime.SpecifyKind(fromLocal, DateTimeKind.Local).ToUniversalTime();
            var to   = DateTime.SpecifyKind(toLocal, DateTimeKind.Local).ToUniversalTime();
            var url = $"/api/v1/reports/account-statement?accountId={account.Id}&from={from:O}&to={to:O}";
            var stmt = await _api.GetAsync<AccountStatementResponse>(url);
            if (stmt is null) { XtraMessageBox.Show("Kayıt bulunamadı"); return; }
            gridStatement.DataSource = stmt.Items.ToList();
            lblTotals.Text = $"Açılış: {stmt.OpeningBalance}  Alacak: {stmt.TotalCredit}  Borç: {stmt.TotalDebit}  Kapanış: {stmt.ClosingBalance}";
        }
        catch (Exception ex) { XtraMessageBox.Show(ex.Message, "Error"); }
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
            
            // Kolon başlıklarını düzenle ve formatla
            if (dgvRates.Columns["CurrencyCode"] != null)
            {
                dgvRates.Columns["CurrencyCode"].HeaderText = "Kod";
                dgvRates.Columns["CurrencyCode"].Width = 80;
            }
            if (dgvRates.Columns["CurrencyName"] != null)
            {
                dgvRates.Columns["CurrencyName"].HeaderText = "Döviz";
                dgvRates.Columns["CurrencyName"].Width = 200;
            }
            if (dgvRates.Columns["Unit"] != null)
            {
                dgvRates.Columns["Unit"].HeaderText = "Birim";
                dgvRates.Columns["Unit"].Width = 60;
                dgvRates.Columns["Unit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvRates.Columns["ForexBuying"] != null)
            {
                dgvRates.Columns["ForexBuying"].HeaderText = "Döviz Alış";
                dgvRates.Columns["ForexBuying"].Width = 120;
                dgvRates.Columns["ForexBuying"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["ForexBuying"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvRates.Columns["ForexSelling"] != null)
            {
                dgvRates.Columns["ForexSelling"].HeaderText = "Döviz Satış";
                dgvRates.Columns["ForexSelling"].Width = 120;
                dgvRates.Columns["ForexSelling"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["ForexSelling"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvRates.Columns["BanknoteBuying"] != null)
            {
                dgvRates.Columns["BanknoteBuying"].HeaderText = "Efektif Alış";
                dgvRates.Columns["BanknoteBuying"].Width = 120;
                dgvRates.Columns["BanknoteBuying"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["BanknoteBuying"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvRates.Columns["BanknoteSelling"] != null)
            {
                dgvRates.Columns["BanknoteSelling"].HeaderText = "Efektif Satış";
                dgvRates.Columns["BanknoteSelling"].Width = 120;
                dgvRates.Columns["BanknoteSelling"].DefaultCellStyle.Format = "N4";
                dgvRates.Columns["BanknoteSelling"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
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
}
