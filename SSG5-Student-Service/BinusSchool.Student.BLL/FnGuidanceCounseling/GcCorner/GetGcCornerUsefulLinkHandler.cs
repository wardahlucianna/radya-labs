using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerUsefulLinkHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetGcCornerUsefulLinkHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGcCornerUsefulLinkRequest>();

            string[] _columns = { "AcademicYear", "Grade", "Description", "Link" };

            var aliasColumns = new Dictionary<string, string>
            {
                { _columns[0], "AcademicYear.Description" },
                { _columns[1], "Grade.Description" },
                { _columns[2], "LinkDescription" },
                { _columns[3], "Link" }
            };

            var predicate = PredicateBuilder.Create<MsUsefulLinkGrade>(x=> true);

            if (!string.IsNullOrEmpty(param.IdRoleGroup))
            {
                if (param.IdRoleGroup == "STD")
                {
                    var grade = _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(x => x.Homeroom)
                                    .Where(x => x.IdStudent == param.IdUser)
                                    .Select(x => x.Homeroom.IdGrade).ToList();

                    predicate = PredicateBuilder.Create<MsUsefulLinkGrade>(x => grade.Contains(x.IdGrade));
                }
                else if (param.IdRoleGroup == "PRT")
                {
                    var idStudent = string.Concat(param.IdUser.Where(char.IsDigit));
                    var siblingGroupId = await _dbContext.Entity<MsSiblingGroup>()
                        .Where(x => x.IdStudent == idStudent)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(CancellationToken);

                    var listSibling = await _dbContext.Entity<MsSiblingGroup>()
                            .Where(x => x.Id == siblingGroupId)
                            .Select(x => x.IdStudent)
                            .ToListAsync(CancellationToken);

                    var listGrade = _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                    .Where(x => listSibling.Contains(x.IdStudent))
                    .Select(x => x.Homeroom.IdGrade).Distinct().ToList();

                    predicate = PredicateBuilder.Create<MsUsefulLinkGrade>(x => listGrade.Contains(x.IdGrade));
                }
            }

            var query = _dbContext.Entity<MsUsefulLinkGrade>()
                .Include(x => x.UsefulLink)
                .OrderByDescending(x => x.DateIn)
                .OrderByDynamic(param, aliasColumns)
                .Where(predicate);
            //.Select(x => new GetGcCornerWellBeingResult
            //{
            //    Id = x.Id,
            //    Link = x.Link,
            //    Description = x.Description
            //});

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.UsefulLink.Link, param.SearchPattern()));
            }
            else if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => x.UsefulLink.IdAcademicYear == param.IdAcademicYear);
            }

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetGcCornerWellBeingResult
                {
                    Id = x.Id,
                    Link = x.UsefulLink.Link,
                    Description = x.UsefulLink.Description
                })
                .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetGcCornerWellBeingResult
                {
                    Id = x.Id,
                    Link = x.UsefulLink.Link,
                    Description = x.UsefulLink.Description
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
