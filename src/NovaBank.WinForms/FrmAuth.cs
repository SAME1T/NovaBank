using NovaBank.WinForms.Services;
using NovaBank.WinForms.Dto;
using NovaBank.Api.Contracts;

namespace NovaBank.WinForms;
public partial class FrmAuth : Form
{
    private readonly ApiClient _api = new();
    public Guid? LoggedInCustomerId { get; private set; }
    public FrmAuth() { InitializeComponent(); this.Text = "NovaBank • Giriş"; }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        try
        {
            var tc = txtLoginTc.Text?.Trim();
            var password = txtLoginPassword.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tc)) { MessageBox.Show("TC Kimlik No giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(password)) { MessageBox.Show("Şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var loginReq = new LoginRequest(tc, password);
            var cust = await _api.PostAsync<LoginRequest, CustomerResponse>("/api/v1/customers/login", loginReq);
            if (cust is null) { MessageBox.Show("Giriş başarısız!\nTC Kimlik No veya şifre hatalı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            LoggedInCustomerId = cust.Id;
            DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex) { MessageBox.Show($"Giriş sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async void btnRegister_Click(object sender, EventArgs e)
    {
        try
        {
            var password = txtRegPassword.Text?.Trim();
            var passwordConfirm = txtRegPasswordConfirm.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(password)) { MessageBox.Show("Şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (password != passwordConfirm) { MessageBox.Show("Şifreler eşleşmiyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (password.Length < 6) { MessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var req = new CreateCustomerRequest(
                txtRegTc.Text?.Trim() ?? "",
                txtRegAd.Text?.Trim() ?? "",
                txtRegSoyad.Text?.Trim() ?? "",
                string.IsNullOrWhiteSpace(txtRegEmail.Text) ? null : txtRegEmail.Text.Trim(),
                string.IsNullOrWhiteSpace(txtRegTel.Text) ? null : txtRegTel.Text.Trim(),
                password
            );
            var resp = await _api.PostAsync("/api/v1/customers", req);
            if (!resp.IsSuccessStatusCode)
            {
                MessageBox.Show(await resp.Content.ReadAsStringAsync(), "Hata");
                return;
            }
            MessageBox.Show("Kayıt oluşturuldu!\nGiriş sekmesine TC'nizi yazarak giriş yapınız.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TC'yi giriş sekmesine taşı
            txtLoginTc.Text = txtRegTc.Text;
            // Giriş sekmesine geç
            tabControl1.SelectedIndex = 0;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata"); }
    }

    private void BtnShowPassword_Click(object? sender, EventArgs e)
    {
        txtLoginPassword.UseSystemPasswordChar = !txtLoginPassword.UseSystemPasswordChar;
        btnShowPassword.Text = txtLoginPassword.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private void BtnShowRegPassword_Click(object? sender, EventArgs e)
    {
        txtRegPassword.UseSystemPasswordChar = !txtRegPassword.UseSystemPasswordChar;
        btnShowRegPassword.Text = txtRegPassword.UseSystemPasswordChar ? "👁" : "🙈";
    }
}
