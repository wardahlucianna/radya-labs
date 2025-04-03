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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance
{
    public class GetSubmittedStudentsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetSubmittedStudentsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetSubmittedStudentsRequest>(nameof(GetSubmittedStudentsRequest.Date),
                                                                            nameof(GetSubmittedStudentsRequest.IdAcademicYear));

            var columns = new[] { "iduser", "name", "level", "homeroom" };

            var predicate = PredicateBuilder.True<TrStudentSafeReport>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    || EF.Functions.Like(x.MsStudent.FirstName, param.SearchPattern())
                    || EF.Functions.Like(x.MsStudent.MiddleName, $"%{param.Search}%")
                    || EF.Functions.Like(x.MsStudent.LastName, $"%{param.Search}%"));

            predicate = predicate.And(x => x.MsStudent.MsHomeroomStudents.Any(y => y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear));

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.MsStudent.MsHomeroomStudents.Any(y => y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.IdLevel == param.IdLevel));

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.MsStudent.MsHomeroomStudents.Any(y => y.Homeroom.MsGradePathwayClassroom.GradePathway.IdGrade == param.IdGrade));

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.MsStudent.MsHomeroomStudents.Any(y => y.IdHomeroom == param.IdHomeroom));

            var query = _dbContext.Entity<TrStudentSafeReport>()
                                  .Include(x => x.MsStudent).ThenInclude(x => x.MsHomeroomStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                                  .Include(x => x.MsStudent).ThenInclude(x => x.MsHomeroomStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                                  .SearchByIds(param)
                                  .Where(predicate)
                                  .Select(x => new SubmittedStudentResult
                                  {
                                      IdEmergencyAttendance = x.Id,
                                      Student = new ItemValueVm
                                      {
                                          Id = x.IdStudent,
                                          Description = NameUtil.GenerateFullName(x.MsStudent.FirstName, x.MsStudent.MiddleName, x.MsStudent.LastName)
                                      },
                                      Level = new CodeWithIdVm
                                      {
                                          Id = x.MsStudent.MsHomeroomStudents.First().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id,
                                          Code = x.MsStudent.MsHomeroomStudents.First().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code,
                                          Description = x.MsStudent.MsHomeroomStudents.First().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description,
                                      },
                                      Homeroom = new ItemValueVm
                                      {
                                          Id = x.MsStudent.MsHomeroomStudents.First().IdHomeroom,
                                          Description = $"{x.MsStudent.MsHomeroomStudents.First().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code}{x.MsStudent.MsHomeroomStudents.First().Homeroom.MsGradePathwayClassroom.Classroom.Code}"
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

            var count = await query.CountAsync();

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                return Request.CreateApiResult2(await query
                    .Select(x => x.Student)
                    .ToListAsync() as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
            else
            {
                var result = param.GetAll.HasValue && param.GetAll.Value
                ? await query.ToListAsync()
                : await query.Skip(param.CalculateOffset()).Take(param.Size).ToListAsync();

                return Request.CreateApiResult2(result as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }
    }
}
