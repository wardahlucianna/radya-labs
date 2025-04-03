using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IBlendedLearningProgram : IFnDocument
    {
        [Get("/document/blp-question")]
        Task<ApiErrorResult<IEnumerable<GetBLPQuestionResult>>> GetBLPQuestion(GetBLPQuestionRequest param);

        [Get("/document/blp-question-with-history")]
        Task<ApiErrorResult<IEnumerable<GetBLPQuestionWithHistoryResult>>> GetBLPQuestionWithHistory(GetBLPQuestionWithHistoryRequest param);


        [Post("/document/blp-save-respond-answer")]
        Task<ApiErrorResult> SaveRespondAnswerBLP([Body] SaveRespondAnswerBLPRequest param);

        [Multipart]
        [Post("/document/blp-upload-file")]
        Task<ApiErrorResult<UploadFileBLPRResult>> UploadFileBLP(
            bool IsUpdate,
            string IdStudent,
            string? IdSurveyPeriod,
            string? IdClearanceWeekPeriod,
            string IdSurveyQuestionMapping,
            string IdSurveyAnswerMapping,
            [AliasAs("file")] StreamPart file
            );
    }
}
