using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{

    public class UpdateOverallProgressStatusStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateOverallProgressStatusStudentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateOverallProgressStatusStudentRequest, UpdateOverallProgressStatusStudentValidator>();

            var AcademicYearMax = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => body.IdAcademicYear.Contains(e.Id))
                                    .Select(e => e.Code)
                                    .MaxAsync(CancellationToken);

            var GetUserCAS = await _dbContext.Entity<TrCasAdvisorStudent>()
                             .Include(e => e.CasAdvisor).ThenInclude(e => e.UserCAS)
                             .Include(e => e.CasAdvisor).ThenInclude(e => e.AcademicYear)
                             .Where(e => e.HomeroomStudent.IdStudent == body.IdStudent && e.CasAdvisor.AcademicYear.Code == AcademicYearMax && e.CasAdvisor.IdUserCAS==body.IdUser)
                             .Select(e => new
                             {
                                 IdUserCas = e.CasAdvisor.IdUserCAS,
                                 CasName = e.CasAdvisor.UserCAS.DisplayName,
                             })
                             .FirstOrDefaultAsync(CancellationToken);

            if (GetUserCAS == null)
                throw new BadRequestException("You are not CAS advisor this student");

            var getExperienceStudent = await _dbContext.Entity<TrExperienceStudent>()
                                .Include(x => x.AcademicYear)
                                .Where(e => e.IdStudent == body.IdStudent && body.IdAcademicYear.Contains(e.IdAcademicYear))
                                .OrderByDescending(e => e.AcademicYear.Code)
                                .FirstOrDefaultAsync(CancellationToken);

            if (getExperienceStudent==null)
            {
                var newExperienceStudent = new TrExperienceStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = body.IdAcademicYear.First(),
                    IdStudent = body.IdStudent,
                    StatusOverall = StatusOverallExperienceStudent.OnTrack
                };

                _dbContext.Entity<TrExperienceStudent>().Add(newExperienceStudent);
            }
            else
            {
                getExperienceStudent.StatusOverall = body.StatusOverallExperienceStudent;
                _dbContext.Entity<TrExperienceStudent>().Update(getExperienceStudent);
            }

           

            await _dbContext.SaveChangesAsync(CancellationToken);


            #region Notification
            var getEmailExperienceStudent = await _dbContext.Entity<TrExperienceStudent>()
                                            .Include(x => x.AcademicYear)
                                            .Where(e => e.IdStudent == body.IdStudent && body.IdAcademicYear.Contains(e.IdAcademicYear))
                                            .Select(e => new EmailStatusOverallResult
                                            {
                                                Id = e.Id,
                                                IdStudent = e.IdStudent,
                                                AcademicYear = e.AcademicYear.Description,
                                                IdAcademicYear = e.AcademicYear.Id,
                                                CasAdvisorName = GetUserCAS.CasName,
                                                OverallStatus = e.StatusOverall.GetDescription()
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);

            if (KeyValues.ContainsKey("GetExperienceStudent"))
            {
                KeyValues.Remove("GetExperienceStudent");
            }

            KeyValues.Add("GetExperienceStudent", getEmailExperienceStudent);
            var Notification = CAS5Notification(KeyValues, AuthInfo);

            #endregion

            return Request.CreateApiResult2();
        }

        public static string CAS5Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceStudent").Value;
            var GetExperienceStudent = JsonConvert.DeserializeObject<EmailStatusOverallResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS5")
                {
                    IdRecipients = new List<string>
                    {
                        GetExperienceStudent.IdStudent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        
    }
}
