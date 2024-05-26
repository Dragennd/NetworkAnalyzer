using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class ExceptionHandler : Exception
    {
        public ResponseCode ExCode { get; }

        public ExceptionHandler(ResponseCode responseCode)
        {
            ExCode = responseCode;
        }

        public override string Message
        {
            get
            {
                switch (ExCode)
                {
                    case ResponseCode.InvalidInputException:
                        return "Invalid user input.";
                    case ResponseCode.BadRangeException:
                        return "Invalid IP range provided.";
                    default:
                        return "Unknown exception occurred.";
                }
            }
        }
    }
}
