using DevExpress.XtraEditors;

namespace NovaBank.WinForms;

partial class FrmForgotPassword
{
    private System.ComponentModel.IContainer components = null;
    
    // Step 1: Email/TC Input
    private PanelControl pnlStep1;
    private LabelControl lblTitle, lblEmailOrTc, lblStepInfo;
    private TextEdit txtEmailOrTc;
    private SimpleButton btnSendCode;
    
    // Step 2: Code Verification
    private PanelControl pnlStep2;
    private LabelControl lblCode;
    private TextEdit txtCode;
    private SimpleButton btnVerifyCode;
    
    // Step 3: New Password
    private PanelControl pnlStep3;
    private LabelControl lblNewPassword, lblNewPassword2;
    private TextEdit txtNewPassword, txtNewPassword2;
    private SimpleButton btnShowNewPassword, btnShowNewPassword2, btnComplete;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        
        // Main Title
        this.lblTitle = new LabelControl()
        {
            Location = new Point(20, 20),
            Size = new Size(500, 30),
            Text = "🔐 Şifremi Unuttum",
            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };

        // Step Info Label
        this.lblStepInfo = new LabelControl()
        {
            Location = new Point(20, 60),
            Size = new Size(540, 40),
            Text = "E-posta veya TC Kimlik No'nuzu girip kod gönder butonuna tıklayın.",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 100) }
        };

        // ========== STEP 1: Email/TC Input ==========
        this.pnlStep1 = new PanelControl()
        {
            Location = new Point(20, 110),
            Size = new Size(540, 200),
            Dock = DockStyle.None
        };

        this.lblEmailOrTc = new LabelControl()
        {
            Location = new Point(0, 20),
            Size = new Size(200, 22),
            Text = "E-posta veya TC Kimlik No:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        this.txtEmailOrTc = new TextEdit()
        {
            Location = new Point(0, 45),
            Size = new Size(400, 38),
            Properties = { NullValuePrompt = "ornek@email.com veya 12345678901" }
        };

        this.btnSendCode = new SimpleButton()
        {
            Location = new Point(420, 45),
            Size = new Size(120, 38),
            Text = "📧 Kod Gönder",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        this.btnSendCode.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        this.btnSendCode.Click += btnSendCode_Click;

        this.pnlStep1.Controls.AddRange(new Control[] { lblEmailOrTc, txtEmailOrTc, btnSendCode });

        // ========== STEP 2: Code Verification ==========
        this.pnlStep2 = new PanelControl()
        {
            Location = new Point(20, 110),
            Size = new Size(540, 200),
            Dock = DockStyle.None,
            Visible = false
        };

        this.lblCode = new LabelControl()
        {
            Location = new Point(0, 20),
            Size = new Size(150, 22),
            Text = "Doğrulama Kodu:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        this.txtCode = new TextEdit()
        {
            Location = new Point(0, 45),
            Size = new Size(200, 38),
            Properties = { MaxLength = 6, NullValuePrompt = "6 haneli kod" }
        };

        this.btnVerifyCode = new SimpleButton()
        {
            Location = new Point(220, 45),
            Size = new Size(120, 38),
            Text = "✓ Kodu Doğrula",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White }
        };
        this.btnVerifyCode.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        this.btnVerifyCode.Click += btnVerifyCode_Click;

        this.pnlStep2.Controls.AddRange(new Control[] { lblCode, txtCode, btnVerifyCode });

        // ========== STEP 3: New Password ==========
        this.pnlStep3 = new PanelControl()
        {
            Location = new Point(20, 110),
            Size = new Size(540, 300),
            Dock = DockStyle.None,
            Visible = false
        };

        this.lblNewPassword = new LabelControl()
        {
            Location = new Point(0, 20),
            Size = new Size(150, 22),
            Text = "Yeni Şifre:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        this.txtNewPassword = new TextEdit()
        {
            Location = new Point(0, 45),
            Size = new Size(350, 38),
            Properties = { UseSystemPasswordChar = true, NullValuePrompt = "En az 6 karakter" }
        };

        this.btnShowNewPassword = new SimpleButton()
        {
            Location = new Point(360, 45),
            Size = new Size(50, 38),
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 12) }
        };
        this.btnShowNewPassword.Click += BtnShowNewPassword_Click;

        this.lblNewPassword2 = new LabelControl()
        {
            Location = new Point(0, 100),
            Size = new Size(150, 22),
            Text = "Yeni Şifre (Tekrar):",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) }
        };

        this.txtNewPassword2 = new TextEdit()
        {
            Location = new Point(0, 125),
            Size = new Size(350, 38),
            Properties = { UseSystemPasswordChar = true, NullValuePrompt = "Şifreyi tekrar girin" }
        };

        this.btnShowNewPassword2 = new SimpleButton()
        {
            Location = new Point(360, 125),
            Size = new Size(50, 38),
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 12) }
        };
        this.btnShowNewPassword2.Click += BtnShowNewPassword2_Click;

        this.btnComplete = new SimpleButton()
        {
            Location = new Point(0, 180),
            Size = new Size(200, 45),
            Text = "✓ Şifreyi Güncelle",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White }
        };
        this.btnComplete.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        this.btnComplete.Click += btnComplete_Click;

        this.pnlStep3.Controls.AddRange(new Control[] {
            lblNewPassword, txtNewPassword, btnShowNewPassword,
            lblNewPassword2, txtNewPassword2, btnShowNewPassword2,
            btnComplete
        });

        // Main Form
        this.Controls.AddRange(new Control[] {
            lblTitle, lblStepInfo, pnlStep1, pnlStep2, pnlStep3
        });

        this.Text = "NovaBank • Şifremi Unuttum";
        this.Width = 600;
        this.Height = 450;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
}
