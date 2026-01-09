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
        this.tabControl1.AppearancePage.Header.Font = new Font("Segoe UI Semibold", 11);
        this.tabControl1.AppearancePage.HeaderActive.Font = new Font("Segoe UI Semibold", 11);
        this.tabControl1.AppearancePage.HeaderActive.ForeColor = Color.FromArgb(0, 120, 215);
        this.tabControl1.AppearancePage.Header.ForeColor = Color.FromArgb(120, 130, 140);
        this.tabControl1.LookAndFeel.UseDefaultLookAndFeel = false;
        this.tabControl1.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        this.tabControl1.Appearance.BackColor = Color.FromArgb(245, 247, 250);
        
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
            Size = new Size(700, 110),
            Dock = DockStyle.Top,
            Appearance = { BackColor = Color.FromArgb(20, 33, 61) }
        };
        pnlLoginHeader.LookAndFeel.UseDefaultLookAndFeel = false;
        pnlLoginHeader.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        pnlLoginHeader.Appearance.Options.UseBackColor = true;
        
        var lblLoginTitle = new LabelControl() 
        { 
            Location = new Point(30, 25), 
            Size = new Size(500, 50), 
            Text = "NOVA BANK", 
            Appearance = { Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White }
        };
        var lblLoginSubtitle = new LabelControl()
        {
            Location = new Point(35, 75),
            Text = "Geleceğin Dijital Bankacılığı",
            Appearance = { Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(150, 160, 180) }
        };
        pnlLoginHeader.Controls.AddRange(new Control[] { lblLoginTitle, lblLoginSubtitle });
        
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
        lblTc.Appearance.Options.UseFont = true;
        lblTc.Appearance.Options.UseForeColor = true;
        txtLoginTc = new TextEdit() 
        { 
            Location = new Point(25, 60), 
            Size = new Size(590, 40)
        };
        txtLoginTc.Properties.MaxLength = 11;
        txtLoginTc.Properties.NullValuePrompt = "TC Kimlik No";
        txtLoginTc.Properties.NullValuePromptShowForEmptyValue = true;
        txtLoginTc.ForeColor = Color.Black;
        txtLoginTc.BackColor = Color.FromArgb(250, 250, 250);
        txtLoginTc.Font = new Font("Segoe UI", 11);
        txtLoginTc.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
        
        var lblPassword = new LabelControl() 
        { 
            Location = new Point(25, 120), 
            Size = new Size(100, 25), 
            Text = "🔑 Şifre:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblPassword.Appearance.Options.UseFont = true;
        lblPassword.Appearance.Options.UseForeColor = true;
        txtLoginPassword = new TextEdit() 
        { 
            Location = new Point(25, 150), 
            Size = new Size(555, 40)
        };
        txtLoginPassword.Properties.PasswordChar = '●';
        txtLoginPassword.Properties.NullValuePrompt = "Şifrenizi giriniz";
        txtLoginPassword.Properties.NullValuePromptShowForEmptyValue = true;
        txtLoginPassword.ForeColor = Color.Black;
        txtLoginPassword.BackColor = Color.FromArgb(250, 250, 250);
        txtLoginPassword.Font = new Font("Segoe UI", 11);
        
        btnShowPassword = new SimpleButton() 
        { 
            Location = new Point(590, 150), 
            Size = new Size(40, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowPassword.Appearance.BackColor = Color.FromArgb(230, 230, 230);
        btnShowPassword.AppearanceHovered.BackColor = Color.FromArgb(200, 200, 200);
        btnShowPassword.Appearance.Options.UseFont = true;
        btnShowPassword.Click += BtnShowPassword_Click;
        
        btnLogin = new SimpleButton() 
        { 
            Location = new Point(25, 220), 
            Size = new Size(590, 55), 
            Text = "Giriş Yap",
            Appearance = { Font = new Font("Segoe UI Semibold", 13), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat },
            Cursor = Cursors.Hand
        };
        btnLogin.Appearance.BackColor = Color.FromArgb(0, 120, 215);
        btnLogin.AppearanceHovered.BackColor = Color.FromArgb(0, 100, 190);
        btnLogin.AppearancePressed.BackColor = Color.FromArgb(0, 80, 160);
        btnLogin.Appearance.Options.UseBackColor = true;
        btnLogin.Click += btnLogin_Click;
        
        // Şifremi Unuttum linki
        var btnForgotPassword = new SimpleButton()
        {
            Location = new Point(25, 280),
            Size = new Size(200, 30),
            Text = "🔓 Şifremi Unuttum",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(25, 118, 210) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnForgotPassword.Appearance.BackColor = Color.Transparent;
        btnForgotPassword.AppearanceHovered.BackColor = Color.FromArgb(240, 240, 240);
        btnForgotPassword.Click += BtnForgotPassword_Click;
        
        pnlLoginContent.Controls.AddRange(new Control[] { lblTc, txtLoginTc, lblPassword, txtLoginPassword, btnShowPassword, btnLogin, btnForgotPassword });
        tabLogin.Controls.Add(pnlLoginHeader);
        tabLogin.Controls.Add(pnlLoginContent);

        // Register tab - Modern Design - YAZILARI SİYAH
        this.tabRegister.Text = "📝 Kayıt Ol";
        this.tabRegister.Padding = new Padding(30);
        this.tabRegister.AutoScroll = true;
        
        var pnlRegisterHeader = new PanelControl()
        {
            Location = new Point(0, 0),
            Size = new Size(700, 110),
            Dock = DockStyle.Top,
            Appearance = { BackColor = Color.FromArgb(20, 33, 61) }
        };
        pnlRegisterHeader.LookAndFeel.UseDefaultLookAndFeel = false;
        pnlRegisterHeader.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        pnlRegisterHeader.Appearance.Options.UseBackColor = true;
        
        var lblRegisterTitle = new LabelControl() 
        { 
            Location = new Point(30, 25), 
            Size = new Size(500, 50), 
            Text = "YENİ HESAP", 
            Appearance = { Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White }
        };
        var lblRegisterSubtitle = new LabelControl()
        {
            Location = new Point(35, 75),
            Text = "NovaBank Dünyasına Katılın",
            Appearance = { Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(150, 160, 180) }
        };
        pnlRegisterHeader.Controls.AddRange(new Control[] { lblRegisterTitle, lblRegisterSubtitle });
        
        var pnlRegisterContent = new PanelControl()
        {
            Location = new Point(30, 130),
            Size = new Size(640, 680),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 235, 240) }
        };
        pnlRegisterContent.LookAndFeel.UseDefaultLookAndFeel = false;
        pnlRegisterContent.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        
        var lblRegTc = new LabelControl() 
        { 
            Location = new Point(25, 25), 
            Size = new Size(150, 25), 
            Text = "🆔 TC Kimlik No:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegTc.Appearance.Options.UseFont = true;
        lblRegTc.Appearance.Options.UseForeColor = true;
        txtRegTc = new TextEdit() 
        { 
            Location = new Point(25, 55), 
            Size = new Size(590, 40)
        };
        txtRegTc.Properties.MaxLength = 11;
        txtRegTc.Properties.NullValuePrompt = "TC Kimlik No";
        txtRegTc.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegTc.ForeColor = Color.Black;
        txtRegTc.BackColor = Color.FromArgb(250, 250, 250);
        txtRegTc.Font = new Font("Segoe UI", 11);
        txtRegTc.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
        
        var lblRegAd = new LabelControl() 
        { 
            Location = new Point(25, 110), 
            Size = new Size(80, 25), 
            Text = "👤 Ad:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegAd.Appearance.Options.UseFont = true;
        lblRegAd.Appearance.Options.UseForeColor = true;
        txtRegAd = new TextEdit() 
        { 
            Location = new Point(25, 140), 
            Size = new Size(285, 40)
        };
        txtRegAd.Properties.NullValuePrompt = "Ad";
        txtRegAd.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegAd.ForeColor = Color.Black;
        txtRegAd.BackColor = Color.FromArgb(250, 250, 250);
        txtRegAd.Font = new Font("Segoe UI", 11);
        
        var lblRegSoyad = new LabelControl() 
        { 
            Location = new Point(330, 110), 
            Size = new Size(100, 25), 
            Text = "👤 Soyad:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegSoyad.Appearance.Options.UseFont = true;
        lblRegSoyad.Appearance.Options.UseForeColor = true;
        txtRegSoyad = new TextEdit() 
        { 
            Location = new Point(330, 140), 
            Size = new Size(285, 40)
        };
        txtRegSoyad.Properties.NullValuePrompt = "Soyad";
        txtRegSoyad.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegSoyad.ForeColor = Color.Black;
        txtRegSoyad.BackColor = Color.FromArgb(250, 250, 250);
        txtRegSoyad.Font = new Font("Segoe UI", 11);
        
        var lblRegEmail = new LabelControl() 
        { 
            Location = new Point(25, 195), 
            Size = new Size(120, 25), 
            Text = "📧 E-posta:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegEmail.Appearance.Options.UseFont = true;
        lblRegEmail.Appearance.Options.UseForeColor = true;
        txtRegEmail = new TextEdit() 
        { 
            Location = new Point(25, 225), 
            Size = new Size(590, 40)
        };
        txtRegEmail.Properties.NullValuePrompt = "E-posta (opsiyonel)";
        txtRegEmail.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegEmail.ForeColor = Color.Black;
        txtRegEmail.BackColor = Color.FromArgb(250, 250, 250);
        txtRegEmail.Font = new Font("Segoe UI", 11);
        
        var lblRegTel = new LabelControl() 
        { 
            Location = new Point(25, 280), 
            Size = new Size(120, 25), 
            Text = "📱 Telefon:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegTel.Appearance.Options.UseFont = true;
        lblRegTel.Appearance.Options.UseForeColor = true;
        txtRegTel = new TextEdit() 
        { 
            Location = new Point(25, 310), 
            Size = new Size(590, 40)
        };
        txtRegTel.Properties.MaxLength = 10;
        txtRegTel.Properties.NullValuePrompt = "Telefon (örn: 5321234567)";
        txtRegTel.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegTel.ForeColor = Color.Black;
        txtRegTel.BackColor = Color.FromArgb(250, 250, 250);
        txtRegTel.Font = new Font("Segoe UI", 11);
        txtRegTel.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
        
        var lblRegPassword = new LabelControl() 
        { 
            Location = new Point(25, 365), 
            Size = new Size(120, 25), 
            Text = "🔑 Şifre:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegPassword.Appearance.Options.UseFont = true;
        lblRegPassword.Appearance.Options.UseForeColor = true;
        txtRegPassword = new TextEdit() 
        { 
            Location = new Point(25, 395), 
            Size = new Size(555, 40)
        };
        txtRegPassword.Properties.PasswordChar = '●';
        txtRegPassword.Properties.NullValuePrompt = "Şifre (min. 6 karakter)";
        txtRegPassword.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegPassword.ForeColor = Color.Black;
        txtRegPassword.BackColor = Color.FromArgb(250, 250, 250);
        txtRegPassword.Font = new Font("Segoe UI", 11);
        btnShowRegPassword = new SimpleButton() 
        { 
            Location = new Point(590, 395), 
            Size = new Size(40, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowRegPassword.Appearance.BackColor = Color.FromArgb(230, 230, 230);
        btnShowRegPassword.AppearanceHovered.BackColor = Color.FromArgb(200, 200, 200);
        btnShowRegPassword.Appearance.Options.UseFont = true;
        btnShowRegPassword.Click += BtnShowRegPassword_Click;
        
        var lblRegPasswordConfirm = new LabelControl() 
        { 
            Location = new Point(25, 450), 
            Size = new Size(180, 25), 
            Text = "🔑 Şifre (Tekrar):", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black }
        };
        lblRegPasswordConfirm.Appearance.Options.UseFont = true;
        lblRegPasswordConfirm.Appearance.Options.UseForeColor = true;
        txtRegPasswordConfirm = new TextEdit() 
        { 
            Location = new Point(25, 480), 
            Size = new Size(555, 40)
        };
        txtRegPasswordConfirm.Properties.PasswordChar = '●';
        txtRegPasswordConfirm.Properties.NullValuePrompt = "Şifre (tekrar)";
        txtRegPasswordConfirm.Properties.NullValuePromptShowForEmptyValue = true;
        txtRegPasswordConfirm.ForeColor = Color.Black;
        txtRegPasswordConfirm.BackColor = Color.FromArgb(250, 250, 250);
        txtRegPasswordConfirm.Font = new Font("Segoe UI", 11);
        
        var btnShowRegPasswordConfirm = new SimpleButton() 
        { 
            Location = new Point(590, 480), 
            Size = new Size(40, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowRegPasswordConfirm.Appearance.BackColor = Color.FromArgb(230, 230, 230);
        btnShowRegPasswordConfirm.AppearanceHovered.BackColor = Color.FromArgb(200, 200, 200);
        btnShowRegPasswordConfirm.Appearance.Options.UseFont = true;
        btnShowRegPasswordConfirm.Click += BtnShowRegPasswordConfirm_Click;
        
        btnRegister = new SimpleButton() 
        { 
            Location = new Point(25, 545), 
            Size = new Size(590, 55), 
            Text = "Kayıt Ol",
            Appearance = { Font = new Font("Segoe UI Semibold", 13), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat },
            Cursor = Cursors.Hand
        };
        btnRegister.Appearance.BackColor = Color.FromArgb(46, 204, 113);
        btnRegister.AppearanceHovered.BackColor = Color.FromArgb(40, 180, 100);
        btnRegister.AppearancePressed.BackColor = Color.FromArgb(35, 160, 90);
        btnRegister.Appearance.Options.UseBackColor = true;
        btnRegister.Click += btnRegister_Click;
        
        pnlRegisterContent.Controls.AddRange(new Control[] { lblRegTc, txtRegTc, lblRegAd, txtRegAd, lblRegSoyad, txtRegSoyad, lblRegEmail, txtRegEmail, lblRegTel, txtRegTel, lblRegPassword, txtRegPassword, btnShowRegPassword, lblRegPasswordConfirm, txtRegPasswordConfirm, btnShowRegPasswordConfirm, btnRegister });
        tabRegister.Controls.Add(pnlRegisterHeader);
        tabRegister.Controls.Add(pnlRegisterContent);
    }
}
