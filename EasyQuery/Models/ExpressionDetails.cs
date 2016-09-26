using EasyQuery.Helpers;

namespace EasyQuery.Models
{
    internal class ExpressionModel
    {
        public enum OperatorGroups
        {
            And,
            Or
        }

        public string PropertyName { get; set; }
        public ExpressionBuilder.ExpressionOperators Operator { get; set; }
        public object Value { get; set; }

        public OperatorGroups OperatorGroupsGroup { get; set; }
    }
}