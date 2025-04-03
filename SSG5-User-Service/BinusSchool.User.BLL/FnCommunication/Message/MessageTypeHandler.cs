using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageTypeHandler : FunctionsHttpSingleHandler
    {
        protected override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetMessageTypeRequest>();

            var types = new GetMessageTypeResult[]
            {
                new GetMessageTypeResult{
                    Id = ((int) UserMessageType.Private).ToString(),
                    Description = UserMessageType.Private.ToString()
                },

                //new GetMessageTypeResult{
                //    Id = ((int) UserMessageType.Announcement).ToString(),
                //    Description = UserMessageType.Announcement.ToString()
                //},

                new GetMessageTypeResult{
                    Id = ((int) UserMessageType.Feedback).ToString(),
                    Description = UserMessageType.Feedback.ToString()
                },

                new GetMessageTypeResult{
                    Id = ((int) UserMessageType.AscTimetable).ToString(),
                    Description = UserMessageType.AscTimetable.GetDescription()
                },

                new GetMessageTypeResult{
                    Id = ((int) UserMessageType.GenerateSchedule).ToString(),
                    Description = UserMessageType.GenerateSchedule.GetDescription()
                },

                new GetMessageTypeResult{
                    Id = ((int) UserMessageType.Information).ToString(),
                    Description = UserMessageType.Information.GetDescription()
                }
            }.ToList();

            if (!string.IsNullOrEmpty(param.Search))
                types = types.Where(x => x.Description.ToLower().Contains(param.Search.ToLower())).ToList();

            return Task.FromResult(Request.CreateApiResult2(types as object));
        }
    }
}
