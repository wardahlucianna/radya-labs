using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using BinusSchool.Student.FnStudent.Student;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentPhoto
{
    public class StudentPhotoEndPoint
    {
        private const string _route = "student-photo";
        private const string _tag = "Student Photo";

        private readonly GetStudentPhotoListHandler _getStudentPhotoListHandler;
        private readonly StudentPhotoHandler _studentPhotoHandler;
        private readonly DeleteStudentPhotoHandler _deleteStudentPhotoHandler;
        private readonly CopyStudentPhotoHandler _copyStudentPhotoHandler;
        public StudentPhotoEndPoint(StudentPhotoHandler studentPhotoHandler,
            DeleteStudentPhotoHandler deleteStudentPhotoHandler,
            CopyStudentPhotoHandler copyStudentPhotoHandler,
            GetStudentPhotoListHandler getStudentPhotoListHandler
            )
        {
            _getStudentPhotoListHandler = getStudentPhotoListHandler;
            _studentPhotoHandler = studentPhotoHandler;
            _deleteStudentPhotoHandler = deleteStudentPhotoHandler;
            _copyStudentPhotoHandler = copyStudentPhotoHandler;
        }

        [FunctionName(nameof(StudentPhotoEndPoint.GetListStudentPhotoData))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Student Photo Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.Return), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.StudentType), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentPhotoListRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentPhotoListResult>))]
        public Task<IActionResult> GetListStudentPhotoData(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetListStudentPhotoData")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentPhotoListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentPhotoEndPoint.SaveStudentPhotoData))]
        [OpenApiOperation(tags: _tag, Summary = "Save Student Photo Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveStudentPhotoDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveStudentPhotoData(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/SaveStudentPhotoData")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _studentPhotoHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentPhotoEndPoint.CopyStudentPhotoData))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Student Photo Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyStudentPhotoRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyStudentPhotoData(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/CopyStudentPhotoData")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _copyStudentPhotoHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentPhotoEndPoint.DeleteStudentPhotoData))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student Photo Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteStudentPhotoRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteStudentPhotoData(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/DeleteStudentPhotoRequest")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteStudentPhotoHandler.Execute(req, cancellationToken);
        }

    }
}
