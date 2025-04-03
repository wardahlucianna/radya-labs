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
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetAllStudentEnrollmentNoIdHomeroomHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "IdStudent", "StudentName" };

        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetAllStudentEnrollmentNoIdHomeroomHandler(
            IStudentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllStudentEnrollmentRequest>(
                         nameof(GetAllStudentEnrollmentRequest.IdSchool));

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

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.Student.Id, param.SearchPattern())
                    || EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern()
                                        )
                    || EF.Functions.Like(x.Homeroom.Grade.Code + x.Homeroom.MsGradePathwayClassroom.Classroom.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Homeroom.Grade.Description, param.SearchPattern())
                    );
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);

            var query = _dbContext.Entity<MsHomeroomStudent>()
                          .Include(x => x.Student)
                          .Include(x => x.Homeroom)
                            .ThenInclude(y => y.Grade)
                          .Include(x => x.Homeroom)
                            .ThenInclude(y => y.MsGradePathwayClassroom)
                            .ThenInclude(y => y.Classroom)
                          .Where(x => x.Student.IdSchool == param.IdSchool &&
                          x.Homeroom.Grade.MsLevel.IdAcademicYear == getActiveAYSemester.IdAcademicYear &&
                          x.Semester == getActiveAYSemester.Semester
                          )
                          .Where(predicate);

            query = param.OrderBy switch
            {
                "IdStudent" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.IdStudent)
                    : query.OrderByDescending(x => x.IdStudent),
                "StudentName" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Student.FirstName).ThenBy(x => x.Student.MiddleName).ThenBy(x => x.Student.LastName)
                    : query.OrderByDescending(x => x.Student.FirstName).ThenByDescending(x => x.Student.MiddleName).ThenByDescending(x => x.Student.LastName),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .GroupBy(e => new
                    {
                        e.Id,
                        e.Student.FirstName,
                        e.Student.MiddleName,
                        e.Student.LastName
                    })
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Key.Id,
                        Description = NameUtil.GenerateFullName(x.Key.FirstName, x.Key.MiddleName, x.Key.LastName)
                    })
                    .Distinct()
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .GroupBy(e => new
                    {
                        e.IdStudent,
                        e.Student.FirstName,
                        e.Student.MiddleName,
                        e.Student.LastName,
                        e.Homeroom.IdGrade,
                        grade = e.Homeroom.Grade.Description,
                        gradeCode = e.Homeroom.Grade.Code,
                        classroomCode = e.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    })
                    .Select(x => new GetAllStudentEnrollmentResult
                    {
                        Student = new NameValueVm
                        {
                            Id = x.Key.IdStudent,
                            Name = NameUtil.GenerateFullName(x.Key.FirstName, x.Key.MiddleName, x.Key.LastName),
                        },
                        Homeroom = new ItemValueVm(null, x.Key.gradeCode + x.Key.classroomCode),
                        Grade = new ItemValueVm(x.Key.IdGrade, x.Key.grade)
                    })
                    .Distinct()
                    .ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
