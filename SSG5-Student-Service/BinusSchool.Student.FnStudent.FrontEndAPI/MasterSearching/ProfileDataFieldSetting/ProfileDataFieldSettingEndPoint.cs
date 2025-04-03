using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.MasterSearching.ProfileDataFieldSetting
{
    public class ProfileDataFieldSettingEndPoint
    {
        private const string _route = "profile-data-field-setting";
        private const string _tag = "Profile Data Field Setting";

        private readonly GetProfileDataFieldSettingListHandler _getProfileDataFieldSettingListHandler;
        private readonly GetProfileDataFieldSettingBinusianIdHandler _getProfileDataFieldSettingBinusianIdHandler;
        private readonly SaveProfileDataFieldSettingHandler _saveProfileDataFieldSettingHandler;

        public ProfileDataFieldSettingEndPoint(
            GetProfileDataFieldSettingListHandler getProfileDataFieldSettingListHandler,
            GetProfileDataFieldSettingBinusianIdHandler getProfileDataFieldSettingBinusianIdHandler,
            SaveProfileDataFieldSettingHandler saveProfileDataFieldSettingHandler)
        {
            _getProfileDataFieldSettingListHandler = getProfileDataFieldSettingListHandler;
            _getProfileDataFieldSettingBinusianIdHandler = getProfileDataFieldSettingBinusianIdHandler;
            _saveProfileDataFieldSettingHandler = saveProfileDataFieldSettingHandler;
        }

        [FunctionName(nameof(ProfileDataFieldSettingEndPoint.GetProfileDataFieldSettingList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Profile Data Field Setting List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingListRequest.IdDataFieldRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingListRequest.IdBinusian), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetProfileDataFieldSettingListResponse))]
        public Task<IActionResult> GetProfileDataFieldSettingList(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-profile-data-field-setting-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getProfileDataFieldSettingListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProfileDataFieldSettingEndPoint.GetProfileDataFieldSettingBinusianId))]
        [OpenApiOperation(tags: _tag, Summary = "Get Profile Data Field Setting Binusian Id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProfileDataFieldSettingBinusianIdRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProfileDataFiedlSettingBinusianIdResponse>))]
        public Task<IActionResult> GetProfileDataFieldSettingBinusianId(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-profile-data-field-setting-binusian-id")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getProfileDataFieldSettingBinusianIdHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProfileDataFieldSettingEndPoint.SaveProfileDataFieldSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Save Profile Data Field Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveProfileDataFieldSettingRequest))]
        public Task<IActionResult> SaveProfileDataFieldSetting(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-profile-data-field-setting")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveProfileDataFieldSettingHandler.Execute(req, cancellationToken);
        }
    }
}
