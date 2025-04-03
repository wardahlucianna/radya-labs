using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Extensions
{
    public static class EnumerableExtension
    {
        // public static IQueryable<T> OrderByDynamic<T, TOrderBy>(this IQueryable<T> query, TOrderBy orderBy, IDictionary<string, string> alias = null, char nestedSeparator = '.')
        //     where T : class
        //     where TOrderBy : IOrderBy
        // {
        //     // if there are no such order by, skip the order
        //     if (string.IsNullOrEmpty(orderBy.OrderBy))
        //         return query;
            
        //     // if any alias, find order by using alias
        //     if (alias != null && alias.TryGetValue(orderBy.OrderBy, out var by))
        //         orderBy.OrderBy = by;
            
        //     // dynamically creates a call like this: query.OrderBy(p =&gt; p.SortColumn)
        //     var parameter = Expression.Parameter(typeof(T), "p");
            
        //     // set order wether Ascending or Descending
        //     var command = orderBy.OrderType == OrderType.Asc ? "OrderBy" : "OrderByDescending";

        //     // recursively find properties
        //     var nestedPropertyName = orderBy.OrderBy.Split(nestedSeparator).Select(x => x.ToUpperFirst());
        //     var property = GetPropertyFrom(typeof(T), nestedPropertyName)
        //         ?? throw new ArgumentException($"Field {orderBy.OrderBy} or {orderBy.OrderBy.ToUpperFirst()} is not found", nameof(orderBy));

        //     // this is the part p.SortColumn
        //     var propertyAccess = Expression.MakeMemberAccess(parameter, property);

        //     // this is the part p =&gt; p.SortColumn
        //     var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        //     // finally, call the "OrderBy" / "OrderByDescending" method with the order by lamba expression
        //     var resultExpression = Expression.Call(
        //         typeof(Queryable), command, new Type[] { typeof(T), property.PropertyType }, 
        //         query.Expression, Expression.Quote(orderByExpression));

        //     return query.Provider.CreateQuery<T>(resultExpression);
        // }

        public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> filter, Func<TSource, int, TResult> selector)
        {
            var index = -1;
            foreach (var s in source)
            {
                checked { ++index; }
                if (filter(s))
                    yield return selector(s, index);
            }
        }

        public static IEnumerable<T> If<T>(this IEnumerable<T> source, bool condition, Func<IEnumerable<T>, IEnumerable<T>> transform)
        { 
            return condition ? transform(source) : source;
        }

        public static IQueryable<T> OrderByDynamic<T, TOrderBy>(this IQueryable<T> query, TOrderBy orderBy, IDictionary<string, string> alias = null)
            where T : class
            where TOrderBy : IOrderBy
        {
            // if there are no such order by, skip the order
            if (string.IsNullOrEmpty(orderBy.OrderBy))
                return query;

            // if any alias, find order by using alias
            if (alias != null && alias.TryGetValue(orderBy.OrderBy, out var by))
                orderBy.OrderBy = by;
            
            // make first letter upper case
            orderBy.OrderBy = orderBy.OrderBy.Contains('.')
                ? orderBy.OrderBy.Split('.').Select(x => x.ToUpperFirst()).Aggregate((x, y) => x + '.' + y)
                : orderBy.OrderBy.ToUpperFirst();

            query = orderBy.OrderType == OrderType.Asc
                ? ApplyOrder(query, orderBy.OrderBy, "OrderBy")
                : ApplyOrder(query, orderBy.OrderBy, "OrderByDescending");

            return query;
        }

        public static IOrderedQueryable<T> ThenByDynamic<T, TOrderBy>(this IOrderedQueryable<T> query, TOrderBy orderBy, IDictionary<string, string> alias = null)
            where T : class
            where TOrderBy : IOrderBy
        {
            // if there are no such order by, skip the order
            if (string.IsNullOrEmpty(orderBy.OrderBy))
                return query;
            
            // if any alias, find order by using alias
            if (alias != null && alias.TryGetValue(orderBy.OrderBy, out var by))
                orderBy.OrderBy = by;
            
            // make first letter upper case
            orderBy.OrderBy = orderBy.OrderBy.Contains('.')
                ? orderBy.OrderBy.Split('.').Select(x => x.ToUpperFirst()).Aggregate((x, y) => x + '.' + y)
                : orderBy.OrderBy.ToUpperFirst();

            query = orderBy.OrderType == OrderType.Asc
                ? ApplyOrder(query, orderBy.OrderBy, "ThenBy")
                : ApplyOrder(query, orderBy.OrderBy, "ThenByDescending");

            return query;
        }
        
        public static IEnumerable<T>[] ChunkBy<T>(this IEnumerable<T> source, int chunkSize) 
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value))
                .ToArray();
        }

        public static IEnumerable<TResult> ZipLongest<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> func, TFirst padder1 = default, TSecond padder2 = default)
        {
            var firstExp = first.Concat(Enumerable.Repeat(padder1, Math.Max(second.Count() - first.Count(), 0)));
            var secExp = second.Concat(Enumerable.Repeat(padder2, Math.Max(first.Count() - second.Count(), 0)));
            
            return firstExp.Zip(secExp, func);
        }

        public static IEnumerable<T> SetPagination<T, TGetAll>(this IEnumerable<T> query, TGetAll pagination)
            where T : class
            where TGetAll : IGetAll
        {
            return pagination.GetAll.HasValue && pagination.GetAll.Value
                ? query
                : query.Skip(pagination.CalculateOffset()).Take(pagination.Size);
        }

        public static IQueryable<T> SetPagination<T, TGetAll>(this IQueryable<T> query, TGetAll pagination)
            where T : class
            where TGetAll : IGetAll
        {
            return pagination.GetAll.HasValue && pagination.GetAll.Value
                ? query
                : query.Skip(pagination.CalculateOffset()).Take(pagination.Size);
        }

        public static IQueryable<T> SelectSelf<T>(this IQueryable<T> query, params string[] memberNames)
        {
            return query.Select(memberNames.SelectSelfExpression<T>());
        }

        public static Expression<Func<T, T>> SelectSelfExpression<T>(this IEnumerable<string> members, char nestedSeparator = '.')
        {
            if (!members.Any())
                members = typeof(T).GetProperties().Select(x => x.Name);

            // input parameter "o"
            var xParameter = Expression.Parameter(typeof(T), "o");

            // new statement "new Data()"
            var xNew = Expression.New(typeof(T));

            // create initializers
            var bindings = members
                .Select(o =>
                {
                    // property "Field1"
                    // var xProp = typeof(T).GetProperty(o);
                    var nestedProperty = o.Split(nestedSeparator);
                    var xProp = GetPropertyFrom(typeof(T), o.Split(nestedSeparator).Select(x => x.ToUpperFirst()));

                    // original value "o.Field1"
                    var xOriginal = Expression.Property(xParameter, xProp);

                    // set value "Field1 = o.Field1"
                    return Expression.Bind(xProp, xOriginal);
                });

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            return Expression.Lambda<Func<T, T>>(xInit, xParameter);
        }
        
        private static PropertyInfo GetPropertyFrom(Type baseType, IEnumerable<string> splittedPropertyNames)
        {
            return splittedPropertyNames.Count() switch
            {
                0 => null,
                1 => baseType.GetProperty(splittedPropertyNames.Last()),
                _ => GetPropertyFrom(baseType.GetProperty(splittedPropertyNames.First()).PropertyType, splittedPropertyNames.Skip(1).ToArray())
            };
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> query, string propertyName, string methodName)
        {
            var props = propertyName.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                if (pi != null)
                {
                    expr = Expression.Property(expr, pi);
                    type = pi.PropertyType;
                }
                    
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods()
                .Single(method 
                    => method.Name == methodName
                    && method.IsGenericMethodDefinition
                    && method.GetGenericArguments().Length == 2
                    && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { query, lambda });

            return (IOrderedQueryable<T>)result;
        }
    }
}
