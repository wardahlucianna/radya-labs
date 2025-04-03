using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.TransferStudentData
{
    public class TransferStudentDataEndpoint
    {
        private const string _route = "student/transfer-student";
        private const string _tag = "Student Transfer from Marketing";


        private readonly TransferPrevMasterSchoolHandler _masterschoolhandler;
        private readonly TransferOccupationTypeHandler _occupationhandler;
        private readonly TransferStudentHandler _transferstudenthandler;
        private readonly TransferMasterDocumentHandler _transfermasterdocument;
        private readonly TransferMasterCountryHandler _transfermastercountry;
        private readonly TransferMasterDistrictHandler _transfermasterdistrict;
        private readonly TransferMasterNationalityHandler _transfermasternationality;

        public TransferStudentDataEndpoint(TransferPrevMasterSchoolHandler masterschoolhandler, TransferOccupationTypeHandler occupationhandler,TransferStudentHandler transferstudenthandler,
                TransferMasterDocumentHandler transfermasterdocument,TransferMasterCountryHandler transfermastercountry, TransferMasterDistrictHandler transfermasterdistrict,
                TransferMasterNationalityHandler transfermasternationality)
        {
            _masterschoolhandler = masterschoolhandler;
            _occupationhandler = occupationhandler;
            _transferstudenthandler = transferstudenthandler;
            _transfermasterdocument = transfermasterdocument;
            _transfermastercountry = transfermastercountry;
            _transfermasterdistrict = transfermasterdistrict;
            _transfermasternationality = transfermasternationality;
        }

        [FunctionName(nameof(TransferStudentDataEndpoint.TransferPrevMasterSchool))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Master School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferPrevMasterSchoolRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferPrevMasterSchool(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route+"/prev-master-school")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _masterschoolhandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(TransferStudentDataEndpoint.TransferOccupationType))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Occupation Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferOccupationTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferOccupationType(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route+"/occupation-type")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _occupationhandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TransferStudentDataEndpoint.TransferStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferStudent(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route+"/student-parent")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _transferstudenthandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TransferStudentDataEndpoint.TransferMasterDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Master Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferMasterDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferMasterDocument(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"/master-document")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _transfermasterdocument.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TransferStudentDataEndpoint.TransferMasterCountry))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Master Country")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferMasterCountryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferMasterCountry(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"/master-country")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _transfermastercountry.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TransferStudentDataEndpoint.TransferMasterDistrict))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Master District")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferMasterDistrictRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferMasterDistrict(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"/master-district")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _transfermasterdistrict.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TransferStudentDataEndpoint.TransferMasterNationality))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Master Nationality")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferMasterNationalityRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> TransferMasterNationality(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"/master-nationality")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _transfermasternationality.Execute(req, cancellationToken);
        }

        


    }
}
