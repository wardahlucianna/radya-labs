using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Scoring.FnScoring.POI;



namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IPOIEntry : IFnScoring
    {
        [Get("/Scoring/POI/GetUOI")]
        Task<ApiErrorResult<IEnumerable<GetUOIResult>>> GetUOI(GetUOIRequest query);

        [Get("/Scoring/POI/GetLinesOfInquiry")]
        Task<ApiErrorResult<IEnumerable<GetLinesOfInquiryResult>>> GetLinesOfInquiry(GetLinesOfInquiryRequest query);

        [Get("/Scoring/POI/GetCentralIdea")]
        Task<ApiErrorResult<IEnumerable<GetCentralIdeaResult>>> GetCentralIdea(GetCentralIdeaRequest query);

        [Get("/Scoring/POI/GetPOI")]
        Task<ApiErrorResult<GetPOIResult>> GetPOI(GetPOIRequest query);

        [Post("/Scoring/POI/AddPOI")]
        Task<ApiErrorResult> AddPOI(List<AddPOIRequest> query);

        [Get("/Scoring/POI/GetProgrammeinqId")]
        Task<ApiErrorResult<GetProgrammeInqIdResult>> GetProgrammeInqId(GetProgrammeInqIdRequest query);

        [Get("/Scoring/POI/GetPOIPerStudent")]
        Task<ApiErrorResult<IEnumerable<GetPOIPerStudentResult>>> GetPOIPerStudent(GetPOIPerStudentRequest query);

        [Post("/Scoring/POI/GetPOIPerStudentExcel")]
        Task<HttpResponseMessage> ExportExcelPoiPerStudent([Body] ExportExcelPOIPerStudentRequest body);
    }
}
