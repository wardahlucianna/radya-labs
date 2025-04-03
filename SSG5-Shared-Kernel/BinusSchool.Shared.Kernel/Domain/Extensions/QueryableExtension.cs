using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BinusSchool.Domain.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<T> SearchByIds<T>(this IQueryable<T> query, IIdCollection id)
            where T : IUniqueId, IEntity
        {
            if (!(id.Ids?.Any() ?? false))
                return query;

            return query.Where(x => id.Ids.Contains(x.Id));
        }

        public static IQueryable<T> SearchByDynamic<T>(this IQueryable<T> query, ISearch search)
            where T : IEntity
        {
            // if there are no such keyword or search by, skip the search
            if (string.IsNullOrEmpty(search.Search) || (!search.SearchBy?.Any(x => !string.IsNullOrEmpty(x)) ?? true))
                return query;
            
            // get all properties
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && search.SearchBy.Contains(p.Name, new IgnoreCaseComparer()))
                .Select(x => x.Name);

            // if there are no such properties, skip the search
            if (!properties.Any())
                return query;

            // get generic object
            var entity = Expression.Parameter(typeof(T), "entity");

            // get the Like Method from EF.Functions
            var efLikeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(DbFunctions), typeof(string), typeof(string) },
                null);

            // make a pattern for the search
            var pattern = Expression.Constant($"%{search.Search}%", typeof(string));

            // this will collect a single search request for all properties
            Expression body = Expression.Constant(false);

            foreach (var propertyName in properties)
            {
                // get property from our object
                var property = Expression.Property(entity, propertyName);

                // call the method with all the required arguments
                var expr = Expression.Call(efLikeMethod, Expression.Property(null, typeof(EF), nameof(EF.Functions)), property, pattern);

                // add to the main request
                body = Expression.OrElse(body, expr);
            }

            // compose and pass the expression to Where
            var expression = Expression.Lambda<Func<T, bool>>(body, entity);

            return query.Where(expression);
        }

        public static IQueryable<T> If<T>(this IQueryable<T> source, bool condition, Func<IQueryable<T>, IQueryable<T>> transform)
        { 
            return condition ? transform(source) : source;
        }
        
        public static IQueryable<T> If<T, P>(this IIncludableQueryable<T, P> source, bool condition, Func<IIncludableQueryable<T, P>, IQueryable<T>> transform)
            where T : class
        {
            return condition ? transform(source) : source;
        }

        public static IQueryable<T> If<T, P>(this IIncludableQueryable<T, IEnumerable<P>> source, bool condition, Func<IIncludableQueryable<T, IEnumerable<P>>, IQueryable<T>> transform)
            where T : class
        {
            return condition ? transform(source) : source;
        }
    }

    class IgnoreCaseComparer : IEqualityComparer<string>
    {
        public bool Equals([AllowNull] string x, [AllowNull] string y)
        {
            return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.GetHashCode();
        }
    }
}