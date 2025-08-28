using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using TestCaseDashboard.Data;

namespace TestCaseDashboard.Controllers
{
    public partial class ExportmydatabaseController : ExportController
    {
        private readonly mydatabaseContext context;
        private readonly mydatabaseService service;

        public ExportmydatabaseController(mydatabaseContext context, mydatabaseService service)
        {
            this.service = service;
            this.context = context;
        }

        [HttpGet("/export/mydatabase/buglists/csv")]
        [HttpGet("/export/mydatabase/buglists/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportBuglistsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetBuglists(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/buglists/excel")]
        [HttpGet("/export/mydatabase/buglists/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportBuglistsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetBuglists(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/projects/csv")]
        [HttpGet("/export/mydatabase/projects/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportProjectsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetProjects(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/projects/excel")]
        [HttpGet("/export/mydatabase/projects/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportProjectsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetProjects(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/projectteammembers/csv")]
        [HttpGet("/export/mydatabase/projectteammembers/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportProjectTeammembersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetProjectTeammembers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/projectteammembers/excel")]
        [HttpGet("/export/mydatabase/projectteammembers/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportProjectTeammembersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetProjectTeammembers(), Request.Query, false), fileName);
        }

     

  
        [HttpGet("/export/mydatabase/testcases/excel")]
        [HttpGet("/export/mydatabase/testcases/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportTestcasesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetTestcases(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/testcaseteammembers/csv")]
        [HttpGet("/export/mydatabase/testcaseteammembers/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportTestcaseTeammembersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetTestcaseTeammembers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/mydatabase/testcaseteammembers/excel")]
        [HttpGet("/export/mydatabase/testcaseteammembers/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportTestcaseTeammembersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetTestcaseTeammembers(), Request.Query, false), fileName);
        }
    }
}
