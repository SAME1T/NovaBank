namespace NovaBank.WinForms;

partial class FrmAuth
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControl1;
    private TabPage tabLogin, tabRegister;
    // Login controls
    private TextBox txtLoginTc, txtLoginPassword;
    private Button btnLogin;
    // Register controls
    private TextBox txtRegTc, txtRegAd, txtRegSoyad, txtRegEmail, txtRegTel, txtRegPassword, txtRegPasswordConfirm;
    private Button btnRegister;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.tabControl1 = new TabControl();
        this.tabLogin = new TabPage("Giriş");
        this.tabRegister = new TabPage("Kayıt Ol");
        this.tabControl1.TabPages.AddRange(new TabPage[]{tabLogin, tabRegister});
        this.tabControl1.Dock = DockStyle.Fill;
        this.Controls.Add(this.tabControl1);
        this.Text = "NovaBank • Giriş";
        this.Width = 500; this.Height = 400;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Login tab
        txtLoginTc = new TextBox(){ Left=50, Top=50, Width=300, PlaceholderText="TC Kimlik No"};
        txtLoginPassword = new TextBox(){ Left=50, Top=80, Width=300, PlaceholderText="Şifre", UseSystemPasswordChar=true};
        btnLogin = new Button(){ Left=200, Top=120, Width=100, Text="Giriş Yap"};
        btnLogin.Click += btnLogin_Click;
        tabLogin.Controls.AddRange(new Control[]{txtLoginTc, txtLoginPassword, btnLogin});

        // Register tab
        txtRegTc = new TextBox(){ Left=50, Top=30, Width=300, PlaceholderText="TC Kimlik No"};
        txtRegAd = new TextBox(){ Left=50, Top=60, Width=140, PlaceholderText="Ad"};
        txtRegSoyad = new TextBox(){ Left=210, Top=60, Width=140, PlaceholderText="Soyad"};
        txtRegEmail = new TextBox(){ Left=50, Top=90, Width=300, PlaceholderText="E-posta (opsiyonel)"};
        txtRegTel = new TextBox(){ Left=50, Top=120, Width=300, PlaceholderText="Telefon (opsiyonel)"};
        txtRegPassword = new TextBox(){ Left=50, Top=150, Width=300, PlaceholderText="Şifre", UseSystemPasswordChar=true};
        txtRegPasswordConfirm = new TextBox(){ Left=50, Top=180, Width=300, PlaceholderText="Şifre Tekrar", UseSystemPasswordChar=true};
        btnRegister = new Button(){ Left=200, Top=220, Width=100, Text="Kayıt Ol"};
        btnRegister.Click += btnRegister_Click;
        tabRegister.Controls.AddRange(new Control[]{txtRegTc, txtRegAd, txtRegSoyad, txtRegEmail, txtRegTel, txtRegPassword, txtRegPasswordConfirm, btnRegister});
    }
}
