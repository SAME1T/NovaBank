namespace NovaBank.WinForms;

partial class FrmAuth
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControl1;
    private TabPage tabLogin, tabRegister;
    // Login controls
    private TextBox txtLoginTc, txtLoginPassword;
    private Button btnLogin, btnShowPassword;
    // Register controls
    private TextBox txtRegTc, txtRegAd, txtRegSoyad, txtRegEmail, txtRegTel, txtRegPassword, txtRegPasswordConfirm;
    private Button btnRegister, btnShowRegPassword;

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
        this.Text = "NovaBank • Güvenli Giriş";
        this.Width = 600; this.Height = 500;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 248, 255);

        // Login tab - Modern Design
        var pnlLogin = new Panel() { Left=50, Top=50, Width=500, Height=300, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        var lblLoginTitle = new Label() { Left=20, Top=20, Width=200, Height=30, Text="Giriş Yap", Font=new Font("Segoe UI", 16, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        
        var lblTc = new Label() { Left=20, Top=70, Width=100, Height=20, Text="TC Kimlik No:", Font=new Font("Segoe UI", 9) };
        txtLoginTc = new TextBox(){ Left=20, Top=95, Width=300, PlaceholderText="TC Kimlik No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        
        var lblPassword = new Label() { Left=20, Top=130, Width=100, Height=20, Text="Şifre:", Font=new Font("Segoe UI", 9) };
        txtLoginPassword = new TextBox(){ Left=20, Top=155, Width=300, PlaceholderText="Şifre", UseSystemPasswordChar=true, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        btnShowPassword = new Button(){ Left=330, Top=155, Width=30, Height=25, Text="👁", BackColor=Color.Transparent, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 12)};
        btnShowPassword.Click += BtnShowPassword_Click;
        
        btnLogin = new Button(){ Left=20, Top=200, Width=120, Height=40, Text="Giriş Yap", BackColor=Color.FromArgb(25, 118, 210), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 10, FontStyle.Bold)};
        btnLogin.Click += btnLogin_Click;
        
        pnlLogin.Controls.AddRange(new Control[]{lblLoginTitle, lblTc, txtLoginTc, lblPassword, txtLoginPassword, btnShowPassword, btnLogin});
        tabLogin.Controls.Add(pnlLogin);

        // Register tab - Modern Design
        var pnlRegister = new Panel() { Left=50, Top=50, Width=500, Height=400, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        var lblRegisterTitle = new Label() { Left=20, Top=20, Width=200, Height=30, Text="Kayıt Ol", Font=new Font("Segoe UI", 16, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        
        var lblRegTc = new Label() { Left=20, Top=60, Width=100, Height=20, Text="TC Kimlik No:", Font=new Font("Segoe UI", 9) };
        txtRegTc = new TextBox(){ Left=20, Top=85, Width=300, PlaceholderText="TC Kimlik No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        
        var lblRegAd = new Label() { Left=20, Top=120, Width=50, Height=20, Text="Ad:", Font=new Font("Segoe UI", 9) };
        txtRegAd = new TextBox(){ Left=20, Top=145, Width=140, PlaceholderText="Ad", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        
        var lblRegSoyad = new Label() { Left=180, Top=120, Width=50, Height=20, Text="Soyad:", Font=new Font("Segoe UI", 9) };
        txtRegSoyad = new TextBox(){ Left=180, Top=145, Width=140, PlaceholderText="Soyad", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        
        var lblRegEmail = new Label() { Left=20, Top=180, Width=100, Height=20, Text="E-posta:", Font=new Font("Segoe UI", 9) };
        txtRegEmail = new TextBox(){ Left=20, Top=205, Width=300, PlaceholderText="E-posta (opsiyonel)", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        
        var lblRegTel = new Label() { Left=20, Top=240, Width=100, Height=20, Text="Telefon:", Font=new Font("Segoe UI", 9) };
        txtRegTel = new TextBox(){ Left=20, Top=265, Width=300, PlaceholderText="Telefon (opsiyonel)", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        
        var lblRegPassword = new Label() { Left=20, Top=300, Width=100, Height=20, Text="Şifre:", Font=new Font("Segoe UI", 9) };
        txtRegPassword = new TextBox(){ Left=20, Top=325, Width=300, PlaceholderText="Şifre", UseSystemPasswordChar=true, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Font=new Font("Segoe UI", 10)};
        btnShowRegPassword = new Button(){ Left=330, Top=325, Width=30, Height=25, Text="👁", BackColor=Color.Transparent, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 12)};
        btnShowRegPassword.Click += BtnShowRegPassword_Click;
        
        btnRegister = new Button(){ Left=20, Top=370, Width=120, Height=40, Text="Kayıt Ol", BackColor=Color.FromArgb(76, 175, 80), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 10, FontStyle.Bold)};
        btnRegister.Click += btnRegister_Click;
        
        pnlRegister.Controls.AddRange(new Control[]{lblRegisterTitle, lblRegTc, txtRegTc, lblRegAd, txtRegAd, lblRegSoyad, txtRegSoyad, lblRegEmail, txtRegEmail, lblRegTel, txtRegTel, lblRegPassword, txtRegPassword, btnShowRegPassword, btnRegister});
        tabRegister.Controls.Add(pnlRegister);
    }
}
