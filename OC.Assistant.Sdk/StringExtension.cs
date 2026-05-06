using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OC.Assistant.Sdk;

/// <summary>
/// <see cref="string"/> extension methods.
/// </summary>
public static partial class StringExtension
{
    
    /// <param name="value">The input string to be converted.</param>
    extension(string value)
    {
        /// <summary>
        /// Converts a string of numbers (e.g. 1,2,3,4,16-32,64-128) into a list.
        /// </summary>
        public int[] ToNumberList()
        {
            var res = new List<int>();

            foreach (Match m in NumberListRegex().Matches(value))
            {
                if (!m.Groups["val"].Success) continue;
                
                //found area (e.g. 16-32)
                if (m.Groups["low"].Success && m.Groups["high"].Success)
                {
                    var low = Convert.ToInt32(m.Groups["low"].Value);
                    var high = Convert.ToInt32(m.Groups["high"].Value);
                    for (var i = low; i <= high; i++)
                    {
                        res.Add(i);
                    }
                    continue; //next match
                }
                res.Add(Convert.ToInt32(m.Groups["val"].Value)); //single value

            }
            return res.ToArray();
        }

        /// <summary>
        /// Returns the string with the first letter capitalized.
        /// </summary>
        public string FirstCharToUpper()
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length == 1 ? $"{char.ToUpper(value[0])}" : $"{char.ToUpper(value[0])}{value[1..]}";
        }

        /// <summary>
        /// Checks if the input string only contains the characters <c>a-z</c> <c>A-Z</c> <c>0-9</c> and <c>_</c>
        /// </summary>
        /// <returns>True if the input string only contains basic characters, otherwise False.</returns>
        public bool IsBasicCharacters()
            => !string.IsNullOrEmpty(value) && !BasicCharactersRegex().IsMatch(value);
        
        internal string Encrypt()
        {
            using var aes = Aes.Create();
            aes.Key = GetAppKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);

            sw.Write(value);
            sw.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        internal string Decrypt()
        {
            var fullCipher = Convert.FromBase64String(value);
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
            Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

            using var aes = Aes.Create();
            aes.Key = GetAppKey();
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
    
    private static byte[] GetAppKey() 
        => SHA256.HashData(Encoding.UTF8.GetBytes($"{nameof(OC)}.{nameof(Assistant)}"));
    
    [GeneratedRegex(@"(?<val>((?<low>\d+)\s*?-\s*?(?<high>\d+))|(\d+))")]
    private static partial Regex NumberListRegex();
    
    [GeneratedRegex("[^a-zA-Z0-9_]+")]
    private static partial Regex BasicCharactersRegex();
}