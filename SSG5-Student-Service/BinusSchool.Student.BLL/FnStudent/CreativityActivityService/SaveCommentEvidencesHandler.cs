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

    public class SaveCommentEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public SaveCommentEvidencesHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveCommentEvidencesRequest, SaveCommentEvidencesValidator>();

            var dataComment = await _dbContext.Entity<TrEvidencesComment>()
                              .Where(e => e.Id == body.IdCommentEvidences)
                              .FirstOrDefaultAsync(CancellationToken);

            var getIdExperience = await _dbContext.Entity<TrEvidences>()
                .Where(x => x.Id == body.IdEvidences)
                .Select(x => x.Experience.Id)
                .FirstOrDefaultAsync(CancellationToken);

            if (dataComment == null)
            {
                var newEvidencesComment = new TrEvidencesComment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvidences = body.IdEvidences,
                    IdUserComment = body.IdUserComment,
                    Comment = body.Comment
                };

                _dbContext.Entity<TrEvidencesComment>().Add(newEvidencesComment);

                await _dbContext.SaveChangesAsync(CancellationToken);

                body.IdCommentEvidences = newEvidencesComment.Id;
            }
            else
            {
                dataComment.Comment = body.Comment;

                _dbContext.Entity<TrEvidencesComment>().Update(dataComment);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }


            #region Notification
            List<UserRoleRecepient> GetUserRecepient = new List<UserRoleRecepient>();
            var GetEvidance = await _dbContext.Entity<TrEvidences>()
                                .Include(e=>e.Experience).ThenInclude(e=>e.HomeroomStudent).ThenInclude(e=>e.Student)
                                .Where(e=>e.Id == body.IdEvidences)
                                .FirstOrDefaultAsync(CancellationToken);

            GetUserRecepient.Add(new UserRoleRecepient
            {
                IdUser = GetEvidance.Experience.HomeroomStudent.IdStudent,
                Role = RoleConstant.Student,
            });

            var GerUserCAS = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.CasAdvisor)
                              .Where(e => e.HomeroomStudent.IdStudent == GetEvidance.Experience.HomeroomStudent.IdStudent && e.CasAdvisor.IdAcademicYear == body.IdAcademicYear)
                              .Select(e => e.CasAdvisor.IdUserCAS)
                              .Distinct().ToListAsync(CancellationToken);

            var GetEvidanceComment = await _dbContext.Entity<TrEvidencesComment>()
                                .Include(e=>e.UserComment)
                                .Where(e => e.IdEvidences == body.IdEvidences)
                                .ToListAsync(CancellationToken);

            //User CAS
            var GetUserCAS = GetEvidanceComment.Where(e => GerUserCAS.Contains(e.IdUserComment) && e.IdUserComment!= GetEvidance.Experience.HomeroomStudent.IdStudent)
                                .Select(e=>new UserRoleRecepient 
                                {
                                    IdUser = e.IdUserComment,
                                    Role = RoleConstant.Staff
                                })
                                .Distinct().ToList();

            GetUserRecepient.AddRange(GetUserCAS);

            //user teacher 
            var GetUserTeacher = GetEvidanceComment.Where(e => !GerUserCAS.Contains(e.IdUserComment) && e.IdUserComment != GetEvidance.Experience.HomeroomStudent.IdStudent)
                                .Select(e => new UserRoleRecepient
                                {
                                    IdUser = e.IdUserComment,
                                    Role = RoleConstant.Teacher
                                })
                                .Distinct().ToList();

            GetUserRecepient.AddRange(GetUserTeacher);

            var GetEvidanceCommentById = GetEvidanceComment.Where(e=>e.Id==body.IdCommentEvidences).FirstOrDefault();
            var GetDataUser = GetUserRecepient.Select(e => new { e.IdUser, e.Role }).Distinct().ToList();
            foreach (var Item in GetDataUser)
            {
                EmailEvidanceCommentResult GetComment = new EmailEvidanceCommentResult
                {
                    Id = getIdExperience,
                    StudentName = GetEvidance.Experience.HomeroomStudent.Student.FirstName
                            + (GetEvidance.Experience.HomeroomStudent.Student.MiddleName == null ? "" : " " + GetEvidance.Experience.HomeroomStudent.Student.MiddleName)
                            + (GetEvidance.Experience.HomeroomStudent.Student.LastName == null ? "" : " " + GetEvidance.Experience.HomeroomStudent.Student.LastName),
                    ExperienceName = GetEvidance.Experience.ExperienceName,
                    Comment = body.Comment,
                    CommentName = GetEvidanceCommentById.UserComment.DisplayName,
                    UserRole = Item.Role,
                    IdUser = Item.IdUser
                };

                if (KeyValues.ContainsKey("GetComment"))
                {
                    KeyValues.Remove("GetComment");
                }

                KeyValues.Add("GetComment", GetComment);
                var Notification = CAS4Notification(KeyValues, AuthInfo);
            }
            #endregion
            return Request.CreateApiResult2();
        }

        public static string CAS4Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetComment").Value;
            var GetComment = JsonConvert.DeserializeObject<EmailEvidanceCommentResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS4")
                {
                    IdRecipients = new List<string>
                    {
                        GetComment.IdUser
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
