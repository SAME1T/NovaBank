using NovaBank.WinForms.Services;
using NovaBank.Contracts.Customers;
using DevExpress.XtraEditors;

namespace NovaBank.WinForms;

public partial class FrmForgotPassword : XtraForm
{
    private readonly ApiClient _api = new();
    private int _currentStep = 1;
    private string? _identifier;

    public FrmForgotPassword()
    {
        InitializeComponent();
        this.Text = "NovaBank • Şifremi Unuttum";
        ShowStep(1);
    }

    private void ShowStep(int step)
    {
        _currentStep = step;
        pnlStep1.Visible = (step == 1);
        pnlStep2.Visible = (step == 2);
        pnlStep3.Visible = (step == 3);
        
        lblStepInfo.Text = step switch
        {
            1 => "E-posta veya TC Kimlik No'nuzu girip kod gönder butonuna tıklayın.",
            2 => "E-posta adresinize gönderilen 6 haneli kodu girin.",
            3 => "Yeni şifrenizi belirleyin.",
            _ => ""
        };
    }

    private async void btnSendCode_Click(object sender, EventArgs e)
    {
        try
        {
            var emailOrTc = txtEmailOrTc.Text?.Trim();
            if (string.IsNullOrWhiteSpace(emailOrTc))
            {
                XtraMessageBox.Show("E-posta veya TC Kimlik No giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSendCode.Enabled = false;
            this.UseWaitCursor = true;
            lblStepInfo.Text = "Kod gönderiliyor...";

            var response = await _api.PasswordResetRequestAsync(emailOrTc);
            if (response.IsSuccessStatusCode)
            {
                _identifier = emailOrTc;
                lblStepInfo.Text = "Kod e-posta adresinize gönderildi. Lütfen e-postanızı kontrol edin.";
                ShowStep(2);
            }
            else
            {
                // Detaylı hata mesajını göster (response body dahil)
                var errorMsg = await ApiClient.GetErrorMessageAsync(response);
                var responseBody = await response.Content.ReadAsStringAsync();
                var fullErrorMsg = $"Kod gönderilemedi:\n{errorMsg}";
                if (!string.IsNullOrWhiteSpace(responseBody) && responseBody != errorMsg)
                {
                    fullErrorMsg += $"\n\nResponse Body:\n{responseBody}";
                }
                XtraMessageBox.Show(fullErrorMsg, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStepInfo.Text = "Kod gönderilemedi.";
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStepInfo.Text = "Bir hata oluştu.";
        }
        finally
        {
            btnSendCode.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async void btnVerifyCode_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_identifier))
            {
                XtraMessageBox.Show("Lütfen önce kod gönderin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var code = txtCode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                XtraMessageBox.Show("Doğrulama kodunu giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (code.Length != 6)
            {
                XtraMessageBox.Show("Kod 6 haneli olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnVerifyCode.Enabled = false;
            this.UseWaitCursor = true;
            lblStepInfo.Text = "Kod doğrulanıyor...";

            var response = await _api.PasswordResetVerifyAsync(_identifier, code);
            if (response.IsSuccessStatusCode)
            {
                lblStepInfo.Text = "Kod doğrulandı. Yeni şifrenizi belirleyin.";
                ShowStep(3);
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Kod doğrulanamadı:\n{errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStepInfo.Text = "Kod doğrulanamadı.";
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStepInfo.Text = "Bir hata oluştu.";
        }
        finally
        {
            btnVerifyCode.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private async void btnComplete_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_identifier))
            {
                XtraMessageBox.Show("Lütfen önce kod gönderin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var code = txtCode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                XtraMessageBox.Show("Doğrulama kodunu giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newPassword = txtNewPassword.Text?.Trim();
            var newPassword2 = txtNewPassword2.Text?.Trim();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                XtraMessageBox.Show("Yeni şifreyi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != newPassword2)
            {
                XtraMessageBox.Show("Şifreler eşleşmiyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                XtraMessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnComplete.Enabled = false;
            this.UseWaitCursor = true;
            lblStepInfo.Text = "Şifre güncelleniyor...";

            var response = await _api.PasswordResetCompleteAsync(_identifier, code, newPassword);
            if (response.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("Şifreniz başarıyla güncellendi!\nGiriş yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                var errorMsg = await ApiClient.GetErrorMessageAsync(response);
                XtraMessageBox.Show($"Şifre güncellenemedi:\n{errorMsg}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStepInfo.Text = "Şifre güncellenemedi.";
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStepInfo.Text = "Bir hata oluştu.";
        }
        finally
        {
            btnComplete.Enabled = true;
            this.UseWaitCursor = false;
        }
    }

    private void BtnShowNewPassword_Click(object sender, EventArgs e)
    {
        txtNewPassword.Properties.UseSystemPasswordChar = !txtNewPassword.Properties.UseSystemPasswordChar;
        btnShowNewPassword.Text = txtNewPassword.Properties.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private void BtnShowNewPassword2_Click(object sender, EventArgs e)
    {
        txtNewPassword2.Properties.UseSystemPasswordChar = !txtNewPassword2.Properties.UseSystemPasswordChar;
        btnShowNewPassword2.Text = txtNewPassword2.Properties.UseSystemPasswordChar ? "👁" : "🙈";
    }
}
