using System.Windows.Forms;

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
        using var auth = new FrmAuth();
        if (auth.ShowDialog() == DialogResult.OK && auth.LoggedInCustomerId.HasValue)
        {
            var main = new FrmMain(auth.LoggedInCustomerId.Value);
            System.Windows.Forms.Application.Run(main);
        }
    }    
}
