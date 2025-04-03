using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
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
    public class DeleteReportStudentToGcHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public DeleteReportStudentToGcHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteReportStudentToGcRequest, DeleteReportStudentToGcValidator>();


            var GetReportStudent = await _dbContext.Entity<TrGcReportStudent>()
                        .Include(x => x.UserConsellor)
                        .Include(x => x.UserReport)
                        .Include(x => x.AcademicYear)
                        .Include(x => x.Student)
                        .Where(e => body.Ids.Contains(e.Id))
                        .ToListAsync(CancellationToken);

            GetReportStudent.ForEach(x=>x.IsActive=false);
            _dbContext.Entity<TrGcReportStudent>().UpdateRange(GetReportStudent);

            await _dbContext.SaveChangesAsync(CancellationToken);

            var listGC3Notification = await _dbContext.Entity<TrGcReportStudent>()
                        .Include(x => x.UserConsellor)
                        .Include(x => x.UserReport)
                        .Include(x => x.AcademicYear)
                        .Include(x => x.Student)
                        .IgnoreQueryFilters()
                        .Where(e => body.Ids.Contains(e.Id))
                        .Select(e => new GC3NotificationResult
                        {
                            AcademicYear = e.AcademicYear.Description,
                            Date = e.Date.ToShortDateString(),
                            IdStudent = e.IdStudent,
                            StudentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                            Note = e.Note,
                            ReportedBy = e.UserReport.DisplayName,
                            UserConsellor = e.UserConsellor.DisplayName,
                            IdConsellor = e.IdUserCounselor,
                            DateUpdate = e.DateUp.Value.ToShortDateString(),
                            EmailConsellor = e.UserConsellor.Email
                        })
                        .ToListAsync(CancellationToken);

            KeyValues.Add("ShoolName", AuthInfo.Tenants.First().Name);
            KeyValues.Add("ListDeletedData", listGC3Notification);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "GC3")
                {
                    IdRecipients = GetReportStudent.Select(e=>e.IdUserCounselor).ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return Request.CreateApiResult2();
        }
    }
}
