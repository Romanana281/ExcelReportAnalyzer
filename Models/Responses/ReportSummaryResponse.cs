using ExcelReportAnalyzer.Models.Enums;

namespace ExcelReportAnalyzer.Models.Responses
{
    public class ReportSummaryResponse
    {
        public Guid Id { get; set; }

        public decimal TotalSum { get; set; }

        public decimal AvgSum { get; set; }

        public int CountRows { get; set; }

        public int CountErors { get; set; }

        public string? TopClient { get; set; }
    }
}