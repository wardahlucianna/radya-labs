using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanApprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IAttendanceSummary _attendanceSummary;
        private readonly IRolePosition _rolePosition;

        public LessonPlanApprovalHandler(ITeachingDbContext dbContext, IAttendanceSummary attendanceSummary, IRolePosition rolePosition)
        {
            _dbContext = dbContext;
            _attendanceSummary = attendanceSummary;
            _rolePosition = rolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonPlanApprovalRequest>(nameof(GetLessonPlanApprovalRequest.IdAcademicYear));

            var data = new List<GetLessonPlanApprovalQueryResult>();

            var msAcademicYear = await _dbContext.Entity<MsAcademicYear>().FirstOrDefaultAsync(x => x.Id == param.IdAcademicYear, CancellationToken);

            var msLessonPlanApprover = await _dbContext.Entity<MsLessonPlanApproverSetting>()
                                            .Include(x => x.Role)
                                            .Include(x => x.TeacherPosition)
                                                .ThenInclude(x => x.Position)
                                            .Where(x => x.Role.IdSchool == msAcademicYear.IdSchool)
                                            .ToListAsync(CancellationToken);

            var approverSetting = new List<GetUserEmailRecepient>();

            foreach (var item in msLessonPlanApprover)
            {
                approverSetting.Add(new GetUserEmailRecepient
                {
                    IdRole = item.IdRole,
                    IdTeacherPosition = item.IdTeacherPosition,
                    IdUser = item.IdBinusian
                });
            }

            var subjectByUser = _rolePosition.GetUserSubjectByEmailRecepient(new GetUserSubjectByEmailRecepientRequest
            {
                IdUser = param.IdUser,
                IdAcademicYear = param.IdAcademicYear,
                IdSchool = msAcademicYear.IdSchool,
                IsShowIdUser = false,
                EmailRecepients = approverSetting
            }).Result.Payload;

            var lessonByUser = subjectByUser.Select(x => x.Lesson.Id).Distinct().ToList();

            var getLessonPlanQuery = await GetLessonPlanApprovalQuery();

            var dataLessonByUser = getLessonPlanQuery.Where(x => lessonByUser.Contains(x.IdLesson)).Distinct().ToList();

            foreach (var item in dataLessonByUser)
            {
                data.Add(new GetLessonPlanApprovalQueryResult
                {
                    IdLessonPlanApproval = item.IdLessonPlanApproval,
                    IdLessonPlan = item.IdLessonPlan,
                    IdSubject = item.IdSubject,
                    IdSubjectLevel = item.IdSubjectLevel,
                    IdWeekSettingDetail = item.IdWeekSettingDetail,
                    AcademicYear = item.AcademicYear,
                    Level = item.Level,
                    Grade = item.Grade,
                    Period = item.Period,
                    Subject = item.Subject,
                    SubjectLevel = item.SubjectLevel,
                    Periode = item.Periode,
                    DeadlineDate = item.DeadlineDate,
                    TeacherId = item.TeacherId,
                    TeacherName = item.TeacherName
                });
            }

            var count = param.CanCountWithoutFetchDb(data.ToList().Count)
                ? data.ToList().Count
                : data.Select(x => x.AcademicYear).Count();

            if (param.IdAcademicYear != null)
                data = data.Where(x => x.AcademicYear.Id == param.IdAcademicYear).ToList();

            if (param.IdLevel != null)
                data = data.Where(x => x.Level.Id == param.IdLevel).ToList();

            if (param.IdGrade != null)
                data = data.Where(x => x.Grade.Id == param.IdGrade).ToList();

            if (param.IdPeriod != null)
                data = data.Where(x => x.Period.Id == param.IdPeriod).ToList();

            if (param.IdSubject != null)
                data = data.Where(x => x.Subject.Id == param.IdSubject).ToList();

            if (param.IdTeacher != null)
                data = data.Where(x => x.TeacherId == param.IdTeacher).ToList();

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                data = data.Where(x
                    => x.AcademicYear.Code.ToLower().Contains(param.Search.ToLower())
                    || x.Level.Code.ToLower().Contains(param.Search.ToLower())
                    || x.Grade.Description.ToLower().Contains(param.Search.ToLower())
                    || x.Subject.Description.ToLower().Contains(param.Search.ToLower())
                    || x.SubjectLevel.ToLower().Contains(param.Search.ToLower())
                    || x.TeacherName.ToLower().Contains(param.Search.ToLower())
                    || x.Periode.ToLower().Contains(param.Search.ToLower())
                ).ToList();
            }

            switch (param.OrderBy)
            {
                case "academicYear":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.AcademicYear.Code).ToList()
                        : data.OrderBy(x => x.AcademicYear.Code).ToList();
                    break;
                case "level":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.Level.Code).ToList()
                        : data.OrderBy(x => x.Level.Code).ToList();
                    break;
                case "grade":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.Grade.Description).ToList()
                        : data.OrderBy(x => x.Grade.Description).ToList();
                    break;
                case "subject":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.Subject.Description).ToList()
                        : data.OrderBy(x => x.Subject.Description).ToList();
                    break;
                case "subjectLevel":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.SubjectLevel).ToList()
                        : data.OrderBy(x => x.SubjectLevel).ToList();
                    break;
                case "teacherName":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.SubjectLevel).ToList()
                        : data.OrderBy(x => x.SubjectLevel).ToList();
                    break;
                case "periode":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.SubjectLevel).ToList()
                        : data.OrderBy(x => x.SubjectLevel).ToList();
                    break;
                case "deadlineDate":
                    data = param.OrderType == OrderType.Desc
                        ? data.OrderByDescending(x => x.DeadlineDate).ToList()
                        : data.OrderBy(x => x.DeadlineDate).ToList();
                    break;
            };

            var lessonPlanApprovalData = data
                .SetPagination(param)
                .ToList();

            var lessonPlanIds = lessonPlanApprovalData
                .Select(x => x.IdLessonPlan)
                .ToList();

            var lessonPlans = _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.LessonPlanDocuments)
                .Where(x => lessonPlanIds.Contains(x.Id))
                .ToList();

            var lessonPlanApprovalQuery = lessonPlanApprovalData.Select(x => new GetLessonPlanApprovalResult
            {
                IdLessonPlanApproval = x.IdLessonPlanApproval,
                IdPeriod = x.IdPeriod,
                IdSubject = x.IdSubject,
                IdSubjectLevel = x.IdSubjectLevel,
                IdWeekSettingDetail = x.IdWeekSettingDetail,
                AcademicYear = x.AcademicYear.Code,
                Level = x.Level.Code,
                Grade = x.Grade.Description,
                Period = x.Period.Description,
                Subject = x.Subject.Description,
                SubjectLevel = x.SubjectLevel,
                Periode = x.Periode,
                DeadlineDate = x.DeadlineDate,
                TeacherName = x.TeacherName,
                IdLessonPlan = x.IdLessonPlan,
                LatestDocumentId = lessonPlans
                                    .Where(y => y.Id == x.IdLessonPlan)
                                    .OrderByDescending(x => x.LessonPlanDocuments.Select(a => a.LessonPlanDocumentDate))
                                    .Select(e => e.LessonPlanDocuments.Select(a => a.Id
                                    ).FirstOrDefault()).FirstOrDefault()
            });

            var lessonPlanApprovals = lessonPlanApprovalQuery.ToList();

            return await Task.FromResult(Request.CreateApiResult2(lessonPlanApprovals as object, param.CreatePaginationProperty(count)));
        }

        public async Task<List<GetLessonPlanApprovalQueryDataResult>> GetLessonPlanApprovalQuery()
        {
            var result = (
                    from lpa in _dbContext.Entity<TrLessonPlanApproval>()
                    join lp in _dbContext.Entity<TrLessonPlan>() on lpa.IdLessonPlan equals lp.Id
                    join lt in _dbContext.Entity<MsLessonTeacher>() on lp.IdLessonTeacher equals lt.Id
                    join s in _dbContext.Entity<MsStaff>() on lt.IdUser equals s.IdBinusian
                    join l in _dbContext.Entity<MsLesson>() on lt.IdLesson equals l.Id
                    join subj in _dbContext.Entity<MsSubject>() on l.IdSubject equals subj.Id
                    join smsl in _dbContext.Entity<MsSubjectMappingSubjectLevel>() on lp.IdSubjectMappingSubjectLevel equals smsl.Id into lsmsl
                    from xsmsl in lsmsl.DefaultIfEmpty()
                    join sl in _dbContext.Entity<MsSubjectLevel>() on xsmsl.IdSubjectLevel equals sl.Id into lsl
                    from xsl in lsl.DefaultIfEmpty()
                    join g in _dbContext.Entity<MsGrade>() on subj.IdGrade equals g.Id
                    join lv in _dbContext.Entity<MsLevel>() on g.IdLevel equals lv.Id
                    join ay in _dbContext.Entity<MsAcademicYear>() on lv.IdAcademicYear equals ay.Id
                    join wsd in _dbContext.Entity<MsWeekSettingDetail>() on lp.IdWeekSettingDetail equals wsd.Id
                    join ws in _dbContext.Entity<MsWeekSetting>() on wsd.IdWeekSetting equals ws.Id
                    join mp in _dbContext.Entity<MsPeriod>() on ws.IdPeriod equals mp.Id
                    where lpa.IsApproved == false && lpa.Reason == null && wsd.Status == true
                    orderby ay.Code descending
                    select new GetLessonPlanApprovalQueryDataResult
                    {
                        IdUser = lpa.IdUser,
                        IdSchool = ay.IdSchool,
                        IdLessonPlanApproval = lpa.Id,
                        IdLesson = l.Id,
                        IdLessonPlan = lp.Id,
                        IdSubject = subj.Id,
                        IdSubjectLevel = xsl.Id,
                        IdWeekSettingDetail = wsd.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = ay.Id,
                            Code = ay.Code,
                            Description = ay.Description
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = lv.Id,
                            Code = lv.Code,
                            Description = lv.Description
                        },
                        Grade = new ItemValueVm
                        {
                            Id = g.Id,
                            Description = g.Description
                        },
                        Period = new ItemValueVm
                        {
                            Id = mp.Id,
                            Description = mp.Description
                        },
                        Subject = new ItemValueVm
                        {
                            Id = subj.Id,
                            Description = subj.Description
                        },
                        SubjectLevel = xsl.Code ?? "-",
                        Periode = "Week " + wsd.WeekNumber,
                        DeadlineDate = wsd.DeadlineDate,
                        TeacherId = s.IdBinusian,
                        TeacherName = s.FirstName + (s.LastName != null ? (" " + s.LastName) : " ")
                    }
                    ).ToList();

            return result;
        }

        public async Task<IReadOnlyList<CodeWithIdVm>> GetAvailablePosition(string idUser, string idAcadyear)
        {
            var query =
                from user in _dbContext.Entity<MsUser>()
                let hasCa = _dbContext
                    .Entity<MsHomeroomTeacher>()
                    .Any(x => x.IdBinusian == idUser && x.Homeroom.IdAcademicYear == idAcadyear)
                let hasSt = _dbContext
                    .Entity<MsLessonTeacher>()
                    .Any(x => x.IdUser == idUser && x.Lesson.IdAcademicYear == idAcadyear)
                let tp = _dbContext.Entity<TrNonTeachingLoad>()
                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == idAcadyear)
                    .Where(x => x.IdUser == idUser)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.MsNonTeachingLoad.TeacherPosition.Position.Code,
                        Code = x.MsNonTeachingLoad.TeacherPosition.Description,
                        Description = x.MsNonTeachingLoad.TeacherPosition.Description
                    })
                    .ToList()
                where user.Id == idUser
                select new
                {
                    user.Id,
                    hasCa,
                    hasSt,
                    tp
                };
            var result = await query.FirstOrDefaultAsync(CancellationToken);

            var positions = new List<CodeWithIdVm>();
            if (result.hasCa)
                positions.Add(new CodeWithIdVm
                {
                    Id = PositionConstant.ClassAdvisor,
                    Code = "Class Advisor",
                    Description = "Class Advisor"
                });
            if (result.hasSt)
                positions.Add(new CodeWithIdVm
                {
                    Id = PositionConstant.SubjectTeacher,
                    Code = "Subject Teacher",
                    Description = "Subject Teacher"
                });
            if (result.tp.Count != 0)
            {
                var op = result.tp.GroupBy(x => new
                {
                    x.Id,
                    x.Code,
                    x.Description
                })
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Key.Id,
                    Code = x.Key.Code,
                    Description = x.Key.Description
                }).ToList();
                foreach (var tp in op)
                    positions.Add(tp);
            }

            positions = positions.GroupBy(x => new
            {
                x.Id,
                x.Code,
                x.Description
            }).Select(x => new CodeWithIdVm
            {
                Id = x.Key.Id,
                Code = x.Key.Code,
                Description = x.Key.Description
            }).ToList();

            return positions;
        }

        public class GetLessonPlanApprovalQueryDataResult
        {
            public string IdUser { get; set; }
            public string IdSchool { get; set; }
            public string IdLessonPlanApproval { get; set; }
            public string IdLesson { get; set; }
            public string IdLessonPlan { get; set; }
            public string IdPeriod { get; set; }
            public string IdSubject { get; set; }
            public string IdSubjectLevel { get; set; }
            public string IdWeekSettingDetail { get; set; }
            public CodeWithIdVm AcademicYear { get; set; }
            public CodeWithIdVm Level { get; set; }
            public ItemValueVm Grade { get; set; }
            public ItemValueVm Period { get; set; }
            public ItemValueVm Subject { get; set; }
            public string SubjectLevel { get; set; }
            public string TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string Periode { get; set; }
            public DateTime? DeadlineDate { get; set; }
        }
    }
}
