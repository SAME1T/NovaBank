using NovaBank.WinForms.Services;
using NovaBank.Api.Contracts;

namespace NovaBank.WinForms;

public partial class FrmMain : Form
{
    private readonly ApiClient _api = new();
    private readonly Guid? _currentCustomerId;
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
            txtDwAccountId.Text = "";
            txtFromId.Text = "";
            txtToId.Text = "";
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
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hesaplar yüklenirken hata: {ex.Message}", "Uyarı");
        }
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
            if (!TryGuidFromShort(txtAccCustomerId.Text, out var custId)) return;
            if (!long.TryParse(txtAccountNo.Text, out var accNo)) { MessageBox.Show("Hesap No sayısal olmalıdır.", "Uyarı"); return; }
            if (!TryDec(txtOverdraft.Text, out var od, "Ek Hesap Limiti")) return;
            var req = new CreateAccountRequest(
                custId,
                accNo,
                (NovaBank.Core.Enums.Currency)cmbCurrency.SelectedItem!,
                od
            );
            var resp = await _api.PostAsync("/api/v1/accounts", req);
            if (!resp.IsSuccessStatusCode) { MessageBox.Show(await resp.Content.ReadAsStringAsync(), "Error"); return; }
            MessageBox.Show("Hesap oluşturuldu. IBAN otomatik oluşturuldu.", "OK");
            // Hesapları yenile
            await LoadAccounts();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnDeposit_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryAccountNo(txtDwAccountId.Text, out var accountNo)) return;
            if (!TryDec(txtDepositAmount.Text, out var amt, "Tutar")) return;
            
            // Hesap numarasından hesap ID'sini bul
            var account = await FindAccountByNumber(accountNo);
            if (account == null) { MessageBox.Show("Hesap bulunamadı.", "Uyarı"); return; }
            
            var req = new DepositRequest(account.Id, amt, (NovaBank.Core.Enums.Currency)cmbDwCurrency.SelectedItem!, txtDepositDesc.Text);
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
            if (!TryAccountNo(txtDwAccountId.Text, out var accountNo)) return;
            if (!TryDec(txtWithdrawAmount.Text, out var amt2, "Tutar")) return;
            
            // Hesap numarasından hesap ID'sini bul
            var account = await FindAccountByNumber(accountNo);
            if (account == null) { MessageBox.Show("Hesap bulunamadı.", "Uyarı"); return; }
            
            var req = new WithdrawRequest(account.Id, amt2, (NovaBank.Core.Enums.Currency)cmbDwCurrency.SelectedItem!, txtWithdrawDesc.Text);
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

    private async void btnInternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryAccountNo(txtFromId.Text, out var fromAccountNo)) return;
            if (!TryAccountNo(txtToId.Text, out var toAccountNo)) return;
            if (!TryDec(txtAmount.Text, out var tamt, "Tutar")) return;
            
            // Hesap numaralarından hesap ID'lerini bul
            var fromAccount = await FindAccountByNumber(fromAccountNo);
            var toAccount = await FindAccountByNumber(toAccountNo);
            if (fromAccount == null) { MessageBox.Show("Gönderen hesap bulunamadı.", "Uyarı"); return; }
            if (toAccount == null) { MessageBox.Show("Alıcı hesap bulunamadı.", "Uyarı"); return; }
            
            var req = new TransferInternalRequest(fromAccount.Id, toAccount.Id, tamt, (NovaBank.Core.Enums.Currency)cmbTransCurrency.SelectedItem!, txtTransDesc.Text);
            var resp = await _api.PostAsync("/api/v1/transfers/internal", req);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show($"İç transfer işlemi başarılı!\nTutar: {tamt:N2} {cmbTransCurrency.SelectedItem}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"İç transfer işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnExternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryAccountNo(txtFromId.Text, out var fromAccountNo)) return;
            if (string.IsNullOrWhiteSpace(txtToIban.Text)) { MessageBox.Show("Alıcı IBAN zorunludur.", "Uyarı"); return; }
            if (!TryDec(txtAmount.Text, out var tamt2, "Tutar")) return;
            
            // Hesap numarasından hesap ID'sini bul
            var fromAccount = await FindAccountByNumber(fromAccountNo);
            if (fromAccount == null) { MessageBox.Show("Gönderen hesap bulunamadı.", "Uyarı"); return; }
            
            var req = new TransferExternalRequest(fromAccount.Id, txtToIban.Text, tamt2, (NovaBank.Core.Enums.Currency)cmbTransCurrency.SelectedItem!, txtTransDesc.Text);
            var resp = await _api.PostAsync("/api/v1/transfers/external", req);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show($"EFT/FAST işlemi başarılı!\nTutar: {tamt2:N2} {cmbTransCurrency.SelectedItem}\nIBAN: {txtToIban.Text}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccounts(); // Hesapları yenile
            }
            else
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"EFT/FAST işlemi başarısız!\nHata: {errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnGetStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryAccountNo(txtStmtAccountId.Text, out var accountNo)) return;
            
            // Hesap numarasından hesap ID'sini bul
            var account = await FindAccountByNumber(accountNo);
            if (account == null) { MessageBox.Show("Hesap bulunamadı.", "Uyarı"); return; }
            
            var from = dtFrom.Value.Date;
            var to   = dtTo.Value.Date.AddDays(1).AddTicks(-1);
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

    private void MnuLogout_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("Çıkış yapmak istediğinizden emin misiniz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            this.Hide();
            var loginForm = new FrmAuth();
            loginForm.Show();
            this.Close();
        }
    }
}
