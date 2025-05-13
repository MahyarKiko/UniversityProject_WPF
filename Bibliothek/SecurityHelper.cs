using System;
using System.Security.Cryptography;
using System.Text;

public static class SecurityHelper
{

    public static string HashString(this string data)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(data);
        SHA256Managed hashstring = new SHA256Managed();
        byte[] hash = hashstring.ComputeHash(bytes);
        string result = Convert.ToBase64String(hash);
        return result;
    }

    public static string EncryptSHA(this string data)
    {
        SHA1Managed sham = new SHA1Managed();
        Convert.ToBase64String(sham.ComputeHash(Encoding.UTF8.GetBytes(data)));
        byte[] enc_data = ASCIIEncoding.UTF8.GetBytes(data);
        string enc_str = Convert.ToBase64String(enc_data);
        return enc_str;
    }
    public static string DecryptSHA(this string data)
    {
        try
        {
            byte[] dec_data = Convert.FromBase64String(data.Trim());
            string dec_str = ASCIIEncoding.UTF8.GetString(dec_data);
            return dec_str;
        }
        catch
        {
            return "";
        }
    }

}

