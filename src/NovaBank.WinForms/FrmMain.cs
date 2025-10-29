using NovaBank.WinForms.Services;
using NovaBank.Api.Contracts;

namespace NovaBank.WinForms;

public partial class FrmMain : Form
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
            MessageBox.Show("Geçerli bir GUID giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            MessageBox.Show("Geçerli bir hesap numarası giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private bool TryDec(string text, out decimal val, string alanAdi)
    {
        if (!decimal.TryParse(text, out val))
        {
            MessageBox.Show($"{alanAdi} sayısal olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private async void FrmMain_Load(object? sender, EventArgs e)
    {
        cmbCurrency.DataSource = Enum.GetValues(typeof(NovaBank.Core.Enums.Currency));
        cmbDwCurrency.DataSource = Enum.GetValues(typeof(NovaBank.Core.Enums.Currency));
        cmbTransCurrency.DataSource = Enum.GetValues(typeof(NovaBank.Core.Enums.Currency));
        cmbCurrency.SelectedItem = NovaBank.Core.Enums.Currency.TRY;
        cmbDwCurrency.SelectedItem = NovaBank.Core.Enums.Currency.TRY;
        cmbTransCurrency.SelectedItem = NovaBank.Core.Enums.Currency.TRY;

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
                lblWelcome.Text = $"Hoş Geldiniz, {customer.FirstName} {customer.LastName}";
                lblStatus.Text = $"Giriş yapıldı: {customer.FirstName} {customer.LastName} | {DateTime.Now:dd.MM.yyyy HH:mm}";
                if (lblProfName != null)
                {
                    lblProfName.Text = $"Ad Soyad: {customer.FirstName} {customer.LastName}";
                    lblProfNationalId.Text = $"TCKN: {customer.NationalId}";
                    lblProfEmail.Text = $"E-posta: {customer.Email ?? "-"}";
                    lblProfPhone.Text = $"Telefon: {customer.Phone ?? "-"}";
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Müşteri bilgileri yüklenirken hata: {ex.Message}", "Uyarı");
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
                
                // Toplam bakiye hesapla
                var totalBalance = list.Sum(a => a.Balance);
                lblTotalBalance.Text = $"Toplam Bakiye: {totalBalance:N2} TL";
                lblAccountCount.Text = $"Hesap Sayısı: {list.Count}";

                // Varsayılan seçili hesap
                if (list.Count > 0)
                {
                    _selectedAccount = list[0];
                    gridAccounts.ClearSelection();
                    gridAccounts.Rows[0].Selected = true;
                    BindSenderSummary();
                }
                gridAccounts.SelectionChanged -= GridAccounts_SelectionChanged;
                gridAccounts.SelectionChanged += GridAccounts_SelectionChanged;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hesaplar yüklenirken hata: {ex.Message}", "Uyarı");
        }
    }

    private void GridAccounts_SelectionChanged(object? sender, EventArgs e)
    {
        if (gridAccounts.SelectedRows.Count > 0)
        {
            _selectedAccount = gridAccounts.SelectedRows[0].DataBoundItem as AccountResponse;
            BindSenderSummary();
        }
    }

    private void BindSenderSummary()
    {
        if (_selectedAccount == null) return;
        // Designer'da oluşturulan label adı: lblSenderBind
        if (lblSenderBind != null)
            lblSenderBind.Text = $"Gönderen: {_selectedAccount.Iban} ({_selectedAccount.Currency})";
    }

    private void GridAccounts_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && gridAccounts.Rows[e.RowIndex].DataBoundItem is AccountResponse account)
        {
            // IBAN'ı panoya kopyala
            Clipboard.SetText(account.Iban);
            MessageBox.Show($"IBAN kopyalandı: {account.Iban}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void btnCreateAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            var custId = _currentCustomerId ?? Guid.Empty;
            if (custId == Guid.Empty) { MessageBox.Show("Müşteri bulunamadı (giriş gerekli)", "Uyarı"); return; }
            if (!TryDec(txtOverdraft.Text, out var od, "Ek Hesap Limiti")) return;
            
            // Hesap numarasını otomatik oluştur (rastgele)
            var random = new Random();
            var accNo = random.Next(100000, 999999);
            
            var req = new CreateAccountRequest(
                custId,
                accNo,
                (NovaBank.Core.Enums.Currency)cmbCurrency.SelectedItem!,
                od
            );
            var resp = await _api.PostAsync("/api/v1/accounts", req);
            if (!resp.IsSuccessStatusCode) { MessageBox.Show(await resp.Content.ReadAsStringAsync(), "Error"); return; }
            MessageBox.Show($"Hesap oluşturuldu!\nHesap No: {accNo}\nIBAN otomatik oluşturuldu.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Hesapları yenile
            await LoadAccounts();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnDeposit_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryDec(txtDepositAmount.Text, out var amt, "Tutar")) return;
            
            var account = _selectedAccount;
            if (account == null) { MessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var req = new DepositRequest(account.Id, amt, (NovaBank.Core.Enums.Currency)cmbDwCurrency.SelectedItem!, txtDepositDesc.Text);
            var confirm = MessageBox.Show($"{amt:N2} {cmbDwCurrency.SelectedItem} yatırılacak. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            var resp = await _api.PostAsync("/api/v1/transactions/deposit", req);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show($"Para yatırma işlemi başarılı!\nTutar: {amt:N2} {cmbDwCurrency.SelectedItem}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Para yatırma işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnWithdraw_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryDec(txtWithdrawAmount.Text, out var amt2, "Tutar")) return;
            
            var account = _selectedAccount;
            if (account == null) { MessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var req = new WithdrawRequest(account.Id, amt2, (NovaBank.Core.Enums.Currency)cmbDwCurrency.SelectedItem!, txtWithdrawDesc.Text);
            var confirm = MessageBox.Show($"{amt2:N2} {cmbDwCurrency.SelectedItem} çekilecek. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            var resp = await _api.PostAsync("/api/v1/transactions/withdraw", req);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show($"Para çekme işlemi başarılı!\nTutar: {amt2:N2} {cmbDwCurrency.SelectedItem}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Para çekme işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private void btnSelectAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            // Hesaplarım sayfasındaki hesaplardan birini seç
            if (gridAccounts.SelectedRows.Count > 0)
            {
                _selectedAccount = gridAccounts.SelectedRows[0].DataBoundItem as AccountResponse;
                BindSenderSummary();
                if (_selectedAccount != null)
                    MessageBox.Show($"Gönderen hesap seçildi: {_selectedAccount.Iban}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Lütfen hesaplarım sayfasından bir hesap seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnExternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtToIban.Text)) { MessageBox.Show("Alıcı IBAN zorunludur.", "Uyarı"); return; }
            if (!TryDec(txtAmount.Text, out var tamt2, "Tutar")) return;
            
            var fromAccount = _selectedAccount;
            if (fromAccount == null) { MessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var req = new TransferExternalRequest(fromAccount.Id, txtToIban.Text.Trim(), tamt2, (NovaBank.Core.Enums.Currency)cmbTransCurrency.SelectedItem!, txtTransDesc.Text);
            var confirm = MessageBox.Show($"{tamt2:N2} {cmbTransCurrency.SelectedItem} tutarında transfer yapılacak. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            var resp = await _api.PostAsync("/api/v1/transfers/external", req);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show($"Transfer işlemi başarılı!\nTutar: {tamt2:N2} {cmbTransCurrency.SelectedItem}\nAlıcı IBAN: {txtToIban.Text}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Transfer işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnGetStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            var account = _selectedAccount;
            if (account == null) { MessageBox.Show("Lütfen bir hesap seçin.", "Uyarı"); return; }
            
            var fromLocal = dtFrom.Value.Date;
            var toLocal   = dtTo.Value.Date.AddDays(1).AddTicks(-1);
            if (fromLocal > toLocal) { MessageBox.Show("Bitiş tarihi başlangıçtan küçük olamaz", "Uyarı"); return; }
            var from = DateTime.SpecifyKind(fromLocal, DateTimeKind.Local).ToUniversalTime();
            var to   = DateTime.SpecifyKind(toLocal, DateTimeKind.Local).ToUniversalTime();
            var url = $"/api/v1/reports/account-statement?accountId={account.Id}&from={from:O}&to={to:O}";
            var stmt = await _api.GetAsync<AccountStatementResponse>(url);
            if (stmt is null) { MessageBox.Show("Kayıt bulunamadı"); return; }
            gridStatement.DataSource = stmt.Items.ToList();
            lblTotals.Text = $"Açılış: {stmt.OpeningBalance}  Alacak: {stmt.TotalCredit}  Borç: {stmt.TotalDebit}  Kapanış: {stmt.ClosingBalance}";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
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

    private void MnuLogout_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("Çıkış yapıp farklı kullanıcıyla giriş yapmak ister misiniz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

    private void FrmMain_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Kullanıcı X ile kapatırsa uygulamayı tamamen kapat
        if (!_isLogoutFlow && e.CloseReason == CloseReason.UserClosing)
        {
            System.Windows.Forms.Application.Exit();
        }
    }

    private async void TxtToIban_Leave(object? sender, EventArgs e)
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
}
