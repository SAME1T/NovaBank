using DevExpress.XtraEditors;
using DevExpress.XtraTab;

namespace NovaBank.WinForms;

partial class FrmAuth
{
    private System.ComponentModel.IContainer components = null;
    private XtraTabControl tabControl1;
    private XtraTabPage tabLogin, tabRegister;
    // Login controls
    private TextEdit txtLoginTc, txtLoginPassword;
    private SimpleButton btnLogin, btnShowPassword;
    // Register controls
    private TextEdit txtRegTc, txtRegAd, txtRegSoyad, txtRegEmail, txtRegTel, txtRegPassword, txtRegPasswordConfirm;
    private SimpleButton btnRegister, btnShowRegPassword;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.tabControl1 = new XtraTabControl();
        this.tabLogin = new XtraTabPage();
        this.tabRegister = new XtraTabPage();
        
        this.tabControl1.Dock = DockStyle.Fill;
        this.tabControl1.TabPages.AddRange(new XtraTabPage[] { tabLogin, tabRegister });
        this.tabControl1.ShowTabHeader = DevExpress.Utils.DefaultBoolean.True;
        this.tabControl1.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top;
        this.tabControl1.AppearancePage.Header.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.tabControl1.AppearancePage.HeaderActive.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.tabControl1.AppearancePage.HeaderActive.ForeColor = Color.FromArgb(25, 118, 210);
        this.tabControl1.AppearancePage.Header.ForeColor = Color.FromArgb(100, 100, 100);
        this.tabControl1.LookAndFeel.UseDefaultLookAndFeel = false;
        this.tabControl1.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        
        this.Controls.Add(this.tabControl1);
        this.Text = "NovaBank • Güvenli Giriş";
        this.Width = 700; 
        this.Height = 550;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Login tab - Modern Design
        this.tabLogin.Text = "🔐 Giriş Yap";
        this.tabLogin.Padding = new Padding(30);
        
        var pnlLoginHeader = new PanelControl()
        {
            Location = new Point(0, 0),
            Size = new Size(700, 100),
            Dock = DockStyle.Top,
            Appearance = { BackColor = Color.FromArgb(25, 118, 210) }
        };
        
        var lblLoginTitle = new LabelControl() 
        { 
            Location = new Point(30, 25), 
            Size = new Size(500, 50), 
            Text = "🏦 NovaBank'a Hoş Geldiniz", 
            Appearance = { Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.Black }
        };
        pnlLoginHeader.Controls.Add(lblLoginTitle);
        
