using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class MasterParticipantEndpoint
    {
        private const string _route = "master-participant";
        private const string _tag = "Extracurricular Master Participant";

        private readonly GetMasterParticipantHandler _getMasterParticipantHandler;
        private readonly GetMasterParticipantHandlerV2 _getMasterParticipantHandlerV2;
        private readonly GetStudentParticipantByExtracurricularHandler _getStudentParticipantByExtracurricularHandler;
        private readonly GetUnselectedStudentByHomeroomHandler _getUnselectedStudentByHomeroomHandler;
        private readonly AddStudentParticipantHandler _addStudentParticipantHandler;
        private readonly UpdateStudentParticipantHandler _updateStudentParticipantHandler;
        private readonly DeleteStudentParticipantHandler _deleteStudentParticipantHandler;
        private readonly ExportExcelStudentParticipantHandler _exportExcelStudentParticipantHandler;
        private readonly DeleteAllStudentParticipantHandler _deleteAllStudentParticipantHandler;
        private readonly EditJoinDateStudentParticipantHandler _editJoinDateStudentParticipantHandler;
        private readonly AddStudentParticipantByExcelHandler _addStudentParticipantByExcelHandler;

        public MasterParticipantEndpoint(
            GetMasterParticipantHandler getMasterParticipantHandler,
            GetMasterParticipantHandlerV2 getMasterParticipantHandlerV2,
            GetStudentParticipantByExtracurricularHandler getStudentParticipantByExtracurricular,
            GetUnselectedStudentByHomeroomHandler getUnselectedStudentByHomeroomHandler,
            AddStudentParticipantHandler addStudentParticipantHandler,
            UpdateStudentParticipantHandler updateStudentParticipantHandler,
            DeleteStudentParticipantHandler deleteStudentParticipantHandler,
            DeleteAllStudentParticipantHandler deleteAllStudentParticipantHandler,
            ExportExcelStudentParticipantHandler exportExcelStudentParticipantHandler,
            EditJoinDateStudentParticipantHandler editJoinDateStudentParticipantHandler,
            AddStudentParticipantByExcelHandler addStudentParticipantByExcelHandler)
        {
            _getMasterParticipantHandler = getMasterParticipantHandler;
            _getMasterParticipantHandlerV2 = getMasterParticipantHandlerV2;
            _getStudentParticipantByExtracurricularHandler = getStudentParticipantByExtracurricular;
            _getUnselectedStudentByHomeroomHandler = getUnselectedStudentByHomeroomHandler;
            _addStudentParticipantHandler = addStudentParticipantHandler;
            _updateStudentParticipantHandler = updateStudentParticipantHandler;
            _deleteStudentParticipantHandler = deleteStudentParticipantHandler;
            _exportExcelStudentParticipantHandler = exportExcelStudentParticipantHandler;
            _deleteAllStudentParticipantHandler = deleteAllStudentParticipantHandler;
            _editJoinDateStudentParticipantHandler = editJoinDateStudentParticipantHandler;
            _addStudentParticipantByExcelHandler = addStudentParticipantByExcelHandler;
        }

        [FunctionName(nameof(MasterParticipantEndpoint.GetMasterParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Master Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterParticipantRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterParticipantResult))]
        public Task<IActionResult> GetMasterParticipant([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-master-participant")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getMasterParticipantHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.GetMasterParticipantV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Master Participant V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterParticipantRequestV2))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterParticipantResultV2))]
        public Task<IActionResult> GetMasterParticipantV2([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-master-participant-v2")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            return _getMasterParticipantHandlerV2.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.GetStudentParticipantByExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Student Participant By Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentParticipantByExtracurricularRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentParticipantByExtracurricularResult))]
        public Task<IActionResult> GetStudentParticipantByExtracurricular([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-participant-by-extracurricular")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getStudentParticipantByExtracurricularHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.GetUnselectedStudentByHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unselected Student Participant By Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnselectedStudentByHomeroomRequest.IdExtracurricular), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnselectedStudentByHomeroomResult))]
        public Task<IActionResult> GetUnselectedStudentByHomeroom([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-unselected-student-by-homeroom")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getUnselectedStudentByHomeroomHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.AddStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Add Extracurricular Student Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<AddStudentParticipantRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStudentParticipant([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-student-participant")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addStudentParticipantHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.UpdateStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentParticipantRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentParticipant([HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-student-participant")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateStudentParticipantHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.DeleteStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<DeleteStudentParticipantRequest>))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<DeleteStudentParticipantResult>))]
        public Task<IActionResult> DeleteStudentParticipant([HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-student-participant")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteStudentParticipantHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.DeleteAllStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Delete All Student Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteAllStudentParticipantRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<DeleteAllStudentParticipantResult>))]
        public Task<IActionResult> DeleteAllStudentParticipant([HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-all-student-participant")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteAllStudentParticipantHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.ExportExcelStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Export Extracurricular Student Participant to Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelStudentParticipantRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelStudentParticipant([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/student-participant-excel")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _exportExcelStudentParticipantHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.EditJoinDateStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Edit Join Date Student Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(EditJoinDateStudentParticipantRequest))]
        public Task<IActionResult> EditJoinDateStudentParticipant([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/edit-join-date")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _editJoinDateStudentParticipantHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterParticipantEndpoint.AddStudentParticipantByExcel))]
        [OpenApiOperation(tags: _tag, Summary = "Add Extracurricular Student Participant by Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(AddStudentParticipantByExcelRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddStudentParticipantByExcelResult))]
        public Task<IActionResult> AddStudentParticipantByExcel([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-student-participant-by-excel")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addStudentParticipantByExcelHandler.Execute(req, cancellationToken);
        }
    }
}
