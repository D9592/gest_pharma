using MiniPharmacie.Business.Security;
using MiniPharmacie.Data.Database;

namespace MiniPharmacie.App.Forms;

public sealed class LoginForm : PharmacyForm
{
    public long UserId { get; private set; }
    public LoginForm(){Text="Connexion Mini-pharmacie";Width=380;Height=240;StartPosition=FormStartPosition.CenterScreen;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;var user=new TextBox{Left=130,Top=30,Width=200};var pass=new TextBox{Left=130,Top=75,Width=200,UseSystemPasswordChar=true};var ok=new Button{Text="Se connecter",Left=210,Top=125,Width=120};Controls.AddRange([new Label{Text="Utilisateur",Left=25,Top=33},user,new Label{Text="Mot de passe",Left=25,Top=78},pass,ok]);AcceptButton=ok;ok.Click+=async(_,_)=>{try{await using var c=new DatabaseInitializer().CreateConnection();await c.OpenAsync();await using var cmd=c.CreateCommand();cmd.CommandText="SELECT Id,PasswordHash FROM Users WHERE Username=$u AND IsActive=1";cmd.Parameters.AddWithValue("$u",user.Text.Trim());long id;string hash;await using(var r=await cmd.ExecuteReaderAsync()){if(!await r.ReadAsync()||!PasswordHasher.Verify(pass.Text,r.GetString(1))){MessageBox.Show("Identifiant ou mot de passe incorrect.","Connexion",MessageBoxButtons.OK,MessageBoxIcon.Warning);return;}id=r.GetInt64(0);hash=r.GetString(1);}await using var audit=c.CreateCommand();audit.CommandText="INSERT INTO AuditLog(UserId,Action,Entity,EntityId,OccurredAt) VALUES($u,'CONNEXION','Users',$u,$d)";audit.Parameters.AddWithValue("$u",id);audit.Parameters.AddWithValue("$d",DateTimeOffset.Now.ToString("O"));await audit.ExecuteNonQueryAsync();UserId=id;DialogResult=DialogResult.OK;Close();}catch(Exception ex){MessageBox.Show(ex.Message,"Connexion impossible",MessageBoxButtons.OK,MessageBoxIcon.Error);}};}
}
