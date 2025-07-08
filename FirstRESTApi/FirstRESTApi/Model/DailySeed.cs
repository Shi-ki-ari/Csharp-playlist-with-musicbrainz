using System;
using System.Security.Cryptography;
using System.Text;

public class SeedGenerator
{
    public static int GetDailySeed()
    {
        string todayString = DateTime.Today.ToString("yyyy-MM-dd");

        byte[] inputBytes = Encoding.UTF8.GetBytes(todayString);

        SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(inputBytes);

        int seed = BitConverter.ToInt32(hashBytes, 0);


        return Math.Abs(seed);
    }
}
