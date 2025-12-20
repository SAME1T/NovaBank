using NovaBank.WinForms.Services;
using NovaBank.Contracts.Customers;
using DevExpress.XtraEditors;

namespace NovaBank.WinForms;
public partial class FrmAuth : XtraForm
{
    private readonly ApiClient _api = new();
    public Guid? LoggedInCustomerId { get; private set; }
    public FrmAuth() { InitializeComponent(); this.Text = "NovaBank • Güvenli Giriş"; }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        try
        {
            var tc = txtLoginTc.Text?.Trim();
            var password = txtLoginPassword.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tc)) { XtraMessageBox.Show("TC Kimlik No giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(password)) { XtraMessageBox.Show("Şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            var loginReq = new LoginRequest(tc, password);
            var loginResp = await _api.PostAsync<LoginRequest, LoginResponse>("/api/v1/customers/login", loginReq);
            if (loginResp is null) { XtraMessageBox.Show("Giriş başarısız!\nTC Kimlik No veya şifre hatalı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            LoggedInCustomerId = loginResp.CustomerId;
            
            // Session'a kaydet
            Session.CurrentCustomerId = loginResp.CustomerId;
            Session.CurrentCustomerName = loginResp.FullName;
            Session.CurrentRole = loginResp.Role;
            
            DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex) { XtraMessageBox.Show($"Giriş sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async void btnRegister_Click(object sender, EventArgs e)
    {
        try
        {
            var password = txtRegPassword.Text?.Trim();
            var passwordConfirm = txtRegPasswordConfirm.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(password)) { XtraMessageBox.Show("Şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (password != passwordConfirm) { XtraMessageBox.Show("Şifreler eşleşmiyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (password.Length < 6) { XtraMessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
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
                XtraMessageBox.Show(await resp.Content.ReadAsStringAsync(), "Hata");
                return;
            }
            XtraMessageBox.Show("Kayıt oluşturuldu!\nGiriş sekmesine TC'nizi yazarak giriş yapınız.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TC'yi giriş sekmesine taşı
            txtLoginTc.Text = txtRegTc.Text;
            // Giriş sekmesine geç
            tabControl1.SelectedTabPage = tabLogin;
        }
        catch (Exception ex) { XtraMessageBox.Show(ex.Message, "Hata"); }
    }

    private void BtnShowPassword_Click(object sender, EventArgs e)
    {
        txtLoginPassword.Properties.UseSystemPasswordChar = !txtLoginPassword.Properties.UseSystemPasswordChar;
        btnShowPassword.Text = txtLoginPassword.Properties.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private void BtnShowRegPassword_Click(object sender, EventArgs e)
    {
        txtRegPassword.Properties.UseSystemPasswordChar = !txtRegPassword.Properties.UseSystemPasswordChar;
        btnShowRegPassword.Text = txtRegPassword.Properties.UseSystemPasswordChar ? "👁" : "🙈";
    }
}
