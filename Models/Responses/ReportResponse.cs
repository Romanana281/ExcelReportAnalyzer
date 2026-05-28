using ExcelReportAnalyzer.Models.Enums;

namespace ExcelReportAnalyzer.Models.Responses
{
    public class ReportResponse
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public Status? Status { get; set; }

        public DateTime Date { get; set; }
    }
}