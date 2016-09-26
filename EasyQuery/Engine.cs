using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using EasyQuery.Helpers;
using EasyQuery.Models;

namespace EasyQuery
{
    public class Engine : IDisposable
    {
        private readonly DbContext _db;

        public Engine(DbContext db)
        {
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        ///     Queries the current request for object properties. Sorts by primary by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> Query<T>() where T : class
        {
            var queryParameters = QueryBuilder.ParseQuery();

            var expressionModels = new List<ExpressionModel>();

            //Build up where clause if search matrix is not null
            if (queryParameters?.SearchMatrix != null)
                foreach (var query in queryParameters.SearchMatrix)
                {
                    if (query == null) continue;

                    for (var i = 1; i < query.Length; i++)
                    {
                        if (query[i] == null) continue;

                        var propertyType =
                            typeof(T).GetProperty(query[0],
                                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)?.PropertyType;

                        if (propertyType == null)
                            throw new Exception("Invalid Query Parameter in URL");
                        var searchValue = query[i];

                        var expressionModel = new ExpressionModel
                        {
                            Operator = ExpressionBuilder.ExpressionOperators.Contains,
                            OperatorGroupsGroup = ExpressionModel.OperatorGroups.Or,
                            Value = searchValue,
                            PropertyName = query[0]
                        };
                        //Correctly parse out datetimes and numbers
                        if ((propertyType == typeof(int)) || (propertyType == typeof(double)) ||
                            (propertyType == typeof(decimal)) || (propertyType == typeof(DateTime)) ||
                            (propertyType == typeof(Guid)) ||
                            (propertyType == typeof(bool)) || (propertyType.BaseType == typeof(Enum)))
                            try
                            {
                                if ((query.Length == 3) && (propertyType.BaseType != typeof(Enum)))
                                {
                                    expressionModel.OperatorGroupsGroup = ExpressionModel.OperatorGroups.And;
                                    if (i == 1)
                                        expressionModel.Operator =
                                            ExpressionBuilder.ExpressionOperators.GreaterThanOrEqual;
                                    if (i == 2)
                                        expressionModel.Operator = ExpressionBuilder.ExpressionOperators.LessThanOrEqual;
                                }
                                else
                                {
                                    expressionModel.Operator = ExpressionBuilder.ExpressionOperators.Equals;
                                }
                                expressionModel.Value =
                                    TypeDescriptor.GetConverter(propertyType).ConvertFromInvariantString(searchValue);

                                if (propertyType == typeof(DateTime))
                                    if (expressionModel.Value != null)
                                        expressionModel.Value = ((DateTime) expressionModel.Value).ToUniversalTime();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Unable to parse URL query properly.", ex);
                            }
                        else if (propertyType != typeof(string))
                            continue;
                        expressionModels.Add(expressionModel);
                    }
                }
            //Build out predicate with expression models
            var expression = ExpressionBuilder.MakePredicate<T>(expressionModels);

            //Get queryable set
            var querySet = _db.Set<T>().AsQueryable();

            if (expression != null)
                querySet = querySet.Where(expression);

            //Sort Query
            var orderSuccess = false;

            if (queryParameters?.Sort.Length > 0)
                for (var x = 0; x < queryParameters.Sort.Length; x++)
                {
                    var property =
                        typeof(T).GetProperties()
                            .SingleOrDefault(
                                i =>
                                    string.Equals(i.Name, queryParameters.Sort[x],
                                        StringComparison.CurrentCultureIgnoreCase))?.Name;

                    if (property == null)
                        continue;
                    querySet = querySet.Order(property,
                        queryParameters.SortAscending[x] ? SortDirection.Ascending : SortDirection.Descending, x != 0);
                    orderSuccess = true;
                }
            //If no order/sort has been applied, do a default sort on primary keys.
            if (!orderSuccess)
            {
                //Find PKs
                var keyNames =
                    (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>()
                        .EntitySet.ElementType.KeyMembers.Select(k => k.Name)
                        .ToArray();
                //Do sort
                for (var x = 0; x < keyNames.Length; x++)
                {
                    var property =
                        typeof(T).GetProperties()
                            .SingleOrDefault(
                                i => string.Equals(i.Name, keyNames[x], StringComparison.CurrentCultureIgnoreCase))?
                            .Name;
                    querySet = querySet.Order(property, SortDirection.Ascending, x != 0);
                }
            }

            return querySet;
        }
    }
}