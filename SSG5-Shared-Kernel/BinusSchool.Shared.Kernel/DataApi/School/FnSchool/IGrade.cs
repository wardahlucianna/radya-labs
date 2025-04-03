using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.GradePathwayDetails;
using BinusSchool.Data.Model.School.FnSchool.GradePathways;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IGrade : IFnSchool
    {
        [Get("/school/grade")]
        Task<ApiErrorResult<IEnumerable<GetGradeResult>>> GetGrades(GetGradeRequest query);

        [Get("/school/grade/{id}")]
        Task<ApiErrorResult<GetGradeDetailResult>> GetGradeDetail(string id);

        [Get("/school/grade-by-sessionset")]
        Task<ApiErrorResult<List<GradePathwayForAscTimeTableResult>>> GetGradeListBysessionset([Query] GradePathwayBySessionSetIdForXmlRequest query);

        [Get("/school/grade-by-ids")]
        Task<ApiErrorResult<List<CodeWithIdVm>>> GetGradeListByIds([Query] GetGradeCodeByIDForAscTimetableRequest query);

        [Get("/school/grade-by-grade-pathway-ids")]
        Task<ApiErrorResult<List<GradePathwayForAscTimeTableResult>>> GetGradeListByGradePathwayIds([Query] GetGradepathwayForXMLRequest query);

        [Post("/school/grade")]
        Task<ApiErrorResult> AddGrade([Body] AddGradeRequest body);

        [Put("/school/grade")]
        Task<ApiErrorResult> UpdateGrade([Body] UpdateGradeRequest body);

        [Delete("/school/grade")]
        Task<ApiErrorResult> DeleteGrade([Body] IEnumerable<string> ids);

        [Get("/school/grade-acadyear")]
        Task<ApiErrorResult<IEnumerable<GetGradeAcadyearResult>>> GetGradesByAcadyear(GetGradeAcadyearRequest query);

        [Get("/school/grade-code-acadyear")]
        Task<ApiErrorResult<IEnumerable<GradeCodeAcademicYearsResult>>> GradeCodeAcademicYears(GradeCodeAcademicYearsRequest query);

        [Get("/school/grade-with-pathway")]
        Task<ApiErrorResult<IEnumerable<GetGradeWithPathwayResult>>> GetGradeWithPathways(GetGradeWithPathwayRequest query);

        [Get("/school/grade-pathway-detail")]
        Task<ApiErrorResult<IEnumerable<GetGradePathwayResult>>> GetGradePathwayDetails(GetGradePathwayDetailRequest query);

        [Get("/school/grade-with-pathway-summary")]
        Task<ApiErrorResult<IEnumerable<GetGradePathwaySummaryResult>>> GetGradeWithPathwaySummary(GetGradePathwaySummaryRequest query);

        [Get("/school/grade-code")]
        Task<ApiErrorResult<IEnumerable<GetGradeCodeResult>>> GetGradeCodeList(GetGradeCodeRequest query);

        [Get("/school/grade-by-multiple-level")]
        Task<ApiErrorResult<List<GetGradeMultipleLevelResult>>> GetGradeMultipleLevel([Query] GetGradeMultipleLevelRequest query);
    }
}
