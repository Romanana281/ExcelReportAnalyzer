using Microsoft.EntityFrameworkCore;

namespace ExcelReportAnalyzer.Models.Entities
{
    public class ReportRow
    {
        public Guid Id { get; set; }

        public Guid ReportId { get; set; }

        public DateTime? Date { get; set; }

        public string? Client { get; set; }

        public string? Product { get; set; }

        public decimal? Amount { get; set; }
    }
}