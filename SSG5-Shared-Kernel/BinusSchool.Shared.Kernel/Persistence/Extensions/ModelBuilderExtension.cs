using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.Extensions
{
    public static class ModelBuilderExtension
    {
        [Obsolete("Use ApplyEntityRegistration3() instead")]
        public static void ApplyEntityRegistration2(this ModelBuilder source)
        {
            var callerMethod = new StackTrace().GetFrame(1).GetMethod();
            var callerType = callerMethod.ReflectedType;

            var callerEntity = callerType.GetInterfaces()
                .First(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(IAppDbContext<>) || x.GetGenericTypeDefinition() == typeof(IAppNoDbContext<>)))
                .GenericTypeArguments[0];
            
            source.ApplyConfigurationsFromAssembly(Assembly.GetCallingAssembly(), x 
                => !(x.BaseType is null) 
                && x.BaseType.GenericTypeArguments.Length != 0 
                && callerEntity.IsAssignableFrom(x.BaseType.GenericTypeArguments[0]));

            var assemblies = callerEntity.Assembly.GetTypes();
            var entities = assemblies.Where(d => callerEntity.IsAssignableFrom(d) && !d.IsInterface);
            
            foreach (var entity in entities)
            {
                var configuration = assemblies
                    .Where(d => typeof(IEntityTypeConfiguration<>).MakeGenericType(entity).IsAssignableFrom(d) && !d.IsInterface)
                    .OrderByDescending(d => d.Name)
                    .FirstOrDefault();
                if (configuration == null)
                    source.Entity(entity);
            }
        }

        public static void ApplyEntityRegistration3<T>(this ModelBuilder source) where T : IEntity
        {
            var callerEntity = typeof(T);
            
            source.ApplyConfigurationsFromAssembly(callerEntity.Assembly, x 
                => !(x.BaseType is null) 
                && x.BaseType.GenericTypeArguments.Length != 0 
                && callerEntity.IsAssignableFrom(x.BaseType.GenericTypeArguments[0]));
        }
    }
}
