using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.User.FnCommunication.Message
{
    public class SetMessageIsReadHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public SetMessageIsReadHandler(IUserDbContext userDbContext, IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetMessageIsReadRequest, SetMessageIsReadValidator>();

            var message = _dbContext.Entity<TrMessage>()
                .Include(x => x.MessageRecepients)
                .Where(x => x.Id == body.MessageId)
                .FirstOrDefault();
            if (message == null) throw new NotFoundException("Message not found");

            var recepient = message.MessageRecepients.Where(x => x.IdRecepient == body.UserId).FirstOrDefault();
            if (recepient == null) throw new NotFoundException("Recepient not found");
            
            recepient.IsRead = true;
            recepient.ReadDate = _dateTime.ServerTime;

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
