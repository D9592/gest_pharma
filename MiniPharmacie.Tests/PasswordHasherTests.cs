using MiniPharmacie.Business.Security;

namespace MiniPharmacie.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_IsNotPlaintext_AndVerifiesCorrectPassword()
    {
        var hash = PasswordHasher.Hash("MotDePasse!123");
        Assert.NotEqual("MotDePasse!123", hash);
        Assert.True(PasswordHasher.Verify("MotDePasse!123", hash));
        Assert.False(PasswordHasher.Verify("autre", hash));
    }
}
