using NetworkAnalyzer.Apps.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NetworkAnalyzer.Apps.Attributes
{
    class IPScannerInputAttribute : ValidationAttribute
    {
        const string ipWithCIDR = @"^(?!.*[<>:""|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\/(?:3[0-2]|[1-2]?[0-9])$";
        const string ipWithSubnetMask = @"^(?!.*[<>:""\/|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)$";
        const string ipRange = @"^(?!.*[<>:""\/|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s*-\s*(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])$";
        private readonly string _booleanPropertyName;

        public IPScannerInputAttribute(string booleanPropertyName)
        {
            _booleanPropertyName = booleanPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Use reflection to get the boolean property value from the validation context
            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(_booleanPropertyName);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Unknown property: {_booleanPropertyName}");
            }

            // Ensure the property is an instance property
            if (propertyInfo.GetMethod.IsStatic)
            {
                return new ValidationResult($"The property: {_booleanPropertyName} should not be static.");
            }

            bool isManualMode = (bool)propertyInfo.GetValue(validationContext.ObjectInstance);

            if (isManualMode)
            {
                // Check if user failed to provide input with Manual Mode selected
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage = "IP Addressing information provided does not match the required formats.\nPlease check your input and try again.");
                }

                // Check if the user input is in a valid format, if not - return error
                if (Regex.IsMatch(value.ToString(), ipWithCIDR))
                {
                    return ValidationResult.Success;
                }
                else if (Regex.IsMatch(value.ToString(), ipWithSubnetMask))
                {
                    return ValidationResult.Success;
                }
                else if (Regex.IsMatch(value.ToString(), ipRange))
                {
                    if (ParseIPRange(value.ToString()) == IPScannerStatusCode.BadRange)
                    {
                        return new ValidationResult(ErrorMessage = "IP Address Range provided is invalid.\nPlease check your input and try again.");
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(ErrorMessage = "IP Addressing information provided does not match the required formats.\nPlease check your input and try again.");
                }
            }

            return ValidationResult.Success;
        }

        // Used with user input validation - check if the input matches an IP Address range (e.g. 172.30.1.13 - 172.30.1.128)
        private static IPScannerStatusCode ParseIPRange(string userInput)
        {
            string ip1 = userInput.Split("-")[0].Trim();
            string ip2 = userInput.Split("-")[1].Trim();
            bool validInput = true;
            IPScannerStatusCode status;

            // Check each octet from the IP Range to see if the left IP is greater than the right IP
            for (int i = 3; i >= 1; i--)
            {
                if (int.Parse(ip1.Split(".")[i]) > int.Parse(ip2.Split(".")[i]) &&
                  (int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]) ||
                   int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1])))
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
                status = IPScannerStatusCode.GoodRange;
            }
            else
            {
                status = IPScannerStatusCode.BadRange;
            }

            return status;
        }
    }
}
