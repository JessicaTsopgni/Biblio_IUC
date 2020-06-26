using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Logics.Tools
{
    public class MD5
    {

        public static string Hash(string value)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] tab = System.Text.Encoding.ASCII.GetBytes(value);
            byte[] tab2 = md5.ComputeHash(tab);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < tab2.Length; i++)
            {
                sb.Append(tab2[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static Boolean Match(string md5, string value)
        {
            return md5.Equals(Hash(value));
        }
    }
}
