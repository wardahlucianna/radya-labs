using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface ICheckUsage : IFnDocument
    {
        [Get("/document/check-usage/term/{id}")]
        Task<ApiErrorResult<bool>> CheckUsageTerm(string id);

        [Get("/document/check-usage/subject/{id}")]
        Task<ApiErrorResult<bool>> CheckUsageSubject(string id);
    }
}