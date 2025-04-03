using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BinusSchool.Common.Functions.Abstractions
{
    public interface IFunctionsHttpHandler
    {
        Task<IActionResult> Execute(HttpRequest request, CancellationToken cancellationToken, bool validateUser = true, params KeyValuePair<string, object>[] keyValues);
    }
}