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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup
{
    public class GetMasterGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetMasterGroupHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetMasterGroupRequest>();

            var predicate = PredicateBuilder.True<MsExtracurricularGroup>();
            if (!string.IsNullOrWhiteSpace(param.Search) && param.Return == CollectionType.Lov)
                predicate = predicate.And(x
                    =>  EF.Functions.Like(x.Name, param.SearchPattern()));
            if (!string.IsNullOrWhiteSpace(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);

            var query = _dbContext.Entity<MsExtracurricularGroup>()
                               .Include(x => x.School)
                               .SearchByDynamic(param)
                               .Where(predicate)
                               .Select(x => new GetMasterGroupResult
                               {
                                   Group = new NameValueVm
                                   {
                                       Id = x.Id,
                                       Name = x.Name
                                   },
                                   School = new ItemValueVm
                                   {
                                       Id = x.School.Id,
                                       Description = x.School.Description
                                   },
                                   Description = x.Description,
                                   Status = x.Status
                               });
                               //.Distinct()
                               //.OrderByDynamic(param);

            // Certain Status
            if (param.Status != null)
            {
                query = (IOrderedQueryable<GetMasterGroupResult>)query.Where(x => x.Status == param.Status);
            }

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
            {
                items = await query
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Group.Id,
                                Description = x.Group.Name
                            })
                            .OrderByDynamic(param)
                            .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                             .SetPagination(param)
                             .Select(x => new GetMasterGroupResult
                             {
                                 Group = new NameValueVm
                                 {
                                     Id = x.Group.Id,
                                     Name = x.Group.Name
                                 },
                                 School = new ItemValueVm
                                 {
                                     Id = x.School.Id,
                                     Description = x.School.Description
                                 },
                                 Description = x.Description,
                                 Status = x.Status
                             })
                             .OrderByDynamic(param)
                             .ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                            ? items.Count
                            : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
