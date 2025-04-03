using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageOptionHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public MessageOptionHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMessageOptionRequest>(nameof(GetMessageOptionRequest.IdSchool), nameof(GetMessageOptionRequest.Code));

            var option = await _dbContext.Entity<MsMessageOption>().Select(o => new GetMessageOptionResult
            {
                Id = o.Id,
                Code = o.Code,
                Description = o.Description,
                Value = o.Value,
                IdSchool = o.IdSchool
            }).Where(predicate: option => option.Code == param.Code && option.IdSchool == param.IdSchool).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(option as object);
        }
    }
}