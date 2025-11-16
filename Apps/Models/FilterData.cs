using System.Runtime.InteropServices;

namespace NetworkAnalyzer.Apps.Models
{
    public class FilterData
    {
        public FilterType FilterType { get; set; } = FilterType.None;
        public AddressFilterType AddressFilterType { get; set; } = AddressFilterType.None;
        public FilterOperator FilterOperator { get; set; } = FilterOperator.None;
        public BinaryFilterOperator BinaryFilterOperator { get; set; } = BinaryFilterOperator.All;
        public string FilterValue { get; set; } = string.Empty;
        public string DisplayType { get; set; } = string.Empty;
        public string DisplayOperator { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string FilterQuery { get; set; } = string.Empty;

        public FilterData([Optional]FilterType filterType, [Optional]AddressFilterType addressFilterType, FilterOperator filterOperator, [Optional]BinaryFilterOperator binaryFilterOperator, string filterValue, [Optional]string guid)
        {
            FilterType = filterType;
            AddressFilterType = addressFilterType;
            FilterOperator = filterOperator;
            BinaryFilterOperator = binaryFilterOperator;
            FilterValue = filterValue;
            GUID = guid;

            if (FilterType != FilterType.None)
            {
                DisplayType = FilterType.ToString();
            }
            else if (AddressFilterType != AddressFilterType.None)
            {
                if (AddressFilterType == AddressFilterType.UserDefinedTarget)
                {
                    DisplayType = "TracerouteGUID";
                }
                else if (AddressFilterType == AddressFilterType.TracerouteTarget)
                {
                    DisplayType = "TargetGUID";
                }
            }

            if (FilterType == FilterType.LostPacket)
            {
                DisplayOperator = BinaryFilterOperator.ToString();
                FilterValue = "-";
            }
            else
            {
                DisplayOperator = FilterOperator.ToString();
            }

            FilterQuery = SetFilterQuery();
        }

        private string SetFilterQuery()
        {
            string convertedFilterOperator = string.Empty;

            switch (FilterOperator)
            {
                case FilterOperator.EqualTo:
                    convertedFilterOperator = "==";
                    break;
                case FilterOperator.NotEqualTo:
                    convertedFilterOperator = "!=";
                    break;
                case FilterOperator.GreaterThan:
                    convertedFilterOperator = ">";
                    break;
                case FilterOperator.GreaterThanOrEqualTo:
                    convertedFilterOperator = ">=";
                    break;
                case FilterOperator.LessThan:
                    convertedFilterOperator = "<";
                    break;
                case FilterOperator.LessThanOrEqualTo:
                    convertedFilterOperator = "<=";
                    break;
            }

            if (BinaryFilterOperator == BinaryFilterOperator.True || BinaryFilterOperator == BinaryFilterOperator.False)
            {
                return $"{DisplayType} == {BinaryFilterOperator}";
            }
            else if (GUID != null)
            {
                return $"{DisplayType} {convertedFilterOperator} \"{GUID}\"";
            }
            else
            {
                return $"{DisplayType} {convertedFilterOperator} \"{FilterValue}\"";
            }
        }
    }

    public enum FilterType
    {
        None = 0,
        CurrentLatency = 1,
        LowestLatency = 2,
        HighestLatency = 3,
        AverageLatency = 4,
        LostPacket = 5,
        TimeStamp = 6
    }

    public enum AddressFilterType
    {
        None = 0,
        UserDefinedTarget = 1,
        TracerouteTarget = 2
    }

    public enum FilterOperator
    {
        None = 0,
        EqualTo = 1,
        NotEqualTo = 2,
        GreaterThan = 3,
        GreaterThanOrEqualTo = 4,
        LessThan = 5,
        LessThanOrEqualTo = 6,
    }

    public enum BinaryFilterOperator
    {
        All = 0,
        True = 1,
        False = 2
    }
}
