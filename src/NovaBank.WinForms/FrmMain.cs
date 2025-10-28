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
            txtAccCustomerId.Text = _currentCustomerId.Value.ToString();
            txtDwAccountId.Text = "";
            txtFromId.Text = "";
            txtToId.Text = "";
            txtStmtAccountId.Text = "";

            // Hesapları yükle
            try
            {
                var list = await _api.GetAsync<List<AccountResponse>>($"/api/v1/accounts/by-customer/{_currentCustomerId.Value}");
                if (list != null && gridAccounts != null)
                {
                    gridAccounts.DataSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hesaplar yüklenirken hata: {ex.Message}", "Uyarı");
            }
        }
    }

    private async void btnCreateCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtSurname.Text)) { MessageBox.Show("Ad ve Soyad boş olamaz.", "Uyarı"); return; }
            var req = new CreateCustomerRequest(txtTc.Text, txtName.Text, txtSurname.Text, txtEmail.Text, txtPhone.Text, "123456");
            var resp = await _api.PostAsync("/api/v1/customers", req);
            if (!resp.IsSuccessStatusCode) { MessageBox.Show(await resp.Content.ReadAsStringAsync(), "Error"); return; }
            MessageBox.Show("Customer created.", "OK");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnGetCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtCustomerIdGet.Text, out var id)) return;
            var c = await _api.GetAsync<CustomerResponse>($"/api/v1/customers/{id}");
            if (c is null) { MessageBox.Show("Kayıt bulunamadı"); return; }
            txtCustomerDetail.Text = $"{c.FirstName} {c.LastName}  {c.NationalId}";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnCreateAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtAccCustomerId.Text, out var custId)) return;
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
            if (_currentCustomerId.HasValue)
            {
                var list = await _api.GetAsync<List<AccountResponse>>($"/api/v1/accounts/by-customer/{_currentCustomerId.Value}");
                if (list != null && gridAccounts != null)
                {
                    gridAccounts.DataSource = list;
                }
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnDeposit_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtDwAccountId.Text, out var accId)) return;
            if (!TryDec(txtDepositAmount.Text, out var amt, "Tutar")) return;
            var req = new DepositRequest(accId, amt, (NovaBank.Core.Enums.Currency)cmbDwCurrency.SelectedItem!, txtDepositDesc.Text);
            var resp = await _api.PostAsync("/api/v1/transactions/deposit", req);
            MessageBox.Show(await resp.Content.ReadAsStringAsync(), resp.IsSuccessStatusCode ? "OK" : "Error");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnWithdraw_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtDwAccountId.Text, out var accId2)) return;
            if (!TryDec(txtWithdrawAmount.Text, out var amt2, "Tutar")) return;
            var req = new WithdrawRequest(accId2, amt2, (NovaBank.Core.Enums.Currency)cmbDwCurrency.SelectedItem!, txtWithdrawDesc.Text);
            var resp = await _api.PostAsync("/api/v1/transactions/withdraw", req);
            MessageBox.Show(await resp.Content.ReadAsStringAsync(), resp.IsSuccessStatusCode ? "OK" : "Error");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnInternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtFromId.Text, out var fromId)) return;
            if (!TryGuid(txtToId.Text, out var toId)) return;
            if (!TryDec(txtAmount.Text, out var tamt, "Tutar")) return;
            var req = new TransferInternalRequest(fromId, toId, tamt, (NovaBank.Core.Enums.Currency)cmbTransCurrency.SelectedItem!, txtTransDesc.Text);
            var resp = await _api.PostAsync("/api/v1/transfers/internal", req);
            MessageBox.Show(await resp.Content.ReadAsStringAsync(), resp.IsSuccessStatusCode ? "OK" : "Error");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnExternalTransfer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtFromId.Text, out var fromId2)) return;
            if (string.IsNullOrWhiteSpace(txtToIban.Text)) { MessageBox.Show("Alıcı IBAN zorunludur.", "Uyarı"); return; }
            if (!TryDec(txtAmount.Text, out var tamt2, "Tutar")) return;
            var req = new TransferExternalRequest(fromId2, txtToIban.Text, tamt2, (NovaBank.Core.Enums.Currency)cmbTransCurrency.SelectedItem!, txtTransDesc.Text);
            var resp = await _api.PostAsync("/api/v1/transfers/external", req);
            MessageBox.Show(await resp.Content.ReadAsStringAsync(), resp.IsSuccessStatusCode ? "OK" : "Error");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async void btnGetStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!TryGuid(txtStmtAccountId.Text, out var id)) return;
            var from = dtFrom.Value.Date;
            var to   = dtTo.Value.Date.AddDays(1).AddTicks(-1);
            var url = $"/api/v1/reports/account-statement?accountId={id}&from={from:O}&to={to:O}";
            var stmt = await _api.GetAsync<AccountStatementResponse>(url);
            if (stmt is null) { MessageBox.Show("Kayıt bulunamadı"); return; }
            gridStatement.DataSource = stmt.Items.ToList();
            lblTotals.Text = $"Açılış: {stmt.OpeningBalance}  Alacak: {stmt.TotalCredit}  Borç: {stmt.TotalDebit}  Kapanış: {stmt.ClosingBalance}";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }
}
