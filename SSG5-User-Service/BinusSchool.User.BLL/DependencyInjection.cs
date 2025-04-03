using BinusSchool.Common.Functions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.User.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayerAPI(this IServiceCollection services, IConfiguration configuration)
        {
            var handlerType = typeof(IFunctionsHttpHandler);
            var _currentFunctionsTypes = typeof(DependencyInjection).Assembly.GetTypes();

            var handlers = _currentFunctionsTypes
                .Where(x
                    => handlerType.IsAssignableFrom(x)
                       && x != handlerType
                       && !x.IsInterface
                       && !x.IsAbstract);

            foreach (var handler in handlers)
                services.Add(new ServiceDescriptor(handler, handler, ServiceLifetime.Transient));

            return services;
        }
    }
}