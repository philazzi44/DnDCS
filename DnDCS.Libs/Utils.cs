using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DnDCS.Libs
{
    public static class Utils
    {
        public static bool IsIPAddress(string address)
        {
            return (Regex.IsMatch(address, @"\d{1,4}\.\d{1,4}\.\d{1,4}\.\d{1,4}"));
        }
    }
}
