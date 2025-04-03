using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreSetting;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectSplitMapping;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ISubjectSplitMapping : IFnScoring
    {
        [Get("/subjectsplitmapping/get-subjectsplitmapping")]
        Task<ApiErrorResult<IEnumerable<GetSubjectSplitMappingResult>>> GetSubjectSplitMapping(GetSubjectSplitMappingRequest query);

        [Put("/subjectsplitmapping/update-subjectsplitmapping")]
        Task<ApiErrorResult> UpdateSubjectSplitMapping([Body] UpdateSubjectSplitMappingRequest query);

        [Post("/subjectsplitmapping/copy-prev-subjectsplitmapping")]
        Task<ApiErrorResult> CopyPrevSubjectSplitMapping([Body] CopySubjectSplitMappingRequest query);

        [Delete("/subjectsplitmapping/delete-subjectsplitmapping-by-subjectparent")]
        Task<ApiErrorResult> DeleteSubjectSplitMappingBySubjectParent([Body] DeleteSubjectSplitMappingBySubjectParentRequest query);

        [Get("/subjectsplitmapping/get-score-legend-detail")]
        Task<ApiErrorResult<IEnumerable<GetSubjectSplitMappingDetailResult>>> GetSubjectSplitMappingDetail(GetSubjectSplitMappingDetailRequest query);

    }
}
