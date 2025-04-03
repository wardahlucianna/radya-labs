using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class ReadStudentToGcHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public ReadStudentToGcHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ReadStudentToGcRequest, ReadStudentToGcValidator>();

            var getGcReportStudent = await _dbContext.Entity<TrGcReportStudent>()
                                        .Where(e=>e.Id==body.IdGcReportStudent)
                                        .FirstOrDefaultAsync(CancellationToken);

            getGcReportStudent.IsRead = true;

            _dbContext.Entity<TrGcReportStudent>().Update(getGcReportStudent);
            await _dbContext.SaveChangesAsync(CancellationToken);

            await EEN1Notification(getGcReportStudent);
            return Request.CreateApiResult2();
        }

        private async Task EEN1Notification(TrGcReportStudent newGcReportStudent)
        {
            var dataGcReportStudent = await _dbContext.Entity<TrGcReportStudent>()
                .Include(x => x.Student)
                .Include(x => x.UserConsellor)
                .Include(x => x.UserReport)
                .Include(x => x.AcademicYear)
                .FirstOrDefaultAsync(x => x.Id == newGcReportStudent.Id);

            KeyValues.Add("StudentName", NameUtil.GenerateFullName(dataGcReportStudent.Student.FirstName,dataGcReportStudent.Student.MiddleName,dataGcReportStudent.Student.LastName));

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EEN1")
                {
                    IdRecipients = new[] { dataGcReportStudent.IdUserReport},
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }
    }
}
