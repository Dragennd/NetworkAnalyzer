using System.ComponentModel;

namespace NetworkAnalyzer.Apps.Models
{
    public class FilterData
    {
        public FilterType FilterType { get; set; }
        public FilterOperator FilterOperator { get; set; }
        public BinaryFilterOperator BinaryFilterOperator { get; set; } = BinaryFilterOperator.All;
        public string FilterValue { get; set; } = string.Empty;
        public string DisplayOperator { get; set; } = string.Empty;
        public string FilterQuery { get; set; }

        public FilterData(FilterType filterType, FilterOperator filterOperator, BinaryFilterOperator binaryFilterOperator, string filterValue)
        {
            FilterType = filterType;
            FilterOperator = filterOperator;
            BinaryFilterOperator = binaryFilterOperator;
            FilterValue = filterValue;
            FilterQuery = SetFilterQuery();

            if (FilterType == FilterType.LostPacket)
            {
                DisplayOperator = BinaryFilterOperator.ToString();
                FilterValue = "-";
            }
            else
            {
                DisplayOperator = FilterOperator.ToString();
            }
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
                return $"{FilterType} == {FilterOperator}";
            }
            else
            {
                return $"{FilterType} {convertedFilterOperator} {FilterValue}";
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
