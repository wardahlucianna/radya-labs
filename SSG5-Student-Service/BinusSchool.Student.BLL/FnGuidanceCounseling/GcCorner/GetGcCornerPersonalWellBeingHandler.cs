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
    public class GetGcCornerPersonalWellBeingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetGcCornerPersonalWellBeingHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGcCornerWellBeingRequest>();

            string[] _columns =  { "ArticleName", "ArticleDescription", "Link" };

            var aliasColumns = new Dictionary<string, string>
            {
                { _columns[0], "ArticleName" },
                { _columns[1], "Description" },
                { _columns[2], "Link" }
            };

            var query = _dbContext.Entity<TrPersonalWellBeing>()
                       .Include(e => e.PersonalWellBeingAttachment)
                       .OrderByDescending(x => x.DateIn)
                       .OrderByDynamic(param, aliasColumns)
                       .Select(x => new GetGcCornerWellBeingResult
                       {
                           Id = x.Id,
                           ArticleName = x.ArticleName,
                           Description = x.Description,
                           Link = x.Link,
                           Attachments = x.PersonalWellBeingAttachment.Select(e => new AttachmentGcCornerPersonalWellBeing
                           {
                               Id = e.Id,
                               Url = e.Url,
                               OriginalFilename = e.OriginalName,
                               FileName = e.FileName,
                               FileSize = e.FileSize,
                               FileType = e.FileType,
                           }).ToList()
                       });

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.ArticleName, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            ////ordering
            //switch (param.OrderBy)
            //{
            //    case "ArticleName":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.ArticleName)
            //            : query.OrderBy(x => x.ArticleName);
            //        break;
            //    case "ArticleDescription":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.Description)
            //            : query.OrderBy(x => x.Description);
            //        break;
            //    case "Link":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.Link)
            //            : query.OrderBy(x => x.Link);
            //        break;
            //};

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetGcCornerWellBeingResult
                {
                    Id = x.Id,
                    ArticleName = x.ArticleName,
                    Description = x.Description,
                    Link = x.Link,
                    Attachments = x.Attachments,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetGcCornerWellBeingResult
                {
                    Id = x.Id,
                    ArticleName = x.ArticleName,
                    Description = x.Description,
                    Link = x.Link,
                    Attachments = x.Attachments,

                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
