using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Notification;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Common.Abstractions;
using Microsoft.Azure.Documents.SystemFunctions;
using Org.BouncyCastle.Crypto;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class ClassDiaryHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;
        private readonly IMachineDateTime _datetime;
        public ClassDiaryHandler(ISchedulingDbContext dbContext, IRolePosition serviceRolePosition, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _serviceRolePosition = serviceRolePosition;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext.Entity<TrClassDiary>()
                .Include(e => e.ClassDiaryAttachments)
                .Include(e => e.HistoryClassDiaries)
                .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                .Include(e => e.ClassDiaryTypeSetting)
                .Include(e => e.User)
               .Where(e => e.Id == id)
              .Select(x => new GetDetailClassDiaryResult
              {
                  ClassDiaryId = x.Id,
                  AcademicYear = new CodeWithIdVm
                  {
                      Id = x.Homeroom.AcademicYear.Id,
                      Code = x.Homeroom.AcademicYear.Code,
                      Description = x.Homeroom.AcademicYear.Description
                  },
                  Level = new CodeWithIdVm
                  {
                      Id = x.Homeroom.Grade.Level.Id,
                      Code = x.Homeroom.Grade.Level.Code,
                      Description = x.Homeroom.Grade.Level.Description
                  },
                  Grade = new CodeWithIdVm
                  {
                      Id = x.Homeroom.Grade.Id,
                      Code = x.Homeroom.Grade.Code,
                      Description = x.Homeroom.Grade.Description
                  },
                  Subject = new CodeWithIdVm
                  {
                      Id = x.Lesson.Subject.Id,
                      Code = x.Lesson.Subject.Code,
                      Description = x.Lesson.Subject.Description
                  },
                  Semester = x.Lesson.Semester,
                  Homeroom = new ItemValueVm
                  {
                      Id = x.Homeroom.Id,
                      Description = (x.Homeroom.Grade.Code) + (x.Homeroom.GradePathwayClassroom.Classroom.Code)
                  },
                  ClassId = new ItemValueVm
                  {
                      Id = x.Lesson.Id,
                      Description = x.Lesson.ClassIdGenerated
                  },
                  date = x.ClassDiaryDate,
                  ClassSettingType = new ItemValueVm
                  {
                      Id = x.ClassDiaryTypeSetting.Id,
                      Description = x.ClassDiaryTypeSetting.TypeName
                  },
                  Teacher = new ItemValueVm
                  {
                      Id = x.User.Id,
                      Description = x.User.DisplayName
                  },
                  Status = x.Status,
                  Topic = x.ClassDiaryTopic,
                  Description = x.ClassDiaryDescription,
                  DeleteReason = x.HistoryClassDiaries.OrderByDescending(e => e.DateIn).FirstOrDefault().DeleteReason,
                  ShowButonCalender = true,
                  ShowButonEdit = x.Status == "Delete Requested" ? false : true,
                  ShowButonDelete = x.Status == "Delete Requested" ? false : true,
                  Attachments = x.ClassDiaryAttachments.Select(e => new AttachmantClassDiary
                  {
                      Id = e.Id,
                      Url = e.Url,
                      OriginalFilename = e.OriginalFilename,
                      FileName = e.Filename,
                      FileSize = e.Filesize,
                      FileType = e.Filetype,
                  }).ToList(),
              }).SingleOrDefaultAsync(CancellationToken);

            if (items is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ClassDiary"], "Id", id));

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetClassDiaryRequest>();
            string[] _columns = { "AcademicYear", "ClassDiaryGrade", "Subject", "Semester", "Homeroom", "ClassId", "ClassDiaryDate", "ClassDiaryTypeSetting", "ClassDiaryTopic", "Status", };

            List<string> listCodePosition = new List<string>()
                                            {
                                                PositionConstant.HeadOfDepartment,
                                                PositionConstant.SubjectHead,
                                                PositionConstant.LevelHead,
                                                PositionConstant.ClassAdvisor,
                                                PositionConstant.AffectiveCoordinator,
                                                PositionConstant.SubjectTeacher,
                                                PositionConstant.VicePrincipal,
                                                PositionConstant.Principal
                                            };

            var listIdTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                            .Where(e => listCodePosition.Contains(e.Position.Code))
                                            .Select(e => e.Id)
                                            .ToListAsync(CancellationToken);

            var paramPositionByUser = new GetSubjectByUserRequest
            {
                IdAcademicYear = param.AcademicYearId,
                IdUser = param.UserId,
                ListIdTeacherPositions = listIdTeacherPosition,
                IsClassDiary = true
            };
            var getApiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(paramPositionByUser);
            var getSubjectByUser = getApiSubjectByUser.IsSuccess ? getApiSubjectByUser.Payload : null;
            var getIdLessonByUser = new List<string>();
            if (getSubjectByUser != null)
            {
                getIdLessonByUser = getSubjectByUser.Select(e => e.Lesson.Id).Distinct().ToList(); ;
            }

            var query = (from ClassDiary in _dbContext.Entity<TrClassDiary>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on ClassDiary.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Homeroom.IdAcademicYear equals AcademicYear.Id
                         join ClassDiaryTypeSetting in _dbContext.Entity<MsClassDiaryTypeSetting>() on ClassDiary.IdClassDiaryTypeSetting equals ClassDiaryTypeSetting.Id
                         join Lesson in _dbContext.Entity<MsLesson>() on ClassDiary.IdLesson equals Lesson.Id
                         join Subject in _dbContext.Entity<MsSubject>() on Lesson.IdSubject equals Subject.Id
                         join _staff in _dbContext.Entity<MsStaff>() on ClassDiary.ClassDiaryUserCreate equals _staff.IdBinusian into _StaffData
                         from Staff in _StaffData.DefaultIfEmpty()
                             //join Staff in _dbContext.Entity<MsStaff>() on ClassDiary.ClassDiaryUserCreate equals Staff.IdBinusian
                         where AcademicYear.Id == param.AcademicYearId
                         select new
                         {
                             ClassDiaryId = ClassDiary.Id,
                             AcademicYear = AcademicYear.Description,
                             ClassDiaryTypeSetting = ClassDiaryTypeSetting.TypeName,
                             ClassDiaryTypeSettingId = ClassDiaryTypeSetting.Id,
                             SubjectId = Subject.Id,
                             Subject = Subject.Description,
                             ClassId = Lesson.ClassIdGenerated,
                             LessonId = Lesson.Id,
                             ClassDiaryDate = ClassDiary.ClassDiaryDate,
                             ClassDiaryTopic = ClassDiary.ClassDiaryTopic,
                             Status = ClassDiary.Status,
                             ShowButtonEdit = ClassDiary.Status == "Active" ? true : false,
                             ShowButtonDelete = ClassDiary.Status == "Active" ? true : false,
                             GradeId = Homeroom.IdGrade,
                             ClassDiaryGrade = Grade.Description,
                             Semester = Homeroom.Semester,
                             HomeroomId = Homeroom.Id,
                             Homeroom = Grade.Code + Classroom.Code,
                             IdLevel = Grade.IdLevel,
                             ClassDiaryUserCreate = ClassDiary.ClassDiaryUserCreate,
                             TeacherId = Staff.IdBinusian,
                             TeacherName = Staff.FirstName + " " + Staff.LastName,
                         });


            //filter
            if (getIdLessonByUser.Any())
                query = query.Where(x => x.ClassDiaryUserCreate == param.UserId || getIdLessonByUser.Contains(x.LessonId));
            else
                query = query.Where(x => x.ClassDiaryUserCreate == param.UserId);

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.GradeId))
                query = query.Where(x => x.GradeId == param.GradeId);
            if (!string.IsNullOrEmpty(param.SubjectId))
                query = query.Where(x => x.SubjectId == param.SubjectId);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.HomeroomId))
                query = query.Where(x => x.HomeroomId == param.HomeroomId);
            if (!string.IsNullOrEmpty(param.LessonId))
                query = query.Where(x => x.LessonId == param.LessonId);
            if (!string.IsNullOrEmpty(param.ClassDiaryDate.ToString()))
                query = query.Where(x => x.ClassDiaryDate == param.ClassDiaryDate);
            if (!string.IsNullOrEmpty(param.ClassDiaryTypeSettingId))
                query = query.Where(x => x.ClassDiaryTypeSettingId == param.ClassDiaryTypeSettingId);
            if (!string.IsNullOrEmpty(param.ClassDiaryStatus))
                query = query.Where(x => x.Status == param.ClassDiaryStatus);
            if (!string.IsNullOrEmpty(param.ClassId))
                query = query.Where(x => x.ClassId == param.ClassId);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.ClassDiaryTopic.ToLower().Contains(param.Search.ToLower()) || x.TeacherName.ToLower().Contains(param.Search.ToLower()));

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "ClassDiaryGrade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryGrade)
                        : query.OrderBy(x => x.ClassDiaryGrade);
                    break;
                case "Subject":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Subject)
                        : query.OrderBy(x => x.Subject);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
                case "ClassId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassId)
                        : query.OrderBy(x => x.ClassId);
                    break;
                case "ClassDiaryDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryDate)
                        : query.OrderBy(x => x.ClassDiaryDate);
                    break;
                case "ClassDiaryTypeSetting":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryTypeSetting)
                        : query.OrderBy(x => x.ClassDiaryTypeSetting);
                    break;

                case "ClassDiaryTopic":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryTopic)
                        : query.OrderBy(x => x.ClassDiaryTopic);
                    break;
                case "Teacher":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TeacherName)
                        : query.OrderBy(x => x.TeacherName);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetClassDiaryResult
                {
                    Id = x.ClassDiaryId,
                    AcademicYear = x.AcademicYear,
                    Grade = x.ClassDiaryGrade,
                    Subject = x.Subject,
                    Semester = x.Semester,
                    Homeroom = x.Homeroom,
                    ClassId = x.ClassId,
                    ClassDiaryDate = x.ClassDiaryDate,
                    ClassDiaryTypeSetting = x.ClassDiaryTypeSetting,
                    ClassDiaryTopic = x.ClassDiaryTopic,
                    Status = x.Status,
                    Teacher = new ItemValueVm
                    {
                        Id = x.TeacherId,
                        Description = x.TeacherName
                    },
                    ShowButtonDelete = x.ClassDiaryUserCreate == param.UserId
                                        ? x.ShowButtonDelete
                                        : getSubjectByUser.Where(w => w.Lesson.Id == x.LessonId).Any()
                                            ? x.ShowButtonDelete
                                            : false,
                    ShowButtonEdit = x.ClassDiaryUserCreate == param.UserId
                                        ? x.ShowButtonEdit
                                        : getSubjectByUser.Where(w => w.Lesson.Id == x.LessonId).Any()
                                            ? x.ShowButtonEdit
                                            : false,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetClassDiaryResult
                {
                    Id = x.ClassDiaryId,
                    AcademicYear = x.AcademicYear,
                    Grade = x.ClassDiaryGrade,
                    Subject = x.Subject,
                    Semester = x.Semester,
                    Homeroom = x.Homeroom,
                    ClassId = x.ClassId,
                    ClassDiaryDate = x.ClassDiaryDate,
                    ClassDiaryTypeSetting = x.ClassDiaryTypeSetting,
                    ClassDiaryTopic = x.ClassDiaryTopic,
                    Status = x.Status,
                    Teacher = new ItemValueVm
                    {
                        Id = x.TeacherId,
                        Description = x.TeacherName
                    },
                    ShowButtonDelete = x.ClassDiaryUserCreate == param.UserId
                                        ? x.ShowButtonDelete
                                        : getSubjectByUser.Where(w => w.Lesson.Id == x.LessonId).Any()
                                            ? x.ShowButtonDelete
                                            : false,
                    ShowButtonEdit = x.ClassDiaryUserCreate == param.UserId
                                        ? x.ShowButtonEdit
                                        : getSubjectByUser.Where(w => w.Lesson.Id == x.LessonId).Any()
                                            ? x.ShowButtonEdit
                                            : false,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.ClassDiaryId).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddClassDiaryRequest, AddClassDiaryValidator>();

            var GetLessonHomeroom = await (from LessonPathway in _dbContext.Entity<MsLessonPathway>()
                                           join HomeroomPathway in _dbContext.Entity<MsHomeroomPathway>() on LessonPathway.IdHomeroomPathway equals HomeroomPathway.Id
                                           join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomPathway.IdHomeroom equals Homeroom.Id
                                           join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                                           join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                           join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                           join Lesson in _dbContext.Entity<MsLesson>() on LessonPathway.IdLesson equals Lesson.Id
                                           where Lesson.IdAcademicYear == body.AcademicYearId && Lesson.IdGrade == body.GradeId && Lesson.IdSubject == body.SubjectId
                                              && body.LessonId.Contains(Lesson.Id) && Homeroom.Semester == body.Semester
                                           select new
                                           {
                                               LessonId = Lesson.Id,
                                               HomeroomId = Homeroom.Id,
                                           }).Distinct().ToListAsync(CancellationToken);

            if (body.Description.Length > 255)
            {
                throw new BadRequestException("Maximum length of description is 255 character");
            }

            List<string> ids = new List<string>();
            foreach (var LessonId in body.LessonId)
            {
                var HomeroomId = GetLessonHomeroom.Any(e => e.LessonId == LessonId) == false ? null : GetLessonHomeroom.FirstOrDefault(e => e.LessonId == LessonId).HomeroomId;
                string idCD = Guid.NewGuid().ToString();
                ids.Add(idCD);
                var newClassDiary = new TrClassDiary
                {
                    Id = idCD,
                    IdClassDiaryTypeSetting = body.ClassDiaryTypeSettingId,
                    IdHomeroom = HomeroomId,
                    IdLesson = LessonId,
                    ClassDiaryDate = body.Date,
                    ClassDiaryTopic = body.Topic,
                    ClassDiaryDescription = body.Description,
                    ClassDiaryUserCreate = AuthInfo.UserId,
                    Status = "Active",
                };
                _dbContext.Entity<TrClassDiary>().Add(newClassDiary);


                //create attachment
                foreach (var ItemAttachment in body.Attachments)
                {
                    var newClassDiaryAttachment = new TrClassDiaryAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdClassDiary = newClassDiary.Id,
                        OriginalFilename = ItemAttachment.OriginalFilename,
                        Url = ItemAttachment.Url,
                        Filename = ItemAttachment.FileName,
                        Filetype = ItemAttachment.FileType,
                        Filesize = ItemAttachment.FileSize,
                    };
                    _dbContext.Entity<TrClassDiaryAttachment>().Add(newClassDiaryAttachment);

                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Email Notif CD3
            await CD3Notification(ids);
            #endregion

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateClassDiaryRequest, UpdateClassDiaryValidator>();
            var GetClassDiary = await _dbContext.Entity<TrClassDiary>()
                .Include(e => e.ClassDiaryAttachments)
                .Include(e => e.HistoryClassDiaries)
                .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                .Include(e => e.Homeroom).ThenInclude(x => x.HomeroomStudents)
                .Include(e => e.Homeroom).ThenInclude(x => x.HomeroomTeachers)
                .Include(e => e.User)
                .Include(e => e.ClassDiaryTypeSetting)
                .Where(e => e.Id == body.ClassDiaryId)
                .SingleOrDefaultAsync(CancellationToken);

            var GetClassDiaryAttachment = GetClassDiary.ClassDiaryAttachments;

            if (GetClassDiary == null)
                throw new BadRequestException("Class diary with id: " + body.ClassDiaryId + " is not found.");

            var oldDataEmail = new CD4NotificationResult
            {
                AcademicYear = GetClassDiary.Homeroom.AcademicYear.Description,
                Homeroom = GetClassDiary.Homeroom.Grade.Code + GetClassDiary.Homeroom.GradePathwayClassroom.Classroom.Code,
                ClassId = GetClassDiary.Lesson.ClassIdGenerated,
                Date = GetClassDiary.ClassDiaryDate.ToString("dd MMM yyyy"),
                Type = GetClassDiary.ClassDiaryTypeSetting.TypeName,
                Topic = GetClassDiary.ClassDiaryTopic,
                RequestDate = GetClassDiary.DateIn.Value.ToString("dd MMM yyyy"),
                StatusApproval = GetClassDiary.Status,
            };

            //update class diary
            GetClassDiary.ClassDiaryDate = body.Date;
            GetClassDiary.IdClassDiaryTypeSetting = body.ClassDiaryTypeSettingId;
            GetClassDiary.ClassDiaryTopic = body.Topic;

            if (body.Description.Length > 255)
                throw new BadRequestException("Maximum length of description is 255 character");

            GetClassDiary.ClassDiaryDescription = body.Description;

            _dbContext.Entity<TrClassDiary>().Update(GetClassDiary);

            //remove attachment
            foreach (var ItemAttachment in GetClassDiaryAttachment)
            {
                var ExsisBodyAttachment = body.Attachments.Any(e => e.Id == ItemAttachment.Id);

                if (!ExsisBodyAttachment)
                {
                    ItemAttachment.IsActive = false;
                    _dbContext.Entity<TrClassDiaryAttachment>().Update(ItemAttachment);
                }
            }

            //Add attachment
            foreach (var ItemAttachment in body.Attachments.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var newClassDiaryAttachment = new TrClassDiaryAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdClassDiary = GetClassDiary.Id,
                    OriginalFilename = ItemAttachment.OriginalFilename,
                    Url = ItemAttachment.Url,
                    Filename = ItemAttachment.FileName,
                    Filetype = ItemAttachment.FileType,
                    Filesize = ItemAttachment.FileSize,
                };
                _dbContext.Entity<TrClassDiaryAttachment>().Add(newClassDiaryAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Email Notif CD4
            var idUserStudent = GetClassDiary.Homeroom.HomeroomStudents.Select(x => x.IdStudent).Distinct().ToList();
            var idUserHomeroomTeacher = GetClassDiary.Homeroom.HomeroomTeachers.Select(x => x.IdBinusian).Distinct().ToList();

            var idUserLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                    .Where(x => x.IdLesson == GetClassDiary.IdLesson)
                                    .Select(e => e.IdUser)
                                    .ToListAsync(CancellationToken);

            var listReceipments = idUserStudent.Union(idUserHomeroomTeacher).Union(idUserLessonTeacher).Distinct().ToList();

            var newDataEmail = new CD4NotificationResult
            {
                AcademicYear = GetClassDiary.Homeroom.AcademicYear.Description,
                Homeroom = GetClassDiary.Homeroom.Grade.Code + GetClassDiary.Homeroom.GradePathwayClassroom.Classroom.Code,
                ClassId = GetClassDiary.Lesson.ClassIdGenerated,
                Date = GetClassDiary.ClassDiaryDate.ToString("dd MMM yyyy"),
                Type = GetClassDiary.ClassDiaryTypeSetting.TypeName,
                Topic = GetClassDiary.ClassDiaryTopic,
                RequestDate = GetClassDiary.DateIn.Value.ToString("dd MMM yyyy"),
                StatusApproval = GetClassDiary.Status,
            };

            IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
            paramTemplateNotification.Add("IdClassDiary", body.ClassDiaryId);
            paramTemplateNotification.Add("OldData", oldDataEmail);
            paramTemplateNotification.Add("NewData", newDataEmail);
            paramTemplateNotification.Add("ClassDiaryEditor", GetClassDiary.User.DisplayName);
            paramTemplateNotification.Add("Type", GetClassDiary.ClassDiaryTypeSetting.TypeName);
            paramTemplateNotification.Add("EditDate", GetClassDiary.DateUp.Value.ToString("dd MMM yyyy"));
            paramTemplateNotification.Add("SchoolName", AuthInfo.Tenants.FirstOrDefault().Name.ToUpper());
            CD4Notification(paramTemplateNotification, listReceipments);
            #endregion

            return Request.CreateApiResult2();
        }

        private async Task CD3Notification(List<string> ids)
        {
            var classDiaries = await _dbContext.Entity<TrClassDiary>()
                .Include(e => e.Homeroom).ThenInclude(x => x.HomeroomStudents)
                .Include(e => e.Homeroom).ThenInclude(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                .Include(e => e.ClassDiaryTypeSetting)
                .Include(e => e.User)
                .Where(x => ids.Contains(x.Id)).ToListAsync(CancellationToken);

            var listIdLesson = classDiaries.Select(e => e.IdLesson).ToList();

            var idUserStudent = classDiaries.SelectMany(x => x.Homeroom.HomeroomStudents.Select(x => x.IdStudent)).Distinct().ToList();
            var idUserHomeroomTeacher = classDiaries.SelectMany(x => x.Homeroom.HomeroomTeachers.Select(x => x.IdBinusian)).Distinct().ToList();
            var idUserLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                    .Where(x => listIdLesson.Contains(x.IdLesson))
                                    .Select(e => e.IdUser)
                                    .ToListAsync(CancellationToken);

            var listReceipments = idUserStudent.Union(idUserHomeroomTeacher).Union(idUserLessonTeacher).Distinct().ToList();

            IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
            paramTemplateNotification.Add("ClassDiaryCreator", classDiaries.Select(e => e.User.DisplayName).FirstOrDefault());
            paramTemplateNotification.Add("CreateDate", _datetime.ServerTime.Date.ToString("dd MMM yyyy"));
            paramTemplateNotification.Add("IdClassDiary", classDiaries.Select(e => e.Id).FirstOrDefault());
            paramTemplateNotification.Add("SchoolName", AuthInfo.Tenants.First().Name.ToUpper());
            paramTemplateNotification.Add("Type", classDiaries.Select(e => e.ClassDiaryTypeSetting.TypeName).FirstOrDefault());

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CD3")
                {
                    IdRecipients = listReceipments,
                    KeyValues = paramTemplateNotification
                });
                collector.Add(message);
            }

        }

        private async Task CD4Notification(IDictionary<string, object> paramTemplateNotification, List<string> listReceipments)
        {
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CD4")
                {
                    IdRecipients = listReceipments,
                    KeyValues = paramTemplateNotification

                });
                collector.Add(message);
            }
        }
    }
}
