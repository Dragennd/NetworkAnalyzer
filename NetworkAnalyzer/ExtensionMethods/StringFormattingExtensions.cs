using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkAnalyzer.ExtensionMethods;

internal static class StringFormattingExtensions
{
    public static string FormatAsMacAddress(this string macAddress)
    {
        var regex = "^([a-fA-F0-9]{2}){6}$";
        return string.Join(":", Regex.Match(macAddress, regex).Groups[1].Captures.Select(x => x.Value));
    }

    public static string DecodeBase64(this string base64EncodedData)
    {
        // Convert the base64 string to a byte array
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

        // Convert the byte array to a readable string
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}