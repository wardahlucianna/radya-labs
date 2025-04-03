using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Extensions
{
    public static class EntityTypeBuilderExtension
    {
        public static IEnumerable<PropertyInfo> AllPropertyInfo<T>(this EntityTypeBuilder<T> _, Func<PropertyInfo, bool> filter = null)
            where T : class
        {
            var properties = typeof(T)
                .GetProperties()
                .AsEnumerable()
                .Where(x => !(x.GetMethod?.IsVirtual ?? false));

            if (filter != null)
                properties = properties.Where(filter);
            
            return properties;
        }
         
        public static IEnumerable<PropertyBuilder> AllPropertyBuilder<T>(this EntityTypeBuilder<T> builder, Func<PropertyInfo, bool> filter = null) 
            where T : class
        {
            return builder
                .AllPropertyInfo(filter)
                .Select(x => builder.Property(x.PropertyType, x.Name));
        }
    }
}