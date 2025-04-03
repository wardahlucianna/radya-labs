using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance
{
    public class GetUnsubmittedStudentsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetUnsubmittedStudentsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetUnsubmittedStudentsRequest>(nameof(GetUnsubmittedStudentsRequest.Date),
                                                                              nameof(GetUnsubmittedStudentsRequest.IdAcademicYear));

            var columns = new[] { "iduser", "name", "level", "homeroom" };

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdStudent, param.SearchPattern())
                    //|| EF.Functions.Like(x.FirstName, param.SearchPattern())
                    //|| EF.Functions.Like(x.MiddleName, $"%{param.Search}%")
                    //|| EF.Functions.Like(x.LastName, $"%{param.Search}%")
                    || EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern()
                                        )
                    );

            predicate = predicate.And(x => !x.Student.TrStudentSafeReports.Any(y => y.Date.Date == param.Date.Date)
                                           && x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                           );

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);

            var query = _dbContext.Entity<MsHomeroomStudent>()
                                  .Include(x => x.Student).ThenInclude(e => e.TrStudentSafeReports)
                                  .Include(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                                  .Include(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                                  .SearchByIds(param)
                                  .Where(predicate)
                                  .GroupBy(e => new
                                  {
                                      e.IdStudent,
                                      e.Student.FirstName,
                                      e.Student.LastName,
                                      LevelId = e.Homeroom.Grade.MsLevel.Id,
                                      LevelCode = e.Homeroom.Grade.MsLevel.Code,
                                      LevelDesc = e.Homeroom.Grade.MsLevel.Description,
                                      GradeCode = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code,
                                      ClassroomCode = e.Homeroom.MsGradePathwayClassroom.Classroom.Code,

                                  })
                                  .Select(x => new UnsubmittedStudentResult
                                  {
                                      Student = new ItemValueVm
                                      {
                                          Id = x.Key.IdStudent,
                                          Description = $"{x.Key.FirstName} {x.Key.LastName}"
                                      },
                                      Level = new CodeWithIdVm
                                      {
                                          Id = x.Key.LevelId,
                                          Code = x.Key.LevelCode,
                                          Description = x.Key.LevelDesc,
                                      },
                                      Homeroom = new ItemValueVm
                                      {
                                          Description = $"{x.Key.GradeCode}{x.Key.ClassroomCode}"
                                      }
                                  });


            query = param.OrderBy switch
            {
                "iduser" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Student.Id)
                    : query.OrderByDescending(x => x.Student.Id),
                "name" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Student.Description)
                    : query.OrderByDescending(x => x.Student.Description),
                "level" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Level.Description)
                    : query.OrderByDescending(x => x.Level.Description),
                "homeroom" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Homeroom.Description)
                    : query.OrderByDescending(x => x.Homeroom.Description),
                _ => query
            };

            var count = query.ToList().Count();

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                return Request.CreateApiResult2(await query
                    .Select(x => x.Student)
                    .ToListAsync(CancellationToken) as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
            else
            {
                var result = param.GetAll.HasValue && param.GetAll.Value
                ? await query.ToListAsync(CancellationToken)
                : await query.Skip(param.CalculateOffset()).Take(param.Size).ToListAsync(CancellationToken);

                return Request.CreateApiResult2(result as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }
    }
}
