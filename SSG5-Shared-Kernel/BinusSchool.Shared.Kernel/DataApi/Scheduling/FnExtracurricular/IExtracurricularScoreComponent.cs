using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularScoreComponent : IFnExtracurricular
    {
        [Delete("/extracurricular/score-component")]
        Task<ApiErrorResult> DeleteExtracurricularScoreComponent([Body] IEnumerable<string> ids);

        [Get("/extracurricular/score-component")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularScoreComponentResult>>> GetExtracurricularScoreComponent(GetExtracurricularScoreComponentRequest body);

        [Post("/extracurricular/score-component")]
        Task<ApiErrorResult> AddExtracurricularScoreComponent([Body] AddExtracurricularScoreComponentRequest body);

        [Delete("/extracurricular/score-component-v2")]
        Task<ApiErrorResult> DeleteExtracurricularScoreComponent2([Body] IEnumerable<string> ids);

        [Get("/extracurricular/score-component-v2")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularScoreComponentResult2>>> GetExtracurricularScoreComponent2(GetExtracurricularScoreComponentRequest2 body);

        [Post("/extracurricular/score-component-v2")]
        Task<ApiErrorResult> AddExtracurricularScoreComponent2([Body] AddExtracurricularScoreComponentRequest2 body);

        [Get("/extracurricular/score-component-calculation-type")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricualrScoreCalculationTypeResult>>> GetExtracurricularScoreCalculationType(GetExtracurricularScoreCalculationTypeRequest body);
    }
}
