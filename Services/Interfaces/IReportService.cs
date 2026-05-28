using ExcelReportAnalyzer.Models.Enums;
using ExcelReportAnalyzer.Models.Responses;

namespace ExcelReportAnalyzer.Interfaces
{
    public interface IReportService
    {
        public Task<List<ReportResponse>> GetAllReports();

        public Task<ReportResponse> GetReportById(Guid id);

        public Task<ReportSummaryResponse?> GetSummary(Guid id);

        public Task<List<ErrorResponse>> GetErrors(Guid id);

        public Task<Status> GetStatus(Guid id);

        public Task<Guid> UploadReport(IFormFile file);

        public Task<Guid> Delete(Guid id);
    }
}