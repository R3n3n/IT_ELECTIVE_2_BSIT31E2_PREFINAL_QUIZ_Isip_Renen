using System.Security.Cryptography;
using System.Text;

namespace RenenPortfolio.Security;

public interface IUserService
{
    string? Validate(string username, string password);
}

public class LoginOptions
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public int Iterations { get; set; } = 210_000;
}

public class HardcodedUserService : IUserService
{
    private readonly LoginOptions _options;
    private readonly byte[] _expectedHash;
    private readonly byte[] _salt;

    public HardcodedUserService(LoginOptions options)
    {
        _options = options;
        _expectedHash = Convert.FromBase64String(options.PasswordHash);
        _salt = Convert.FromBase64String(options.PasswordSalt);
    }

    public string? Validate(string username, string password)
    {
        var candidate = Hash(password, _salt, _options.Iterations, _expectedHash.Length);

        var passwordMatches = CryptographicOperations.FixedTimeEquals(candidate, _expectedHash);
        var usernameMatches = FixedTimeStringEquals(username, _options.Username);

        return passwordMatches && usernameMatches ? _options.DisplayName : null;
    }

    public static byte[] Hash(string password, byte[] salt, int iterations, int length) =>
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            length);

    private static bool FixedTimeStringEquals(string a, string b)
    {
        var ha = SHA256.HashData(Encoding.UTF8.GetBytes(a.Trim().ToLowerInvariant()));
        var hb = SHA256.HashData(Encoding.UTF8.GetBytes(b.Trim().ToLowerInvariant()));
        return CryptographicOperations.FixedTimeEquals(ha, hb);
    }
}