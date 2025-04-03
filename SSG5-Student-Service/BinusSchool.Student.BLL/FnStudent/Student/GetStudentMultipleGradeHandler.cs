using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentMultipleGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentMultipleGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentMultipleGradeRequest>(nameof(GetStudentMultipleGradeRequest.IdGrades), nameof(GetStudentMultipleGradeRequest.IdAcadYear));
            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => x.Grade.MsLevel.IdAcademicYear == param.IdAcadYear &&
                param.IdGrades.Contains(x.IdGrade));

            var query = _dbContext.Entity<MsStudentGrade>()
                .Where(predicate);

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.Student.FirstName.Contains(param.Search) 
                || x.Student.MiddleName.Contains(param.Search)
                || x.Student.LastName.Contains(param.Search)
                || x.Student.IdBinusian.Contains(param.Search));


            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == Common.Model.Enums.OrderType.Asc
                        ? query.OrderBy(x => x.Grade.MsLevel.MsAcademicYear.Code)
                        : query.OrderByDescending(x => x.Grade.MsLevel.MsAcademicYear.Code);
                    break;
                case "Name":
                    query = param.OrderType == Common.Model.Enums.OrderType.Asc
                        ? query.OrderBy(x => x.Student.FirstName)
                        : query.OrderByDescending(x => x.Student.FirstName);
                    break;
                case "IdBinusian":
                    query = param.OrderType == Common.Model.Enums.OrderType.Asc
                        ? query.OrderBy(x => x.Student.IdBinusian)
                        : query.OrderByDescending(x => x.Student.IdBinusian);
                    break;
                case "Level":
                    query = param.OrderType == Common.Model.Enums.OrderType.Asc
                        ? query.OrderBy(x => x.Grade.MsLevel.OrderNumber)
                        : query.OrderByDescending(x => x.Grade.MsLevel.OrderNumber);
                    break;
                case "Grade":
                    query = param.OrderType == Common.Model.Enums.OrderType.Asc
                        ? query.OrderBy(x => x.Grade.OrderNumber)
                        : query.OrderByDescending(x => x.Grade.OrderNumber);
                    break;
                case "Homeroom":
                    query = param.OrderType == Common.Model.Enums.OrderType.Asc
                        ? query.OrderBy(x => x.Student.MsHomeroomStudents.First(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == x.Grade.MsLevel.IdAcademicYear).Homeroom.MsGradePathwayClassroom.Classroom.Description)
                        : query.OrderByDescending(x => x.Student.MsHomeroomStudents.First(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == x.Grade.MsLevel.IdAcademicYear).Homeroom.MsGradePathwayClassroom.Classroom.Description);
                    break;
                default:
                    query = query.OrderByDynamic(param);
                    break;
            }

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == Common.Model.Enums.CollectionType.Lov)
            {
                items = await query.Select(x => new GetStudentMultipleGradeResult
                {
                    Id = x.IdStudent,
                    Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                    IdBinusian = x.Student.IdBinusian,
                    Level = new CodeWithIdVm
                    {
                        Id = x.Grade.IdLevel,
                        Code = x.Grade.MsLevel.Code,
                        Description = x.Grade.MsLevel.Description
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = x.IdGrade,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    },
                    Homeroom = new CodeWithIdVm
                    {
                        Id = x.Student.MsHomeroomStudents.First(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == x.Grade.MsLevel.IdAcademicYear).IdHomeroom,
                        Code = x.Grade.Code,
                        Description = x.Student.MsHomeroomStudents.First(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == x.Grade.MsLevel.IdAcademicYear).Homeroom.MsGradePathwayClassroom.Classroom.Description
                    },
                    StudentPhoto = new CodeWithIdVm
                    {
                        Id = x.Student.StudentPhotos.OrderByDescending(x => x.DateIn).FirstOrDefault().Id,
                        Code = x.Student.StudentPhotos.OrderByDescending(x => x.DateIn).FirstOrDefault().FileName,
                        Description = x.Student.StudentPhotos.OrderByDescending(x => x.DateIn).FirstOrDefault().FilePath
                    }
                }).ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStudentMultipleGradeResult
                    {
                        Id = x.IdStudent,
                        Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                        IdBinusian = x.Student.IdBinusian,
                        Level = new CodeWithIdVm
                        {
                            Id = x.Grade.IdLevel,
                            Code = x.Grade.MsLevel.Code,
                            Description = x.Grade.MsLevel.Description
                        },
                        Grade = new CodeWithIdVm
                        {
                            Id = x.IdGrade,
                            Code = x.Grade.Code,
                            Description = x.Grade.Description
                        },
                        Homeroom = new CodeWithIdVm
                        {
                            Id = x.Student.MsHomeroomStudents.First(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == x.Grade.MsLevel.IdAcademicYear).IdHomeroom,
                            Code = x.Grade.Code,
                            Description = x.Student.MsHomeroomStudents.First(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == x.Grade.MsLevel.IdAcademicYear).Homeroom.MsGradePathwayClassroom.Classroom.Description
                        },
                        StudentPhoto = new CodeWithIdVm
                        {
                            Id = x.Student.StudentPhotos.OrderByDescending(x => x.DateIn).FirstOrDefault().Id,
                            Code = x.Student.StudentPhotos.OrderByDescending(x => x.DateIn).FirstOrDefault().FileName,
                            Description = x.Student.StudentPhotos.OrderByDescending(x => x.DateIn).FirstOrDefault().FilePath
                        }
                    }).ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
