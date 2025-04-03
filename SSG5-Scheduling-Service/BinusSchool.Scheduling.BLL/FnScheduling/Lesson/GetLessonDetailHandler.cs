using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetLessonDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetLessonDetailHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            KeyValues.TryGetValue("id", out var id);

            var query = await _dbContext.Entity<MsLesson>()
                .Select(x => new GetLessonDetailResult
                {
                    Id = x.Id,
                    Acadyear = new CodeWithIdVm
                    {
                        Id = x.IdAcademicYear,
                        Code = x.Grade.Level.AcademicYear.Code,
                        Description = x.Grade.Level.AcademicYear.Description
                    },
                    Semester = x.Semester,
                    Grade = new CodeWithIdVm
                    {
                        Id = x.IdGrade,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    },
                    ClassIdFormat = x.ClassIdFormat,
                    ClassIdExample = x.ClassIdExample,
                    ClassIdGenerated = x.ClassIdGenerated,
                    TotalPerWeek = x.TotalPerWeek,
                    Homerooms = x.LessonPathways.Select(y => new LessonHomeroomDetail
                    {
                        Id = y.HomeroomPathway.IdHomeroom,
                        Code = y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                        Description = y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Pathways = new[]
                        {
                            new CodeWithIdVm
                            {
                                Id = y.IdHomeroomPathway,
                                Code = y.HomeroomPathway.GradePathwayDetail.Pathway.Code,
                                Description = y.HomeroomPathway.GradePathwayDetail.Pathway.Description
                            }
                        }
                    }),
                    WeekVarian = new CodeWithIdVm
                    {
                        Id = x.IdWeekVariant,
                        Code = x.WeekVariant.Code,
                        Description = string.Join(", ", x.WeekVariant.WeekVarianDetails.Select(y => y.Week.Description).OrderBy(x => x)),
                    },
                    Teachers = x.LessonTeachers.Select(y => new LessonTeacherDetail
                    {
                        Teacher = new NameValueVm
                        {
                            Id = y.IdUser,
                            Name = y.Staff.FirstName != "" ? y.Staff.FirstName : y.Staff.LastName
                        },
                        HasAttendance = y.IsAttendance,
                        HasScore = y.IsScore,
                        IsPrimary = y.IsPrimary,
                        IsClassDiary = y.IsClassDiary,
                        IsLessonPlan = y.IsLessonPlan,
                    }),
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == (string)id, CancellationToken);

            query.Homerooms = query.Homerooms
                .GroupBy(x => x.Id)
                .Select(x => new LessonHomeroomDetail
                {
                    Id = x.Key,
                    Code = x.First().Code,
                    Description = x.First().Description,
                    Pathways = x.SelectMany(y => y.Pathways)
                });

            return Request.CreateApiResult2(query as object);
        }
    }
}
