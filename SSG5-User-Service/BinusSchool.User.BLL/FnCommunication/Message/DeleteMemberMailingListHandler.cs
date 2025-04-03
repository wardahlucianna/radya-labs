using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using HandlebarsDotNet;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.User.FnCommunication.Message
{
    public class DeleteMemberMailingListHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public DeleteMemberMailingListHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteMemberMailingListRequest, DeleteMemberMailingListValidator>();


            var userMember = await _dbContext.Entity<MsGroupMailingListMember>()
                                                .Include(x => x.GroupMailingList)
                                                .Include(x => x.User)
                                                .Where(x => x.IdGroupMailingList == body.IdGroupMailingList  && x.IdUser == body.IdUser)
                                                .FirstOrDefaultAsync(CancellationToken);
            if(userMember != null)
            {
                userMember.IsActive = false;

                _dbContext.Entity<MsGroupMailingListMember>().Update(userMember);

                await _dbContext.SaveChangesAsync(CancellationToken);

                #region Notification
                var DetailGroupMailingList = new GetGroupMailingListDetailsResult
                {
                    Id = userMember.IdGroupMailingList,
                    GroupName = userMember.GroupMailingList.GroupName,
                    IdUser = userMember.GroupMailingList.IdUser,
                    GroupMembers = new List<GroupMember> { new GroupMember {
                        Name = userMember.User.DisplayName
                    }}
                };

                if (KeyValues.ContainsKey("GetDetailGroupMailingList"))
                {
                    KeyValues.Remove("GetDetailGroupMailingList");
                }

                KeyValues.Add("GetDetailGroupMailingList", DetailGroupMailingList);
                var Notification = EHN3Notification(KeyValues, AuthInfo);
                #endregion
            }

            return Request.CreateApiResult2();
        }

        public static string EHN3Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EHN3")
                {
                    IdRecipients = new string[] { DetailMailingList.IdUser },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
