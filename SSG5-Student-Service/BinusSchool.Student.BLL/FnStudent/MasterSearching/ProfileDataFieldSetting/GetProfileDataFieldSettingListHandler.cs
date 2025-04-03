using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MasterSearching.ProfileDataFieldSetting
{
    public class GetProfileDataFieldSettingListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetProfileDataFieldSettingListHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetProfileDataFieldSettingListRequest>
                (nameof(GetProfileDataFieldSettingListRequest.IdDataFieldRole),
                 nameof(GetProfileDataFieldSettingListRequest.IdBinusian));

            var response = new GetProfileDataFieldSettingListResponse();

            var getDataFieldAccess = _dbContext.Entity<TrProfileDataFieldPrivilege>()
                .Where(a => a.IdBinusian == request.IdBinusian)
                .ToList();

            var getProfileDataField = _dbContext.Entity<MsProfileDataField>()
                .Include(a => a.ProfileDataFieldGroup)
                .Where(a => (request.IdDataFieldRole == "STF" ? a.IdProfileDataFieldGroup == "5" : a.IdProfileDataFieldGroup != "5"))
                .OrderBy(a => a.IdProfileDataFieldGroup)
                    .ThenBy(a => a.OrderNumber)
                .Select(a => new 
                {
                    IdProfileDataFieldGroup = a.IdProfileDataFieldGroup,
                    GroupDesc = a.ProfileDataFieldGroup.Description,
                    GroupCode = a.ProfileDataFieldGroup.Code,
                    IdProfileDataField = a.Id,
                    FieldDesc = a.FieldDataProfile,
                    AliasName = a.AliasName
                })
                .ToList();

            var groupProfileDataField = getProfileDataField
                .GroupBy(a => new
                {
                    a.IdProfileDataFieldGroup,
                    a.GroupDesc,
                    a.GroupCode
                })
                .Select(b => new GetProfileDataFieldSettingListResponse_FieldGroup
                {
                    IdProfileDataFieldGroup = b.Key.IdProfileDataFieldGroup,
                    Description = b.Key.GroupDesc,
                    Code = b.Key.GroupCode,
                    Field = b.Select(c => new GetProfileDataFieldSettingListResponse_FieldGroup_Field
                    {
                        IdProfileDataField = c.IdProfileDataField,
                        AliasName = c.AliasName,
                        Description = c.FieldDesc,
                        IsChecked = getDataFieldAccess.Select(d => d.IdProfileDataField).Any(d => d == c.IdProfileDataField)
                    })
                    .ToList()
                })
                .ToList();

            response.IdBinusian = request.IdBinusian;
            response.FieldGroup = groupProfileDataField;

            return Request.CreateApiResult2(response as object);
        }
    }
}
