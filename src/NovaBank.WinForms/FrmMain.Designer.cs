using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraBars;
using NovaBank.Contracts.Accounts;
using NovaBank.WinForms.Controls;

namespace NovaBank.WinForms;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null;
    private XtraTabControl tabs;
    private XtraTabPage tabMyAccounts, tabDw, tabTransfer, tabReports, tabSettings, tabExchangeRates, tabAdmin, tabCards, tabBills;
    // Sol Sidebar
    private PanelControl pnlSidebar;
    private SimpleButton btnSidebarAccounts, btnSidebarMoneyOps, btnSidebarTransfer, btnSidebarCards, btnSidebarBills, btnSidebarStatements, btnSidebarFx, btnSidebarSettings, btnSidebarAdmin;
    private LabelControl lblSidebarTooltip;
    // My Accounts controls
    private TextEdit txtAccCustomerId, txtAccountNo, txtOverdraft;
    private ComboBoxEdit cmbCurrency;
    private SimpleButton btnCreateAccount;
    private GridControl gridAccounts, gridMyCards, gridCardsMain;
    private GridView gridAccountsView, gridMyCardsView, gridCardsMainView;
    private LabelControl lblWelcome, lblTotalBalance, lblAccountCount, lblCardsTitle;
    private PanelControl pnlAccountSummary;
    // Hesaplarım - Yeni Kart Görünümü
    private FlowLayoutPanel pnlAccountCards;
    private PanelControl pnlQuickSummary, pnlRecentTransactions, pnlQuickActions;
    private LabelControl lblTotalTry, lblTotalUsd, lblTotalEur, lblRecentTitle;
    private SimpleButton btnQuickDeposit, btnQuickWithdraw, btnQuickTransfer, btnQuickFx;
    private GridControl gridRecentTransactions;
    private GridView gridRecentTransactionsView;
    // Deposit/Withdraw
    private TextEdit txtDepositAmount, txtDepositDesc, txtWithdrawAmount, txtWithdrawDesc;
    private ComboBoxEdit cmbDwCurrency;
    private LookUpEdit cmbDwAccount, cmbCardPayAccount;
    private LabelControl lblDwIban, lblDwCurrency, lblDwBalance, lblDwOverdraft, lblDwAvailable;
    private SimpleButton btnDeposit, btnWithdraw;
    // Transfer
    private TextEdit txtToId, txtAmount, txtTransDesc, txtToIban;
    private ComboBoxEdit cmbTransCurrency, cmbTransferAccount, cmbRecipientAccount, cmbOwnRecipientAccount;
    private SimpleButton btnInternalTransfer, btnExternalTransfer, btnOwnAccountTransfer;
    private LabelControl lblSenderBind, lblRecipientName, lblOwnRecipientAccount, lblOwnRecipientInfo, lblCommissionInfo, lblIban;
    private RadioButton rdoOwnAccounts, rdoExternalIban;
    // Reports
    private TextEdit txtStmtAccountId;
    private LookUpEdit cmbStmtAccount;
    private DateEdit dtFrom, dtTo;
    private SimpleButton btnGetStatement;
    private GridControl gridStatement;
    private GridView gridStatementView;
    private LabelControl lblTotals;
    // Status
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    // Exchange Rates
    private DataGridView dgvRates;
    private PanelControl pnlExchangeTop;
    private LabelControl lblExchangeInfo;
    private SimpleButton btnRefreshRates;
    // Currency Exchange (Döviz Al/Sat)
    private PanelControl pnlFxBuy, pnlFxSell, pnlFxPositions;
    private ComboBoxEdit cmbFxBuyCurrency, cmbFxSellCurrency;
    private ComboBoxEdit cmbFxBuyFromTry, cmbFxBuyToForeign, cmbFxSellFromForeign, cmbFxSellToTry;
    private TextEdit txtFxBuyAmount, txtFxSellAmount;
    private LabelControl lblFxBuyRate, lblFxSellRate, lblFxBuyCalc, lblFxSellCalc;
    private SimpleButton btnFxBuy, btnFxSell;
    private GridControl gridFxPositions;
    private GridView gridFxPositionsView;
    private LabelControl lblFxPositionsSummary;
    // Settings/Profile
    private LabelControl lblProfName, lblProfNationalId, lblProfEmail, lblProfPhone;
    // Admin
    private TextEdit txtAdminSearch;
    private SimpleButton btnAdminSearch;
    private GridControl gridAdminCustomers, gridAdminAccounts;
    private GridView gridAdminCustomersView, gridAdminAccountsView;
    private TextEdit txtAdminOverdraft;
    private ComboBoxEdit cmbAdminStatus;
    private SimpleButton btnAdminUpdateOverdraft, btnAdminUpdateStatus;
    private CheckEdit chkAdminIsActive;
    private SimpleButton btnAdminSaveActive, btnAdminResetPassword;
    // Audit Log
    private DateEdit dtAuditFrom, dtAuditTo;
    private TextEdit txtAuditSearch;
    private ComboBoxEdit cmbAuditAction, cmbAuditSuccess;
    private SimpleButton btnAuditLoad;
    private GridControl gridAuditLogs;
    private GridView gridAuditLogsView;
    // Pending Approvals
    private GridControl gridPendingApprovals;
    private GridView gridPendingApprovalsView;
    private SimpleButton btnApproveCustomer, btnRejectCustomer, btnRefreshPending;
    private CheckEdit chkAdminIsApproved;
    private LabelControl lblPendingTitle;
    // Credit Cards
    private GridControl gridCardApplications;
    private GridView gridCardApplicationsView;
    private SimpleButton btnApplyCard, btnPayCardDebt, btnRefreshCards;
    private TextEdit txtCardLimit, txtCardIncome, txtCardPaymentAmount;
    // Bills
    private ComboBoxEdit cmbBillInstitution, cmbBillAccount;
    private TextEdit txtSubscriberNo;
    private SimpleButton btnInquireBill, btnPayBill;
    private LabelControl lblBillAmount, lblBillDueDate;
    private GridControl gridBillHistory;
    private GridView gridBillHistoryView;
    // Admin Credit Cards
    private XtraTabControl tabAdminSub;
    private XtraTabPage tabAdminUsers, tabAdminCards, tabAdminAudit, tabAdminBills, tabAdminBranchManager;
    private GridControl gridAdminCardApplications, gridAdminInstitutions;
    private GridView gridAdminCardApplicationsView, gridAdminInstitutionsView;
    private SimpleButton btnApproveCardApp, btnRejectCardApp, btnRefreshCardApps, btnAddInstitution, btnDeleteInstitution, btnRefreshInstitutions;
    private TextEdit txtInstCode, txtInstName, txtInstLogo;
    private ComboBoxEdit cmbInstCategory;
    // Branch Manager (Şube Yönetici) Yönetimi
    private TextEdit txtBmNationalId, txtBmFirstName, txtBmLastName, txtBmEmail, txtBmPhone, txtBmPassword;
    private SimpleButton btnCreateBranchManager;
    private GridControl gridBranchManagers;
    private GridView gridBranchManagersView;
    // Admin - Hesap/Müşteri Silme butonları
    private SimpleButton btnDeleteAccount, btnDeleteCustomer;
    // Chatbot
    private ChatbotPanel chatbotPanel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        
        // Status Bar - Modern Design
        this.statusStrip = new StatusStrip();
        this.statusStrip.BackColor = Color.FromArgb(25, 118, 210);
        this.statusStrip.ForeColor = Color.White;
        this.lblStatus = new ToolStripStatusLabel("🔒 NovaBank - Güvenli Dijital Bankacılık");
        this.lblStatus.ForeColor = Color.White;
        this.lblStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        this.lblStatus.Spring = true;
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        var lblNotifications = new ToolStripStatusLabel("🔔 Bildirimler: 0");
        lblNotifications.Name = "lblNotifications";
        lblNotifications.ForeColor = Color.Yellow;
        lblNotifications.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        this.statusStrip.Items.Add(this.lblStatus);
        this.statusStrip.Items.Add(lblNotifications);
        
        this.tabs = new XtraTabControl();
        this.tabMyAccounts = new XtraTabPage();
        this.tabDw = new XtraTabPage();
        this.tabTransfer = new XtraTabPage();
        this.tabReports = new XtraTabPage();
        this.tabExchangeRates = new XtraTabPage();
        this.tabSettings = new XtraTabPage();
        
        this.tabMyAccounts.Text = "Hesaplarım";
        this.tabDw.Text = "Para İşlemleri";
        this.tabTransfer.Text = "Transfer";
        this.tabReports.Text = "Ekstreler";
        this.tabExchangeRates.Text = "Döviz Kurları";
        this.tabSettings.Text = "Ayarlar / Profil";
        
        this.tabAdmin = new XtraTabPage();
        this.tabAdmin.Text = "Yönetim";
        this.tabCards = new XtraTabPage();
        this.tabCards.Text = "💳 Kartlarım";
        this.tabBills = new XtraTabPage();
        this.tabBills.Text = "📄 Fatura Öde";
        // tabAdmin sadece admin kullanıcılar için görünür olacak - ApplyRoleBasedUI() ile kontrol edilir
        // Başlangıçta tab listesine eklenmez, admin login olduğunda eklenecek
        this.tabs.TabPages.AddRange(new XtraTabPage[] { tabMyAccounts, tabDw, tabTransfer, tabCards, tabBills, tabReports, tabExchangeRates, tabSettings });
        this.tabs.Dock = DockStyle.None; // Manuel konumlandırma için Dock kapalı
        this.tabs.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top;
        this.tabs.AppearancePage.Header.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.tabs.AppearancePage.HeaderActive.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.tabs.AppearancePage.HeaderActive.ForeColor = Color.FromArgb(25, 118, 210);
        this.tabs.AppearancePage.Header.ForeColor = Color.FromArgb(100, 100, 100);
        this.tabs.LookAndFeel.UseDefaultLookAndFeel = false;
        this.tabs.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        // Tab header'ları gizle - sidebar kullanacağız
        this.tabs.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;
        
        // Ana içerik alanını başlangıçta sağa kaydır (sidebar genişliği kadar)
        var sidebarWidth = 180; // Sidebar genişliği
        this.tabs.Location = new Point(sidebarWidth, 0);
        this.tabs.Size = new Size(this.Width - sidebarWidth, this.Height - statusStrip.Height);
        this.tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        
        this.Controls.Add(this.tabs);
        this.Controls.Add(this.statusStrip);
        
        this.Text = "NovaBank - Güvenli Bankacılık";
        this.Width = 1300; 
        this.Height = 850;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;
        
        // Chatbot Panel - Sağ altta açılır/kapanır panel (form boyutlandırmasından sonra ekleniyor)
        chatbotPanel = new ChatbotPanel();
        this.Controls.Add(chatbotPanel);
        // Panelin sağ altta görünmesi için konumunu ayarla
        this.Resize += (s, e) => {
            if (chatbotPanel != null && statusStrip != null)
            {
                chatbotPanel.Location = new Point(this.Width - chatbotPanel.Width - 10, this.Height - chatbotPanel.Height - statusStrip.Height - 10);
            }
        };
        // İlk konumlandırma
        if (statusStrip != null)
        {
            chatbotPanel.Location = new Point(this.Width - chatbotPanel.Width - 10, this.Height - chatbotPanel.Height - statusStrip.Height - 10);
        }
        // Panelin en üstte görünmesi için BringToFront
        chatbotPanel.BringToFront();

        // ========== HESAPLARIM - MODERN KART GÖRÜNÜMÜ ==========
        
        // Üst Özet Paneli - Hoş Geldiniz + Para Birimi Toplamları
        pnlAccountSummary = new PanelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(1240, 100),
            Appearance = { BackColor = Color.FromArgb(240, 240, 240), BorderColor = Color.FromArgb(240, 240, 240) }
        };
        pnlAccountSummary.Appearance.Options.UseBackColor = true;
        pnlAccountSummary.Appearance.Options.UseBorderColor = true;
        pnlAccountSummary.LookAndFeel.UseDefaultLookAndFeel = false;
        
        lblWelcome = new LabelControl() 
        { 
            Location = new Point(30, 15), 
            Size = new Size(600, 30), 
            Text = "👋 Hoş Geldiniz, Admin User", 
            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.Black },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblWelcome.Appearance.Options.UseForeColor = true;
        lblWelcome.Appearance.Options.UseFont = true;
        
        lblTotalTry = new LabelControl() 
        { 
            Location = new Point(30, 55), 
            Size = new Size(200, 30), 
            Text = "₺ TRY: 96.549,28", 
            Appearance = { Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Black },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblTotalTry.Appearance.Options.UseForeColor = true;
        lblTotalTry.Appearance.Options.UseFont = true;
        
        lblTotalUsd = new LabelControl() 
        { 
            Location = new Point(250, 55), 
            Size = new Size(200, 30), 
            Text = "$ USD: 80,00", 
            Appearance = { Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Black },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblTotalUsd.Appearance.Options.UseForeColor = true;
        lblTotalUsd.Appearance.Options.UseFont = true;
        
        lblTotalEur = new LabelControl() 
        { 
            Location = new Point(470, 55), 
            Size = new Size(200, 30), 
            Text = "€ EUR: 0,00", 
            Appearance = { Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Black },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblTotalEur.Appearance.Options.UseForeColor = true;
        lblTotalEur.Appearance.Options.UseFont = true;
        
        lblAccountCount = new LabelControl() 
        { 
            Location = new Point(700, 55), 
            Size = new Size(200, 25), 
            Text = "📊 3 Hesap", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Black },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblAccountCount.Appearance.Options.UseForeColor = true;
        lblAccountCount.Appearance.Options.UseFont = true;
        
        pnlAccountSummary.Controls.AddRange(new Control[] { lblWelcome, lblTotalTry, lblTotalUsd, lblTotalEur, lblAccountCount });
        
        // Hızlı Aksiyon Butonları
        pnlQuickActions = new PanelControl()
        {
            Location = new Point(20, 130),
            Size = new Size(1240, 60),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        btnQuickDeposit = new SimpleButton()
        {
            Location = new Point(20, 12),
            Size = new Size(150, 36),
            Text = "💰 Para Yatır",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnQuickDeposit.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnQuickDeposit.Click += BtnQuickDeposit_Click;
        
        btnQuickWithdraw = new SimpleButton()
        {
            Location = new Point(185, 12),
            Size = new Size(150, 36),
            Text = "💸 Para Çek",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnQuickWithdraw.Appearance.BackColor = Color.FromArgb(244, 67, 54);
        btnQuickWithdraw.Click += BtnQuickWithdraw_Click;
        
        btnQuickTransfer = new SimpleButton()
        {
            Location = new Point(350, 12),
            Size = new Size(150, 36),
            Text = "↔️ Transfer",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnQuickTransfer.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        btnQuickTransfer.Click += BtnQuickTransfer_Click;
        
        btnQuickFx = new SimpleButton()
        {
            Location = new Point(515, 12),
            Size = new Size(150, 36),
            Text = "💱 Döviz Al/Sat",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnQuickFx.Appearance.BackColor = Color.FromArgb(156, 39, 176);
        btnQuickFx.Click += BtnQuickFx_Click;
        
        // Yeni Hesap Aç Butonu (Sağ tarafta)
        btnCreateAccount = new SimpleButton() 
        { 
            Location = new Point(1030, 12), 
            Size = new Size(190, 36), 
            Text = "+ Yeni Hesap Aç",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnCreateAccount.Appearance.BackColor = Color.FromArgb(255, 152, 0);
        btnCreateAccount.Click += btnCreateAccount_Click;
        
        // Hidden controls for create account
        txtAccCustomerId = new TextEdit() { Visible = false };
        cmbCurrency = new ComboBoxEdit() { Visible = false };
        cmbCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        txtOverdraft = new TextEdit() { Visible = false };
        
        pnlQuickActions.Controls.AddRange(new Control[] { btnQuickDeposit, btnQuickWithdraw, btnQuickTransfer, btnQuickFx, btnCreateAccount });
        
        // Hesap Kartları - FlowLayoutPanel ile dinamik kart görünümü
        pnlAccountCards = new FlowLayoutPanel()
        {
            Location = new Point(20, 200),
            Size = new Size(820, 320),
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(5)
        };
        
        // Son 5 İşlem Paneli
        pnlRecentTransactions = new PanelControl()
        {
            Location = new Point(850, 200),
            Size = new Size(410, 320),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        lblRecentTitle = new LabelControl()
        {
            Location = new Point(15, 12),
            Text = "📜 Son İşlemler",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        gridRecentTransactions = new GridControl()
        {
            Location = new Point(10, 45),
            Size = new Size(390, 260)
        };
        gridRecentTransactionsView = new GridView();
        gridRecentTransactions.MainView = gridRecentTransactionsView;
        gridRecentTransactionsView.OptionsBehavior.Editable = false;
        gridRecentTransactionsView.OptionsView.ShowGroupPanel = false;
        gridRecentTransactionsView.OptionsView.ShowColumnHeaders = true;
        gridRecentTransactionsView.OptionsView.RowAutoHeight = true;
        gridRecentTransactionsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(245, 245, 245);
        gridRecentTransactionsView.Appearance.HeaderPanel.ForeColor = Color.FromArgb(60, 60, 60);
        gridRecentTransactionsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        gridRecentTransactionsView.Appearance.Row.Font = new Font("Segoe UI", 9);
        
        pnlRecentTransactions.Controls.AddRange(new Control[] { lblRecentTitle, gridRecentTransactions });
        
        // Eski grid (gizli tutulacak, arka plan işle için)
        gridAccounts = new GridControl() { Visible = false };
        gridAccountsView = new GridView();
        gridAccounts.MainView = gridAccountsView;
        gridAccountsView.DoubleClick += GridAccounts_CellDoubleClick;
        gridAccountsView.SelectionChanged += GridAccounts_SelectionChanged;
        
        // Eski alanlar (uyumluluk için)
        lblTotalBalance = new LabelControl() { Visible = false };
        lblCardsTitle = new LabelControl() { Visible = false };
        gridMyCards = new GridControl() { Visible = false };
        gridMyCardsView = new GridView();
        gridMyCards.MainView = gridMyCardsView;
        gridMyCardsView.DoubleClick += GridMyCards_CellDoubleClick;
        
        tabMyAccounts.Controls.AddRange(new Control[] { pnlAccountSummary, pnlQuickActions, pnlAccountCards, pnlRecentTransactions, gridAccounts, txtAccCustomerId, cmbCurrency, txtOverdraft });

        // Deposit/Withdraw - Modern Design
        var pnlDeposit = new PanelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(600, 280),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblDeposit = new LabelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(250, 30), 
            Text = "💰 Para Yatırma", 
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80) }
        };
        var lblDepInfo = new LabelControl() 
        { 
            Location = new Point(20, 55), 
            Size = new Size(500, 22), 
            Text = "Hesap seçin ve para yatırın", 
            Appearance = { Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(100, 100, 100) }
        };
        var lblDwAccountLabel = new LabelControl()
        {
            Location = new Point(20, 85),
            Size = new Size(100, 22),
            Text = "Hesap:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbDwAccount = new LookUpEdit() 
        { 
            Location = new Point(20, 107), 
            Size = new Size(350, 38)
        };
        cmbDwAccount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbDwAccount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        cmbDwAccount.Properties.NullText = "Hesap seçin...";
        cmbDwAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        
        // Hesap bilgileri label'ları
        lblDwIban = new LabelControl()
        {
            Location = new Point(390, 85),
            Size = new Size(200, 22),
            Text = "IBAN: -",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80) }
        };
        lblDwCurrency = new LabelControl()
        {
            Location = new Point(390, 107),
            Size = new Size(200, 22),
            Text = "Para Birimi: -",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80) }
        };
        lblDwBalance = new LabelControl()
        {
            Location = new Point(390, 129),
            Size = new Size(200, 22),
            Text = "Bakiye: -",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80) }
        };
        lblDwOverdraft = new LabelControl()
        {
            Location = new Point(390, 151),
            Size = new Size(200, 22),
            Text = "Ek Hesap Limiti: -",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80) }
        };
        lblDwAvailable = new LabelControl()
        {
            Location = new Point(390, 173),
            Size = new Size(200, 22),
            Text = "Kullanılabilir: -",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        // Currency label ve dropdown gizle
        var lblDepCurrency = new LabelControl()
        {
            Location = new Point(20, 90),
            Size = new Size(100, 22),
            Text = "Para Birimi:",
            Visible = false,
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbDwCurrency = new ComboBoxEdit() 
        { 
            Location = new Point(20, 112), 
            Size = new Size(150, 38),
            Visible = false
        };
        cmbDwCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbDwCurrency.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbDwCurrency.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        var lblDepAmount = new LabelControl()
        {
            Location = new Point(20, 155),
            Size = new Size(80, 22),
            Text = "Tutar:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtDepositAmount = new TextEdit() 
        { 
            Location = new Point(20, 177), 
            Size = new Size(180, 38)
        };
        txtDepositAmount.Properties.NullValuePrompt = "0,00";
        txtDepositAmount.Properties.NullValuePromptShowForEmptyValue = true;
        txtDepositAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtDepositAmount.Properties.Mask.EditMask = "n2";
        txtDepositAmount.Properties.Mask.UseMaskAsDisplayFormat = true;
        txtDepositAmount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        txtDepositAmount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        txtDepositAmount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        var lblDepDesc = new LabelControl()
        {
            Location = new Point(20, 225),
            Size = new Size(100, 22),
            Text = "Açıklama:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtDepositDesc = new TextEdit() 
        { 
            Location = new Point(20, 247), 
            Size = new Size(350, 38)
        };
        txtDepositDesc.Properties.NullValuePrompt = "İşlem açıklaması";
        txtDepositDesc.Properties.NullValuePromptShowForEmptyValue = true;
        txtDepositDesc.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtDepositDesc.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        btnDeposit = new SimpleButton() 
        { 
            Location = new Point(20, 300), 
            Size = new Size(350, 42), 
            Text = "✓ Para Yatır",
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnDeposit.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnDeposit.AppearanceHovered.BackColor = Color.FromArgb(69, 160, 73);
        btnDeposit.AppearancePressed.BackColor = Color.FromArgb(56, 142, 60);
        btnDeposit.Click += btnDeposit_Click;
        
        var pnlWithdraw = new PanelControl() 
        { 
            Location = new Point(640, 20), 
            Size = new Size(600, 360),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblWithdraw = new LabelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(250, 30), 
            Text = "💸 Para Çekme", 
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(244, 67, 54) }
        };
        var lblWithdrawInfo = new LabelControl()
        {
            Location = new Point(20, 55),
            Size = new Size(500, 22),
            Text = "Seçili hesaptan para çekin",
            Appearance = { Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(100, 100, 100) }
        };
        var lblWithdrawAmount = new LabelControl()
        {
            Location = new Point(20, 155),
            Size = new Size(80, 22),
            Text = "Tutar:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtWithdrawAmount = new TextEdit() 
        { 
            Location = new Point(20, 177), 
            Size = new Size(180, 38)
        };
        txtWithdrawAmount.Properties.NullValuePrompt = "0,00";
        txtWithdrawAmount.Properties.NullValuePromptShowForEmptyValue = true;
        txtWithdrawAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtWithdrawAmount.Properties.Mask.EditMask = "n2";
        txtWithdrawAmount.Properties.Mask.UseMaskAsDisplayFormat = true;
        txtWithdrawAmount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        txtWithdrawAmount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        txtWithdrawAmount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        var lblWithdrawDesc = new LabelControl()
        {
            Location = new Point(20, 225),
            Size = new Size(100, 22),
            Text = "Açıklama:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtWithdrawDesc = new TextEdit() 
        { 
            Location = new Point(20, 247), 
            Size = new Size(350, 38)
        };
        txtWithdrawDesc.Properties.NullValuePrompt = "İşlem açıklaması";
        txtWithdrawDesc.Properties.NullValuePromptShowForEmptyValue = true;
        txtWithdrawDesc.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtWithdrawDesc.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        btnWithdraw = new SimpleButton() 
        { 
            Location = new Point(20, 300), 
            Size = new Size(350, 42), 
            Text = "✓ Para Çek",
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnWithdraw.Appearance.BackColor = Color.FromArgb(244, 67, 54);
        btnWithdraw.AppearanceHovered.BackColor = Color.FromArgb(229, 57, 53);
        btnWithdraw.AppearancePressed.BackColor = Color.FromArgb(211, 47, 47);
        btnWithdraw.Click += btnWithdraw_Click;
        
        pnlDeposit.Size = new Size(600, 360); // Panel boyutunu artır
        pnlDeposit.Controls.AddRange(new Control[] { 
            lblDeposit, lblDepInfo, lblDwAccountLabel, cmbDwAccount, 
            lblDwIban, lblDwCurrency, lblDwBalance, lblDwOverdraft, lblDwAvailable,
            lblDepCurrency, cmbDwCurrency, lblDepAmount, txtDepositAmount, lblDepDesc, txtDepositDesc, btnDeposit 
        });
        pnlWithdraw.Controls.AddRange(new Control[] { lblWithdraw, lblWithdrawInfo, lblWithdrawAmount, txtWithdrawAmount, lblWithdrawDesc, txtWithdrawDesc, btnWithdraw });
        tabDw.Controls.AddRange(new Control[] { pnlDeposit, pnlWithdraw });

        // Transfer - Modern Design
        var pnlTransfer = new PanelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(1240, 520),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblTransfer = new LabelControl() 
        { 
            Location = new Point(25, 20), 
            Size = new Size(300, 32), 
            Text = "💸 Para Transferi", 
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        // ===== TRANSFER TİPİ SEÇİMİ =====
        var lblTransferType = new LabelControl()
        {
            Location = new Point(25, 65),
            Size = new Size(130, 22),
            Text = "Transfer Tipi:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        
        rdoOwnAccounts = new RadioButton()
        {
            Location = new Point(160, 62),
            Size = new Size(230, 28),
            Text = "🔄 Kendi Hesaplarım",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(76, 175, 80),
            Checked = true
        };
        rdoOwnAccounts.CheckedChanged += RdoTransferType_CheckedChanged;
        
        rdoExternalIban = new RadioButton()
        {
            Location = new Point(410, 62),
            Size = new Size(200, 28),
            Text = "📤 IBAN'a Transfer",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 118, 210)
        };
        rdoExternalIban.CheckedChanged += RdoTransferType_CheckedChanged;
        
        // ===== GÖNDEREN HESAP =====
        var lblTransferAccount = new LabelControl() 
        { 
            Location = new Point(25, 110), 
            Size = new Size(150, 22), 
            Text = "Gönderen Hesap:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbTransferAccount = new ComboBoxEdit() 
        { 
            Location = new Point(25, 135), 
            Size = new Size(400, 38)
        };
        cmbTransferAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbTransferAccount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbTransferAccount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        cmbTransferAccount.EditValueChanged += CmbTransferAccount_EditValueChanged;
        
        lblSenderBind = new LabelControl() 
        { 
            Location = new Point(440, 138), 
            Size = new Size(750, 30), 
            Text = "📤 TR00 0000 0000 0000 0000 0000 00 - USD | Bakiye: 80,00 | Kullanılabilir: 10.080,00", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        // ===== KENDİ HESAPLARIM ARASI - ALICI HESAP =====
        lblOwnRecipientAccount = new LabelControl()
        {
            Location = new Point(25, 195),
            Size = new Size(200, 22),
            Text = "Alıcı Hesabım:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbOwnRecipientAccount = new ComboBoxEdit()
        {
            Location = new Point(25, 220),
            Size = new Size(400, 38)
        };
        cmbOwnRecipientAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbOwnRecipientAccount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbOwnRecipientAccount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        cmbOwnRecipientAccount.Properties.NullText = "Alıcı hesabınızı seçin...";
        cmbOwnRecipientAccount.EditValueChanged += CmbOwnRecipientAccount_EditValueChanged;
        
        lblOwnRecipientInfo = new LabelControl()
        {
            Location = new Point(440, 223),
            Size = new Size(750, 30),
            Text = "📥 TR17 0003 2732 2465 9566 4616 9802 - USD | Bakiye: 80,00 | Kullanılabilir: 10.080,00",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        // ===== IBAN'A TRANSFER - ALICI IBAN =====
        lblIban = new LabelControl() 
        { 
            Location = new Point(25, 195), 
            Size = new Size(120, 22), 
            Text = "Alıcı IBAN:", 
            Visible = false,
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtToIban = new TextEdit() 
        { 
            Location = new Point(25, 220), 
            Size = new Size(400, 38),
            Visible = false
        };
        txtToIban.Properties.NullValuePrompt = "TR00 0000 0000 0000 0000 0000 00";
        txtToIban.Properties.NullValuePromptShowForEmptyValue = true;
        txtToIban.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtToIban.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        txtToIban.Leave += TxtToIban_Leave;
        lblRecipientName = new LabelControl() 
        { 
            Location = new Point(440, 223), 
            Size = new Size(750, 30), 
            Text = "", 
            Visible = false,
            Appearance = { Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        // ===== TUTAR VE AÇIKLAMA =====
        var lblAmount = new LabelControl() 
        { 
            Location = new Point(25, 280), 
            Size = new Size(100, 22), 
            Text = "Tutar:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtAmount = new TextEdit() 
        { 
            Location = new Point(25, 305), 
            Size = new Size(200, 38)
        };
        txtAmount.Properties.NullValuePrompt = "0,00";
        txtAmount.Properties.NullValuePromptShowForEmptyValue = true;
        txtAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtAmount.Properties.Mask.EditMask = "n2";
        txtAmount.Properties.Mask.UseMaskAsDisplayFormat = true;
        txtAmount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        txtAmount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        txtAmount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblCurrency = new LabelControl() 
        { 
            Location = new Point(245, 280), 
            Size = new Size(120, 22), 
            Text = "Para Birimi:", 
            Visible = false, // Gizli - hesaptan otomatik alınır
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbTransCurrency = new ComboBoxEdit() 
        { 
            Location = new Point(245, 305), 
            Size = new Size(140, 38),
            Visible = false // Gizli - hesaptan otomatik alınır
        };
        cmbTransCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbTransCurrency.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbTransCurrency.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        // Komisyon bilgisi
        lblCommissionInfo = new LabelControl()
        {
            Location = new Point(245, 305),
            Size = new Size(400, 38),
            Text = "💰 Komisyon: 0,00 TL (Kendi hesaplar arası ücretsiz)",
            Appearance = { Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(76, 175, 80) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        var lblDesc = new LabelControl() 
        { 
            Location = new Point(25, 365), 
            Size = new Size(100, 22), 
            Text = "Açıklama:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtTransDesc = new TextEdit() 
        { 
            Location = new Point(25, 390), 
            Size = new Size(600, 38)
        };
        txtTransDesc.Properties.NullValuePrompt = "Transfer açıklaması";
        txtTransDesc.Properties.NullValuePromptShowForEmptyValue = true;
        txtTransDesc.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtTransDesc.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        // ===== TRANSFER BUTONLARI =====
        btnOwnAccountTransfer = new SimpleButton() 
        { 
            Location = new Point(25, 455), 
            Size = new Size(300, 50), 
            Text = "✓ Kendi Hesabıma Transfer",
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnOwnAccountTransfer.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnOwnAccountTransfer.AppearanceHovered.BackColor = Color.FromArgb(67, 160, 71);
        btnOwnAccountTransfer.AppearancePressed.BackColor = Color.FromArgb(56, 142, 60);
        btnOwnAccountTransfer.Click += BtnOwnAccountTransfer_Click;
        
        btnExternalTransfer = new SimpleButton() 
        { 
            Location = new Point(25, 455), 
            Size = new Size(300, 50), 
            Text = "✓ Transfer Yap (IBAN)",
            Visible = false,
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnExternalTransfer.Appearance.BackColor = Color.FromArgb(25, 118, 210);
        btnExternalTransfer.AppearanceHovered.BackColor = Color.FromArgb(21, 101, 192);
        btnExternalTransfer.AppearancePressed.BackColor = Color.FromArgb(13, 71, 161);
        btnExternalTransfer.Click += btnExternalTransfer_Click;
        
        btnInternalTransfer = new SimpleButton() 
        { 
            Location = new Point(345, 455), 
            Size = new Size(200, 50), 
            Text = "📋 Hesap Seç",
            Visible = false, // Gizlendi - admin için
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnInternalTransfer.Appearance.BackColor = Color.FromArgb(156, 39, 176);
        btnInternalTransfer.AppearanceHovered.BackColor = Color.FromArgb(142, 36, 170);
        btnInternalTransfer.AppearancePressed.BackColor = Color.FromArgb(123, 31, 162);
        btnInternalTransfer.Click += btnSelectAccount_Click;
        
        // Admin için alıcı hesap seçimi (gizli - kullanılmıyor)
        cmbRecipientAccount = new ComboBoxEdit()
        {
            Location = new Point(25, 500),
            Size = new Size(350, 26),
            Visible = false
        };
        cmbRecipientAccount.Properties.NullText = "Alıcı hesap seçin (Admin)";
        cmbRecipientAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbRecipientAccount.Properties.Appearance.Font = new Font("Segoe UI", 9F);
        
        pnlTransfer.Controls.AddRange(new Control[] { 
            lblTransfer, 
            lblTransferType, rdoOwnAccounts, rdoExternalIban,
            lblTransferAccount, cmbTransferAccount, lblSenderBind, 
            lblOwnRecipientAccount, cmbOwnRecipientAccount, lblOwnRecipientInfo,
            lblIban, txtToIban, lblRecipientName, 
            lblAmount, txtAmount, lblCurrency, cmbTransCurrency, lblCommissionInfo,
            lblDesc, txtTransDesc, 
            btnOwnAccountTransfer, btnExternalTransfer, btnInternalTransfer, cmbRecipientAccount 
        });
        tabTransfer.Controls.Add(pnlTransfer);

        // Reports - Modern Design
        var pnlReports = new PanelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(1240, 140),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblReports = new LabelControl() 
        { 
            Location = new Point(25, 25), 
            Size = new Size(300, 32), 
            Text = "📄 Hesap Ekstreleri", 
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        var lblAccount = new LabelControl() 
        { 
            Location = new Point(25, 70), 
            Size = new Size(100, 22), 
            Text = "Hesap:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbStmtAccount = new LookUpEdit() 
        { 
            Location = new Point(25, 92), 
            Size = new Size(280, 38)
        };
        cmbStmtAccount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbStmtAccount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        cmbStmtAccount.Properties.NullText = "Hesap seçin...";
        cmbStmtAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        
        var lblStmtIban = new LabelControl() 
        { 
            Location = new Point(315, 70), 
            Size = new Size(80, 22), 
            Text = "IBAN:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtStmtAccountId = new TextEdit() 
        { 
            Location = new Point(315, 92), 
            Size = new Size(280, 38),
            ReadOnly = true
        };
        txtStmtAccountId.Properties.NullValuePrompt = "TR00 0000 0000 0000 0000 0000 00";
        txtStmtAccountId.Properties.NullValuePromptShowForEmptyValue = true;
        txtStmtAccountId.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtStmtAccountId.Properties.Appearance.BackColor = Color.FromArgb(240, 240, 240);
        
        var lblFromDate = new LabelControl() 
        { 
            Location = new Point(615, 70), 
            Size = new Size(100, 22), 
            Text = "Başlangıç:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        dtFrom = new DateEdit() 
        { 
            Location = new Point(615, 92), 
            Size = new Size(160, 38),
            EditValue = DateTime.Today.AddDays(-7)
        };
        dtFrom.Properties.Appearance.Font = new Font("Segoe UI", 10);
        dtFrom.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblToDate = new LabelControl() 
        { 
            Location = new Point(795, 70), 
            Size = new Size(80, 22), 
            Text = "Bitiş:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        dtTo = new DateEdit() 
        { 
            Location = new Point(795, 92), 
            Size = new Size(160, 38),
            EditValue = DateTime.Today
        };
        dtTo.Properties.Appearance.Font = new Font("Segoe UI", 10);
        dtTo.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        btnGetStatement = new SimpleButton() 
        { 
            Location = new Point(975, 90), 
            Size = new Size(220, 40), 
            Text = "📊 Ekstre Getir",
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnGetStatement.Appearance.BackColor = Color.FromArgb(255, 152, 0);
        btnGetStatement.AppearanceHovered.BackColor = Color.FromArgb(245, 124, 0);
        btnGetStatement.AppearancePressed.BackColor = Color.FromArgb(230, 81, 0);
        btnGetStatement.Click += btnGetStatement_Click;
        
        gridStatement = new GridControl();
        gridStatementView = new GridView();
        gridStatement.MainView = gridStatementView;
        gridStatement.Location = new Point(20, 180);
        gridStatement.Size = new Size(1240, 400);
        gridStatementView.OptionsBehavior.Editable = false;
        gridStatementView.OptionsSelection.MultiSelect = false;
        gridStatementView.OptionsView.ShowGroupPanel = false;
        gridStatementView.OptionsView.EnableAppearanceEvenRow = true;
        gridStatementView.OptionsView.EnableAppearanceOddRow = true;
        gridStatementView.Appearance.EvenRow.BackColor = Color.FromArgb(250, 250, 250);
        gridStatementView.Appearance.OddRow.BackColor = Color.White;
        gridStatementView.Appearance.HeaderPanel.BackColor = Color.FromArgb(25, 118, 210);
        gridStatementView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridStatementView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        gridStatementView.Appearance.Row.Font = new Font("Segoe UI", 9.5F);
        gridStatementView.Appearance.SelectedRow.BackColor = Color.FromArgb(230, 240, 255);
        
        lblTotals = new LabelControl() 
        { 
            Location = new Point(20, 600), 
            Size = new Size(1240, 35), 
            Text = "Toplamlar", 
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210), TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
        };
        
        pnlReports.Controls.AddRange(new Control[] { lblReports, lblAccount, cmbStmtAccount, lblStmtIban, txtStmtAccountId, lblFromDate, dtFrom, lblToDate, dtTo, btnGetStatement });
        tabReports.Controls.AddRange(new Control[] { pnlReports, gridStatement, lblTotals });

        // Settings / Profile - Modern Design
        var pnlProfile = new PanelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(600, 280),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblProfileTitle = new LabelControl() 
        { 
            Location = new Point(25, 25), 
            Size = new Size(350, 35), 
            Text = "👤 Profil Bilgileri", 
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        lblProfName = new LabelControl() 
        { 
            Location = new Point(25, 75), 
            Size = new Size(550, 28), 
            Text = "👤 Ad Soyad: Admin User", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfName.Appearance.Options.UseFont = true;
        lblProfName.Appearance.Options.UseForeColor = true;
        
        lblProfNationalId = new LabelControl() 
        { 
            Location = new Point(25, 110), 
            Size = new Size(550, 28), 
            Text = "🆔 TCKN: 12345678901", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfNationalId.Appearance.Options.UseFont = true;
        lblProfNationalId.Appearance.Options.UseForeColor = true;
        
        lblProfEmail = new LabelControl() 
        { 
            Location = new Point(25, 145), 
            Size = new Size(550, 28), 
            Text = "📧 E-posta: admin@novabank.com", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfEmail.Appearance.Options.UseFont = true;
        lblProfEmail.Appearance.Options.UseForeColor = true;
        
        lblProfPhone = new LabelControl() 
        { 
            Location = new Point(25, 180), 
            Size = new Size(550, 28), 
            Text = "📱 Telefon: 0532 123 4567", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfPhone.Appearance.Options.UseFont = true;
        lblProfPhone.Appearance.Options.UseForeColor = true;
        var btnLogout = new SimpleButton() 
        { 
            Location = new Point(25, 225), 
            Size = new Size(200, 45), 
            Text = "🚪 Çıkış Yap",
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnLogout.Appearance.BackColor = Color.FromArgb(244, 67, 54);
        btnLogout.AppearanceHovered.BackColor = Color.FromArgb(229, 57, 53);
        btnLogout.AppearancePressed.BackColor = Color.FromArgb(211, 47, 47);
        btnLogout.Click += MnuLogout_Click;
        pnlProfile.Controls.AddRange(new Control[] { lblProfileTitle, lblProfName, lblProfNationalId, lblProfEmail, lblProfPhone, btnLogout });
        
        // Şifre Değiştirme Paneli
        var pnlChangePassword = new PanelControl() 
        { 
            Location = new Point(640, 20), 
            Size = new Size(600, 430),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblChangePasswordTitle = new LabelControl() 
        { 
            Location = new Point(25, 25), 
            Size = new Size(350, 35), 
            Text = "🔒 Şifre Değiştir", 
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        var lblCurrentPassword = new LabelControl() 
        { 
            Location = new Point(25, 80), 
            Size = new Size(150, 25), 
            Text = "Mevcut Şifre:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblCurrentPassword.Appearance.Options.UseFont = true;
        lblCurrentPassword.Appearance.Options.UseForeColor = true;
        
        var txtCurrentPassword = new TextEdit() 
        { 
            Name = "txtCurrentPassword",
            Location = new Point(25, 110), 
            Size = new Size(510, 40)
        };
        txtCurrentPassword.Properties.PasswordChar = '●';
        txtCurrentPassword.Properties.UseSystemPasswordChar = true;
        txtCurrentPassword.Properties.NullValuePrompt = "Mevcut şifrenizi giriniz";
        txtCurrentPassword.Properties.NullValuePromptShowForEmptyValue = true;
        txtCurrentPassword.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtCurrentPassword.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var btnShowCurrentPassword = new SimpleButton() 
        { 
            Name = "btnShowCurrentPassword",
            Location = new Point(545, 110), 
            Size = new Size(40, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowCurrentPassword.Appearance.BackColor = Color.FromArgb(230, 230, 230);
        btnShowCurrentPassword.AppearanceHovered.BackColor = Color.FromArgb(200, 200, 200);
        btnShowCurrentPassword.Click += (s, e) => {
            var txt = pnlChangePassword.Controls.Find("txtCurrentPassword", false).FirstOrDefault() as TextEdit;
            if (txt != null)
            {
                txt.Properties.UseSystemPasswordChar = !txt.Properties.UseSystemPasswordChar;
                btnShowCurrentPassword.Text = txt.Properties.UseSystemPasswordChar ? "👁" : "🙈";
            }
        };
        
        var lblNewPassword = new LabelControl() 
        { 
            Location = new Point(25, 170), 
            Size = new Size(150, 25), 
            Text = "Yeni Şifre:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblNewPassword.Appearance.Options.UseFont = true;
        lblNewPassword.Appearance.Options.UseForeColor = true;
        
        var txtNewPassword = new TextEdit() 
        { 
            Name = "txtNewPassword",
            Location = new Point(25, 200), 
            Size = new Size(510, 40)
        };
        txtNewPassword.Properties.PasswordChar = '●';
        txtNewPassword.Properties.NullValuePrompt = "Yeni şifre (min. 6 karakter)";
        txtNewPassword.Properties.NullValuePromptShowForEmptyValue = true;
        txtNewPassword.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtNewPassword.Properties.Appearance.ForeColor = Color.Black;
        txtNewPassword.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        txtNewPassword.Properties.Appearance.Options.UseForeColor = true;
        txtNewPassword.Properties.Appearance.Options.UseBackColor = true;
        
        var btnShowNewPassword = new SimpleButton() 
        { 
            Name = "btnShowNewPassword",
            Location = new Point(545, 200), 
            Size = new Size(40, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowNewPassword.Appearance.BackColor = Color.FromArgb(230, 230, 230);
        btnShowNewPassword.AppearanceHovered.BackColor = Color.FromArgb(200, 200, 200);
        btnShowNewPassword.Click += (s, e) => {
            var txt = pnlChangePassword.Controls.Find("txtNewPassword", false).FirstOrDefault() as TextEdit;
            if (txt != null)
            {
                if (txt.Properties.PasswordChar == '●')
                {
                    txt.Properties.PasswordChar = '\0';
                    btnShowNewPassword.Text = "🙈";
                }
                else
                {
                    txt.Properties.PasswordChar = '●';
                    btnShowNewPassword.Text = "👁";
                }
            }
        };
        
        var lblNewPasswordConfirm = new LabelControl() 
        { 
            Location = new Point(25, 260), 
            Size = new Size(180, 25), 
            Text = "Yeni Şifre (Tekrar):", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblNewPasswordConfirm.Appearance.Options.UseFont = true;
        lblNewPasswordConfirm.Appearance.Options.UseForeColor = true;
        
        var txtNewPasswordConfirm = new TextEdit() 
        { 
            Name = "txtNewPasswordConfirm",
            Location = new Point(25, 290), 
            Size = new Size(510, 40)
        };
        txtNewPasswordConfirm.Properties.PasswordChar = '●';
        txtNewPasswordConfirm.Properties.NullValuePrompt = "Yeni şifre (tekrar)";
        txtNewPasswordConfirm.Properties.NullValuePromptShowForEmptyValue = true;
        txtNewPasswordConfirm.Properties.Appearance.Font = new Font("Segoe UI", 11);
        txtNewPasswordConfirm.Properties.Appearance.ForeColor = Color.Black;
        txtNewPasswordConfirm.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        txtNewPasswordConfirm.Properties.Appearance.Options.UseForeColor = true;
        txtNewPasswordConfirm.Properties.Appearance.Options.UseBackColor = true;
        
        var btnShowNewPasswordConfirm = new SimpleButton() 
        { 
            Name = "btnShowNewPasswordConfirm",
            Location = new Point(545, 290), 
            Size = new Size(40, 40), 
            Text = "👁",
            Appearance = { Font = new Font("Segoe UI", 14) },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnShowNewPasswordConfirm.Appearance.BackColor = Color.FromArgb(230, 230, 230);
        btnShowNewPasswordConfirm.AppearanceHovered.BackColor = Color.FromArgb(200, 200, 200);
        btnShowNewPasswordConfirm.Click += (s, e) => {
            var txt = pnlChangePassword.Controls.Find("txtNewPasswordConfirm", false).FirstOrDefault() as TextEdit;
            if (txt != null)
            {
                if (txt.Properties.PasswordChar == '●')
                {
                    txt.Properties.PasswordChar = '\0';
                    btnShowNewPasswordConfirm.Text = "🙈";
                }
                else
                {
                    txt.Properties.PasswordChar = '●';
                    btnShowNewPasswordConfirm.Text = "👁";
                }
            }
        };
        
        var btnChangePassword = new SimpleButton() 
        { 
            Location = new Point(25, 355), 
            Size = new Size(560, 50), 
            Text = "✓ Şifreyi Değiştir",
            Appearance = { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnChangePassword.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnChangePassword.AppearanceHovered.BackColor = Color.FromArgb(69, 160, 73);
        btnChangePassword.AppearancePressed.BackColor = Color.FromArgb(56, 142, 60);
        btnChangePassword.Click += async (s, e) => {
            try
            {
                var txtCurrent = pnlChangePassword.Controls.Find("txtCurrentPassword", false).FirstOrDefault() as TextEdit;
                var txtNew = pnlChangePassword.Controls.Find("txtNewPassword", false).FirstOrDefault() as TextEdit;
                var txtNewConfirm = pnlChangePassword.Controls.Find("txtNewPasswordConfirm", false).FirstOrDefault() as TextEdit;
                
                var currentPassword = txtCurrent?.Text?.Trim();
                var newPassword = txtNew?.Text?.Trim();
                var newPasswordConfirm = txtNewConfirm?.Text?.Trim();
                
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    XtraMessageBox.Show("Mevcut şifrenizi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    XtraMessageBox.Show("Yeni şifrenizi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (newPassword.Length < 6)
                {
                    XtraMessageBox.Show("Yeni şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (newPassword != newPasswordConfirm)
                {
                    XtraMessageBox.Show("Yeni şifreler eşleşmiyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (currentPassword == newPassword)
                {
                    XtraMessageBox.Show("Yeni şifre mevcut şifre ile aynı olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // API çağrısı (şimdilik simüle ediyoruz)
                XtraMessageBox.Show("Şifre değiştirme özelliği yakında aktif olacaktır.\n\nŞu anda backend API endpoint'i bekleniyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // İleride bu şekilde kullanılacak:
                // var response = await _api.ChangePasswordAsync(currentPassword, newPassword);
                // if (response.IsSuccessStatusCode)
                // {
                //     XtraMessageBox.Show("Şifreniz başarıyla değiştirildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //     if (txtCurrent != null) txtCurrent.Text = "";
                //     if (txtNew != null) txtNew.Text = "";
                //     if (txtNewConfirm != null) txtNewConfirm.Text = "";
                // }
                // else
                // {
                //     XtraMessageBox.Show("Şifre değiştirme başarısız!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        
        pnlChangePassword.Controls.AddRange(new Control[] { 
            lblChangePasswordTitle, 
            lblCurrentPassword, txtCurrentPassword, btnShowCurrentPassword,
            lblNewPassword, txtNewPassword, btnShowNewPassword,
            lblNewPasswordConfirm, txtNewPasswordConfirm, btnShowNewPasswordConfirm,
            btnChangePassword
        });
        
        tabSettings.Controls.Add(pnlProfile);
        tabSettings.Controls.Add(pnlChangePassword);

        // Döviz Kurları Tab - Modern Design
        pnlExchangeTop = new PanelControl()
        {
            Dock = DockStyle.Top,
            Height = 90,
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblExchangeTitle = new LabelControl()
        {
            Location = new Point(25, 15),
            Size = new Size(500, 35),
            Text = "💱 TCMB Günlük Döviz Kurları",
            Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        lblExchangeInfo = new LabelControl()
        {
            Location = new Point(25, 55),
            Size = new Size(1000, 25),
            Text = "Yükleniyor...",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        btnRefreshRates = new SimpleButton()
        {
            Location = new Point(1100, 20),
            Size = new Size(140, 45),
            Text = "🔄 Yenile",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnRefreshRates.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnRefreshRates.AppearanceHovered.BackColor = Color.FromArgb(69, 160, 73);
        btnRefreshRates.AppearancePressed.BackColor = Color.FromArgb(56, 142, 60);
        btnRefreshRates.Click += BtnRefreshRates_Click;
        
        pnlExchangeTop.Controls.AddRange(new Control[] { lblExchangeTitle, lblExchangeInfo, btnRefreshRates });
        
        dgvRates = new DataGridView()
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            GridColor = Color.FromArgb(240, 240, 240),
            Font = new Font("Segoe UI", 9),
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle()
            {
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                WrapMode = DataGridViewTriState.False
            },
            ColumnHeadersHeight = 40,
            DefaultCellStyle = new DataGridViewCellStyle()
            {
                Padding = new Padding(5),
                SelectionBackColor = Color.FromArgb(230, 240, 255),
                SelectionForeColor = Color.Black
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle()
            {
                BackColor = Color.FromArgb(250, 250, 250)
            }
        };
        dgvRates.Size = new Size(400, 350);
        dgvRates.Dock = DockStyle.None;
        dgvRates.Location = new Point(20, 100);
        
        // ===== DÖVİZ AL PANELİ =====
        pnlFxBuy = new PanelControl()
        {
            Location = new Point(440, 100),
            Size = new Size(380, 350),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblFxBuyTitle = new LabelControl()
        {
            Location = new Point(15, 10),
            Size = new Size(350, 28),
            Text = "📈 DÖVİZ AL",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80) }
        };
        
        var lblFxBuyCurrencyLbl = new LabelControl()
        {
            Location = new Point(15, 45),
            Size = new Size(80, 20),
            Text = "Döviz:",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbFxBuyCurrency = new ComboBoxEdit()
        {
            Location = new Point(15, 65),
            Size = new Size(100, 30)
        };
        cmbFxBuyCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbFxBuyCurrency.Properties.DropDownRows = 15;
        // Döviz listesi LoadExchangeRatesAsync_Internal'da doldurulacak
        cmbFxBuyCurrency.EditValueChanged += CmbFxBuyCurrency_EditValueChanged;
        
        lblFxBuyRate = new LabelControl()
        {
            Location = new Point(130, 68),
            Size = new Size(230, 25),
            Text = "Kur: -- TL (Banka Satış)",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        var lblFxBuyAmountLbl = new LabelControl()
        {
            Location = new Point(15, 100),
            Size = new Size(80, 20),
            Text = "Miktar:",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtFxBuyAmount = new TextEdit()
        {
            Location = new Point(15, 120),
            Size = new Size(150, 30)
        };
        txtFxBuyAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtFxBuyAmount.Properties.Mask.EditMask = "n2";
        txtFxBuyAmount.Properties.NullValuePrompt = "0,00";
        txtFxBuyAmount.EditValueChanged += TxtFxBuyAmount_EditValueChanged;
        
        var lblFxBuyFromLbl = new LabelControl()
        {
            Location = new Point(15, 155),
            Size = new Size(150, 20),
            Text = "TL Hesabı (Kaynak):",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbFxBuyFromTry = new ComboBoxEdit()
        {
            Location = new Point(15, 175),
            Size = new Size(350, 30)
        };
        cmbFxBuyFromTry.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbFxBuyFromTry.Properties.DropDownRows = 10;
        
        var lblFxBuyToLbl = new LabelControl()
        {
            Location = new Point(15, 210),
            Size = new Size(150, 20),
            Text = "Döviz Hesabı (Hedef):",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbFxBuyToForeign = new ComboBoxEdit()
        {
            Location = new Point(15, 230),
            Size = new Size(350, 30)
        };
        cmbFxBuyToForeign.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbFxBuyToForeign.Properties.DropDownRows = 10;
        
        lblFxBuyCalc = new LabelControl()
        {
            Location = new Point(15, 270),
            Size = new Size(350, 30),
            Text = "💰 Ödenecek: 0,00 TL (Komisyon dahil)",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        btnFxBuy = new SimpleButton()
        {
            Location = new Point(15, 305),
            Size = new Size(350, 35),
            Text = "✓ Döviz Al",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnFxBuy.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnFxBuy.AppearanceHovered.BackColor = Color.FromArgb(69, 160, 73);
        btnFxBuy.Click += BtnFxBuy_Click;
        
        pnlFxBuy.Controls.AddRange(new Control[] { lblFxBuyTitle, lblFxBuyCurrencyLbl, cmbFxBuyCurrency, lblFxBuyRate, lblFxBuyAmountLbl, txtFxBuyAmount, lblFxBuyFromLbl, cmbFxBuyFromTry, lblFxBuyToLbl, cmbFxBuyToForeign, lblFxBuyCalc, btnFxBuy });
        
        // ===== DÖVİZ SAT PANELİ =====
        pnlFxSell = new PanelControl()
        {
            Location = new Point(840, 100),
            Size = new Size(380, 350),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblFxSellTitle = new LabelControl()
        {
            Location = new Point(15, 10),
            Size = new Size(350, 28),
            Text = "📉 DÖVİZ SAT",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(244, 67, 54) }
        };
        
        var lblFxSellCurrencyLbl = new LabelControl()
        {
            Location = new Point(15, 45),
            Size = new Size(80, 20),
            Text = "Döviz:",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbFxSellCurrency = new ComboBoxEdit()
        {
            Location = new Point(15, 65),
            Size = new Size(100, 30)
        };
        cmbFxSellCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbFxSellCurrency.Properties.DropDownRows = 15;
        // Döviz listesi LoadExchangeRatesAsync_Internal'da doldurulacak
        cmbFxSellCurrency.EditValueChanged += CmbFxSellCurrency_EditValueChanged;
        
        lblFxSellRate = new LabelControl()
        {
            Location = new Point(130, 68),
            Size = new Size(230, 25),
            Text = "Kur: -- TL (Banka Alış)",
            Appearance = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        var lblFxSellAmountLbl = new LabelControl()
        {
            Location = new Point(15, 100),
            Size = new Size(80, 20),
            Text = "Miktar:",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtFxSellAmount = new TextEdit()
        {
            Location = new Point(15, 120),
            Size = new Size(150, 30)
        };
        txtFxSellAmount.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtFxSellAmount.Properties.Mask.EditMask = "n2";
        txtFxSellAmount.Properties.NullValuePrompt = "0,00";
        txtFxSellAmount.EditValueChanged += TxtFxSellAmount_EditValueChanged;
        
        var lblFxSellFromLbl = new LabelControl()
        {
            Location = new Point(15, 155),
            Size = new Size(150, 20),
            Text = "Döviz Hesabı (Kaynak):",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbFxSellFromForeign = new ComboBoxEdit()
        {
            Location = new Point(15, 175),
            Size = new Size(350, 30)
        };
        cmbFxSellFromForeign.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbFxSellFromForeign.Properties.DropDownRows = 10;
        
        var lblFxSellToLbl = new LabelControl()
        {
            Location = new Point(15, 210),
            Size = new Size(150, 20),
            Text = "TL Hesabı (Hedef):",
            Appearance = { Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbFxSellToTry = new ComboBoxEdit()
        {
            Location = new Point(15, 230),
            Size = new Size(350, 30)
        };
        cmbFxSellToTry.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbFxSellToTry.Properties.DropDownRows = 10;
        
        lblFxSellCalc = new LabelControl()
        {
            Location = new Point(15, 270),
            Size = new Size(350, 30),
            Text = "💰 Alınacak: 0,00 TL (Komisyon düşülmüş)",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        btnFxSell = new SimpleButton()
        {
            Location = new Point(15, 305),
            Size = new Size(350, 35),
            Text = "✓ Döviz Sat",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnFxSell.Appearance.BackColor = Color.FromArgb(244, 67, 54);
        btnFxSell.AppearanceHovered.BackColor = Color.FromArgb(229, 57, 53);
        btnFxSell.Click += BtnFxSell_Click;
        
        pnlFxSell.Controls.AddRange(new Control[] { lblFxSellTitle, lblFxSellCurrencyLbl, cmbFxSellCurrency, lblFxSellRate, lblFxSellAmountLbl, txtFxSellAmount, lblFxSellFromLbl, cmbFxSellFromForeign, lblFxSellToLbl, cmbFxSellToTry, lblFxSellCalc, btnFxSell });
        
        // ===== POZİSYONLAR PANELİ =====
        pnlFxPositions = new PanelControl()
        {
            Location = new Point(20, 470),
            Size = new Size(1200, 200),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblFxPositionsTitle = new LabelControl()
        {
            Location = new Point(15, 10),
            Size = new Size(350, 28),
            Text = "📊 DÖVİZ POZİSYONLARIM",
            Appearance = { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        gridFxPositions = new GridControl()
        {
            Location = new Point(15, 45),
            Size = new Size(900, 140)
        };
        gridFxPositionsView = new GridView();
        gridFxPositions.MainView = gridFxPositionsView;
        gridFxPositionsView.OptionsBehavior.Editable = false;
        gridFxPositionsView.OptionsView.ShowGroupPanel = false;
        gridFxPositionsView.OptionsView.EnableAppearanceEvenRow = true;
        gridFxPositionsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(25, 118, 210);
        gridFxPositionsView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridFxPositionsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        
        lblFxPositionsSummary = new LabelControl()
        {
            Location = new Point(930, 60),
            Size = new Size(250, 100),
            Text = "📈 Toplam Maliyet: 0 TL\n📊 Güncel Değer: 0 TL\n💰 K/Z: 0 TL (0%)",
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        pnlFxPositions.Controls.AddRange(new Control[] { lblFxPositionsTitle, gridFxPositions, lblFxPositionsSummary });
        
        tabExchangeRates.Controls.Add(pnlFxPositions);
        tabExchangeRates.Controls.Add(pnlFxSell);
        tabExchangeRates.Controls.Add(pnlFxBuy);
        tabExchangeRates.Controls.Add(dgvRates);
        tabExchangeRates.Controls.Add(pnlExchangeTop);

        // Load/Close events
        this.Load += FrmMain_Load;
        this.FormClosing += FrmMain_FormClosing;
        this.Resize += FrmMain_Resize;
        this.tabs.SelectedPageChanged += Tabs_SelectedPageChanged;
    }
}
