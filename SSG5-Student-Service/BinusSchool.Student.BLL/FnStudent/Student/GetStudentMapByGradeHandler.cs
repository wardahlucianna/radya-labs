using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
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
    public class GetStudentMapByGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentMapByGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var columns = new[] { string.Empty, "id", "name", "gender", "level", "grade" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0]   , string.Empty },
                { columns[1]   , "id"},
                { columns[2]   , "name"},
                { columns[3]   , "gender"},
                { columns[4]   , "level"},
                { columns[5]   , "grade"}
            };

            var param = Request.ValidateParams<GetStudentMapByGradeRequest>(new string[] { });

            if (!string.IsNullOrEmpty(param.IdGrade) || !string.IsNullOrEmpty(param.IdLevel))
            {
                var predicate = PredicateBuilder.Create<MsStudentGrade>(x => x.Student.IdSchool == param.IdSchool
                    && x.Grade.MsLevel.IdAcademicYear == param.AcademicYear);

                if (param.Ids != null)
                {
                    predicate = predicate.And(x => param.Ids.Contains(x.IdStudent));
                }
                else
                {
                    if (!string.IsNullOrEmpty(param.Gender))
                        predicate = predicate.And(x => x.Student.Gender == (Gender)Enum.Parse(typeof(Gender), param.Gender));
                    if (!string.IsNullOrEmpty(param.IdGrade))
                        predicate = predicate.And(x => x.IdGrade == param.IdGrade);
                    if (!string.IsNullOrEmpty(param.IdLevel))
                        predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
                }

                if (!string.IsNullOrEmpty(param.Search))
                    predicate = predicate.And(x
                        => EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                    (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                    (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern())
                        || EF.Functions.Like(x.Id, param.SearchPattern()));

                var query = _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.MsLevel)
                    .Include(x => x.Student)
                    .Where(predicate)
                    .AsQueryable();

                query = param.OrderBy switch
                {
                    "id" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Id)
                        : query.OrderByDescending(x => x.Id),
                    "name" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Student.FirstName)
                        : query.OrderByDescending(x => x.Student.FirstName),
                    "gender" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Student.Gender)
                        : query.OrderByDescending(x => x.Student.Gender),
                    "level" => param.OrderType == OrderType.Asc
                            ? query.OrderBy(x => x.Grade.MsLevel.OrderNumber)
                            : query.OrderByDescending(x => x.Grade.MsLevel.OrderNumber),
                    "grade" => param.OrderType == OrderType.Asc
                            ? query.OrderBy(x => x.Grade.OrderNumber)
                            : query.OrderByDescending(x => x.Grade.OrderNumber),
                    _ => query.OrderByDynamic(param, aliasColumns)
                };

                IReadOnlyList<IItemValueVm> items;
                if (param.Return == CollectionType.Lov)
                {
                    items = await query
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Student.Id,
                            Description = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                        }).ToListAsync(CancellationToken);
                }
                else
                {
                    items = await query
                        .SetPagination(param)
                        .Select(x => new GetStudentMapByGradeResult
                        {
                            Id = x.Student.Id,
                            Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                            Gender = x.Student.Gender,
                            Grade = new ItemValueVm
                            {
                                Id = x.IdGrade,
                                Description = x.Grade.Description
                            },
                            Level = new ItemValueVm
                            {
                                Id = x.Grade.IdLevel,
                                Description = x.Grade.MsLevel.Description
                            }
                        }).ToListAsync(CancellationToken);
                }

                var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.Select(x => x.Id).CountAsync(CancellationToken);

                return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
            }
            else
            {
                if (string.IsNullOrEmpty(param.AcademicYear))
                    throw new BadRequestException("Id Academic Year is required");

                var idGrades = await _dbContext.Entity<MsGrade>()
                    .Where(x => x.MsLevel.IdAcademicYear == param.AcademicYear).Select(x => x.Id).ToListAsync(CancellationToken);

                if (!idGrades.Any())
                    throw new NotFoundException("Grades not found!");

                var predicate = PredicateBuilder.Create<MsStudent>(x => x.IdSchool == param.IdSchool &&
                (!x.StudentGrades.Any() || (x.StudentGrades.Where(y => idGrades.Contains(y.IdGrade)).Any() == false)));

                if (param.Ids != null)
                    predicate = predicate.And(x => param.Ids.Contains(x.Id));
                if (!string.IsNullOrEmpty(param.Gender))
                    predicate = predicate.And(x => x.Gender == (Gender)Enum.Parse(typeof(Gender), param.Gender));
                if (!string.IsNullOrEmpty(param.Search))
                    predicate = predicate.And(x
                        => EF.Functions.Like((string.IsNullOrWhiteSpace(x.FirstName) ? "" : x.FirstName.Trim() + " ") +
                                    (string.IsNullOrWhiteSpace(x.MiddleName) ? "" : x.MiddleName.Trim() + " ") +
                                    (string.IsNullOrWhiteSpace(x.LastName) ? "" : x.LastName.Trim()), param.SearchPattern())
                        || EF.Functions.Like(x.Id, param.SearchPattern()));

                var query = _dbContext.Entity<MsStudent>()
                    .Include(x => x.StudentGrades)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.MsLevel)
                    .Where(predicate)
                    .AsQueryable();

                query = param.OrderBy switch
                {
                    "id" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Id)
                        : query.OrderByDescending(x => x.Id),
                    "studentname" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.FirstName)
                        : query.OrderByDescending(x => x.FirstName),
                    "gender" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Gender)
                        : query.OrderByDescending(x => x.Gender),
                    _ => query.OrderByDynamic(param)
                };

                IReadOnlyList<IItemValueVm> items;
                if (param.Return == CollectionType.Lov)
                {
                    items = await query
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Id,
                            Description = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName)
                        }).ToListAsync(CancellationToken);
                }
                else
                {
                    items = await query
                        .SetPagination(param)
                        .Select(x => new GetStudentMapByGradeResult
                        {
                            Id = x.Id,
                            Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                            Gender = x.Gender,
                            Grade = new ItemValueVm
                            {
                                Id = null,
                                Description = null
                            },
                            Level = new ItemValueVm
                            {
                                Id = null,
                                Description = null
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
}
