using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetAllStudentWithStatusAndHomeroomHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "IdStudent", "StudentName" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};


        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetAllStudentWithStatusAndHomeroomHandler(
            IStudentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllStudentWithStatusAndHomeroomRequest>(
                            nameof(GetAllStudentWithStatusAndHomeroomRequest.IdSchool));

            // get Active AY
            var getActiveAYSemester = _dbContext.Entity<MsPeriod>()
                                    .Include(x => x.Grade)
                                    .Include(x => x.Grade.MsLevel)
                                    .Include(x => x.Grade.MsLevel.MsAcademicYear)
                                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                    .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                                    .OrderByDescending(x => x.StartDate)
                                    .Select(x => new
                                    {
                                        IdAcademicYear = x.Grade.MsLevel.MsAcademicYear.Id,
                                        AcademicYearDescription = x.Grade.MsLevel.MsAcademicYear.Description,
                                        Semester = x.Semester
                                    })
                                    .FirstOrDefault();

            var predicate = PredicateBuilder.True<MsStudent>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    || EF.Functions.Like((string.IsNullOrWhiteSpace(x.FirstName) ? "" : x.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.MiddleName) ? "" : x.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.LastName) ? "" : x.LastName.Trim()), param.SearchPattern()
                                        )
                    );

            var query = _dbContext.Entity<MsStudent>()
                            .Include(x => x.StudentStatus)
                            .Include(x => x.TrStudentStatuss)
                                .ThenInclude(x => x.StudentStatus)
                            .Include(x => x.MsHomeroomStudents)
                                .ThenInclude(x => x.Homeroom)
                                .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.MsLevel)
                            .Include(x => x.MsHomeroomStudents)
                                .ThenInclude(x => x.Homeroom)
                                .ThenInclude(x => x.MsGradePathwayClassroom)
                                .ThenInclude(x => x.Classroom)
                            .Where(x => x.IdSchool == param.IdSchool)
                            .Where(predicate);

            query = param.OrderBy switch
            {
                "IdStudent" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id),
                "StudentName" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.FirstName).ThenBy(x => x.MiddleName).ThenBy(x => x.LastName)
                    : query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.MiddleName).ThenByDescending(x => x.LastName),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName)
                    })
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetAllStudentWithStatusAndHomeroomResult
                    {
                        Student = new NameValueVm
                        {
                            Id = x.Id,
                            Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                        },
                        Homeroom = x.MsHomeroomStudents
                                    .Where(x =>
                                        x.Homeroom.Grade.MsLevel.IdAcademicYear == getActiveAYSemester.IdAcademicYear &&
                                        x.Homeroom.Semester == getActiveAYSemester.Semester)
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.IdHomeroom,
                                        Description = x.Homeroom.Grade.Description + x.Homeroom.MsGradePathwayClassroom.Classroom.Description
                                    })
                                    .FirstOrDefault(),
                        LatestStudentStatus = x.TrStudentStatuss
                                        .Where(y => y.CurrentStatus == "A")
                                        .OrderByDescending(y => y.StartDate)
                                        .Any() ?
                                        x.TrStudentStatuss
                                        .Where(y => y.CurrentStatus == "A")
                                        .OrderByDescending(y => y.StartDate)
                                        .Select(y => new GetAllStudentWithStatusAndHomeroomResult_StudentStatus
                                        {
                                            Id = y.IdStudentStatus.ToString(),
                                            Description = y.StudentStatus.LongDesc,
                                            StartDate = y.StartDate
                                        })
                                        .FirstOrDefault() :
                                        new GetAllStudentWithStatusAndHomeroomResult_StudentStatus
                                        {
                                            Id = x.IdStudentStatus.ToString(),
                                            Description = x.StudentStatus.LongDesc,
                                            StartDate = null
                                        },
                    })
                    .ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
