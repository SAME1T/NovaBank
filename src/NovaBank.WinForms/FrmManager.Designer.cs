namespace NovaBank.WinForms;

partial class FrmManager
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.gridControl1 = new DevExpress.XtraGrid.GridControl();
        this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
        this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
        this.btnApprove = new DevExpress.XtraEditors.SimpleButton();
        this.btnReject = new DevExpress.XtraEditors.SimpleButton();
        this.btnRefresh = new DevExpress.XtraEditors.SimpleButton();
        this.lblStatus = new DevExpress.XtraEditors.LabelControl();

        ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
        this.panelControl1.SuspendLayout();
        this.SuspendLayout();

        // 
        // gridControl1
        // 
        this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.gridControl1.Location = new System.Drawing.Point(0, 50);
        this.gridControl1.MainView = this.gridView1;
        this.gridControl1.Name = "gridControl1";
        this.gridControl1.Size = new System.Drawing.Size(800, 400);
        this.gridControl1.TabIndex = 0;
        this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});

        // 
        // gridView1
        // 
        this.gridView1.GridControl = this.gridControl1;
        this.gridView1.Name = "gridView1";
        this.gridView1.OptionsBehavior.Editable = false;
        this.gridView1.OptionsView.ShowGroupPanel = false;

        // 
        // panelControl1
        // 
        this.panelControl1.Controls.Add(this.lblStatus);
        this.panelControl1.Controls.Add(this.btnRefresh);
        this.panelControl1.Controls.Add(this.btnReject);
        this.panelControl1.Controls.Add(this.btnApprove);
        this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelControl1.Location = new System.Drawing.Point(0, 0);
        this.panelControl1.Name = "panelControl1";
        this.panelControl1.Size = new System.Drawing.Size(800, 50);
        this.panelControl1.TabIndex = 1;

        // 
        // btnApprove
        // 
        this.btnApprove.Location = new System.Drawing.Point(12, 12);
        this.btnApprove.Name = "btnApprove";
        this.btnApprove.Size = new System.Drawing.Size(75, 23);
        this.btnApprove.TabIndex = 0;
        this.btnApprove.Text = "Onayla";
        this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);

        // 
        // btnReject
        // 
        this.btnReject.Location = new System.Drawing.Point(93, 12);
        this.btnReject.Name = "btnReject";
        this.btnReject.Size = new System.Drawing.Size(75, 23);
        this.btnReject.TabIndex = 1;
        this.btnReject.Text = "Reddet";
        this.btnReject.Click += new System.EventHandler(this.btnReject_Click);

        // 
        // btnRefresh
        // 
        this.btnRefresh.Location = new System.Drawing.Point(174, 12);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(75, 23);
        this.btnRefresh.TabIndex = 2;
        this.btnRefresh.Text = "Yenile";
        this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

        // 
        // lblStatus
        // 
        this.lblStatus.Location = new System.Drawing.Point(265, 17);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(0, 13);
        this.lblStatus.TabIndex = 3;

        // 
        // FrmManager
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.gridControl1);
        this.Controls.Add(this.panelControl1);
        this.Name = "FrmManager";
        this.Text = "Yönetici Paneli - Bekleyen Onaylar";
        this.Load += new System.EventHandler(this.FrmManager_Load);
        ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
        this.panelControl1.ResumeLayout(false);
        this.panelControl1.PerformLayout();
        this.ResumeLayout(false);

    }

    private DevExpress.XtraGrid.GridControl gridControl1;
    private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
    private DevExpress.XtraEditors.PanelControl panelControl1;
    private DevExpress.XtraEditors.SimpleButton btnApprove;
    private DevExpress.XtraEditors.SimpleButton btnReject;
    private DevExpress.XtraEditors.SimpleButton btnRefresh;
    private DevExpress.XtraEditors.LabelControl lblStatus;
}
