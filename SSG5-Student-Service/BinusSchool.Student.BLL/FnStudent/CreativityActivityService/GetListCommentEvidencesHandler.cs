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
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetListCommentEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListCommentEvidencesHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListCommentEvidencesRequest>();

            var predicate = PredicateBuilder.Create<TrEvidencesComment>(x => x.IdEvidences == param.IdEvidences);

            var query = _dbContext.Entity<TrEvidencesComment>()
              .Include(x => x.UserComment)
              .Where(predicate)
              .Select(x => new
              {
                  Id = x.Id,
                  IdEvidences = x.IdEvidences,
                  IdUserComment = x.IdUserComment,
                  DisplayName = x.UserComment.DisplayName,
                  DateIn = x.DateUp == null ? x.DateIn : x.DateUp,
                  Comment = x.Comment,
                  CanEdit = x.UserIn == param.IdUser,
                  CanDelete = x.UserIn == param.IdUser
              })
              .OrderByDescending(x => x.DateIn);

            IReadOnlyList<GetListCommentEvidencesResult> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetListCommentEvidencesResult
                {
                    Id = x.Id,
                    IdEvidences = x.IdEvidences,
                    IdUserComment = x.IdUserComment,
                    DisplayName = x.DisplayName,
                    DateIn = x.DateIn,
                    Comment = x.Comment,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetListCommentEvidencesResult
                {
                    Id = x.Id,
                    IdEvidences = x.IdEvidences,
                    IdUserComment = x.IdUserComment,
                    DisplayName = x.DisplayName,
                    DateIn = x.DateIn,
                    Comment = x.Comment,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
