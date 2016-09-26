using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EasyQuery.Models;

namespace EasyQuery.Helpers
{
    internal class ExpressionBuilder
    {
        public enum ExpressionOperators
        {
            Equals,
            GreaterThan,
            LessThan,
            GreaterThanOrEqual,
            LessThanOrEqual,
            Contains,
            StartsWith,
            EndsWith
        }


        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains");
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});

        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith",
            new[] {typeof(string)});


        public static Expression<Func<T, bool>> MakePredicate<T>(List<ExpressionModel> filters)
        {
            var item = Expression.Parameter(typeof(T), "item");
            var andBody =
                filters.Where(i => i.OperatorGroupsGroup == ExpressionModel.OperatorGroups.And)
                    .Select(filter => MakePredicate(item, filter))
                    .ToList();
            var orBody =
                filters.Where(i => i.OperatorGroupsGroup == ExpressionModel.OperatorGroups.Or)
                    .Select(filter => MakePredicate(item, filter))
                    .ToList();

            Expression body = null;

            foreach (
                var orExp in
                filters.Where(i => i.OperatorGroupsGroup == ExpressionModel.OperatorGroups.Or)
                    .GroupBy(i => i.PropertyName))
            {
                var orExpressions = new List<Expression>();

                orExpressions.AddRange(
                    filters.Where(i => i.PropertyName == orExp.Key).Select(filter => MakePredicate(item, filter)));

                body = body == null
                    ? orExpressions.Aggregate(Expression.Or)
                    : Expression.And(body, orExpressions.Aggregate(Expression.Or));
            }


            if (andBody.Any() && orBody.Any())
            {
                if (body != null) body = Expression.AndAlso(andBody.Aggregate(Expression.And), body);
            }
            else if (andBody.Any())
            {
                body = andBody.Aggregate(Expression.And);
            }
            if (body == null)
                return null;
            var predicate = Expression.Lambda<Func<T, bool>>(body, item);

            return predicate;
        }


        private static Expression MakePredicate(Expression item, ExpressionModel filter)
        {
            var member = Expression.Property(item, filter.PropertyName);
            var constant = Expression.Constant(filter.Value);

            switch (filter.Operator)
            {
                case ExpressionOperators.Equals:
                    return Expression.Equal(member, constant);
                case ExpressionOperators.GreaterThan:
                    return Expression.GreaterThan(member, constant);
                case ExpressionOperators.LessThan:
                    return Expression.LessThan(member, constant);
                case ExpressionOperators.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);
                case ExpressionOperators.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);
                case ExpressionOperators.Contains:
                    return Expression.Call(member, ContainsMethod, constant);
                case ExpressionOperators.StartsWith:
                    return Expression.Call(member, StartsWithMethod, constant);
                case ExpressionOperators.EndsWith:
                    return Expression.Call(member, EndsWithMethod, constant);
                default:
                    return null;
            }
        }
    }
}