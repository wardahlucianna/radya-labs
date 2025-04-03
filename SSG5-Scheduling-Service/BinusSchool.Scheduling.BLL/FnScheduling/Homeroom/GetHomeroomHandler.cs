using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetHomeroomRequest.IdSchool), nameof(GetHomeroomRequest.IdAcadyear) };
        private static readonly string[] _columns = { "grade", "classroom", "description", "streaming", "teacher", "buildingAndVenue" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "grade.description" },
            { _columns[1], "gradePathwayClassroom.Classroom.code" },
            { _columns[2], "gradePathwayClassroom.Classroom.description" },
            { _columns[5], "venue.building.code" },
        };

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetHomeroomHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomRequest>(_requiredParams);

            var predicate = PredicateBuilder.Create<MsHomeroom>(x => x.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            if (param.Semester != 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdClassroom))
                predicate = predicate.And(x => x.IdGradePathwayClassRoom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.IdPathway))
                predicate = predicate.And(x => x.IdGradePathway == param.IdPathway);
            if (!string.IsNullOrEmpty(param.IdVenue))
                predicate = predicate.And(x => x.IdVenue == param.IdVenue);
            if (!string.IsNullOrEmpty(param.IdTeacher))
                predicate = predicate.And(x => x.HomeroomTeachers.Any(y => y.IdBinusian == param.IdTeacher));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Grade.Description, param.SearchPattern())
                    || EF.Functions.Like(x.GradePathwayClassroom.Classroom.Code, param.SearchPattern())
                    || EF.Functions.Like(x.GradePathwayClassroom.Classroom.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Venue.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Venue.Building.Code, param.SearchPattern())
                    || x.HomeroomPathways.Any(y => EF.Functions.Like(y.GradePathwayDetail.Pathway.Code, param.SearchPattern()))
                    || x.HomeroomTeachers.Any(y => EF.Functions.Like(y.Staff.FirstName, param.SearchPattern())));

            var query = _dbContext.Entity<MsHomeroom>()
                .Include(x => x.HomeroomStudents)
                .Include(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                .Include(X => X.HomeroomPathways).ThenInclude(x => x.GradePathwayDetail).ThenInclude(x => x.Pathway)
                .Include(x => x.Venue)
                .Include(x => x.GradePathway).ThenInclude(x => x.Grade)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .SearchByIds(param)
                .Where(predicate);

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "classroom" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.GradePathwayClassroom.Classroom.Code).ThenBy(x => x.GradePathway.Grade.Code).ThenBy(x => x.GradePathway.Grade.Code.Length)
                        : query.OrderByDescending(x => x.GradePathwayClassroom.Classroom.Code).ThenBy(x => x.GradePathway.Grade.Code).ThenByDescending(x => x.Grade.Code.Length),
                    "streaming" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.HomeroomPathways.First().GradePathwayDetail.Pathway.Code)
                        : query.OrderByDescending(x => x.HomeroomPathways.First().GradePathwayDetail.Pathway.Code),
                    "teacher" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.HomeroomTeachers.First().Staff.FirstName)
                        : query.OrderByDescending(x => x.HomeroomTeachers.First().Staff.FirstName),
                    "grade.code" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Grade.Code.Length).ThenBy(x => x.Grade.Code)
                        : query.OrderByDescending(x => x.Grade.Code.Length).ThenByDescending(x => x.Grade.Code),
                    _ => query.OrderByDynamic(param, _aliasColumns)
                };
            }

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
            .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                || (x.StartDate < _dateTime.ServerTime.Date
                    ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                    : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
            .ToListAsync();

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .Select(x => new GetLOVHomeroomResult
                    {
                        Id = x.Id,
                        Code = x.GradePathwayClassroom.Classroom.Code,
                        Description = string.Format("{0}{1}", x.Grade.Code, x.GradePathwayClassroom.Classroom.Description),
                        Semesester = x.Semester
                    })
                    .ToListAsync(CancellationToken);

                items = result;
            }
            else
            {
                var queryResults = await query
                    .Include(x => x.HomeroomPathways).ThenInclude(x => x.GradePathwayDetail).ThenInclude(x => x.Pathway)
                    .Include(x => x.Venue).ThenInclude(x => x.Building)
                    .Include(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);


                var results = queryResults
                    .Select(x => new GetHomeroomResult
                    {
                        Id = x.Id,
                        Code = x.GradePathwayClassroom.Classroom.Code,
                        Description = x.GradePathwayClassroom.Classroom.Description,
                        Grade = new CodeWithIdVm(x.IdGrade, x.Grade.Code, x.Grade.Description),
                        Semester = x.Semester,
                        Pathway = string.Join(", ", x.HomeroomPathways.Select(y => y.GradePathwayDetail.Pathway.Code)),
                        Pathways = x.HomeroomPathways.Select(y => new CodeWithIdVm(y.Id,
                            y.GradePathwayDetail.Pathway.Code, y.GradePathwayDetail.Pathway.Description)),
                        Venue = x.Venue?.Code,
                        Building = x.Venue?.Building.Code,
                        TeacherName = string.Join(", ", x.HomeroomTeachers.OrderBy(y => y.DateIn).Select(y => string.IsNullOrEmpty(y.Staff.FirstName) ? y.Staff.LastName : y.Staff.FirstName)),
                        TotalGender = new HomeroomStudentGender
                        {
                            Male = x.HomeroomStudents.Count != 0 ? x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Gender == Gender.Male) : 0,
                            Female = x.HomeroomStudents.Count != 0 ? x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Gender == Gender.Female) : 0,
                            Other = x.HomeroomStudents.Count != 0 ? x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Gender != Gender.Male && y.Gender != Gender.Female) : 0
                        },
                        TotalReligion = new HomeroomStudentReligion
                        {
                            Islam = x.HomeroomStudents.Where(x=> !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Religion == Religion.Islam.GetDescription()),
                            Protestan = x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Religion == Religion.Protestan.GetDescription()),
                            Katolik = x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Religion == Religion.Katolik.GetDescription()),
                            Hindu = x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Religion == Religion.Hindu.GetDescription()),
                            //Buddha = x.HomeroomStudents.Count(y => y.Religion == Religion.Buddha.GetDescription()),
                            Buddha = x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Religion.ToLower().Contains("bud")),
                            //Khonghucu = x.HomeroomStudents.Count(y => y.Religion == Religion.Khonghucu.GetDescription()),
                            Khonghucu = x.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).Count(y => y.Religion.ToLower().Contains("kong")),
                        }
                    })
                    .ToList();

                items = results;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