        var pnlLoginContent = new PanelControl()
        {
            Location = new Point(30, 120),
            Size = new Size(640, 350),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblTc = new LabelControl() 
        { 
            Location = new Point(25, 30), 
            Size = new Size(150, 25), 
            Text = "🆔 TC Kimlik No:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtLoginTc = new TextEdit() 
        { 
            Location = new Point(25, 60), 
            Size = new Size(590, 40)
        };
        txtLoginTc.Properties.NullValuePrompt = "TC Kimlik No giriniz";
        txtLoginTc.Properties.NullValuePromptShowForEmptyValue = true;
        txtLoginTc.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtLoginTc.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblPassword = new LabelControl() 
        { 
            Location = new Point(25, 120), 
            Size = new Size(100, 25), 
            Text = "🔒 Şifre:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtLoginPassword = new TextEdit() 
        { 
            Location = new Point(25, 150), 
            Size = new Size(560, 40)
        };
        txtLoginPassword.Properties.PasswordChar = '●';
        txtLoginPassword.Properties.UseSystemPasswordChar = true;
        txtLoginPassword.Properties.NullValuePrompt = "Şifrenizi giriniz";
        txtLoginPassword.Properties.NullValuePromptShowForEmptyValue = true;
        txtLoginPassword.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtLoginPassword.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        btnShowPassword = new SimpleButton() 
        { 
            Location = new Point(590, 150), 
            Size = new Size(25, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowPassword.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        btnShowPassword.Click += BtnShowPassword_Click;
        
        btnLogin = new SimpleButton() 
        { 
            Location = new Point(25, 220), 
            Size = new Size(590, 50), 
            Text = "✓ Giriş Yap",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnLogin.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        btnLogin.AppearanceHovered.BackColor = Color.FromArgb(21, 101, 192);
        btnLogin.AppearancePressed.BackColor = Color.FromArgb(13, 71, 161);
        btnLogin.Click += btnLogin_Click;
        
        pnlLoginContent.Controls.AddRange(new Control[] { lblTc, txtLoginTc, lblPassword, txtLoginPassword, btnShowPassword, btnLogin });
        tabLogin.Controls.Add(pnlLoginHeader);
        tabLogin.Controls.Add(pnlLoginContent);

        // Register tab - Modern Design
        this.tabRegister.Text = "📝 Kayıt Ol";
        this.tabRegister.Padding = new Padding(30);
        this.tabRegister.AutoScroll = true;
        
        var pnlRegisterHeader = new PanelControl()
        {
            Location = new Point(0, 0),
            Size = new Size(700, 100),
            Dock = DockStyle.Top,
            Appearance = { BackColor = Color.FromArgb(76, 175, 80) }
        };
        
        var lblRegisterTitle = new LabelControl() 
        { 
            Location = new Point(30, 25), 
            Size = new Size(500, 50), 
            Text = "✨ Yeni Hesap Oluştur", 
            Appearance = { Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White }
        };
        pnlRegisterHeader.Controls.Add(lblRegisterTitle);
        
        var pnlRegisterContent = new PanelControl()
        {
            Location = new Point(30, 120),
            Size = new Size(640, 650),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblRegTc = new LabelControl() 
        { 
            Location = new Point(25, 25), 
            Size = new Size(150, 25), 
            Text = "🆔 TC Kimlik No:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegTc = new TextEdit() 
        { 
            Location = new Point(25, 55), 
            Size = new Size(590, 40)
        };
        txtRegTc.Properties.NullValuePrompt = "TC Kimlik No";
        txtRegTc.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegTc.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegTc.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblRegAd = new LabelControl() 
        { 
            Location = new Point(25, 110), 
            Size = new Size(80, 25), 
            Text = "👤 Ad:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegAd = new TextEdit() 
        { 
            Location = new Point(25, 140), 
            Size = new Size(285, 40)
        };
        txtRegAd.Properties.NullValuePrompt = "Ad";
        txtRegAd.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegAd.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegAd.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblRegSoyad = new LabelControl() 
        { 
            Location = new Point(330, 110), 
            Size = new Size(100, 25), 
            Text = "👤 Soyad:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegSoyad = new TextEdit() 
        { 
            Location = new Point(330, 140), 
            Size = new Size(285, 40)
        };
        txtRegSoyad.Properties.NullValuePrompt = "Soyad";
        txtRegSoyad.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegSoyad.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegSoyad.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblRegEmail = new LabelControl() 
        { 
            Location = new Point(25, 195), 
            Size = new Size(120, 25), 
            Text = "📧 E-posta:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegEmail = new TextEdit() 
        { 
            Location = new Point(25, 225), 
            Size = new Size(590, 40)
        };
        txtRegEmail.Properties.NullValuePrompt = "E-posta (opsiyonel)";
        txtRegEmail.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegEmail.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegEmail.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblRegTel = new LabelControl() 
        { 
            Location = new Point(25, 280), 
            Size = new Size(120, 25), 
            Text = "📱 Telefon:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegTel = new TextEdit() 
        { 
            Location = new Point(25, 310), 
            Size = new Size(590, 40)
        };
        txtRegTel.Properties.NullValuePrompt = "Telefon (opsiyonel)";
        txtRegTel.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegTel.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegTel.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblRegPassword = new LabelControl() 
        { 
            Location = new Point(25, 365), 
            Size = new Size(120, 25), 
            Text = "🔒 Şifre:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegPassword = new TextEdit() 
        { 
            Location = new Point(25, 395), 
            Size = new Size(560, 40)
        };
        txtRegPassword.Properties.PasswordChar = '●';
        txtRegPassword.Properties.UseSystemPasswordChar = true;
        txtRegPassword.Properties.NullValuePrompt = "Şifre (min. 6 karakter)";
        txtRegPassword.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegPassword.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegPassword.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        btnShowRegPassword = new SimpleButton() 
        { 
            Location = new Point(590, 395), 
            Size = new Size(25, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowRegPassword.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        btnShowRegPassword.Click += BtnShowRegPassword_Click;
        
        var lblRegPasswordConfirm = new LabelControl() 
        { 
            Location = new Point(25, 450), 
            Size = new Size(180, 25), 
            Text = "🔒 Şifre (Tekrar):", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtRegPasswordConfirm = new TextEdit() 
        { 
            Location = new Point(25, 480), 
            Size = new Size(590, 40)
        };
        txtRegPasswordConfirm.Properties.PasswordChar = '●';
        txtRegPasswordConfirm.Properties.UseSystemPasswordChar = true;
        txtRegPasswordConfirm.Properties.NullValuePrompt = "Şifre (tekrar)";
        txtRegPasswordConfirm.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegPasswordConfirm.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtRegPasswordConfirm.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        btnRegister = new SimpleButton() 
        { 
            Location = new Point(25, 545), 
            Size = new Size(590, 50), 
            Text = "✓ Kayıt Ol",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnRegister.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnRegister.AppearanceHovered.BackColor = Color.FromArgb(69, 160, 73);
        btnRegister.AppearancePressed.BackColor = Color.FromArgb(56, 142, 60);
        btnRegister.Click += btnRegister_Click;
        
        pnlRegisterContent.Controls.AddRange(new Control[] { lblRegTc, txtRegTc, lblRegAd, txtRegAd, lblRegSoyad, txtRegSoyad, lblRegEmail, txtRegEmail, lblRegTel, txtRegTel, lblRegPassword, txtRegPassword, btnShowRegPassword, lblRegPasswordConfirm, txtRegPasswordConfirm, btnRegister });
        tabRegister.Controls.Add(pnlRegisterHeader);
        tabRegister.Controls.Add(pnlRegisterContent);
    }
}
