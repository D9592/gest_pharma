using MiniPharmacie.Data.Database;
using MiniPharmacie.Business.Services;

namespace MiniPharmacie.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.ThreadException += (_, e) => MessageBox.Show($"Une erreur inattendue est survenue. Consultez le journal technique.\n{e.Exception.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniPharmacie", "errors.log"), $"{DateTimeOffset.Now:u} {e.ExceptionObject}\n");
        try { var database=new DatabaseInitializer();database.InitializeAsync().GetAwaiter().GetResult();var security=new SecurityAdministrationService(database);if(!security.HasAnyUserAsync().GetAwaiter().GetResult()){using var first=new Forms.FirstAdministratorForm();if(first.ShowDialog()!=DialogResult.OK)return;}using var login=new Forms.LoginForm(); if(login.ShowDialog()!=DialogResult.OK)return; Application.Run(new MainForm(login.UserId)); }
        catch (Exception ex) { MessageBox.Show($"Impossible de démarrer l'application : {ex.Message}", "Erreur de démarrage", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
