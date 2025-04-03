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
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentCopyByGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentCopyByGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentCopyByGradeRequest, GetStudentCopyByGradeValidator>();

            var currentGrade = await _dbContext.Entity<MsGrade>()
                .Where(x => x.Id == param.IdGrade).FirstOrDefaultAsync(CancellationToken);
            if (currentGrade == null)
                throw new NotFoundException("Current grade not found");

            var nextGrade = await _dbContext.Entity<MsGrade>()
                .Include(x => x.MsLevel)
                .Where(x => x.MsLevel.IdAcademicYear == param.IdAcademicYearTarget
                && x.OrderNumber == currentGrade.OrderNumber + 1).FirstOrDefaultAsync();

            if (nextGrade == null)
                throw new NotFoundException("Target grade not found");

            var predicate = PredicateBuilder.Create<MsStudent>(x => x.StudentGrades.Any(y => y.IdGrade == param.IdGrade)
            && !x.StudentGrades.Any(y => y.IdGrade == nextGrade.Id));

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
                            .ThenInclude(x => x.MsAcademicYear)
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
                    .Select(x => new GetStudentCopyByGradeResult
                    {
                        Id = x.Id,
                        Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                        Gender = x.Gender,
                        Grade = new ItemValueVm
                        {
                            Id = x.StudentGrades.FirstOrDefault(x => x.IdGrade == param.IdGrade).IdGrade,
                            Description = x.StudentGrades.FirstOrDefault(x => x.IdGrade == param.IdGrade).Grade.Description
                        },
                        Level = new ItemValueVm
                        {
                            Id = x.StudentGrades.FirstOrDefault(x => x.IdGrade == param.IdGrade).Grade.IdLevel,
                            Description = x.StudentGrades.FirstOrDefault(x => x.IdGrade == param.IdGrade).Grade.MsLevel.Description
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
