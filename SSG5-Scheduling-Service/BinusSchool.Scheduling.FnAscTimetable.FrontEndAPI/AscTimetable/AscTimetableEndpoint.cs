using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class AscTimetableEndpoint
    {

        private const string _route = "Scheduling/asc-timetable";
        private const string _tag = "Scheduling Asc Timetable";


        private readonly UploadXmlHandler _uploadXmlHandler;
        private readonly AscTimeTableHandler _ascTimeTableHandler;
        private readonly ReUploadXMLHandler _reUploadXMLHandler;
        private readonly SaveFileXmlHandler _saveFileXmlHandler;
        private readonly CheckAscTimetableProcessHandler _checkAscHandler;
        private readonly StartAscTimetableProcessHandler _startProcessHandler;
        private readonly FinishAscTimetableProcessHandler _finishProcessHandler;

        public AscTimetableEndpoint(UploadXmlHandler uploadXmlHandler,
            AscTimeTableHandler ascTimeTableHandler,
            ReUploadXMLHandler reUploadXMLHandler,
            SaveFileXmlHandler saveFileXmlHandler,
            CheckAscTimetableProcessHandler checkAscHandler,
            StartAscTimetableProcessHandler startProcessHandler,
            FinishAscTimetableProcessHandler finishProcessHandler
            )
        {
            _uploadXmlHandler = uploadXmlHandler;
            _ascTimeTableHandler = ascTimeTableHandler;
            _reUploadXMLHandler = reUploadXMLHandler;
            _saveFileXmlHandler = saveFileXmlHandler;
            _checkAscHandler = checkAscHandler;
            _startProcessHandler = startProcessHandler;
            _finishProcessHandler = finishProcessHandler;
        }

        [FunctionName(nameof(AscTimetableEndpoint.SaveFileXML))]
        [OpenApiOperation(tags: _tag, Summary = "Save file xml ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(AscTimeTableUploadXmlRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveFileXML(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-file-xml")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _saveFileXmlHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AscTimetableEndpoint.UploadXMLData))]
        [OpenApiOperation(tags: _tag, Summary = "Upload XML asc timetable ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(AscTimeTableUploadXmlRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UploadXmlFileResult))]
        public Task<IActionResult> UploadXMLData(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/upload-xml")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _uploadXmlHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AscTimetableEndpoint.ReUploadXMLData))]
        [OpenApiOperation(tags: _tag, Summary = "Re Upload XML asc timetable ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(AscTimeTableUploadXmlRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UploadXmlFileResult))]
        public Task<IActionResult> ReUploadXMLData(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reupload-xml")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _reUploadXMLHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AscTimetableEndpoint.GetListAsc))]
        [OpenApiOperation(tags: _tag, Summary = "Get asc time table List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.IdSessionSet), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.IdSchoolAcademicyears), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.IdSchoolLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AscTimetableGetListRequest.IdSchoolGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AscTimetableGetListResult))]
        public Task<IActionResult> GetListAsc(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _ascTimeTableHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AscTimetableEndpoint.AscTimetableDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get AscTimetable Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AscTimetableGetDetailResult))]
        public Task<IActionResult> AscTimetableDetail(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
         string id,
         CancellationToken cancellationToken)
        {
            return _ascTimeTableHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AscTimetableEndpoint.CheckIsJobRunning))]
        [OpenApiOperation(tags: _tag, Summary = "Check is there are running job on asc")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(StartAscTimetableProcessRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(StartAscTimetableProcessRequest.Grades), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(bool))]
        public Task<IActionResult> CheckIsJobRunning(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/check/is-there-job-run")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _checkAscHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AscTimetableEndpoint.StartProcess))]
        [OpenApiOperation(tags: _tag, Summary = "Start upload asc process")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(StartAscTimetableProcessRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> StartProcess(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/start-process")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _startProcessHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AscTimetableEndpoint.FinishProcess))]
        [OpenApiOperation(tags: _tag, Summary = "Finish upload asc process")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(FinishAscTimetableProcessRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> FinishProcess(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/finish-process")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _finishProcessHandler.Execute(req, cancellationToken);
        }
    }
}
