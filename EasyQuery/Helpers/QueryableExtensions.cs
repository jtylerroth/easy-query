using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;

namespace EasyQuery.Helpers
{
    internal static class QueryableExtensions
    {
        //Credit: http://how-to-code-net.blogspot.ro/2014/04/how-to-call-for-dynamic-orderby-method.html
        /// <summary>
        ///     Orders an Iqueryable based on property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <param name="descending"></param>
        /// <param name="anotherLevel"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> Order<T>(this IQueryable<T> source, string propertyName,
            SortDirection descending, bool anotherLevel = false)
        {
            var param = Expression.Parameter(typeof(T), string.Empty);
            var property = Expression.PropertyOrField(param, propertyName);
            var sort = Expression.Lambda(property, param);

            var call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") +
                (descending == SortDirection.Descending ? "Descending" : string.Empty),
                new[] {typeof(T), property.Type},
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<T>) source.Provider.CreateQuery<T>(call);
        }
    }
}