using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using BinusSchool.School.FnSchool.TextbookPreparationApprovalSetting.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.School.FnSchool.TextbookPreparationApprovalSetting
{
    public class TextbookPreparationApprovalSettingHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public TextbookPreparationApprovalSettingHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationApprovalSettingRequest>();

            var GetSettingApproval = await _dbContext.Entity<MsTextbookSettingApproval>()
                .Include(e => e.Staff)
                .Where(e => e.IdSchool == param.IdSchool)
               .Select(x => new GetTextbookPreparationApprovalSettingResult
               {
                   Id = x.Id,
                   IdUser = x.IdBinusian,
                   UserDisplayName = (!string.IsNullOrEmpty(x.Staff.FirstName) ? x.Staff.FirstName : "") + (!string.IsNullOrEmpty(x.Staff.LastName) ? " " + x.Staff.LastName : ""),
                   IdRole = x.IdRole,
                   IdTeacherPosition = x.IdTeacherPosition,
                   IsDelete = x.IsDelete,
                   IsEdit = x.IsEdit,
                   ApproverTo = x.ApproverTo,
               }).OrderBy(e => e.ApproverTo).ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items = GetSettingApproval;

            return Request.CreateApiResult2(items);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddTextbookPreparationApprovalSettingRequest, AddTextbookPreparationApprovalSettingValidator>();
            List<string> IdUserApproval = new List<string>();

            var ExsisTextbookPreparation = await _dbContext.Entity<TrTextbook>()
                                   .Where(e => e.Status == TextbookPreparationStatus.OnReview1 || e.Status == TextbookPreparationStatus.OnReview2 || e.Status == TextbookPreparationStatus.OnReview3)
                                   .AnyAsync(CancellationToken);

            if (ExsisTextbookPreparation)
                throw new BadRequestException("Please, complete all approval");

            var GetApprovalSetting = await _dbContext.Entity<MsTextbookSettingApproval>()
                                        .Where(e => e.IdSchool == body.IdSchool)
                                        .ToListAsync(CancellationToken);

            GetApprovalSetting.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsTextbookSettingApproval>().UpdateRange(GetApprovalSetting);

            foreach (var item in body.TextbookPreparationApprovalSetting)
            {
                var ExcludeIdUserApproval = GetApprovalSetting.Where(e => e.IdBinusian == item.IdUser).Any();
                if (!ExcludeIdUserApproval)
                    IdUserApproval.Add(item.IdUser);

                var NewTextBookSettingApproval = new MsTextbookSettingApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSchool = body.IdSchool,
                    IdBinusian = item.IdUser,
                    IdRole = item.IdRole,
                    IdTeacherPosition = item.IdTeacherPosition,
                    IsEdit = item.IsEdit,
                    IsDelete = item.IsDelete,
                    ApproverTo = item.ApproverTo,
                };
                _dbContext.Entity<MsTextbookSettingApproval>().Add(NewTextBookSettingApproval);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Send Email

            var GetUserCreate = await _dbContext.Entity<MsUser>()
                                        .Where(e => e.Id == body.IdUser)
                                        .FirstOrDefaultAsync(CancellationToken);

            var GetApprovalSettingEmail = new GetApprovalSettingEmailResult
            {
                NamaAdministrator = GetUserCreate.DisplayName,
                IdUser = IdUserApproval,
            };

            if (KeyValues.ContainsKey("EmailApprovalSetting"))
            {
                KeyValues.Remove("EmailApprovalSetting");
            }
            KeyValues.Add("EmailApprovalSetting", GetApprovalSettingEmail);

            var Notification = TP5Notification(KeyValues, AuthInfo);
            #endregion

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            return Request.CreateApiResult2();
        }

        public static string TP5Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailApprovalSetting").Value;
            var EmailApprovalSetting = JsonConvert.DeserializeObject<GetApprovalSettingEmailResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP5")
                {
                    IdRecipients = EmailApprovalSetting.IdUser,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

    }
}
