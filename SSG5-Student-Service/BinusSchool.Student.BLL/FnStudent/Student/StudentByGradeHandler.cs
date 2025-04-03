using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class StudentByGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public StudentByGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentByGradeRequest>(nameof(GetStudentByGradeRequest.IdGrade));
            var includePathway = param.IncludePathway.HasValue && param.IncludePathway.Value;

            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => x.IdGrade == param.IdGrade);
            if (param.Ids?.Any() ?? false)
                predicate = predicate.And(x => param.Ids.Contains(x.IdStudent));
            if (param.ExceptIds?.Any() ?? false)
                predicate = predicate.And(x => !param.ExceptIds.Contains(x.IdStudent));
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern()
                                        ));

            var query = _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                .Include(x => x.Grade)
                .Include(x => x.StudentGradePathways)
                    .ThenInclude(x => x.Pathway)
                .Where(predicate)
                .IgnoreQueryFilters();

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = NameUtil.GenerateFullNameWithId(x.Student.Id, x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                    })
                    .ToListAsync(CancellationToken);
            }
            else
            {
                var queryResults = await query
                    .SetPagination(param)
                    .Select(x => new 
                    {
                        x.Id,
                        x.IdStudent,
                        x.IsActive,
                        Student = new 
                        {
                            x.Student.FirstName,
                            x.Student.MiddleName,
                            x.Student.LastName,
                            x.Student.Gender
                        },
                        Grade = new { x.Grade.Description },
                        StudentGradePathways = x.StudentGradePathways
                            .Select(y => new
                            {
                                y.IdPathway,
                                y.IsActive,
                                y.DateIn,
                                y.DateUp,
                                Pathway = new { y.Pathway.Code, y.Pathway.Description }
                            })
                            .ToArray()
                    })
                    .ToListAsync(CancellationToken);
                // get student grade pathways based on student grade
                var studentGradePathways = default(List<MsStudentGradePathway>);
                if (includePathway)
                {
                    studentGradePathways = await _dbContext.Entity<MsStudentGradePathway>()
                        .Where(x => queryResults.Select(y => y.Id).Contains(x.IdStudentGrade))
                        .IgnoreQueryFilters()
                        .ToListAsync(CancellationToken);
                }

                var results = queryResults
                    .Select(x => new GetStudentByGradeResult
                    {
                        Id = x.Id,
                        StudentId = x.IdStudent,
                        Grade = x.Grade.Description,
                        FullName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                        Gender = x.Student.Gender,
                        IsActive = x.IsActive,
                        LastPathway = x.StudentGradePathways.Length != 0 && includePathway ? x.StudentGradePathways
                         .Where(x => !x.IsActive)
                        .OrderByDescending(x => x.DateIn).ThenByDescending(x => x.DateUp)
                        .Select(x => new CodeWithIdVm
                        {
                            Id = x.IdPathway,
                            Code = x.Pathway.Code,
                            Description = x.Pathway.Description
                        }).FirstOrDefault()
                        : null,
                        CurrentPathway = x.StudentGradePathways.Length != 0 && includePathway ? x.StudentGradePathways
                        .Where(x => x.IsActive)
                        .OrderByDescending(x => x.DateIn).ThenByDescending(x => x.DateUp)
                        .Select(x => new CodeWithIdVm
                        {
                            Id = x.IdPathway,
                            Code = x.Pathway.Code,
                            Description = x.Pathway.Description
                        }).FirstOrDefault()
                        : null
                    })
                    .ToList();
                items = results;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
