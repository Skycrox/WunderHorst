using WunderHorst.Server.Infrastructure.Auth;

namespace WunderHorst.Server.Tests.Infrastructure.Auth;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_AndVerify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.Hash("SuperSecretPassword123!");

        Assert.StartsWith("pbkdf2-sha256$", hash, StringComparison.Ordinal);
        Assert.True(PasswordHasher.Verify("SuperSecretPassword123!", hash));
        Assert.False(PasswordHasher.Verify("WrongPassword!", hash));
    }

    [Theory]
    [InlineData("", "pbkdf2-sha256$1000$AAAA$AAAA")]
    [InlineData("pass", "")]
    [InlineData("pass", "   ")]
    [InlineData("pass", "invalid-format")]
    [InlineData("pass", "argon2$1000$AAAA$AAAA")]
    [InlineData("pass", "pbkdf2-sha256$notanumber$AAAA$AAAA")]
    [InlineData("pass", "pbkdf2-sha256$0$AAAA$AAAA")]
    [InlineData("pass", "pbkdf2-sha256$1000$not_base64!$AAAA")]
    public void Verify_WithInvalidInputsOrMalformedHash_ReturnsFalse(string password, string storedHash)
    {
        Assert.False(PasswordHasher.Verify(password, storedHash));
    }
}
