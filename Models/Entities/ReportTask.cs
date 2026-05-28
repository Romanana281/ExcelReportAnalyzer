using ExcelReportAnalyzer.Models.Enums;

namespace ExcelReportAnalyzer.Models.Entities
{
    public class ReportTask
    {
        public required Guid Id { get; set; }

        public required string FileName { get; set; }

        public required string FilePath { get; set; }

        public required Status Status { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public int TotalRows { get; set; }

        public int ErrorCount { get; set; }
    }
}