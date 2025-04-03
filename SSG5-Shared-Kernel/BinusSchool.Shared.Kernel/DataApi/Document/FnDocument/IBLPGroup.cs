using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IBLPGroup : IFnDocument
    {
        [Get("/blp/blp-group")]
        Task<ApiErrorResult<IEnumerable<GetBLPGroupResult>>> GetBLPGroup(GetBLPGroupRequest body);

        [Get("/blp/blp-group/{id}")]
        Task<ApiErrorResult<GetBLPGroupDetailResult>> GetBLPGroupDetail(string id);

        [Post("/blp/blp-group")]
        Task<ApiErrorResult> SaveBLPGroup([Body] SaveBLPGroupRequest body);

        [Put("/blp/blp-group")]
        Task<ApiErrorResult> UpdateBLPGroup([Body] UpdateBLPGroupRequest body);

        [Delete("/blp/blp-group")]
        Task<ApiErrorResult> DeleteBLPGroup([Body] IEnumerable<string> ids);

        [Get("/blp/blp-group-student")]
        Task<ApiErrorResult<List<GetBLPGroupStudentResult>>> GetBLPGroupStudent(GetBLPGroupStudentRequest param);

        [Post("/blp/blp-group-student")]
        Task<ApiErrorResult> UpdateBLPGroupStudent([Body] List<UpdateBLPGroupStudentRequest> body);

        [Post("/blp/blp-group-student-excel")]
        Task<HttpResponseMessage> ExportExcelBLPGroupStudent([Body] ExportExcelBLPGroupStudentRequest body);
    }
}
