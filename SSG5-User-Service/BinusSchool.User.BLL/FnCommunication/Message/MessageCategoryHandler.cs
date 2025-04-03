using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public MessageCategoryHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetMessageCategoryRequest>();

            var predicate = PredicateBuilder.Create<MsMessageCategory>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.Description, $"%{param.Search}%"));

            if (!string.IsNullOrWhiteSpace(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);
            

            var categories = await _dbContext.Entity<MsMessageCategory>()
                .Where(predicate)
                .Select(c => new GetMessageCategoryResult
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Description = c.Description,
                    })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(categories as object);
        }
    }
}