using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BinusSchool.Common.Abstractions
{
    public interface IFunctionsHttpHandler
    {
        Task<ApiResult<IReadOnlyList<IItemValueVm>>> GetHandler(HttpRequest request, CancellationToken cancellationToken);
        Task<ApiResult<object>> GetDetailHandler(HttpRequest request, string id, CancellationToken cancellationToken);
        Task<ApiResult> PostHandler(HttpRequest request, CancellationToken cancellationToken);
        Task<ApiResult> PutHandler(HttpRequest request, CancellationToken cancellationToken);
        Task<ApiResult> DeleteHandler(HttpRequest request, CancellationToken cancellationToken);
    }
}