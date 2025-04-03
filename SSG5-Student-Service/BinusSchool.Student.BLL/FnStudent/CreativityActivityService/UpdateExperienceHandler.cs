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

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{

    public class UpdateExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateExperienceHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateExperienceRequest, UpdateExperienceValidator>();
            var GetUserCAS = await _dbContext.Entity<TrCasAdvisorStudent>()
                             .Include(e => e.CasAdvisor).ThenInclude(e=>e.UserCAS)
                             .Where(e => e.HomeroomStudent.IdStudent == body.IdStudent && e.CasAdvisor.IdAcademicYear == body.IdAcademicYear)
                             .Select(e => new
                             {
                                 IdUserCas =  e.CasAdvisor.IdUserCAS,
                                 CasName =  e.CasAdvisor.UserCAS.DisplayName,
                             })
                             .Distinct().ToListAsync(CancellationToken);
            if (!GetUserCAS.Any())
                throw new BadRequestException("User CAS not found");

            var GetExperience = await _dbContext.Entity<TrExperience>()
                                .Where(e => e.Id == body.Id)
                                .FirstOrDefaultAsync(CancellationToken);
            var IsRevision = GetExperience.Status== ExperienceStatus.NeedRevision?true:false;

            var dataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel).ThenInclude(x => x.MsAcademicYear)
                       .Where(x => x.IdStudent == body.IdStudent && x.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear).FirstOrDefaultAsync(CancellationToken);

            if(dataStudent == null)
                throw new BadRequestException("Student not have homeroom in this Academic Year");

            GetExperience.IdAcademicYear = body.IdAcademicYear;
            GetExperience.IdHomeroomStudent = dataStudent.Id;
            GetExperience.ExperienceName = body.ExperienceName;
            GetExperience.ExperienceLocation = body.ExperienceLocation;
            GetExperience.StartDate = body.StartDate;
            GetExperience.EndDate = body.EndDate;
            GetExperience.SupervisorName = body.SupervisorName;
            GetExperience.SupervisorTitle = body.SupervisorTitle;
            GetExperience.SupervisorEmail = body.SupervisorEmail;
            GetExperience.SupervisorContact = body.SupervisorContact;
            GetExperience.IdUserSupervisor = body.IdUserSupervisor;
            GetExperience.RoleName = body.RoleName;
            GetExperience.PositionName = body.PositionName;
            GetExperience.Organizer = body.Organization;
            GetExperience.Description = body.Description;
            GetExperience.ContributionOrganizer = body.ContributionOrganizer;
            GetExperience.Status = body.Role==RoleConstant.Staff?GetExperience.Status:ExperienceStatus.ToBeDetermined;

            _dbContext.Entity<TrExperience>().Update(GetExperience);

            var dataExperienceStudent = await _dbContext.Entity<TrExperienceStudent>()
                                        .Where(x => x.IdAcademicYear == body.IdAcademicYear && x.IdStudent == dataStudent.IdStudent)
                                        .FirstOrDefaultAsync(CancellationToken);

            if(dataExperienceStudent == null)
            {
                var newExperienceStudent = new TrExperienceStudent
                {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                IdStudent = body.IdStudent,
                StatusOverall = StatusOverallExperienceStudent.OnTrack
                };

                _dbContext.Entity<TrExperienceStudent>().Add(newExperienceStudent);
            }
            else
            {
                dataExperienceStudent.StatusOverall = StatusOverallExperienceStudent.OnTrack;
                _dbContext.Entity<TrExperienceStudent>().Update(dataExperienceStudent);
            }

            var ListExperienceType = new List<TrExperienceType>();

            var dataExperienceType = await _dbContext.Entity<TrExperienceType>()
                                        .Where(x => x.IdExperience == GetExperience.Id)
                                        .ToListAsync(CancellationToken);
            foreach(var dataOldType in dataExperienceType)
            {
                dataOldType.IsActive = false;
                _dbContext.Entity<TrExperienceType>().UpdateRange(dataOldType);
            }

            var dataExperienceLearning = await _dbContext.Entity<TrExperienceLearning>()
                                        .Where(x => x.IdExperience == GetExperience.Id)
                                        .ToListAsync(CancellationToken);
            foreach(var dataOldLearning in dataExperienceLearning)
            {
                dataOldLearning.IsActive = false;
                _dbContext.Entity<TrExperienceLearning>().UpdateRange(dataOldLearning);
            }

            if (body.ExperienceType.Count > 0)
            {
               foreach (var et in body.ExperienceType)
               {
                   var newExperienceType = new TrExperienceType
                   {
                       Id = Guid.NewGuid().ToString(),
                       IdExperience = GetExperience.Id,
                       ExperienceType = et
                   };
                   ListExperienceType.Add(newExperienceType);

               }
               _dbContext.Entity<TrExperienceType>().AddRange(ListExperienceType);
            }

            var ListExperienceLearning = new List<TrExperienceLearning>();

            if (body.IdLearningOutcomes.Count > 0)
            {
               foreach (var lo in body.IdLearningOutcomes)
               {
                   var newIdExperienceLearning = Guid.NewGuid().ToString();
                   var newExperienceLearning = new TrExperienceLearning
                   {
                       Id = newIdExperienceLearning,
                       IdExperience = GetExperience.Id,
                       IdLearningOutcome = lo
                   };
                   ListExperienceLearning.Add(newExperienceLearning);

               }
               _dbContext.Entity<TrExperienceLearning>().AddRange(ListExperienceLearning);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Notification
            if (RoleConstant.Student==body.Role && IsRevision)
            {

                var GetExperienceEmail = await _dbContext.Entity<TrExperience>()
                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                    .Where(e => e.Id == body.Id)
                                    .Select(e => new EmailRevisionExperienceResult
                                    {
                                        Id = e.Id,
                                        StudentName = e.HomeroomStudent.Student.FirstName
                                                        + (e.HomeroomStudent.Student.MiddleName == null ? "" : e.HomeroomStudent.Student.MiddleName)
                                                        + (e.HomeroomStudent.Student.LastName == null ? "" : e.HomeroomStudent.Student.LastName),
                                        BinusianId = e.HomeroomStudent.IdStudent,
                                        StartDate = e.StartDate.ToString("dd MMM yyyy"),
                                        EndDate = e.EndDate.ToString("dd MMM yyyy"),
                                        Location = e.ExperienceLocation.GetDescription(),
                                        Status = e.Status.GetDescription(),
                                        ExperienceName = e.ExperienceName,
                                        IdUserCASList = GetUserCAS.Select(e=>e.IdUserCas).ToList(),
                                    })
                                    .FirstOrDefaultAsync(CancellationToken);

                if (KeyValues.ContainsKey("GetExperienceEmail"))
                {
                    KeyValues.Remove("GetExperienceEmail");
                }

                KeyValues.Add("GetExperienceEmail", GetExperienceEmail);
                var Notification = CAS3Notification(KeyValues, AuthInfo);
            }
            else if (RoleConstant.Staff==body.Role)
            {
               var UserCAS = GetUserCAS.Where(e => e.IdUserCas == body.IdUser).FirstOrDefault();

                var GetExperienceEmail = await _dbContext.Entity<TrExperience>()
                                   .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                   .Where(e => e.Id == body.Id)
                                   .Select(e => new EmailRevisionExperienceResult
                                   {
                                       Id = e.Id,
                                       StudentName = e.HomeroomStudent.Student.FirstName
                                                       + (e.HomeroomStudent.Student.MiddleName == null ? "" : e.HomeroomStudent.Student.MiddleName)
                                                       + (e.HomeroomStudent.Student.LastName == null ? "" : e.HomeroomStudent.Student.LastName),
                                       BinusianId = e.HomeroomStudent.IdStudent,
                                       StartDate = e.StartDate.ToString("dd MMM yyyy"),
                                       EndDate = e.EndDate.ToString("dd MMM yyyy"),
                                       Location = e.ExperienceLocation.GetDescription(),
                                       Status = e.Status.GetDescription(),
                                       ExperienceName = e.ExperienceName,
                                       IdUserCas = UserCAS.IdUserCas,
                                       CasAdvisorName = UserCAS.CasName,
                                       LastUpdate = Convert.ToDateTime(e.DateUp).ToString("dd MMM yyyy")
                                   })
                                   .FirstOrDefaultAsync(CancellationToken);

                if (KeyValues.ContainsKey("GetExperienceEmail"))
                {
                    KeyValues.Remove("GetExperienceEmail");
                }

                KeyValues.Add("GetExperienceEmail", GetExperienceEmail);
                var Notification = CAS8Notification(KeyValues, AuthInfo);
            }
            #endregion

            return Request.CreateApiResult2();
        }

        public static string CAS3Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailRevisionExperienceResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS3")
                {
                    IdRecipients = GetExperienceEmail.IdUserCASList,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string CAS8Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailRevisionExperienceResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS8")
                {
                    IdRecipients = new List<string>
                    {
                        GetExperienceEmail.BinusianId
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
