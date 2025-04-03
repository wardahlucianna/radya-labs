using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IProfileDataFieldSetting : IFnStudent
    {
        [Get("/profile-data-field-setting/get-profile-data-field-setting-list")]
        Task<ApiErrorResult<GetProfileDataFieldSettingListResponse>> GetProfileDataFieldSettingList(GetProfileDataFieldSettingListRequest request);

        [Get("/profile-data-field-setting/get-profile-data-field-setting-binusian-id")]
        Task<ApiErrorResult<IEnumerable<GetProfileDataFiedlSettingBinusianIdResponse>>> GetProfileDataFieldSettingBinusianId(GetProfileDataFieldSettingBinusianIdRequest request);

        [Post("/profile-data-field-setting/save-profile-data-field-setting")]
        Task<ApiErrorResult> SaveProfileDataFieldSetting([Body] SaveProfileDataFieldSettingRequest request);
    }
}
