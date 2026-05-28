using ExcelReportAnalyzer.Models.Enums;

namespace ExcelReportAnalyzer.Models.Responses
{
    public class ErrorResponse
    {
        public int RowNumber { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}