using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IMasterRule : IFnExtracurricular
    {
        [Get("/extracurricular/master-rule")]
        Task<ApiErrorResult<IEnumerable<GetMasterExtracurricularRuleResult>>> GetMasterExtracurricularRule(GetMasterExtracurricularRuleRequest body);

        [Get("/extracurricular/master-rule/{id}")]
        Task<ApiErrorResult<GetMasterExtracurricularRuleResult>> GetMasterExtracurricularRuleDetail(string id);

        [Post("/extracurricular/master-rule")]
        Task<ApiErrorResult> AddMasterExtracurricularRule([Body] UpdateMasterExtracurricularRuleRequest body);

        [Put("/extracurricular/master-rule")]
        Task<ApiErrorResult> UpdateMasterExtracurricularRule([Body] UpdateMasterExtracurricularRuleRequest body);

        [Delete("/extracurricular/master-rule")]
        Task <ApiErrorResult> DeleteMasterExtracurricularRule([Body] IEnumerable<string> ids);

        [Get("/extracurricular/master-rule-support-doc")]
        Task<ApiErrorResult<IEnumerable<GetSupportingDucumentResult>>> GetSupportingDucument(GetSupportingDucumentRequest body);

        [Get("/extracurricular/master-rule-support-doc/{id}")]
        Task<ApiErrorResult<GetSupportingDucumentDetailResult>> GetSupportingDucumentDetail(string id);

        //[Post("/extracurricular/master-rule-support-doc")]
        //Task<ApiErrorResult> AddSupportingDucument([Body] UpdateSupportingDocumentRequest body);
         
        [Multipart]
        [Post("/extracurricular/master-rule-support-doc")]
        Task<ApiErrorResult> AddStudentDocument(string Name,
                                                bool ShowToParent,
                                                bool ShowToStudent,
                                                bool Status,
                                                string FileName,
                                                decimal FileSize,
                                                string Grades,  // pake split '~'                                                      
                                                [AliasAs("file")] StreamPart file);

        //[Put("/extracurricular/master-rule-support-doc")]
        //Task<ApiErrorResult> UpdateSupportingDucument([Body] UpdateSupportingDocumentRequest body);

        [Multipart]
        [Put("/extracurricular/master-rule-support-doc")]
        Task<ApiErrorResult> UpdateSupportingDucument(bool ActionUpdateStatus,
                                             string IdExtracurricularSupportDoc,
                                             string Name,
                                             bool ShowToParent,
                                             bool ShowToStudent,
                                             bool Status,
                                             string FileName,
                                             decimal FileSize,
                                             string Grades,  // pake split '~'                                                      
                                             [AliasAs("file")] StreamPart file);

        [Delete("/extracurricular/master-rule-support-doc")]
        Task<ApiErrorResult> DeleteSupportingDucument([Body] IEnumerable<string> ids);

        [Post("/extracurricular/master-rule/copy-extracurricular-rule-from-last-ay")]
        Task<ApiErrorResult> CopyExtracurricularRuleFromLastAY([Body] CopyExtracurricularRuleFromLastAYRequest body);
    }
}
