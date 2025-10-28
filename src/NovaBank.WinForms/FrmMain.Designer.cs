namespace NovaBank.WinForms;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabs;
    private TabPage tabCustomers, tabAccounts, tabDw, tabTransfer, tabReports;
    // Customers controls
    private TextBox txtTc, txtName, txtSurname, txtEmail, txtPhone, txtCustomerIdGet, txtCustomerDetail;
    private Button btnCreateCustomer, btnGetCustomer;
    // Accounts controls
    private TextBox txtAccCustomerId, txtAccountNo, txtOverdraft;
    private ComboBox cmbCurrency;
    private Button btnCreateAccount;
    // Deposit/Withdraw
    private TextBox txtDwAccountId, txtDepositAmount, txtDepositDesc, txtWithdrawAmount, txtWithdrawDesc;
    private ComboBox cmbDwCurrency;
    private Button btnDeposit, btnWithdraw;
    // Transfer
    private TextBox txtFromId, txtToId, txtAmount, txtTransDesc, txtToIban;
    private ComboBox cmbTransCurrency;
    private Button btnInternalTransfer, btnExternalTransfer;
    // Reports
    private TextBox txtStmtAccountId;
    private DateTimePicker dtFrom, dtTo;
    private Button btnGetStatement;
    private DataGridView gridStatement;
    private Label lblTotals;
    // Accounts grid
    private DataGridView gridAccounts;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.tabs = new TabControl();
        this.tabCustomers = new TabPage("Müşteriler");
        this.tabAccounts = new TabPage("Hesaplar");
        this.tabDw = new TabPage("Yatır/Çek");
        this.tabTransfer = new TabPage("Transfer");
        this.tabReports = new TabPage("Raporlar");
        this.tabs.TabPages.AddRange(new TabPage[]{tabCustomers, tabAccounts, tabDw, tabTransfer, tabReports});
        this.tabs.Dock = DockStyle.Fill;
        this.Controls.Add(this.tabs);
        this.Text = "NovaBank Client";
        this.Width = 1000; this.Height = 700;

        // Customers
        txtTc = new TextBox(){ Left=20, Top=20, Width=180, PlaceholderText="TC Kimlik No"};
        txtName = new TextBox(){ Left=210, Top=20, Width=120, PlaceholderText="Ad"};
        txtSurname = new TextBox(){ Left=340, Top=20, Width=120, PlaceholderText="Soyad"};
        txtEmail = new TextBox(){ Left=470, Top=20, Width=180, PlaceholderText="E-posta"};
        txtPhone = new TextBox(){ Left=660, Top=20, Width=140, PlaceholderText="Telefon"};
        btnCreateCustomer = new Button(){ Left=820, Top=18, Width=120, Text="Oluştur"};
        btnCreateCustomer.Click += btnCreateCustomer_Click;
        txtCustomerIdGet = new TextBox(){ Left=20, Top=60, Width=260, PlaceholderText="Müşteri Id (Guid)"};
        btnGetCustomer = new Button(){ Left=290, Top=58, Width=120, Text="Getir"};
        btnGetCustomer.Click += btnGetCustomer_Click;
        txtCustomerDetail = new TextBox(){ Left=20, Top=100, Width=920, ReadOnly=true};
        tabCustomers.Controls.AddRange(new Control[]{txtTc,txtName,txtSurname,txtEmail,txtPhone,btnCreateCustomer,txtCustomerIdGet,btnGetCustomer,txtCustomerDetail});

        // Accounts
        txtAccCustomerId = new TextBox(){ Left=20, Top=20, Width=260, PlaceholderText="Müşteri Id"};
        txtAccountNo = new TextBox(){ Left=290, Top=20, Width=120, PlaceholderText="Hesap No"};
        cmbCurrency = new ComboBox(){ Left=420, Top=20, Width=100, DropDownStyle=ComboBoxStyle.DropDownList};
        txtOverdraft = new TextBox(){ Left=530, Top=20, Width=100, PlaceholderText="Ek Hesap Limiti"};
        btnCreateAccount = new Button(){ Left=640, Top=18, Width=90, Text="Hesap Aç"};
        btnCreateAccount.Click += btnCreateAccount_Click;
        gridAccounts = new DataGridView(){ Left=20, Top=60, Width=920, Height=520, ReadOnly=true, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
        tabAccounts.Controls.AddRange(new Control[]{txtAccCustomerId,txtAccountNo,cmbCurrency,txtOverdraft,btnCreateAccount,gridAccounts});

        // Deposit/Withdraw
        txtDwAccountId = new TextBox(){ Left=20, Top=20, Width=260, PlaceholderText="Hesap Id"};
        cmbDwCurrency = new ComboBox(){ Left=290, Top=20, Width=100, DropDownStyle=ComboBoxStyle.DropDownList};
        txtDepositAmount = new TextBox(){ Left=400, Top=20, Width=100, PlaceholderText="Para Yatır"};
        txtDepositDesc = new TextBox(){ Left=510, Top=20, Width=200, PlaceholderText="Açıklama"};
        btnDeposit = new Button(){ Left=720, Top=18, Width=80, Text="Para Yatır"};
        btnDeposit.Click += btnDeposit_Click;
        txtWithdrawAmount = new TextBox(){ Left=400, Top=60, Width=100, PlaceholderText="Para Çek"};
        txtWithdrawDesc = new TextBox(){ Left=510, Top=60, Width=200, PlaceholderText="Açıklama"};
        btnWithdraw = new Button(){ Left=720, Top=58, Width=80, Text="Para Çek"};
        btnWithdraw.Click += btnWithdraw_Click;
        tabDw.Controls.AddRange(new Control[]{txtDwAccountId,cmbDwCurrency,txtDepositAmount,txtDepositDesc,btnDeposit,txtWithdrawAmount,txtWithdrawDesc,btnWithdraw});

        // Transfer
        txtFromId = new TextBox(){ Left=20, Top=20, Width=260, PlaceholderText="Gönderen Hesap Id"};
        txtToId = new TextBox(){ Left=290, Top=20, Width=260, PlaceholderText="Alıcı Hesap Id"};
        txtToIban = new TextBox(){ Left=560, Top=20, Width=200, PlaceholderText="Alıcı IBAN"};
        cmbTransCurrency = new ComboBox(){ Left=20, Top=60, Width=100, DropDownStyle=ComboBoxStyle.DropDownList};
        txtAmount = new TextBox(){ Left=130, Top=60, Width=100, PlaceholderText="Tutar"};
        txtTransDesc = new TextBox(){ Left=240, Top=60, Width=300, PlaceholderText="Açıklama"};
        btnInternalTransfer = new Button(){ Left=560, Top=58, Width=140, Text="İç Transfer"};
        btnInternalTransfer.Click += btnInternalTransfer_Click;
        btnExternalTransfer = new Button(){ Left=710, Top=58, Width=140, Text="EFT/FAST"};
        btnExternalTransfer.Click += btnExternalTransfer_Click;
        tabTransfer.Controls.AddRange(new Control[]{txtFromId,txtToId,txtToIban,cmbTransCurrency,txtAmount,txtTransDesc,btnInternalTransfer,btnExternalTransfer});

        // Reports
        txtStmtAccountId = new TextBox(){ Left=20, Top=20, Width=260, PlaceholderText="Hesap Id"};
        dtFrom = new DateTimePicker(){ Left=290, Top=20, Width=180, Value = DateTime.Today.AddDays(-7)};
        dtTo = new DateTimePicker(){ Left=480, Top=20, Width=180, Value = DateTime.Today};
        btnGetStatement = new Button(){ Left=670, Top=18, Width=160, Text="Hesap Ekstresi Getir"};
        btnGetStatement.Click += btnGetStatement_Click;
        gridStatement = new DataGridView(){ Left=20, Top=60, Width=920, Height=520, ReadOnly=true, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
        lblTotals = new Label(){ Left=20, Top=590, Width=920, Text="Toplamlar"};
        tabReports.Controls.AddRange(new Control[]{txtStmtAccountId,dtFrom,dtTo,btnGetStatement,gridStatement,lblTotals});

        // Load events
        this.Load += FrmMain_Load;
    }
}
