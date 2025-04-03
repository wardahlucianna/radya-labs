using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
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
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class ClassDiaryDeletionApprovalHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public ClassDiaryDeletionApprovalHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext.Entity<HTrClassDiary>()
               .Include(e => e.HistoryClassDiaryAttachments)
               .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
               .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
               .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
               .Include(e => e.Lesson).ThenInclude(e => e.Subject)
               .Include(e => e.ClassDiaryTypeSetting)
              .Where(e => e.IdHTrClassDiary == id)
             .Select(x => new GetDetailClassDiaryDeletionApprovalResult
             {
                 ClassDiaryId = x.IdHTrClassDiary,
                 AcademicYear = new CodeWithIdVm
                 {
                     Id = x.Homeroom.AcademicYear.Id,
                     Code = x.Homeroom.AcademicYear.Code,
                     Description = x.Homeroom.AcademicYear.Description
                 },
                 Grade = new CodeWithIdVm
                 {
                     Id = x.Homeroom.Grade.Id,
                     Code = x.Homeroom.Grade.Code,
                     Description = x.Homeroom.Grade.Description
                 },
                 Level = new CodeWithIdVm
                 {
                     Id = x.Homeroom.Grade.Level.Id,
                     Code = x.Homeroom.Grade.Level.Code,
                     Description = x.Homeroom.Grade.Level.Description

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
                 Topic = x.ClassDiaryTopic,
                 Description = x.ClassDiaryDescription,
                 DeleteReason = x.DeleteReason,
                 Note = x.Note,
                 Status = x.Status,
                 DisabledButtonCalender = x.Status == "Deleted" ? true : false,
                 ShowButtonApproval = x.Status == "Delete Requested" ? true : false,
                 ShowButtonDecline = x.Status == "Delete Requested" ? true : false,
                 Attachments = x.HistoryClassDiaryAttachments.Select(e => new AttachmantClassDiary
                 {
                     Id = e.IdHTrClassDiaryAttachment,
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
            var param = Request.ValidateParams<GetClassDiaryDeletionApprovalRequest>();
            string[] _columns = { "AcademicYear", "ClassDiaryGrade", "Subject", "Semester", "Homeroom", "ClassId", "ClassDiaryDate", "ClassDiaryTypeSetting", "ClassDiaryTopic", "Status", "RequestDate" };

            var QueryLevel = _dbContext.Entity<MsLevel>()
                            .Where(e => e.IdAcademicYear == param.AcademicYearId);
            if (!string.IsNullOrEmpty(param.IdLevel))
                QueryLevel = QueryLevel.Where(x => x.Id == param.IdLevel);
            var getLevel = await QueryLevel.ToListAsync(CancellationToken);


            var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                           .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                           .Where(e => e.IdUser == param.UserId &&
                                                   e.MsNonTeachingLoad.IdAcademicYear == param.AcademicYearId &&
                                                   !string.IsNullOrEmpty(e.Data) &&
                                                   (e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal)
                                                   )
                                           .ToListAsync(CancellationToken);

            List<ItemValueVm> idGrades = new List<ItemValueVm>();

            if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal))
            {
                if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).Count() > 0)
                {
                    var VicePrincipal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList();
                    List<string> IdLevels = new List<string>();
                    foreach (var item in VicePrincipal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        if (getLevel.Select(e=>e.Id).ToList().Contains(_levelLH.Id))
                        {
                            var grades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == _levelLH.Id)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);

                            if (grades.Any())
                            {
                                idGrades.AddRange(grades);
                            }
                        }
                    }
                }
            }
            var _idGrades = idGrades.Select(e => e.Id).Distinct().ToList();

            IReadOnlyList<IItemValueVm> items = default;
            var query = (from ClassDiary in _dbContext.Entity<HTrClassDiary>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on ClassDiary.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Homeroom.IdAcademicYear equals AcademicYear.Id
                         join ClassDiaryTypeSetting in _dbContext.Entity<MsClassDiaryTypeSetting>() on ClassDiary.IdClassDiaryTypeSetting equals ClassDiaryTypeSetting.Id
                         join Lesson in _dbContext.Entity<MsLesson>() on ClassDiary.IdLesson equals Lesson.Id
                         join Subject in _dbContext.Entity<MsSubject>() on Lesson.IdSubject equals Subject.Id
                         where AcademicYear.Id == param.AcademicYearId && _idGrades.Contains(Grade.Id)
                         select new
                         {
                             ClassDiaryId = ClassDiary.IdHTrClassDiary,
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
                             ShowButtonApproval = ClassDiary.Status == "Delete Requested" ? true : false,
                             GradeId = Homeroom.IdGrade,
                             ClassDiaryGrade = Grade.Description,
                             Semester = Homeroom.Semester,
                             HomeroomId = Homeroom.Id,
                             Homeroom = Grade.Code + Classroom.Code,
                             RequestDate = ClassDiary.DateIn,
                             RequestBy = ClassDiary.UserIn,
                             IdLevel = Grade.IdLevel,
                         });

            //filter
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
                query = query.Where(x => x.ClassDiaryDate.Date.Date == Convert.ToDateTime(param.ClassDiaryDate).Date);
            if (!string.IsNullOrEmpty(param.ClassDiaryTypeSettingId))
                query = query.Where(x => x.ClassDiaryTypeSettingId == param.ClassDiaryTypeSettingId);
            if (!string.IsNullOrEmpty(param.ClassDiaryStatus))
                query = query.Where(x => x.Status == param.ClassDiaryStatus);
            if (!string.IsNullOrEmpty(param.RequestDate.ToString()))
                query = query.Where(x => x.RequestDate >= Convert.ToDateTime(param.RequestDate).Date && x.RequestDate <= Convert.ToDateTime(param.RequestDate).Date.AddDays(1).AddMilliseconds(-1));
            if (!string.IsNullOrEmpty(param.RequestBy))
                query = query.Where(x => x.RequestBy == param.RequestBy);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.ClassDiaryTopic.Contains(param.Search));


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
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "RequestDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.RequestDate)
                        : query.OrderBy(x => x.RequestDate);
                    break;
            };

            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetClassDiaryDeletionApprovalResult
                {
                    Id = x.ClassDiaryId,
                    AcademicYear = x.AcademicYear,
                    Grade = x.ClassDiaryGrade,
                    Subject = x.Subject,
                    Semester = x.Semester.ToString(),
                    Homeroom = x.Homeroom,
                    ClassId = x.ClassId,
                    ClassDiaryDate = x.ClassDiaryDate,
                    ClassDiaryTypeSetting = x.ClassDiaryTypeSetting,
                    ClassDiaryTopic = x.ClassDiaryTopic,
                    RequestDate = x.RequestDate,
                    Status = x.Status,
                    ShowButtonApproval = x.ShowButtonApproval,
                }).ToList();

            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetClassDiaryDeletionApprovalResult
                {
                    Id = x.ClassDiaryId,
                    AcademicYear = x.AcademicYear,
                    Grade = x.ClassDiaryGrade,
                    Subject = x.Subject,
                    Semester = x.Semester.ToString(),
                    Homeroom = x.Homeroom,
                    ClassId = x.ClassId,
                    ClassDiaryDate = x.ClassDiaryDate,
                    ClassDiaryTypeSetting = x.ClassDiaryTypeSetting,
                    ClassDiaryTopic = x.ClassDiaryTopic,
                    RequestDate = x.RequestDate,
                    Status = x.Status,
                    ShowButtonApproval = x.ShowButtonApproval,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.ClassDiaryId).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddClassDiaryDeletionApprovalRequest, AddClassDiaryDeletionApprovalValidator>();

            var GetHsClassDiary = await _dbContext.Entity<HTrClassDiary>().Where(e => e.IdHTrClassDiary == body.ClassDiaryId).SingleOrDefaultAsync(CancellationToken);

            if (GetHsClassDiary == null)
                throw new BadRequestException("Class diary is not found.");

            var GetClassDiary = await _dbContext.Entity<TrClassDiary>().Where(e => e.Id == GetHsClassDiary.IdTrClassDiary).SingleOrDefaultAsync(CancellationToken);
            var GetClassDiaryAttachment = await _dbContext.Entity<TrClassDiaryAttachment>().Where(e => e.Id == GetHsClassDiary.IdTrClassDiary).ToListAsync(CancellationToken);

            if (body.Approval)
            {
                //Update History Class Diary
                GetHsClassDiary.Status = "Deleted";
                _dbContext.Entity<HTrClassDiary>().Update(GetHsClassDiary);

                //Update Class Diary
                GetClassDiary.IsActive = false;
                _dbContext.Entity<TrClassDiary>().Update(GetClassDiary);
            }
            else
            {
                //Update History Class Diary
                GetHsClassDiary.Status = "Declined";
                GetHsClassDiary.Note = body.Note;
                _dbContext.Entity<HTrClassDiary>().Update(GetHsClassDiary);

                //Update Class Diary
                GetClassDiary.Status = "Active";
                _dbContext.Entity<TrClassDiary>().Update(GetClassDiary);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            //send notification to counselor && collect other data for template
            var hsClassDiary = await _dbContext.Entity<HTrClassDiary>()
               .Include(e => e.HistoryClassDiaryAttachments)
               .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
               .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
               .Include(e => e.Homeroom).ThenInclude(e => e.HomeroomStudents)
               .Include(e => e.Homeroom).ThenInclude(e => e.HomeroomTeachers)
               .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
               .Include(e => e.Lesson).ThenInclude(e => e.Subject)
               .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(x => x.Staff)
               .Include(e => e.ClassDiaryTypeSetting)
               .SingleOrDefaultAsync(e => e.IdHTrClassDiary == body.ClassDiaryId);

            var user = await _dbContext.Entity<MsUser>().SingleOrDefaultAsync(x => x.Id == hsClassDiary.UserIn);

            IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
            paramTemplateNotification.Add("ReceiverName", user.DisplayName);
            paramTemplateNotification.Add("ReceiverEmail", user.Email);
            paramTemplateNotification.Add("AcademicYear", hsClassDiary.Homeroom.AcademicYear.Description);
            paramTemplateNotification.Add("Class/Homeroom", $"{ hsClassDiary.Homeroom.Grade.Code } { hsClassDiary.Homeroom.GradePathwayClassroom.Classroom.Code }");
            paramTemplateNotification.Add("ClassId", hsClassDiary.Lesson.ClassIdGenerated);
            paramTemplateNotification.Add("Date", hsClassDiary.ClassDiaryDate.ToShortDateString());
            paramTemplateNotification.Add("Topic", hsClassDiary.ClassDiaryTopic);
            paramTemplateNotification.Add("RequestDate", hsClassDiary.DateIn.Value.ToShortDateString());
            paramTemplateNotification.Add("StatusApproval", body.Approval ? "Approve" : "Decline");
            paramTemplateNotification.Add("Detail", hsClassDiary.Note);
            paramTemplateNotification.Add("SchoolName", AuthInfo.Tenants.First().Name);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, body.Approval ? "CD1" : "CD2")
                {
                    IdRecipients = new[] { user.Id },
                    KeyValues = paramTemplateNotification
                });
                collector.Add(message);
            }

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            return Request.CreateApiResult2();
        }


    }
}
