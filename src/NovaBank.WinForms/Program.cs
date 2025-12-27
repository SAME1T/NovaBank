using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;

namespace NovaBank.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // DevExpress tema ayarları
        // BonusSkins.Register(); // DevExpress 24+ versiyonlarında gerekli değil
        SkinManager.EnableFormSkins();
        UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
        
        using var auth = new FrmAuth();
        if (auth.ShowDialog() == DialogResult.OK && auth.LoggedInCustomerId.HasValue)
        {
            if (Services.Session.CurrentRole == Core.Enums.UserRole.Manager)
            {
                 System.Windows.Forms.Application.Run(new FrmManager());
            }
            else
            {
                var main = new FrmMain(auth.LoggedInCustomerId.Value);
                System.Windows.Forms.Application.Run(main);
            }
        }
    }    
}
