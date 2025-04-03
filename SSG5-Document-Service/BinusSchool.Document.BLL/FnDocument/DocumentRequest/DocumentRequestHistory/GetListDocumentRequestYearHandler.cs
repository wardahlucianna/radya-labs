using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class GetListDocumentRequestYearHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "Year" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetListDocumentRequestYearHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListDocumentRequestYearRequest>(
                            nameof(GetListDocumentRequestYearRequest.IdSchool));

            var predicate = PredicateBuilder.True<MsPeriod>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.StartDate.Year.ToString(), param.SearchPattern())
                    || EF.Functions.Like(x.EndDate.Year.ToString(), param.SearchPattern())
                    );

            var gradeList = new List<string>();

            if (!string.IsNullOrEmpty(param.IdStudent))
            {
                var getGradeList = await _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(x => x.Homeroom)
                                    .Where(x => x.IdStudent == param.IdStudent)
                                    .Select(x => x.Homeroom.IdGrade)
                                    .Distinct()
                                    .ToListAsync(CancellationToken);

                gradeList.AddRange(getGradeList);
            }
            else if (!string.IsNullOrEmpty(param.IdParent))
            {
                var listIdStudent = new List<string>();
                #region Get all children if idstudent is null
                var username = await _dbContext.Entity<MsUser>()
                               .Where(x => x.Id == param.IdParent)
                               .Select(x => x.Username)
                               .FirstOrDefaultAsync(CancellationToken);

                if (username == null)
                    throw new BadRequestException("User is not found");

                var idStudent = string.Concat(username.Where(char.IsDigit));

                var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                        .Where(x => x.IdStudent == idStudent)
                                        .Select(x => new
                                        {
                                            idParent = x.IdParent
                                        }).FirstOrDefaultAsync(CancellationToken);

                var siblingGroup = await _dbContext.Entity<MsSiblingGroup>()
                                    .Where(x => x.IdStudent == idStudent)
                                    .Select(x => x.Id)
                                    .FirstOrDefaultAsync(CancellationToken);

                if (siblingGroup != null)
                {
                    var siblingStudent = await _dbContext.Entity<MsSiblingGroup>()
                                            .Where(x => x.Id == siblingGroup)
                                            .Select(x => x.IdStudent)
                                            .ToListAsync(CancellationToken);

                    listIdStudent = await _dbContext.Entity<MsStudent>()
                                    .Where(x => siblingStudent.Any(y => y == x.Id))
                                    .Select(x => x.Id)
                                    .ToListAsync(CancellationToken);
                }
                else if (dataStudentParent != null)
                {
                    listIdStudent = await _dbContext.Entity<MsStudentParent>()
                                    .Include(x => x.Student)
                                    .Where(x => x.IdParent == dataStudentParent.idParent)
                                    .Select(x => x.Student.Id)
                                    .ToListAsync(CancellationToken);
                }
                #endregion

                var getGradeList = await _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(x => x.Homeroom)
                                    .Where(x => listIdStudent.Any(y => y == x.IdStudent))
                                    .Select(x => x.Homeroom.IdGrade)
                                    .Distinct()
                                    .ToListAsync(CancellationToken);

                gradeList.AddRange(getGradeList);
            }
            else
            {
                var getAllGradeList = await _dbContext.Entity<MsPeriod>()
                                    .Include(x => x.Grade)
                                        .ThenInclude(x => x.Level)
                                        .ThenInclude(x => x.AcademicYear)
                                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                                    .Select(x => x.IdGrade)
                                    .Distinct()
                                    .ToListAsync(CancellationToken);

                gradeList.AddRange(getAllGradeList);
            }

            var query = _dbContext.Entity<MsPeriod>()
                            .Where(x => gradeList.Any(y => y == x.IdGrade))
                            .Where(predicate)
                            .ToList()
                            .SelectMany(x => new[] { x.StartDate.Year, x.EndDate.Year })
                            .Distinct()
                            .AsQueryable();

            query = param.OrderBy switch
            {
                "Year" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x)
                    : query.OrderByDescending(x => x),
                _ => query.OrderBy(x => x)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.ToString(),
                        Description = x.ToString()
                    })
                    .ToList();
            else
                items = query
                    //.SetPagination(param)
                    .Select(x => new ItemValueVm
                    {
                        Id = x.ToString(),
                        Description = x.ToString()
                    })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
