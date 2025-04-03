using System.Threading.Tasks;
using System.Linq;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Utils;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageEmptyTrashHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public MessageEmptyTrashHandler(
            IUserDbContext userDbContext,
            IMachineDateTime dateTime)

        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MessageEmptyTrashRequest>(nameof(MessageEmptyTrashRequest.UserId));

            var predicate = PredicateBuilder.Create<TrMessageRecepient>(x => true);

            var recepients = _dbContext.Entity<TrMessageRecepient>().Include(x => x.Message).Where(predicate);
            recepients = recepients.Where(x =>
                    (x.Message.Type == UserMessageType.Announcement ?
                     (!x.Message.PublishEndDate.HasValue || (x.Message.PublishEndDate.HasValue && x.Message.PublishEndDate.Value.Date < _dateTime.ServerTime.Date) || x.Message.MessageRecepients.Any(m =>
                        m.IdRecepient == param.UserId && m.MessageFolder == MessageFolder.Trash
                        && x.Message.IsDraft == false
                    )) : true)
                    && x.Message.MessageRecepients.Any(m =>
                        m.IdRecepient == param.UserId &&
                        (x.Message.Type == UserMessageType.Announcement ? m.IdRecepient == param.UserId && (m.MessageFolder == MessageFolder.Trash || m.MessageFolder == MessageFolder.Inbox) : m.MessageFolder == MessageFolder.Trash) 
                        && x.Message.IsDraft == false
                    )
                );

            foreach (var r in recepients)
            {
                r.MessageFolder = MessageFolder.TrashPermanent;
                _dbContext.Entity<TrMessageRecepient>().Update(r);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
