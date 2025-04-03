using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.Achievement.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Student.FnStudent.Achievement
{
    public class AchievementHandler : FunctionsHttpCrudHandler
    {
        private readonly string KeyName = "studentAchievementResult";
        private readonly IStudentDbContext _dbContext;
        public AchievementHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var item = await _dbContext.Entity<TrEntryMeritStudent>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.MeritDemeritMapping)
                .Include(e => e.EntryMeritStudentEvidances)
                .Include(e => e.FocusArea)
                .Include(e => e.StudentMeritApproval).ThenInclude(e => e.User1)
                .Where(e=>e.Id==id)
               .Select(x => new DetailAchievementResult
               {
                   Id = x.Id,
                   Student = new AchievementStudent
                   {
                       IdStudent = x.HomeroomStudent.IdStudent,
                       IdHomeroomStudent = x.IdHomeroomStudent,
                       Level = new ItemValueVm
                       {
                           Id = x.HomeroomStudent.Homeroom.Grade.MsLevel.Id,
                           Description = x.HomeroomStudent.Homeroom.Grade.MsLevel.Description
                       },
                       Grade = new ItemValueVm
                       {
                           Id = x.HomeroomStudent.Homeroom.Grade.Id,
                           Description = x.HomeroomStudent.Homeroom.Grade.Description
                       },
                       Homeroom = new ItemValueVm
                       {
                           Id = x.HomeroomStudent.Homeroom.Id,
                           Description = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code
                       }

                   },
                   AchievementName = x.Note,
                   AchievementCategory = new ItemValueVm
                   {
                       Id = x.MeritDemeritMapping.Id,
                       Description = x.MeritDemeritMapping.DisciplineName
                   },
                   DateOfCompletion = x.DateMerit,
                   UserTeacher = x.StudentMeritApproval
                               .Select(e=> new ItemValueVm
                               {
                                   Id = e.User1.Id,
                                   Description = e.User1.DisplayName
                               }).FirstOrDefault(),
                   Evidance = x.EntryMeritStudentEvidances.OrderBy(e => e.DateIn).Select(e => new AchievementEvidance
                   {
                       Id = e.Id,
                       FileName = e.FileName,
                       FileSize = e.FileSize,
                       FileType = e.FileType,
                       OriginalName = e.OriginalName,
                       Url = e.Url
                   }).LastOrDefault(),
                   FocusArea = new ItemValueVm
                   {
                       Id = x.FocusArea.Id,
                       Description = x.FocusArea.Description
                   },
               }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }

        protected async override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAchievementRequest>();
            string[] _columns = { "AcademicYear", "Semester", "AchievementName", "AchievementCategory", "VerifyingTeacher","FocusArea", "DateOfCompletion", "Point", "CreateBy","MeritAchievement","ApprovalNote", "Status" };

            var predicate = PredicateBuilder.Create<TrEntryMeritStudent>(x=>x.IsActive);

            if (!string.IsNullOrEmpty(param.IdUser))
            {
                if (!string.IsNullOrEmpty(param.Role))
                {
                    if (param.Role == RoleConstant.Student) 
                        predicate = predicate.And(x =>  x.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear==param.IdAcademicYear
                                                        && x.HomeroomStudent.IdStudent==param.IdUser
                                                        && ((x.Type == EntryMeritStudentType.Achievement && x.UserIn == param.IdUser && x.Status!= "Deleted")
                                                            || (x.Type == EntryMeritStudentType.Merit && x.Status == "Approved")
                                                            ));
                    else if (param.Role != RoleConstant.Student && param.Role != RoleConstant.Parent)
                        predicate = predicate.And(x => x.StudentMeritApproval.Where(e=>e.IdUserApproved1==param.IdUser).Any() 
                                                    && x.Type == EntryMeritStudentType.Achievement);
                }
            }

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.Status))
                predicate = predicate.And(x => x.Status == param.Status);

            if (!string.IsNullOrEmpty(param.Type.ToString()))
                predicate = predicate.And(x => x.Type == param.Type);

            //serach
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Note.ToLower().Contains(param.Search) || x.MeritDemeritMapping.DisciplineName.ToLower().Contains(param.Search));

            var listUser = await _dbContext.Entity<MsUser>()
                           .ToListAsync(CancellationToken);

            var listEntryMerit = await _dbContext.Entity<TrEntryMeritStudent>()
                            .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
                            .Include(e => e.MeritDemeritMapping)
                            .Include(e => e.EntryMeritStudentEvidances)
                            .Include(e => e.FocusArea)
                            .Include(e => e.StudentMeritApproval).ThenInclude(e=>e.User1)
                            .Where(predicate)
                           .Select(x => new GetAchievementResult
                           {
                               Id = x.Id,
                               AcademicYear = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                               Semester = x.HomeroomStudent.Homeroom.Semester,
                               AchievementName = x.Note,
                               AchievementCategory = x.MeritDemeritMapping.DisciplineName,
                               VerifyingTeacher = x.Type == EntryMeritStudentType.Merit
                                                            ? NameUtil.GenerateFullNameWithId(x.UserCraete.Id, x.UserCraete.DisplayName)
                                                            : x.StudentMeritApproval
                                                                .OrderBy(f => f.DateIn)
                                                                .Select(f => NameUtil.GenerateFullNameWithId(f.User1.Id, f.User1.DisplayName))
                                                                .FirstOrDefault(),
                               FocusArea = x.FocusArea!=null?x.FocusArea.Description:null,
                               DateOfCompletion = x.DateMerit,
                               Evidance = x.EntryMeritStudentEvidances.OrderBy(e=>e.DateIn).Select(e=> new AchievementEvidance
                               {
                                   FileName = e.FileName,
                                   FileSize = e.FileSize,
                                   FileType = e.FileType,
                                   OriginalName = e.OriginalName,
                                   Url = e.Url
                               }).LastOrDefault(),
                               Point = x.Point,
                               CreateBy = GetUserCreate(x.UserIn,listUser),
                               Status = x.Status,
                               ApprovalNote = x.StudentMeritApproval.OrderBy(e=>e.DateIn).Select(e=>e.Note1).LastOrDefault(),
                               Type = x.Type.GetDescription(),
                               Student = new AchievementStudent
                               {
                                   IdStudent = x.HomeroomStudent.IdStudent,
                                   StudentName = NameUtil.GenerateFullName(x.HomeroomStudent.Student.FirstName, x.HomeroomStudent.Student.MiddleName, x.HomeroomStudent.Student.LastName)
                               }
                           }).ToListAsync(CancellationToken);

            var query = listEntryMerit.Distinct();

            //orderBy
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "AchievementName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AchievementName)
                        : query.OrderBy(x => x.AchievementName);
                    break;
                case "AchievementCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AchievementCategory)
                        : query.OrderBy(x => x.AchievementCategory);
                    break;
                case "VerifyingTeacher":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.VerifyingTeacher)
                        : query.OrderBy(x => x.VerifyingTeacher);
                    break;
                case "DateOfCompletion":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.DateOfCompletion)
                        : query.OrderBy(x => x.DateOfCompletion);
                    break;
                case "Point":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Point)
                        : query.OrderBy(x => x.Point);
                    break;
                case "CreateBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CreateBy)
                        : query.OrderBy(x => x.CreateBy);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "FocusArea":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.FocusArea)
                        : query.OrderBy(x => x.FocusArea);
                    break;
                case "ApprovalNote":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.FocusArea)
                        : query.OrderBy(x => x.FocusArea);
                    break;
                case "MeritAchievement":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Type)
                        : query.OrderBy(x => x.Type);
                    break;

            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = query
                        .ToList();
            }
            else
            {
                items = query
                        .SetPagination(param)
                        .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddAchievementRequest, AddAchievementValidator>();

            #region ValidationDoubleInputData
            var dataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(x => x.Student)
                            .Where(e => e.Id == body.IdHomeroomStudent)
                            .Select(x => new
                            {
                                FullName = (x.Student.FirstName == null ? "" : x.Student.FirstName) + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName)
                            })
                           .FirstOrDefaultAsync(CancellationToken);

            var exitsData = await _dbContext.Entity<TrEntryMeritStudent>()
            .Where(e => e.IdHomeroomStudent == body.IdHomeroomStudent &&
                        e.IdMeritDemeritMapping == body.IdAchievementCategory &&
                        e.Point == body.Point &&
                        e.Note == body.AchievementName &&
                        e.RequestType == RequestType.Create &&
                        e.DateMerit == body.Date &&
                        e.MeritUserCreate == body.IdUser &&
                        e.IdFocusArea == body.IdFocusArea &&
                        e.Type == EntryMeritStudentType.Achievement)
            .AnyAsync(CancellationToken);

            if (exitsData)
            {
                throw new BadRequestException(string.Format($"Data is exist for student : {dataStudent.FullName}"));
            }
            #endregion

            var NewEntryMeritStudent = new TrEntryMeritStudent
            {
                Id = Guid.NewGuid().ToString(),
                IdHomeroomStudent = body.IdHomeroomStudent,
                IdMeritDemeritMapping = body.IdAchievementCategory,
                Point = body.Point,
                Note = body.AchievementName,
                RequestType = RequestType.Create,
                Status = "Waiting for Approval",
                DateMerit = body.Date,
                MeritUserCreate = body.IdUser,
                IdFocusArea = body.IdFocusArea,
                Type = EntryMeritStudentType.Achievement,
                IsHasBeenApproved = true,
            };
            _dbContext.Entity<TrEntryMeritStudent>().Add(NewEntryMeritStudent);

            if (body.Evidance != null)
            {
                var NewEntryMeritStudentEvidance = new TrEntryMeritStudentEvidance
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEntryMeritStudent = NewEntryMeritStudent.Id,
                    FileName = body.Evidance.FileName,
                    FileSize = body.Evidance.FileSize,
                    FileType = body.Evidance.FileType,
                    OriginalName = body.Evidance.OriginalName,
                    Url = body.Evidance.Url
                };
                _dbContext.Entity<TrEntryMeritStudentEvidance>().Add(NewEntryMeritStudentEvidance);
            }
            
            var NewStudentMeritApprovalHs = new TrStudentMeritApprovalHs
            {
                Id = Guid.NewGuid().ToString(),
                IdTrEntryMeritStudent = NewEntryMeritStudent.Id,
                IdUserApproved1 = body.IdUserTecaher,
                RequestType = RequestType.Create,
                Status = NewEntryMeritStudent.Status,
            };
            _dbContext.Entity<TrStudentMeritApprovalHs>().Add(NewStudentMeritApprovalHs);

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Send Email ESA001
            var emailDataRequest = await _dbContext.Entity<TrEntryMeritStudent>()
                .Where(x => x.Id == NewEntryMeritStudent.Id)
                .Select(x => new
                {
                    academicYear = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                    achievementCategory = x.MeritDemeritMapping.DisciplineName,
                    focusArea = x.FocusArea.Description,
                    studentName = $"{NameUtil.GenerateFullName(x.HomeroomStudent.Student.FirstName, x.HomeroomStudent.Student.MiddleName, x.HomeroomStudent.Student.LastName)} - {x.HomeroomStudent.IdStudent}",
                    semester = x.HomeroomStudent.Semester
                }).FirstOrDefaultAsync(CancellationToken);

            if (emailDataRequest is null)
                throw new NotFoundException("Email data request not found");

            var dataEmail = new StudentAchievementNotificationResult
            {
                Id = NewEntryMeritStudent.Id,
                AcademicYear = emailDataRequest.academicYear,
                Semester = emailDataRequest.semester,
                AchievementName = NewEntryMeritStudent.Note,
                AchievementCategory = emailDataRequest.achievementCategory,
                FocusArea = emailDataRequest.focusArea,
                StudentName = emailDataRequest.studentName,
                DateCompletion = NewEntryMeritStudent.DateMerit,
                Point = NewEntryMeritStudent.Point,
                ApprovalNotes = "-",
                Status = NewEntryMeritStudent.Status,
            };

            if (KeyValues.ContainsKey(KeyName))
                KeyValues.Remove(KeyName);

            KeyValues.Add(KeyName, dataEmail);

            List<string> idUserRecipients = new List<string>
            {
                NewStudentMeritApprovalHs.IdUserApproved1
            };

            await SA1Notification(KeyValues, AuthInfo, idUserRecipients);
            #endregion

            return Request.CreateApiResult2();
        }

        private async Task SA1Notification(IDictionary<string, object> keyValues, AuthenticationInfo authInfo, List<string> idUserRecipients)
        {
            var obj = keyValues.FirstOrDefault(x => x.Key == KeyName).Value;
            var emailDownload = JsonConvert.DeserializeObject<StudentAchievementNotificationResult>(JsonConvert.SerializeObject(obj));

            if (keyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SA1")
                {
                    IdRecipients = idUserRecipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }

        protected async override Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateAchievementRequest, UpdateAchievementValidator>();

            var entryMeritStudent = await _dbContext.Entity<TrEntryMeritStudent>()
                .Include(e => e.EntryMeritStudentEvidances)
                .Include(e => e.FocusArea)
                .Include(e => e.StudentMeritApproval)
                .Where(e => e.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);

            entryMeritStudent.IdMeritDemeritMapping = body.IdAchievementCategory;
            entryMeritStudent.Point = body.Point;
            entryMeritStudent.Note = body.AchievementName;
            entryMeritStudent.DateMerit = body.Date;
            entryMeritStudent.IdFocusArea = body.IdFocusArea;
            _dbContext.Entity<TrEntryMeritStudent>().Update(entryMeritStudent);

            if (body.Evidance != null)
            {
                var updateEvidance = entryMeritStudent.EntryMeritStudentEvidances.FirstOrDefault();

                if(updateEvidance != null)
                {
                    updateEvidance.FileName = body.Evidance.FileName;
                    updateEvidance.FileSize = body.Evidance.FileSize;
                    updateEvidance.FileType = body.Evidance.FileType;
                    updateEvidance.OriginalName = body.Evidance.OriginalName;
                    updateEvidance.Url = body.Evidance.Url;
                    _dbContext.Entity<TrEntryMeritStudentEvidance>().Update(updateEvidance);
                }
                else
                {
                    var NewEntryMeritStudentEvidance = new TrEntryMeritStudentEvidance
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEntryMeritStudent = entryMeritStudent.Id,
                        FileName = body.Evidance.FileName,
                        FileSize = body.Evidance.FileSize,
                        FileType = body.Evidance.FileType,
                        OriginalName = body.Evidance.OriginalName,
                        Url = body.Evidance.Url
                    };
                    _dbContext.Entity<TrEntryMeritStudentEvidance>().Add(NewEntryMeritStudentEvidance);
                }
                
            }
            

            var updateApproval = entryMeritStudent.StudentMeritApproval.FirstOrDefault();
            updateApproval.IdUserApproved1 = body.IdUserTecaher;
            _dbContext.Entity<TrStudentMeritApprovalHs>().Update(updateApproval);

            //cek status apakah berubah
            var status = await _dbContext.Entity<TrEntryMeritStudent>()
                .Where(e => e.Id == body.Id)
                .Select(e=>e.Status)
                .FirstOrDefaultAsync(CancellationToken);

            if (body.IdUser == entryMeritStudent.UserIn && status != "Waiting for Approval")
                throw new BadRequestException($"cant update because status is {status}");

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        public static string GetUserCreate(string IdUser, List<MsUser> listUser)
        {
            var User = listUser.Where(e=>e.Id==IdUser).FirstOrDefault();

            var value = NameUtil.GenerateFullNameWithId(User.Id, User.DisplayName);

            return value;
        }
    }
}
