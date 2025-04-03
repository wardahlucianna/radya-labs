using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.School.FnSchool.ClassRoom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Common.Model;

namespace BinusSchool.School.FnSchool.ClassRoomMapping
{
    public class ClassRoomMappingEndpoint
    {
        private const string _route = "school/class_room_mapping_to_grade_streaming";
        private const string _tag = "School Class Mapping";

        private readonly MappingClassHandler _classRoomMappingHandler;
        private readonly GetListClassHandler _getListClassHandler;
        private readonly DeleteClassHandler _deleteClassHandler;
        private readonly DeleteDivisionHandler _deleteDivisionHandler;
        private readonly DeletePathwayHandler _deletePathwayHandler;
        private readonly GetClassroomByGradePathwayHandler _getClassroomByGradePathway;
        public ClassRoomMappingEndpoint(MappingClassHandler classRoomMappingHandler,
            GetListClassHandler getListClassHandler,
            DeleteClassHandler deleteClassHandler,
            DeleteDivisionHandler deleteDivisionHandler,
            DeletePathwayHandler deletePathwayHandler
, GetClassroomByGradePathwayHandler getClassroomByGradePathway)
        {
            _classRoomMappingHandler = classRoomMappingHandler;
            _getListClassHandler = getListClassHandler;
            _deleteClassHandler = deleteClassHandler;
            _deleteDivisionHandler = deleteDivisionHandler;
            _deletePathwayHandler = deletePathwayHandler;
            _getClassroomByGradePathway = getClassroomByGradePathway;
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetMappingClasses))]
        [OpenApiOperation(tags: _tag, Summary = "Get Mapping Class List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMappingClassRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMappingClassRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMappingClassRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMappingClassRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMappingClassRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMappingClassResult))]
        public Task<IActionResult> GetMappingClasses(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _classRoomMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetMappingClassDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Mapping Class Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMappingClassDetailResult))]
        public Task<IActionResult> GetMappingClassDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _classRoomMappingHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.AddMappingClass))]
        [OpenApiOperation(tags: _tag, Summary = "Add Mapping Class")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMappingClass))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMappingClass(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _classRoomMappingHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ClassRoomMappingEndpoint.UpdateMappingClass))]
        [OpenApiOperation(tags: _tag, Summary = "Update Mapping Class")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMappingClass))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMappingClass(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _classRoomMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.DeleteMappingClass))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Mapping Class")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMappingClass(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _classRoomMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetClassRoomMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Pathway Class List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListClassRequest.IdSchoolGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassroomMapResult))]
        public Task<IActionResult> GetClassRoomMapping(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/class_room_mapping_to_grade_streaming/get_class")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getListClassHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetListClassRoomMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Room Mapping List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassroomMapRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetClassroomMapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassroomMapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassroomMapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassroomMapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassroomMapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassroomMapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassroomMapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetClassroomMapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassroomMapRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassroomMapResult))]
        public Task<IActionResult> GetListClassRoomMapping(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/classroom-map")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListClassroomMapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetClassroomMapByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Classroom Mapping by Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListGradePathwayClassRoomRequest.Ids), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetListGradePathwayClassRoomRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassroomMapByGradeResult[]))]
        public Task<IActionResult> GetClassroomMapByGrade(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/classroom-map-by-grade")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassroomMapByGrade>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetClassroomMapByLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Classroom Mapping by Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiParameter(nameof(GetListGradePathwayClassRoomRequest.Ids), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassroomMapByGradeResult[]))]
        public Task<IActionResult> GetClassroomMapByLevel(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/classroom-map-by-level")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassroomMapByLevelHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.DeleteClassPathway))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Clas Map Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DeleteSchoolPathwayRequest.IdPathwayDetail), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteClassPathway(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "school/class_room_mapping_to_grade_streaming/delete_pathway")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deletePathwayHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.DeleteClassRoomDivision))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Class Room Division")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DeleteSchoolClassRoomDivisionRequest.IdClassRoomDivision), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteClassRoomDivision(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "school/class_room_mapping_to_grade_streaming/delete_division")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deleteDivisionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.DeleteClassRoomMap))]
        [OpenApiOperation(tags: _tag, Summary = "Delete  Class Room Map")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DeleteSchoolClassRoomMappingRequest.IdGradePathwayClassroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteClassRoomMap(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "school/class_room_mapping_to_grade_streaming/delete_class")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _deleteClassHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassRoomMappingEndpoint.GetClassroomMapByGradePathway))]
        [OpenApiOperation(tags: _tag, Summary = "Get Classroom Mapping by Grade Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassroomByGradePathwayRequest.Ids), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetClassroomByGradePathwayRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassroomMapByGradeResult[]))]
        public Task<IActionResult> GetClassroomMapByGradePathway(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/classroom-map-by-grade-pathway")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getClassroomByGradePathway.Execute(req, cancellationToken);
        }
    }
}
