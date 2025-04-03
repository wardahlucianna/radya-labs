using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly ITeacherPosition _teacherPositionService;

        public GetHomeroomDetailHandler(ISchedulingDbContext dbContext, ITeacherPosition teacherPositionService)
        {
            _dbContext = dbContext;
            _teacherPositionService = teacherPositionService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            KeyValues.TryGetValue("id", out var id);

            var query = await _dbContext.Entity<MsHomeroom>()
                .Select(x => new GetHomeroomDetailResult
                {
                    Id = x.Id,
                    Acadyear = new CodeWithIdVm(x.IdAcademicYear, x.AcademicYear.Code, x.AcademicYear.Description),
                    Semester = x.Semester,
                    Grade = new CodeWithIdVm(x.IdGrade, x.Grade.Code, x.Grade.Description),
                    Pathway = string.Join(", ", x.HomeroomPathways.Select(y => y.GradePathwayDetail.Pathway.Code)),
                    Pathways = x.HomeroomPathways.Select(y => new CodeWithIdVm(y.Id,
                        y.GradePathwayDetail.Pathway.Code, y.GradePathwayDetail.Pathway.Description)),
                    Venue = x.IdVenue != null
                        ? new CodeWithIdVm
                        {
                            Id = x.IdVenue,
                            Description = $"{x.Venue.Building.Code} - {x.Venue.Code}"
                        }
                        : null,
                    Classroom = $"{x.Grade.Code}{x.GradePathwayClassroom.Classroom.Code}",
                    Teachers = x.HomeroomTeachers
                        .OrderBy(y => y.DateIn)
                        .Select(y => new HomeroomTeacherDetail
                        {
                            Teacher = new CodeWithIdVm(y.IdBinusian, y.IdBinusian, !string.IsNullOrEmpty(y.Staff.FirstName) ? y.Staff.FirstName : y.Staff.LastName),
                            Position = new CodeWithIdVm(y.IdTeacherPosition),
                            HasAttendance = y.IsAttendance,
                            HasScore = y.IsScore,
                            ShowInReportCard = y.IsShowInReportCard
                        }),
                    Audit = x.GetRawAuditResult2(),
                    School = new GetSchoolResult
                    {
                        Id = x.AcademicYear.IdSchool,
                        Name = x.AcademicYear.School.Name,
                        Description = x.AcademicYear.School.Description
                    }
                })
                .FirstOrDefaultAsync(x => x.Id == (string)id, CancellationToken);

            if (query is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", id));

            if (query.Teachers.Any())
            {
                var teacherPositionsResult = await _teacherPositionService.GetTeacherPositions(new GetTeacherPositionRequest
                {
                    Ids = query.Teachers.Select(x => x.Position.Id).Distinct(),
                    IdSchool = new[] { query.School.Id },
                    Return = CollectionType.Lov
                });

                query.Teachers
                    .ToList()
                    .ForEach(x =>
                    {
                        var tp = teacherPositionsResult.Payload?.FirstOrDefault(y => y.Id == x.Position.Id);
                        x.Position = new CodeWithIdVm(x.Position.Id, tp?.Code, tp?.Description);
                    });
            }

            return Request.CreateApiResult2(query as object);
        }
    }
}
