using System.ComponentModel;

namespace NetworkAnalyzer.Apps.Models
{
    public class FilterData
    {
        public FilterType FilterType { get; set; } = FilterType.None;
        public FilterOperator FilterOperator { get; set; } = FilterOperator.None;
        public BinaryFilterOperator BinaryFilterOperator { get; set; } = BinaryFilterOperator.All;
        public string FilterValue { get; set; } = string.Empty;

        private string _filterQuery = string.Empty;
        public string FilterQuery
        {
            get => _filterQuery;
            set
            {
                var convertedFilterOperator = string.Empty;
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
                    _filterQuery = $"{FilterType} == {FilterOperator}";
                }
                else
                {
                    _filterQuery = $"{FilterType} {convertedFilterOperator} {FilterValue}";
                }
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
