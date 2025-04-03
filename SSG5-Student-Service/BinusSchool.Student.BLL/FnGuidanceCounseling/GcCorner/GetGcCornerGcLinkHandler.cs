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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerGcLinkHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetGcCornerGcLinkHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGcCornerGcLinkRequest>();

            string[] _columns = { "LinkDescription", "Link" };

            var aliasColumns = new Dictionary<string, string>
            {
                { _columns[0], "Description" },
                { _columns[1], "Link" }
            };

            var query = _dbContext.Entity<MsGcLink>()
                       .Include(e => e.AcademicYear)
                       .Include(e => e.GcLinkGrades).ThenInclude(e => e.Grade)
                       .Include(e => e.GcLinkLogo)
                       .OrderByDescending(x => x.DateIn)
                       .OrderByDynamic(param, aliasColumns)
                       .Select(x => new GetGcCornerGcLinkResult
                       {
                           Id = x.Id,
                           Link = x.Link,
                           Description = x.LinkDescription,
                           Logo = x.GcLinkLogo.Select(e => new LogoGcCornerGcLink
                           {
                               Id = e.Id,
                               Url = e.Url,
                               OriginalFilename = e.OriginalName,
                               FileName = e.FileName,
                               FileSize = e.FileSize,
                               FileType = e.FileType,
                           }).ToList()

                       });

            //filter
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => EF.Functions.Like(x.Description, param.SearchPattern()));

            ////ordering
            //switch (param.OrderBy)
            //{
            //    case "Link":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.Link)
            //            : query.OrderBy(x => x.Link);
            //        break;
            //};

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetGcCornerGcLinkResult
                    {
                        Id = x.Id,
                        Description = x.Description,
                        Link = x.Link,
                        Logo = x.Logo
                    })
                    .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetGcCornerGcLinkResult
                    {
                        Id = x.Id,
                        Description = x.Description,
                        Link = x.Link,
                        Logo = x.Logo
                    }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
