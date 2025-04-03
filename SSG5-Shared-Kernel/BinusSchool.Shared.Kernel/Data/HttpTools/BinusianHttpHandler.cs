using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Data.HttpTools
{
    public class BinusianHttpHandler : HttpLoggingHandler
    {
        public BinusianHttpHandler(ILogger<BinusianHttpHandler> logger) : base(logger)
        {

        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}