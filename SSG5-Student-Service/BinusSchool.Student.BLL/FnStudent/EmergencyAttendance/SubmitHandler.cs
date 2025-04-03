using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.EmergencyAttendance.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance
{
    public class SubmitHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public SubmitHandler(
            IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SubmitRequest, SubmitValidator>();
            List<string> idTrs = new List<string>();
            var hasTransaction = false;
            foreach (var id in body.IdStudents.Distinct())
            {
                if (!await _dbContext.Entity<TrStudentSafeReport>()
                                     .AnyAsync(x => x.Date.Date == body.Date.Date
                                                    && x.IdStudent == id))
                {
                    // todo: send email
                    string idTr = Guid.NewGuid().ToString();
                    _dbContext.Entity<TrStudentSafeReport>().Add(new TrStudentSafeReport
                    {
                        Id = idTr,
                        Date = body.Date.Date,
                        IdStudent = id
                    });
                    hasTransaction = true;
                    idTrs.Add(idTr);
                }
            }

            if (hasTransaction)
            {
                await _dbContext.SaveChangesAsync(CancellationToken);

                //send notification
                var listSubmitedData = _dbContext.Entity<TrStudentSafeReport>()
                    .Include(x => x.MsStudent)
                    .Include(x => x.MsStudent.StudentParents).ThenInclude(x => x.Parent)
                    .Where(x => idTrs.Contains(x.Id) && x.Date == body.Date.Date)
                    .Select(e=>new Atd13Result
                    {
                        IdStudent = e.IdStudent,
                        IdParent = "P" + e.IdStudent,
                        StudentName = NameUtil.GenerateFullName(e.MsStudent.FirstName,e.MsStudent.MiddleName,e.MsStudent.LastName),
                    }).ToList();

                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
                KeyValues.Add("SchoolName", AuthInfo.Tenants.FirstOrDefault().Name);
                KeyValues.Add("ListSubmitedData", listSubmitedData);

                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD13")
                    {
                        IdRecipients = listSubmitedData.Select(e=>e.IdParent).Distinct().ToList(),
                        KeyValues = KeyValues
                    });
                    collector.Add(message);
                }
            }

            return Request.CreateApiResult2();
        }
    }
}
