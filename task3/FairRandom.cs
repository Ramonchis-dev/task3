using System.Security.Cryptography;

namespace task3;
public class FairRandom
{
    private const int KeySizeBytes = 32;  // 256-bit key
    private byte[]? _key;

    public int ComputerValue;
    public string Key => BitConverter.ToString(_key!).Replace("-", "").ToLower();

    public string Prepare(int max)
    {
        using var rng = RandomNumberGenerator.Create();
        _key = new byte[KeySizeBytes];
        rng.GetBytes(_key);

        ComputerValue = RandomNumberGenerator.GetInt32(max);
        var hash = new HMACSHA256(_key).ComputeHash(BitConverter.GetBytes(ComputerValue));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public int Calculate(int user, int max) => (ComputerValue + user) % max;
}