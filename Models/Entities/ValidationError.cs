namespace ExcelReportAnalyzer.Models.Entities
{
    public class ValidationError
    {
        public Guid Id { get; set; }

        public required Guid ReportId { get; set; }

        public int RowNumber { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}