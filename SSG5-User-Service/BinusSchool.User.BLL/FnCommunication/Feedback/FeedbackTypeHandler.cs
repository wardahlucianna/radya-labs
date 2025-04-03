using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnCommunication.Feedback;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
namespace BinusSchool.User.FnCommunication.Feedback
{
    public class FeedbackTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public FeedbackTypeHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetFeedbackTypeRequest>();

            var predicate = PredicateBuilder.Create<MsFeedbackType>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.Description, $"%{param.Search}%"));

            if (!string.IsNullOrWhiteSpace(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);

            var feedbackTypes = await _dbContext.Entity<MsFeedbackType>()
                .Where(predicate)
                .Select(c => new GetFeedbackTypeResult
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Description = c.Description,
                    })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(feedbackTypes as object);
        }
    }
}