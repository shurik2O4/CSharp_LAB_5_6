using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;

namespace LAB_5
{
    internal static class Utils
    {
        // All lowercase, each 1-8 segment has ";" in-between, final segment ends with "."
        // REGEX: ^(([a-яґєії]{1,8}[;])){0,}[a-яґєії]{1,8}\.$
        public static bool CheckInputRegex(string input) => new Regex(@"^(([a-яґєії]{1,8}[;])){0,}[a-яґєії]{1,8}\.$").Match(input).Success;
    }
}