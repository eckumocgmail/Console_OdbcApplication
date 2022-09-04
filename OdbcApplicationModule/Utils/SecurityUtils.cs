using ServerApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace syntez
{
    public class SecurityUtils
    {
        public SecurityUtils() { }

        public static long GetTime( )
        {
            TimeSpan uTimeSpan = ( DateTime.Now - new DateTime( 1970, 1, 1, 0, 0, 0 ) );
            return ( long ) uTimeSpan.TotalMilliseconds;
        }

        /** применяет хэш-функцию к текстовым данным */
        public static string GetHashSha256(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }



        /** генерация случайной последовательности символов **/        
        public static string RandomString(int length)
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                            "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower() +
                            "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
