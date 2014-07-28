using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Utils
{
    public class GenerateRandomID
    {
        public static string Generate()
        {
            return String.Format("{0}_{1}", Math.Round( new TimeSpan( DateTime.Now.Ticks ).TotalMilliseconds ), new Random().Next(0, 99999999) );
        }
    }
}
