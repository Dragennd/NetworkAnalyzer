using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    internal class PingTargetAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var ipAddress = value as string;

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                // Return success if the IP address is not provided
                return ValidationResult.Success;
            }

            // Attempt to ping the IP address
            var ping = new Ping();
            try
            {
                var reply = ping.Send(ipAddress, 1000);
                if (reply != null && reply.Status == IPStatus.Success)
                {
                    // Ping succeeded
                    return ValidationResult.Success;
                }
                else
                {
                    // Ping failed, return successful as long as the failure doesn't cause an exception
                    return ValidationResult.Success;
                }
            }
            catch (PingException)
            {
                // Handle any exceptions that might occur during ping
                return new ValidationResult("Please enter a valid IP Address or DNS Name.");
            }
            finally
            {
                ping.Dispose();
            }
        }
    }
}
