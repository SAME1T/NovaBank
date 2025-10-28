namespace NovaBank.WinForms;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabs;
    private TabPage tabMyAccounts, tabDw, tabTransfer, tabReports;
    // My Accounts controls
    private TextBox txtAccCustomerId, txtAccountNo, txtOverdraft;
    private ComboBox cmbCurrency;
    private Button btnCreateAccount;
    private DataGridView gridAccounts;
    private Label lblWelcome, lblTotalBalance, lblAccountCount;
    private Panel pnlAccountSummary;
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
    // Menu
    private MenuStrip menuStrip;
    private ToolStripMenuItem mnuFile, mnuLogout;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        
        // Menu
        this.menuStrip = new MenuStrip();
        this.mnuFile = new ToolStripMenuItem("Dosya");
        this.mnuLogout = new ToolStripMenuItem("Çıkış Yap");
        this.mnuFile.DropDownItems.Add(this.mnuLogout);
        this.menuStrip.Items.Add(this.mnuFile);
        this.mnuLogout.Click += MnuLogout_Click;
        
        // Status Bar
        this.statusStrip = new StatusStrip();
        this.lblStatus = new ToolStripStatusLabel("NovaBank - Güvenli Bankacılık");
        this.statusStrip.Items.Add(this.lblStatus);
        
        this.tabs = new TabControl();
        this.tabMyAccounts = new TabPage("Hesaplarım");
        this.tabDw = new TabPage("Para İşlemleri");
        this.tabTransfer = new TabPage("Transfer");
        this.tabReports = new TabPage("Ekstreler");
        this.tabs.TabPages.AddRange(new TabPage[]{tabMyAccounts, tabDw, tabTransfer, tabReports});
        this.tabs.Dock = DockStyle.Fill;
        
        this.Controls.Add(this.tabs);
        this.Controls.Add(this.menuStrip);
        this.Controls.Add(this.statusStrip);
        
        this.Text = "NovaBank - Güvenli Bankacılık";
        this.Width = 1200; this.Height = 800;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 248, 255);

        // My Accounts - Modern Design
        pnlAccountSummary = new Panel() { Left=20, Top=20, Width=1140, Height=120, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        lblWelcome = new Label() { Left=20, Top=15, Width=400, Height=25, Text="Hoş Geldiniz", Font=new Font("Segoe UI", 14, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        lblTotalBalance = new Label() { Left=20, Top=45, Width=300, Height=20, Text="Toplam Bakiye: 0,00 TL", Font=new Font("Segoe UI", 10, FontStyle.Bold), ForeColor=Color.FromArgb(76, 175, 80) };
        lblAccountCount = new Label() { Left=20, Top=70, Width=200, Height=20, Text="Hesap Sayısı: 0", Font=new Font("Segoe UI", 9), ForeColor=Color.Gray };
        
        txtAccCustomerId = new TextBox(){ Left=20, Top=160, Width=120, PlaceholderText="Müşteri No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        txtAccountNo = new TextBox(){ Left=150, Top=160, Width=120, PlaceholderText="Hesap No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        cmbCurrency = new ComboBox(){ Left=280, Top=160, Width=100, DropDownStyle=ComboBoxStyle.DropDownList, BackColor=Color.White };
        txtOverdraft = new TextBox(){ Left=390, Top=160, Width=100, PlaceholderText="Ek Hesap Limiti", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        btnCreateAccount = new Button(){ Left=500, Top=158, Width=120, Height=25, Text="Yeni Hesap Aç", BackColor=Color.FromArgb(25, 118, 210), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 9, FontStyle.Bold) };
        btnCreateAccount.Click += btnCreateAccount_Click;
        
        gridAccounts = new DataGridView(){ Left=20, Top=200, Width=1140, Height=400, ReadOnly=true, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill, 
            BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, GridColor=Color.LightGray, 
            RowHeadersVisible=false, SelectionMode=DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows=false };
        gridAccounts.CellDoubleClick += GridAccounts_CellDoubleClick;
        gridAccounts.EnableHeadersVisualStyles = false;
        gridAccounts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 118, 210);
        gridAccounts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        gridAccounts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        
        pnlAccountSummary.Controls.AddRange(new Control[]{lblWelcome, lblTotalBalance, lblAccountCount});
        tabMyAccounts.Controls.AddRange(new Control[]{pnlAccountSummary, txtAccCustomerId, txtAccountNo, cmbCurrency, txtOverdraft, btnCreateAccount, gridAccounts});

        // Deposit/Withdraw - Modern Design
        var pnlDeposit = new Panel() { Left=20, Top=20, Width=560, Height=200, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        var lblDeposit = new Label() { Left=15, Top=15, Width=200, Height=25, Text="Para Yatırma", Font=new Font("Segoe UI", 12, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        txtDwAccountId = new TextBox(){ Left=15, Top=50, Width=120, PlaceholderText="Hesap No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        cmbDwCurrency = new ComboBox(){ Left=145, Top=50, Width=100, DropDownStyle=ComboBoxStyle.DropDownList, BackColor=Color.White };
        txtDepositAmount = new TextBox(){ Left=255, Top=50, Width=120, PlaceholderText="Tutar", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        txtDepositDesc = new TextBox(){ Left=15, Top=85, Width=300, PlaceholderText="Açıklama", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        btnDeposit = new Button(){ Left=15, Top=125, Width=120, Height=35, Text="Para Yatır", BackColor=Color.FromArgb(76, 175, 80), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 9, FontStyle.Bold) };
        btnDeposit.Click += btnDeposit_Click;
        
        var pnlWithdraw = new Panel() { Left=600, Top=20, Width=560, Height=200, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        var lblWithdraw = new Label() { Left=15, Top=15, Width=200, Height=25, Text="Para Çekme", Font=new Font("Segoe UI", 12, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        txtWithdrawAmount = new TextBox(){ Left=15, Top=50, Width=120, PlaceholderText="Tutar", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        txtWithdrawDesc = new TextBox(){ Left=15, Top=85, Width=300, PlaceholderText="Açıklama", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        btnWithdraw = new Button(){ Left=15, Top=125, Width=120, Height=35, Text="Para Çek", BackColor=Color.FromArgb(244, 67, 54), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 9, FontStyle.Bold) };
        btnWithdraw.Click += btnWithdraw_Click;
        
        pnlDeposit.Controls.AddRange(new Control[]{lblDeposit, txtDwAccountId, cmbDwCurrency, txtDepositAmount, txtDepositDesc, btnDeposit});
        pnlWithdraw.Controls.AddRange(new Control[]{lblWithdraw, txtWithdrawAmount, txtWithdrawDesc, btnWithdraw});
        tabDw.Controls.AddRange(new Control[]{pnlDeposit, pnlWithdraw});

        // Transfer - Modern Design
        var pnlTransfer = new Panel() { Left=20, Top=20, Width=1140, Height=300, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        var lblTransfer = new Label() { Left=15, Top=15, Width=200, Height=25, Text="Para Transferi", Font=new Font("Segoe UI", 12, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        
        var lblFrom = new Label() { Left=15, Top=50, Width=100, Height=20, Text="Gönderen Hesap:", Font=new Font("Segoe UI", 9) };
        txtFromId = new TextBox(){ Left=15, Top=75, Width=150, PlaceholderText="Hesap No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        
        var lblTo = new Label() { Left=180, Top=50, Width=100, Height=20, Text="Alıcı Hesap:", Font=new Font("Segoe UI", 9) };
        txtToId = new TextBox(){ Left=180, Top=75, Width=150, PlaceholderText="Hesap No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        
        var lblIban = new Label() { Left=345, Top=50, Width=100, Height=20, Text="Alıcı IBAN:", Font=new Font("Segoe UI", 9) };
        txtToIban = new TextBox(){ Left=345, Top=75, Width=200, PlaceholderText="TR00 0000 0000 0000 0000 0000 00", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        
        var lblAmount = new Label() { Left=15, Top=110, Width=100, Height=20, Text="Tutar:", Font=new Font("Segoe UI", 9) };
        txtAmount = new TextBox(){ Left=15, Top=135, Width=150, PlaceholderText="0,00", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        
        var lblCurrency = new Label() { Left=180, Top=110, Width=100, Height=20, Text="Para Birimi:", Font=new Font("Segoe UI", 9) };
        cmbTransCurrency = new ComboBox(){ Left=180, Top=135, Width=100, DropDownStyle=ComboBoxStyle.DropDownList, BackColor=Color.White };
        
        var lblDesc = new Label() { Left=295, Top=110, Width=100, Height=20, Text="Açıklama:", Font=new Font("Segoe UI", 9) };
        txtTransDesc = new TextBox(){ Left=295, Top=135, Width=250, PlaceholderText="Transfer açıklaması", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        
        btnInternalTransfer = new Button(){ Left=15, Top=180, Width=150, Height=40, Text="İç Transfer", BackColor=Color.FromArgb(25, 118, 210), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 9, FontStyle.Bold) };
        btnInternalTransfer.Click += btnInternalTransfer_Click;
        btnExternalTransfer = new Button(){ Left=180, Top=180, Width=150, Height=40, Text="EFT/FAST", BackColor=Color.FromArgb(255, 152, 0), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 9, FontStyle.Bold) };
        btnExternalTransfer.Click += btnExternalTransfer_Click;
        
        pnlTransfer.Controls.AddRange(new Control[]{lblTransfer, lblFrom, txtFromId, lblTo, txtToId, lblIban, txtToIban, lblAmount, txtAmount, lblCurrency, cmbTransCurrency, lblDesc, txtTransDesc, btnInternalTransfer, btnExternalTransfer});
        tabTransfer.Controls.Add(pnlTransfer);

        // Reports - Modern Design
        var pnlReports = new Panel() { Left=20, Top=20, Width=1140, Height=100, BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        var lblReports = new Label() { Left=15, Top=15, Width=200, Height=25, Text="Hesap Ekstreleri", Font=new Font("Segoe UI", 12, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210) };
        
        var lblAccount = new Label() { Left=15, Top=50, Width=80, Height=20, Text="Hesap No:", Font=new Font("Segoe UI", 9) };
        txtStmtAccountId = new TextBox(){ Left=15, Top=70, Width=120, PlaceholderText="Hesap No", BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle };
        
        var lblFromDate = new Label() { Left=150, Top=50, Width=80, Height=20, Text="Başlangıç:", Font=new Font("Segoe UI", 9) };
        dtFrom = new DateTimePicker(){ Left=150, Top=70, Width=120, Value = DateTime.Today.AddDays(-7), BackColor=Color.White };
        
        var lblToDate = new Label() { Left=285, Top=50, Width=80, Height=20, Text="Bitiş:", Font=new Font("Segoe UI", 9) };
        dtTo = new DateTimePicker(){ Left=285, Top=70, Width=120, Value = DateTime.Today, BackColor=Color.White };
        
        btnGetStatement = new Button(){ Left=420, Top=68, Width=150, Height=25, Text="Ekstre Getir", BackColor=Color.FromArgb(25, 118, 210), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Font=new Font("Segoe UI", 9, FontStyle.Bold) };
        btnGetStatement.Click += btnGetStatement_Click;
        
        gridStatement = new DataGridView(){ Left=20, Top=140, Width=1140, Height=400, ReadOnly=true, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill, 
            BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, GridColor=Color.LightGray, 
            RowHeadersVisible=false, SelectionMode=DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows=false };
        gridStatement.EnableHeadersVisualStyles = false;
        gridStatement.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 118, 210);
        gridStatement.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        gridStatement.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        
        lblTotals = new Label(){ Left=20, Top=560, Width=1140, Height=30, Text="Toplamlar", Font=new Font("Segoe UI", 10, FontStyle.Bold), ForeColor=Color.FromArgb(25, 118, 210), TextAlign=ContentAlignment.MiddleCenter };
        
        pnlReports.Controls.AddRange(new Control[]{lblReports, lblAccount, txtStmtAccountId, lblFromDate, dtFrom, lblToDate, dtTo, btnGetStatement});
        tabReports.Controls.AddRange(new Control[]{pnlReports, gridStatement, lblTotals});

        // Load events
        this.Load += FrmMain_Load;
    }
}
