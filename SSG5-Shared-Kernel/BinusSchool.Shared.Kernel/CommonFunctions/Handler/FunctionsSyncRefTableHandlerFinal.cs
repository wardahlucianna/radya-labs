using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Common.Functions.Handler
{
    public class FunctionsSyncRefTableHandlerFinal<T> where T : DbContext, IAppDbContext
    {
        private readonly IServiceProvider _services;
        private readonly string _hubName;
        private readonly string _domain;

        protected FunctionsSyncRefTableHandlerFinal(IServiceProvider services, string hubName, string domain)
        {
            _services = services;
            _hubName = hubName;
            _domain = domain;
        }

        protected async Task Synchronize(string message, Guid invocationId, CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var function = new FunctionSync<T>(_hubName, _domain);
            await function.RunAsync(message, invocationId, scope, cancellationToken);
        }

        protected async Task RetrySyncAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var function = new FunctionSyncRetry<T>(_domain);
            await function.RunAsync(scope, cancellationToken);
        }
    }
}
