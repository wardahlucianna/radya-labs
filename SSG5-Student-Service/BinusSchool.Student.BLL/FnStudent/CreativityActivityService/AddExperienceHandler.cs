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
using System.Net;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{

    public class AddExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public AddExperienceHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddExperienceRequest, AddExperienceValidator>();

            var GerUserCAS = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.CasAdvisor)
                              .Where(e => e.HomeroomStudent.IdStudent == body.IdStudent && e.CasAdvisor.IdAcademicYear == body.IdAcademicYear)
                              .Select(e => e.CasAdvisor.IdUserCAS)
                              .ToListAsync(CancellationToken);


            if (!GerUserCAS.Any())
                throw new BadRequestException("User CAS not found");

            var dataStudentExist = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel).ThenInclude(x => x.MsAcademicYear)
               .Where(x => x.IdStudent == body.IdStudent && x.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear).ToListAsync(CancellationToken);

            var isNameExist = await _dbContext.Entity<TrExperience>()
                       .Where(x => ((body.StartDate >= x.StartDate && body.EndDate <= x.EndDate)
                                     || (body.EndDate > x.StartDate && body.EndDate <= x.EndDate)
                                     || (body.StartDate <= x.StartDate && body.EndDate >= x.EndDate)) 
                                    && (dataStudentExist.Select(y=> y.Id).ToList().Contains(x.IdHomeroomStudent) == true && x.ExperienceName.ToLower() == body.ExperienceName.ToLower()))
                       .FirstOrDefaultAsync(CancellationToken);

            if (isNameExist != null)
               throw new BadRequestException($"{body.ExperienceName} already exists in this experience date");

            var newIdExperience = Guid.NewGuid().ToString();

            var dataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel).ThenInclude(x => x.MsAcademicYear)
                       .Where(x => x.IdStudent == body.IdStudent && x.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear).FirstOrDefaultAsync(CancellationToken);

            if(dataStudent == null)
                throw new BadRequestException("Student not have homeroom in this Academic Year");
                       

            var newExperience = new TrExperience
            {
               Id = newIdExperience,
               IdAcademicYear = body.IdAcademicYear,
               IdHomeroomStudent = dataStudent.Id,
               ExperienceName = body.ExperienceName,
               ExperienceLocation = body.ExperienceLocation,
               StartDate = body.StartDate,
               EndDate = body.EndDate,
               SupervisorName = body.SupervisorName,
               SupervisorTitle = body.SupervisorTitle,
               SupervisorEmail = body.SupervisorEmail,
               SupervisorContact = body.SupervisorContact,
               IdUserSupervisor = body.IdUserSupervisor,
               RoleName = body.RoleName,
               PositionName = body.PositionName,
               Organizer = body.Organization,
               Description = body.Description,
               ContributionOrganizer = body.ContributionOrganizer,
               Status = ExperienceStatus.ToBeDetermined
            };

            _dbContext.Entity<TrExperience>().Add(newExperience);

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

            if (body.ExperienceType.Count > 0)
            {
               foreach (var et in body.ExperienceType)
               {
                   var newExperienceType = new TrExperienceType
                   {
                       Id = Guid.NewGuid().ToString(),
                       IdExperience = newIdExperience,
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
                       IdExperience = newIdExperience,
                       IdLearningOutcome = lo
                   };
                   ListExperienceLearning.Add(newExperienceLearning);

                   // var newExperienceStatusChangeHs = new TrExperienceStatusChangeHs
                   // {
                   //     Id = Guid.NewGuid().ToString(),
                   //     IdExperienceLearning = newIdExperienceLearning,
                   //     IdUserApproval = AuthInfo.UserId,
                   //     ExperienceStatusChangeDate = DateTime.Now
                   // };

                   // _dbContext.Entity<TrExperienceStatusChangeHs>().Add(newExperienceStatusChangeHs);

               }
               _dbContext.Entity<TrExperienceLearning>().AddRange(ListExperienceLearning);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Notification
            var GetExperienceEmail = await _dbContext.Entity<TrExperience>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                .Where(e => e.Id == newExperience.Id)
                                .Select(e => new EmailRequestExperienceResult
                                {
                                    Id = e.Id,
                                    StudentName = e.HomeroomStudent.Student.FirstName
                                                    + (e.HomeroomStudent.Student.MiddleName == null ? " " : e.HomeroomStudent.Student.MiddleName)
                                                    + (e.HomeroomStudent.Student.LastName == null ? " " : e.HomeroomStudent.Student.LastName),
                                    BinusianId = e.HomeroomStudent.IdStudent,
                                    StartDate = e.StartDate.ToString("dd MMM yyyy"),
                                    EndDate = e.EndDate.ToString("dd MMM yyyy"),
                                    Location = e.ExperienceLocation.GetDescription(),
                                    Status = e.Status.GetDescription(),
                                    ExperienceName = e.ExperienceName,
                                    IdUserCAS = GerUserCAS
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            if (KeyValues.ContainsKey("GetExperienceEmail"))
            {
                KeyValues.Remove("GetExperienceEmail");
            }

            KeyValues.Add("GetExperienceEmail", GetExperienceEmail);
            var Notification = CAS1Notification(KeyValues, AuthInfo);

            #endregion

            return Request.CreateApiResult2();
        }

        public static string CAS1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailRequestExperienceResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS1")
                {
                    IdRecipients = GetExperienceEmail.IdUserCAS,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
