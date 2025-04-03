using BinusSchool.Common.Functions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BinusSchool.Employee.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayerAPI(this IServiceCollection services, IConfiguration configuration)
        {
            var handlerType = typeof(IFunctionsHttpHandler);
            var _currentFunctionTypes = typeof(DependencyInjection).Assembly.GetTypes();

            var handlers = _currentFunctionTypes
                .Where(x =>
                handlerType.IsAssignableFrom(x)
                && x != handlerType
                && !x.IsInterface
                && !x.IsAbstract);

            foreach (var handler in handlers)
                services.Add(new ServiceDescriptor(handler, handler, ServiceLifetime.Transient));

            return services;
        }
    }
}
