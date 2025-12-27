using DevExpress.XtraEditors;
using NovaBank.Contracts.ApprovalWorkflows;
using NovaBank.WinForms.Services;
using NovaBank.Core.Enums;

namespace NovaBank.WinForms;

public partial class FrmManager : XtraForm
{
    private readonly ApiClient _api = new();
    private List<ApprovalWorkflowResponse> _workflows = new();

    public FrmManager()
    {
        InitializeComponent();
    }

    private async void FrmManager_Load(object sender, EventArgs e)
    {
        await LoadWorkflows();
    }

    private async Task LoadWorkflows()
    {
        try
        {
            _workflows = await _api.GetApprovalWorkflowsAsync();
            gridControl1.DataSource = _workflows;
            lblStatus.Text = $"Bekleyen işlem sayısı: {_workflows.Count}";
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata");
        }
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await LoadWorkflows();
    }

    private async void btnApprove_Click(object sender, EventArgs e)
    {
        var row = gridView1.GetFocusedRow() as ApprovalWorkflowResponse;
        if (row == null) { XtraMessageBox.Show("Lütfen bir kayıt seçin.", "Uyarı"); return; }

        if (XtraMessageBox.Show("Bu işlemi onaylamak istiyor musunuz?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        try
        {
            var resp = await _api.ApproveWorkflowAsync(row.Id);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("İşlem başarıyla onaylandı.", "Başarılı");
                await LoadWorkflows();
            }
            else
            {
                var err = await ApiClient.GetErrorMessageAsync(resp);
                XtraMessageBox.Show($"Hata: {err}", "Hata");
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata");
        }
    }

    private async void btnReject_Click(object sender, EventArgs e)
    {
        var row = gridView1.GetFocusedRow() as ApprovalWorkflowResponse;
        if (row == null) { XtraMessageBox.Show("Lütfen bir kayıt seçin.", "Uyarı"); return; }
        
        var reason = XtraInputBox.Show("Reddetme nedeni:", "Reddet", "");
        if (string.IsNullOrWhiteSpace(reason)) return;

        try
        {
            var resp = await _api.RejectWorkflowAsync(row.Id, reason);
            if (resp.IsSuccessStatusCode)
            {
                XtraMessageBox.Show("İşlem reddedildi.", "Başarılı");
                await LoadWorkflows();
            }
            else
            {
                 var err = await ApiClient.GetErrorMessageAsync(resp);
                XtraMessageBox.Show($"Hata: {err}", "Hata");
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Hata: {ex.Message}", "Hata");
        }
    }
}
