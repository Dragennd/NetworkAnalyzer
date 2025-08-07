using System.ComponentModel;

namespace NetworkAnalyzer.Apps.Models
{
    public class FilterData
    {
        public FilterType FilterType { get; set; }
        public FilterOperator FilterOperator { get; set; }
        public string FilterValue { get; set; }

        private string _filterQuery;
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
                    case FilterOperator.True:
                        convertedFilterOperator = "true";
                        break;
                    case FilterOperator.False:
                        convertedFilterOperator = "false";
                        break;
                }

                if (FilterOperator == FilterOperator.True || FilterOperator == FilterOperator.False)
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
        Select_a_Filter = 0,
        CurrentLatency = 1,
        LowestLatency = 2,
        HighestLatency = 3,
        AverageLatency = 4,
        LostPacket = 5,
        TimeStamp = 6
    }

    public enum FilterOperator
    {
        EqualTo = 1,
        NotEqualTo = 2,
        GreaterThan = 3,
        GreaterThanOrEqualTo = 4,
        LessThan = 5,
        LessThanOrEqualTo = 6,
        True = 7,
        False = 8
    }
}
