using System.Text.RegularExpressions;

namespace NetworkAnalyzer.Apps.Models
{
    internal class IPv4Info
    {
        const string ipWithCIDR = @"^(?!.*[<>:""|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\/(?:3[0-2]|[1-2]?[0-9])$";
        const string ipWithSubnetMask = @"^(?!.*[<>:""\/|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)$";
        const string ipRange = @"^(?!.*[<>:""\/|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s*-\s*(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])$";

        public string? UserInput { get; set; }
        public string? IPv4Address { get; set; }
        public string? SubnetMask { get; set; }
        public string? UpperRange { get; set; }
        public string? LowerRange { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsIPv4Range { get; set; } = false;
        public bool IsIPv4WithMask { get; set; } = false;
        public bool IsIPv4WithCIDR { get; set; } = false;
        public bool IsError { get; set; } = false;

        public IPv4Info(string userInput)
        {
            UserInput = userInput;
            ParseUserInput();
        }

        private void ParseUserInput()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
            {
                IsError = true;
                ErrorMessage = "Input cannot be empty.";
                return;
            }

            // If user input is an IP Address followed by cidr notation (e.g. 172.30.1.1/24)
            if (Regex.IsMatch(UserInput, ipWithCIDR))
            {
                ParseIPWithCIDRAsync();
                IsIPv4WithCIDR = true;
            }
            // If user input is an IP Address followed by the full subnet mask (e.g. 172.30.1.1 255.255.255.0)
            else if (Regex.IsMatch(UserInput, ipWithSubnetMask))
            {
                ParseIPWithSubnetMaskAsync();
                IsIPv4WithMask = true;
            }
            // If user input is two IP Addresses separated by a hyphen (e.g. 172.30.1.1 - 172.30.1.50)
            else if (Regex.IsMatch(UserInput, ipRange))
            {
                ParseIPRangeAsync();
                IsIPv4Range = true;
            }
            else
            {
                IsError = true;
                ErrorMessage = "Input is incorrectly formatted.";
                return;
            }
        }

        private void ParseIPWithCIDRAsync()
        {
            int hostBits = int.Parse(UserInput.Split("/")[1]);
            int[] maskParts = new int[4];

            // Loop through the octets of the Subnet Mask
            // and assign a mask to that position based upon the provided CIDR Notation
            for (int i = 0; i < maskParts.Length; i++)
            {
                if (hostBits >= 8)
                {
                    maskParts[i] = 255;
                    hostBits -= 8;
                }
                else if (hostBits > 0)
                {
                    maskParts[i] = 255 - ((int)Math.Pow(2, 8 - hostBits) - 1);
                    hostBits = 0;
                }
                else
                {
                    maskParts[i] = 0;
                }
            }

            IPv4Address = UserInput.Split("/")[0];
            SubnetMask = string.Join(".", maskParts);
        }

        // Used with user input validation - check if the input matches an IP Address with a Subnet Mask (e.g. 172.30.1.13 255.255.255.0)
        private void ParseIPWithSubnetMaskAsync()
        {
            if (UserInput.Split(' ').Count() < 2)
            {
                IsError = true;
                ErrorMessage = "Input is incorrectly formatted.";
                return;
            }

            IPv4Address = UserInput.Split(' ')[0];
            SubnetMask = UserInput.Split(' ')[1];
        }

        // Used with user input validation - check if the input matches an IP Address range (e.g. 172.30.1.13 - 172.30.1.128)
        private void ParseIPRangeAsync()
        {
            string ip1 = UserInput.Split("-")[0].Trim();
            string ip2 = UserInput.Split("-")[1].Trim();
            bool validInput = true;

            // Check each octet from the IP Range to see if the left IP is greater than the right IP
            for (int i = 3; i >= 1; i--)
            {
                if (int.Parse(ip1.Split(".")[i]) > int.Parse(ip2.Split(".")[i]) &&
                  ((int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1])) ||
                   (int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1]))))
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) == int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]))
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) == int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1]) &&
                         validInput == false)
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) < int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]))
                {
                    validInput = false;
                }
                else
                {
                    validInput = true;
                }
            }

            if (validInput)
            {
                LowerRange = ip1;
                UpperRange = ip2;
            }
            else
            {
                IsError = true;
                ErrorMessage = "Input is incorrectly formatted.";
            }
        }
    }
}
