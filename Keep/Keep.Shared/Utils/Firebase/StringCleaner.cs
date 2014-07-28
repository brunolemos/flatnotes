using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Firebase
{
    public static class StringCleaner
    {
        public static string firebaseClean(this string s)
        {
            StringBuilder sb = new StringBuilder(s);

            sb.Replace(".", "_");
            sb.Replace("$", "_");
            sb.Replace("[", "_");
            sb.Replace("]", "_");
            sb.Replace("#", "_");
            sb.Replace("/", "_");

            return sb.ToString();
        }
    }
}
