using System.Linq.Expressions;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Visitors
{
    internal class ExpressionRebinder: ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly Expression _member;

        public ExpressionRebinder(ParameterExpression parameter, Expression member)
        {
            _parameter = parameter;
            _member = member;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _parameter)
            {
                node = _member;
            }

            return base.Visit(node);
        }
    }
}
