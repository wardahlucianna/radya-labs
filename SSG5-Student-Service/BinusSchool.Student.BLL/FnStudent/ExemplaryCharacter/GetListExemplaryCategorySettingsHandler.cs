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
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class GetListExemplaryCategorySettingsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListExemplaryCategorySettingsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListExemplaryCategorySettingsRequest>(
                            nameof(GetListExemplaryCategorySettingsRequest.IdSchool));

            var predicate = PredicateBuilder.True<LtExemplaryCategory>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.ShortDesc, param.SearchPattern())
                    || EF.Functions.Like(x.LongDesc, param.SearchPattern())
                    );

            var query = _dbContext.Entity<LtExemplaryCategory>()
                            .Where(x => x.IdSchool == param.IdSchool)
                            .Where(predicate)
                            .Select(x => new GetListExemplaryCategorySettingsResult
                            {
                                IdExemplaryCategory = x.Id,
                                ShortDesc = x.ShortDesc,
                                LongDesc = x.LongDesc,
                                CurrentStatus = x.CurrentStatus
                            })
                            .OrderBy(x => x.ShortDesc)
                            .ThenBy(x => x.LongDesc)
                            .ThenBy(x => x.CurrentStatus)
                            .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> resultItems;
            if (param.Return == CollectionType.Lov)
                resultItems = await query
                                    .Select(x => new ItemValueVm()
                                    {
                                        Id = x.IdExemplaryCategory,
                                        Description = x.LongDesc
                                    })
                                    .ToListAsync(CancellationToken);
            else
                resultItems = await query
                                .Select(x => new GetListExemplaryCategorySettingsResult
                                {
                                    IdExemplaryCategory = x.IdExemplaryCategory,
                                    ShortDesc = x.ShortDesc,
                                    LongDesc = x.LongDesc,
                                    CurrentStatus = x.CurrentStatus
                                })
                                //.SetPagination(param)
                                .ToListAsync(CancellationToken);

            //var count = resultItems.Count;

            //return Request.CreateApiResult2(resultItems as object, param.CreatePaginationProperty(count));
            return Request.CreateApiResult2(resultItems as object);
        }
    }
}
