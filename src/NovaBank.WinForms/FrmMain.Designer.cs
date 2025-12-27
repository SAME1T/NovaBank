using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraBars;
using NovaBank.Contracts.Accounts;

namespace NovaBank.WinForms;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null;
    private XtraTabControl tabs;
    private XtraTabPage tabMyAccounts, tabDw, tabTransfer, tabReports, tabSettings, tabExchangeRates, tabAdmin, tabCards, tabBills;
    // My Accounts controls
    private TextEdit txtAccCustomerId, txtAccountNo, txtOverdraft;
    private ComboBoxEdit cmbCurrency;
    private SimpleButton btnCreateAccount;
    private GridControl gridAccounts, gridMyCards, gridCardsMain;
    private GridView gridAccountsView, gridMyCardsView, gridCardsMainView;
    private LabelControl lblWelcome, lblTotalBalance, lblAccountCount, lblCardsTitle;
    private PanelControl pnlAccountSummary;
    // Deposit/Withdraw
    private TextEdit txtDepositAmount, txtDepositDesc, txtWithdrawAmount, txtWithdrawDesc;
    private ComboBoxEdit cmbDwCurrency;
    private LookUpEdit cmbDwAccount;
    private LabelControl lblDwIban, lblDwCurrency, lblDwBalance, lblDwOverdraft, lblDwAvailable;
    private SimpleButton btnDeposit, btnWithdraw;
    // Transfer
    private TextEdit txtToId, txtAmount, txtTransDesc, txtToIban;
    private ComboBoxEdit cmbTransCurrency, cmbTransferAccount, cmbRecipientAccount;
    private SimpleButton btnInternalTransfer, btnExternalTransfer;
    private LabelControl lblSenderBind, lblRecipientName;
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
    private XtraTabPage tabAdminUsers, tabAdminCards, tabAdminAudit, tabAdminBills;
    private GridControl gridAdminCardApplications, gridAdminInstitutions;
    private GridView gridAdminCardApplicationsView, gridAdminInstitutionsView;
    private SimpleButton btnApproveCardApp, btnRejectCardApp, btnRefreshCardApps, btnAddInstitution, btnDeleteInstitution, btnRefreshInstitutions;
    private TextEdit txtInstCode, txtInstName, txtInstLogo;
    private ComboBoxEdit cmbInstCategory;

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
        this.tabs.Dock = DockStyle.Fill;
        this.tabs.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top;
        this.tabs.AppearancePage.Header.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.tabs.AppearancePage.HeaderActive.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.tabs.AppearancePage.HeaderActive.ForeColor = Color.FromArgb(25, 118, 210);
        this.tabs.AppearancePage.Header.ForeColor = Color.FromArgb(100, 100, 100);
        this.tabs.LookAndFeel.UseDefaultLookAndFeel = false;
        this.tabs.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        
        this.Controls.Add(this.tabs);
        this.Controls.Add(this.statusStrip);
        
        this.Text = "NovaBank - Güvenli Bankacılık";
        this.Width = 1300; 
        this.Height = 850;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;

        // My Accounts - Modern Digital Banking Design
        pnlAccountSummary = new PanelControl() 
        { 
            Location = new Point(20, 20), 
            Size = new Size(1240, 160),
            Dock = DockStyle.None,
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        lblWelcome = new LabelControl() 
        { 
            Location = new Point(30, 25), 
            Size = new Size(1000, 35), 
            Text = "👋 Hoş Geldiniz", 
            Appearance = { Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblTotalBalance = new LabelControl() 
        { 
            Location = new Point(30, 65), 
            Size = new Size(800, 35), 
            Text = "💰 Toplam Bakiye: 0,00 TL", 
            Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        lblAccountCount = new LabelControl() 
        { 
            Location = new Point(30, 105), 
            Size = new Size(600, 25), 
            Text = "📊 Hesap Sayısı: 0", 
            Appearance = { Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        // Hesap oluşturma paneli - Modern Design
        var pnlCreateAccount = new PanelControl() 
        { 
            Location = new Point(20, 200), 
            Size = new Size(1240, 140),
            Dock = DockStyle.None,
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        
        var lblCreateAccount = new LabelControl() 
        { 
            Location = new Point(25, 20), 
            Size = new Size(350, 32), 
            Text = "💳 Yeni Hesap Aç", 
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        var lblCreateInfo = new LabelControl()
        {
            Location = new Point(25, 52),
            Size = new Size(700, 22),
            Text = "Para birimi seçin ve ek hesap limiti belirleyin. Hesap numarası ve IBAN otomatik oluşturulacaktır.",
            Appearance = { Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(100, 100, 100) }
        };
        
        txtAccCustomerId = new TextEdit() { Location = new Point(25, 85), Size = new Size(120, 30), Visible = false };
        
        var lblCreateCurrency = new LabelControl() 
        { 
            Location = new Point(25, 85), 
            Size = new Size(130, 22), 
            Text = "Para Birimi:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbCurrency = new ComboBoxEdit() 
        { 
            Location = new Point(25, 107), 
            Size = new Size(160, 38)
        };
        cmbCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbCurrency.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbCurrency.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblOverdraft = new LabelControl() 
        { 
            Location = new Point(210, 85), 
            Size = new Size(200, 22), 
            Text = "Ek Hesap Limiti (TL):", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtOverdraft = new TextEdit() 
        { 
            Location = new Point(210, 107), 
            Size = new Size(200, 38)
        };
        txtOverdraft.Properties.NullValuePrompt = "0,00";
        txtOverdraft.Properties.NullValuePromptShowForEmptyValue = true;
        txtOverdraft.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
        txtOverdraft.Properties.Mask.EditMask = "n2";
        txtOverdraft.Properties.Mask.UseMaskAsDisplayFormat = true;
        txtOverdraft.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        txtOverdraft.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        txtOverdraft.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        btnCreateAccount = new SimpleButton() 
        { 
            Location = new Point(430, 105), 
            Size = new Size(220, 40), 
            Text = "✓ Hesap Oluştur",
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnCreateAccount.Appearance.BackColor = Color.FromArgb(76, 175, 80);
        btnCreateAccount.AppearanceHovered.BackColor = Color.FromArgb(69, 160, 73);
        btnCreateAccount.AppearancePressed.BackColor = Color.FromArgb(56, 142, 60);
        btnCreateAccount.Click += btnCreateAccount_Click;
        
        gridAccounts = new GridControl();
        gridAccountsView = new GridView();
        gridAccounts.MainView = gridAccountsView;
        gridAccounts.Location = new Point(20, 360);
        gridAccounts.Size = new Size(1240, 200);
        gridAccountsView.OptionsBehavior.Editable = false;
        gridAccountsView.OptionsSelection.MultiSelect = false;
        gridAccountsView.OptionsView.ShowGroupPanel = false;
        gridAccountsView.OptionsView.EnableAppearanceEvenRow = true;
        gridAccountsView.OptionsView.EnableAppearanceOddRow = true;
        gridAccountsView.Appearance.EvenRow.BackColor = Color.FromArgb(250, 250, 250);
        gridAccountsView.Appearance.OddRow.BackColor = Color.White;
        gridAccountsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(25, 118, 210);
        gridAccountsView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridAccountsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        gridAccountsView.Appearance.Row.Font = new Font("Segoe UI", 9.5F);
        gridAccountsView.Appearance.SelectedRow.BackColor = Color.FromArgb(230, 240, 255);
        gridAccountsView.DoubleClick += GridAccounts_CellDoubleClick;
        gridAccountsView.SelectionChanged += GridAccounts_SelectionChanged;
        
        lblCardsTitle = new LabelControl()
        {
            Location = new Point(20, 575),
            Text = "💳 Kredi Kartlarım",
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };

        gridMyCards = new GridControl();
        gridMyCardsView = new GridView();
        gridMyCards.MainView = gridMyCardsView;
        gridMyCards.Location = new Point(20, 610);
        gridMyCards.Size = new Size(1240, 180);
        gridMyCardsView.OptionsBehavior.Editable = false;
        gridMyCardsView.OptionsView.ShowGroupPanel = false;
        gridMyCardsView.Appearance.HeaderPanel.BackColor = Color.FromArgb(25, 118, 210);
        gridMyCardsView.Appearance.HeaderPanel.ForeColor = Color.White;
        gridMyCardsView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        gridMyCardsView.DoubleClick += GridMyCards_CellDoubleClick;

        pnlAccountSummary.Controls.AddRange(new Control[] { lblWelcome, lblTotalBalance, lblAccountCount });
        pnlCreateAccount.Controls.AddRange(new Control[] { lblCreateAccount, lblCreateInfo, txtAccCustomerId, lblCreateCurrency, cmbCurrency, lblOverdraft, txtOverdraft, btnCreateAccount });
        tabMyAccounts.Controls.AddRange(new Control[] { pnlAccountSummary, pnlCreateAccount, gridAccounts, lblCardsTitle, gridMyCards });

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
            Size = new Size(1240, 380),
            Appearance = { BackColor = Color.White, BorderColor = Color.FromArgb(230, 230, 230) }
        };
        var lblTransfer = new LabelControl() 
        { 
            Location = new Point(25, 25), 
            Size = new Size(300, 32), 
            Text = "💸 Para Transferi", 
            Appearance = { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(25, 118, 210) }
        };
        
        var lblTransferAccount = new LabelControl() 
        { 
            Location = new Point(25, 62), 
            Size = new Size(150, 22), 
            Text = "Gönderen Hesap:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbTransferAccount = new ComboBoxEdit() 
        { 
            Location = new Point(25, 84), 
            Size = new Size(400, 38)
        };
        cmbTransferAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbTransferAccount.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbTransferAccount.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        cmbTransferAccount.EditValueChanged += CmbTransferAccount_EditValueChanged;
        
        lblSenderBind = new LabelControl() 
        { 
            Location = new Point(440, 87), 
            Size = new Size(700, 30), 
            Text = "📤 Hesap seçin", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        var lblIban = new LabelControl() 
        { 
            Location = new Point(25, 135), 
            Size = new Size(120, 22), 
            Text = "Alıcı IBAN:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtToIban = new TextEdit() 
        { 
            Location = new Point(25, 157), 
            Size = new Size(350, 38)
        };
        txtToIban.Properties.NullValuePrompt = "TR00 0000 0000 0000 0000 0000 00";
        txtToIban.Properties.NullValuePromptShowForEmptyValue = true;
        txtToIban.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtToIban.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        txtToIban.Leave += TxtToIban_Leave;
        lblRecipientName = new LabelControl() 
        { 
            Location = new Point(390, 160), 
            Size = new Size(600, 30), 
            Text = "", 
            Appearance = { Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80) },
            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None
        };
        
        var lblAmount = new LabelControl() 
        { 
            Location = new Point(25, 210), 
            Size = new Size(100, 22), 
            Text = "Tutar:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtAmount = new TextEdit() 
        { 
            Location = new Point(25, 232), 
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
            Location = new Point(245, 210), 
            Size = new Size(120, 22), 
            Text = "Para Birimi:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        cmbTransCurrency = new ComboBoxEdit() 
        { 
            Location = new Point(245, 232), 
            Size = new Size(140, 38)
        };
        cmbTransCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbTransCurrency.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
        cmbTransCurrency.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        var lblDesc = new LabelControl() 
        { 
            Location = new Point(405, 210), 
            Size = new Size(100, 22), 
            Text = "Açıklama:", 
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        txtTransDesc = new TextEdit() 
        { 
            Location = new Point(405, 232), 
            Size = new Size(350, 38)
        };
        txtTransDesc.Properties.NullValuePrompt = "Transfer açıklaması";
        txtTransDesc.Properties.NullValuePromptShowForEmptyValue = true;
        txtTransDesc.Properties.Appearance.Font = new Font("Segoe UI", 10);
        txtTransDesc.Properties.Appearance.BackColor = Color.FromArgb(250, 250, 250);
        
        btnExternalTransfer = new SimpleButton() 
        { 
            Location = new Point(25, 295), 
            Size = new Size(280, 45), 
            Text = "✓ Transfer Yap (IBAN)",
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
            Location = new Point(320, 295), 
            Size = new Size(200, 45), 
            Text = "📋 Hesap Seç",
            Visible = false, // Gizlendi
            Appearance = { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = Color.White },
            AppearanceHovered = { ForeColor = Color.White },
            LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
        };
        btnInternalTransfer.Appearance.BackColor = Color.FromArgb(156, 39, 176);
        btnInternalTransfer.AppearanceHovered.BackColor = Color.FromArgb(142, 36, 170);
        btnInternalTransfer.AppearancePressed.BackColor = Color.FromArgb(123, 31, 162);
        btnInternalTransfer.Click += btnSelectAccount_Click;
        
        // Admin için alıcı hesap seçimi
        cmbRecipientAccount = new ComboBoxEdit()
        {
            Location = new Point(150, 130),
            Size = new Size(350, 26),
            Visible = false
        };
        cmbRecipientAccount.Properties.NullText = "Alıcı hesap seçin (Admin)";
        cmbRecipientAccount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbRecipientAccount.Properties.Appearance.Font = new Font("Segoe UI", 9F);
        
        pnlTransfer.Controls.AddRange(new Control[] { lblTransfer, lblTransferAccount, cmbTransferAccount, lblSenderBind, lblIban, txtToIban, lblRecipientName, lblAmount, txtAmount, lblCurrency, cmbTransCurrency, lblDesc, txtTransDesc, btnExternalTransfer, btnInternalTransfer, cmbRecipientAccount });
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
            Size = new Size(1240, 280),
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
            Size = new Size(600, 28), 
            Text = "👤 Ad Soyad:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfNationalId = new LabelControl() 
        { 
            Location = new Point(25, 110), 
            Size = new Size(600, 28), 
            Text = "🆔 TCKN:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfEmail = new LabelControl() 
        { 
            Location = new Point(25, 145), 
            Size = new Size(600, 28), 
            Text = "📧 E-posta:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
        lblProfPhone = new LabelControl() 
        { 
            Location = new Point(25, 180), 
            Size = new Size(600, 28), 
            Text = "📱 Telefon:", 
            Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) }
        };
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
        tabSettings.Controls.Add(pnlProfile);

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
        
        tabExchangeRates.Controls.Add(dgvRates);
        tabExchangeRates.Controls.Add(pnlExchangeTop);

        // Load/Close events
        this.Load += FrmMain_Load;
        this.FormClosing += FrmMain_FormClosing;
        this.tabs.SelectedPageChanged += Tabs_SelectedPageChanged;
    }
}
