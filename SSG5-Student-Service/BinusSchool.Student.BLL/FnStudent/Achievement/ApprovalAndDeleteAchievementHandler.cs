using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.FnStudent.Achievement.Validator;
using BinusSchool.Common.Extensions;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Exceptions;
using BinusSchool.Auth.Authentications.Jwt;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using BinusSchool.Common.Utils;

namespace BinusSchool.Student.FnStudent.Achievement
{
    public class ApprovalAndDeleteAchievementHandler : FunctionsHttpSingleHandler
    {
        private readonly string KeyName = "studentAchievementResult";
        private readonly IStudentDbContext _dbContext;

        public ApprovalAndDeleteAchievementHandler(IStudentDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ApprovalAndDeleteAchievementRequest, ApprovalAndDeleteAchievementValidator>();

            var entryMeritStudent = await _dbContext.Entity<TrEntryMeritStudent>()
               .Include(e => e.StudentMeritApproval)
                    .ThenInclude(e => e.User1)
               .Include(e => e.HomeroomStudent)
                    .ThenInclude(e => e.Homeroom)
                        .ThenInclude(e => e.Grade)
                            .ThenInclude(e => e.MsLevel)
                                .ThenInclude(e => e.MsAcademicYear)
               .Include(e => e.MeritDemeritMapping)
               .Include(e => e.FocusArea)
               .Where(e => e.Id == body.IdAchievement)
               .FirstOrDefaultAsync(CancellationToken);

            if (entryMeritStudent==null)
                throw new BadRequestException($"Id achievement is not found");

            var meritHistory = entryMeritStudent.StudentMeritApproval.OrderBy(e => e.DateIn).LastOrDefault();

            if (meritHistory == null)
                throw new BadRequestException($"this merit dont have history");

            var studentName = await _dbContext.Entity<MsStudent>()
                .Where(x => x.Id == entryMeritStudent.HomeroomStudent.IdStudent)
                .Select(x => new
                {
                    name = $"{NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName)} - {x.Id}"
                }).FirstOrDefaultAsync(CancellationToken);

            if (studentName == null)
                throw new NotFoundException("Student not found");

            if (body.TypeRequest == AchievementTypeStatus.Deleted)
            {
                if(entryMeritStudent.Status== "Waiting for Approval")
                {
                    entryMeritStudent.IsDeleted = true;
                    entryMeritStudent.RequestReason = body.Note;
                    entryMeritStudent.RequestType = RequestType.Delete;
                    entryMeritStudent.Status = "Deleted";
                    entryMeritStudent.IsHasBeenApproved = true;
                    _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);
                }
                else
                {
                    entryMeritStudent.IsDeleted = true;
                    entryMeritStudent.RequestReason = body.Note;
                    entryMeritStudent.RequestType = RequestType.Delete;
                    entryMeritStudent.Status = "Delete Requested";
                    entryMeritStudent.IsHasBeenApproved = true;
                    _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);

                    var NewStudentMeritApprovalHs = new TrStudentMeritApprovalHs
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdTrEntryMeritStudent = meritHistory.IdTrEntryMeritStudent,
                        IdUserApproved1 = meritHistory.IdUserApproved1,
                        RequestType = RequestType.Delete,
                        Status = entryMeritStudent.Status,
                    };
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Add(NewStudentMeritApprovalHs);
                }
            }
            else if(body.TypeRequest == AchievementTypeStatus.Approved)
            {
                TrStudentPoint studentPoint = default;
                studentPoint = await _dbContext.Entity<TrStudentPoint>()
                .Where(e => e.IdHomeroomStudent == entryMeritStudent.IdHomeroomStudent)
                .FirstOrDefaultAsync(CancellationToken);

                if (entryMeritStudent.RequestType == RequestType.Delete)
                {
                    var lastHistoryCreate = entryMeritStudent.StudentMeritApproval
                                        .Where(e => e.RequestType == RequestType.Create)
                                        .OrderBy(e => e.DateIn)
                                        .LastOrDefault();

                    meritHistory.Status = "Approved";
                    meritHistory.Note1 = body.Note;
                    meritHistory.IsApproved1 = true;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(meritHistory);

                    entryMeritStudent.Status = "Deleted";
                    _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);
                }
                else if (entryMeritStudent.RequestType == RequestType.Create)
                {
                    meritHistory.Status = "Approved";
                    meritHistory.Note1 = body.Note;
                    meritHistory.IsApproved1 = true;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(meritHistory);

                    if (studentPoint == null)
                    {
                        studentPoint = new TrStudentPoint
                        {
                            Id = Guid.NewGuid().ToString(),
                            MeritPoint = entryMeritStudent.Point,
                            DemeritPoint = 0,
                            IdHomeroomStudent = entryMeritStudent.IdHomeroomStudent,
                        };
                            _dbContext.Entity<TrStudentPoint>().Add(studentPoint);
                    }
                    else
                    {
                        studentPoint.MeritPoint = studentPoint.MeritPoint + entryMeritStudent.Point;
                        _dbContext.Entity<TrStudentPoint>().Update(studentPoint);
                    }

                    entryMeritStudent.Status = meritHistory.Status;
                    _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);
                }
            }
            else if (body.TypeRequest == AchievementTypeStatus.Declined)
            {
                if (entryMeritStudent.RequestType == RequestType.Delete)
                {
                    var lastHistoryCreate = entryMeritStudent.StudentMeritApproval
                                        .Where(e => e.RequestType == RequestType.Create)
                                        .OrderBy(e => e.DateIn)
                                        .LastOrDefault();

                    meritHistory.Status = "Declined";
                    meritHistory.Note1 = body.Note;
                    meritHistory.IsApproved1 = false;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(meritHistory);

                    var NewStudentMeritApprovalHs = new TrStudentMeritApprovalHs
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdTrEntryMeritStudent = meritHistory.IdTrEntryMeritStudent,
                        IdUserApproved1 = meritHistory.IdUserApproved1,
                        RequestType = RequestType.Create,
                        Status = lastHistoryCreate.Status,
                    };
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Add(NewStudentMeritApprovalHs);

                    entryMeritStudent.Status = lastHistoryCreate.Status;
                    _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);
                }
                else if (entryMeritStudent.RequestType == RequestType.Create)
                {
                    meritHistory.Status = "Declined";
                    meritHistory.Note1 = body.Note;
                    meritHistory.IsApproved1 = false;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(meritHistory);

                    entryMeritStudent.Status = meritHistory.Status;
                    _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            
            // Send email & notification
            if (body.TypeRequest == AchievementTypeStatus.Approved || body.TypeRequest == AchievementTypeStatus.Declined)
            {
                var dataEmail = new StudentAchievementNotificationResult
                {
                    Id = entryMeritStudent.Id,
                    AcademicYear = entryMeritStudent.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                    Semester = entryMeritStudent.HomeroomStudent.Semester,
                    AchievementName = entryMeritStudent.Note,
                    AchievementCategory = entryMeritStudent.MeritDemeritMapping.DisciplineName,
                    IdVerifyingTeacher = meritHistory.IdUserApproved1,
                    VerifyingTeacher = meritHistory.User1.DisplayName,
                    FocusArea = entryMeritStudent.FocusArea.Description,
                    DateCompletion = entryMeritStudent.DateMerit,
                    CreatedBy = studentName.name.ToString(),
                    Point = entryMeritStudent.Point,
                    ApprovalNotes = body.Note,
                    Status = meritHistory.Status,
                    StudentName = studentName.name.ToString()
                };

                if (KeyValues.ContainsKey(KeyName))
                    KeyValues.Remove(KeyName);

                KeyValues.Add(KeyName, dataEmail);

                List<string> idUserRecipients = new List<string>
                {
                    entryMeritStudent.HomeroomStudent.IdStudent
                };

                if (body.TypeRequest == AchievementTypeStatus.Approved)
                    await SANotification(KeyValues, AuthInfo, idUserRecipients, "SA3");
                else
                    await SANotification(KeyValues, AuthInfo, idUserRecipients, "SA4");
            }
            else if (body.TypeRequest == AchievementTypeStatus.Deleted && entryMeritStudent.Status == "Delete Requested")
            {
                var dataEmail = new StudentAchievementNotificationResult
                {
                    Id = entryMeritStudent.Id,
                    AcademicYear = entryMeritStudent.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                    Semester = entryMeritStudent.HomeroomStudent.Semester,
                    AchievementName = entryMeritStudent.Note,
                    AchievementCategory = entryMeritStudent.MeritDemeritMapping.DisciplineName,
                    FocusArea = entryMeritStudent.FocusArea.Description,
                    StudentName = studentName.name.ToString(),
                    DateCompletion = entryMeritStudent.DateMerit,
                    Point = entryMeritStudent.Point,
                    ApprovalNotes = "Achievement Decline",
                    Status = entryMeritStudent.Status,
                };

                if (KeyValues.ContainsKey(KeyName))
                    KeyValues.Remove(KeyName);

                KeyValues.Add(KeyName, dataEmail);

                List<string> idUserRecipients = new List<string>
                {
                    meritHistory.IdUserApproved1
                };

                await SANotification(KeyValues, AuthInfo, idUserRecipients, "SA2");
            }
            return Request.CreateApiResult2();
        }

        private async Task SANotification(IDictionary<string, object> keyValues, AuthenticationInfo authInfo, List<string> idUserRecipients, string idScenario)
        {
            var obj = keyValues.FirstOrDefault(x => x.Key == KeyName).Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentAchievementNotificationResult>(JsonConvert.SerializeObject(obj));

            if (keyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, idScenario)
                {
                    IdRecipients = idUserRecipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }
    }
}
