using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.Filter;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IFilter : IFnScoring
    {
        [Get("/scoringfilter/semester-term")]
        Task<ApiErrorResult<List<GetSemesterResult>>> SemesterFilter(GetSemesterRequest query);

        [Get("/scoringfilter/term")]
        Task<ApiErrorResult<List<ItemValueVm>>> TermFilter(GetTermRequest query);
    
        [Get("/bnsreportfilter/reporttypemapping/getlist-filter-levelgrade")]
        Task<ApiErrorResult<IEnumerable<GetListFilterLevelGradeReportTypeMappingResult>>> GetListFilterLevelGradeReportTypeMapping(GetListFilterLevelGradeReportTypeMappingRequest query);

        [Get("/bnsreportfilter/GetListFilterLevelGradeByTerm")]
        Task<ApiErrorResult<IEnumerable<GetListFilterLevelGradeByTermResult>>> GetListFilterLevelGradeByTerm(GetListFilterLevelGradeByTermRequest query);

        [Get("/scoringfilter/get-list-filter-scoring")]
        Task<ApiErrorResult<GetListFilterScoringResult>> GetListFilterScoring(GetListFilterScoringRequest query);

        [Get("/pmbenchmarksettings/get-list-filter-assessment-component")]
        Task<ApiErrorResult<GetFilterComponentByAcademicLevelGradeTermResult>> GetListFilterAssessmentComponent(GetFilterComponentByAcademicLevelGradeTermRequest query);

        [Get("/scoringfilter/get-list-filter-subject-id-by-academic-year")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListFilterSubjectIdByAcademicYear(GetListFilterSubjectIDByAcademicYearRequest query);
    }
}
