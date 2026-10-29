using MiniPharmacie.Business.Services;
using MiniPharmacie.Data.Database;

namespace MiniPharmacie.App.Forms;

public sealed class FirstAdministratorForm: PharmacyForm
{
    public FirstAdministratorForm(){Text="Création du premier compte responsable";Width=430;Height=310;StartPosition=FormStartPosition.CenterScreen;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;var user=new TextBox{Left=180,Top=30,Width=205};var pass=new TextBox{Left=180,Top=75,Width=205,UseSystemPasswordChar=true};var confirm=new TextBox{Left=180,Top=120,Width=205,UseSystemPasswordChar=true};var save=new Button{Text="Créer le compte",Left=245,Top=180,Width=140};Controls.AddRange([new Label{Text="Identifiant",Left=25,Top=33},user,new Label{Text="Mot de passe (12 caractères minimum)",Left=25,Top=78,Width=150},pass,new Label{Text="Confirmer le mot de passe",Left=25,Top=123,Width=150},confirm,save]);save.Click+=async(_,_)=>{if(pass.Text!=confirm.Text){MessageBox.Show("Les mots de passe ne correspondent pas.");return;}try{await new SecurityAdministrationService(new DatabaseInitializer()).CreateInitialAdministratorAsync(user.Text,pass.Text);DialogResult=DialogResult.OK;Close();}catch(Exception ex){MessageBox.Show(ex.Message,"Compte non créé",MessageBoxButtons.OK,MessageBoxIcon.Warning);}};}
}
