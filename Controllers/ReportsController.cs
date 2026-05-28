using System.Threading.Tasks;
using ExcelReportAnalyzer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExcelReportAnalyzer.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _reportService.GetAllReports();

            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(Guid id)
        {
            try
            {
                var service = await _reportService.GetReportById(id);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(Guid id)
        {
            try
            {
                var service = await _reportService.GetSummary(id);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/errors")]
        public async Task<IActionResult> GetErrors(Guid id)
        {
            try
            {
                var service = await _reportService.GetErrors(id);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> ProcessReport(Guid id)
        {
            try
            {
                var service = await _reportService.GetStatus(id);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadReport(IFormFile file)
        {
            try
            {
                if (Request.HasFormContentType && Request.Form.Files.Count > 1)
                {
                    throw new ArgumentException("The method supports uploading only one file at a time");
                }

                var service = await _reportService.UploadReport(file);
                return Ok(service);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var service = await _reportService.Delete(id);

            if (service != Guid.Empty)
            {
                return Ok(service);
            }

            return NotFound("Id not found");
        }
    }
}