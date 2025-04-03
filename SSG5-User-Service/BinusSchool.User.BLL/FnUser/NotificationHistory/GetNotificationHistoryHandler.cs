using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.User.FnUser.NotificationHistory
{
    public class GetNotificationHistoryHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[]
        {
            "title", "content"
        });

        private readonly IUserDbContext _dbContext;

        public GetNotificationHistoryHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetNotificationHistoryRequest>(nameof(GetNotificationHistoryRequest.UserId));

            var predicate = PredicateBuilder.Create<TrNotification>(x
                => param.IdSchool.Contains(x.IdSchool)
                   && (x.NotificationUsers.Any(y => y.IdUser == param.UserId) || x.IsBlast));

            if (param.Type.HasValue)
                predicate = predicate.And(x => x.NotificationType == param.Type.Value);
            if (param.IsRead.HasValue)
                predicate = predicate.And(x =>
                    x.NotificationUsers == null || (param.IsRead.Value
                        ? x.NotificationUsers.Where(e=>e.IdUser==param.UserId).Select(e=>e.ReadDate).First() != null
                        : x.NotificationUsers.Where(e => e.IdUser == param.UserId).Select(e => e.ReadDate).First() == null));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Title, param.SearchPattern())
                    || EF.Functions.Like(x.Content, param.SearchPattern()));

            var query = _dbContext.Entity<TrNotification>()
                .SearchByIds(param)
                .OrderByDescending(x => x.DateIn)
                .Where(predicate);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Title))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetNotificationHistoryResult
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Content = x.Content,
                        Feature = x.FeatureSchool.Feature.Description,
                        Scenario = x.ScenarioNotificationTemplate,
                        Data = JsonConvert.DeserializeObject<IDictionary<string, string>>(x.Data),
                        ReadDate = x.NotificationUsers.Any(y => y.IdUser == param.UserId && !y.IsDeleteBySystem) ? x.NotificationUsers.First(y => y.IdUser == param.UserId && !y.IsDeleteBySystem).ReadDate : null,
                        Audit = x.GetRawAuditResult2(),
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
