using System.Linq.Expressions;

namespace TicketNest.Shared.Expressions;

public static class PredicateExpressionsExtensions
{
    /// <summary>
    /// Генерирует expression, которое объединяет все переданные expression через AND
    /// exp1 && exp2 && exp3 и т.д.
    /// </summary>
    public static Expression<Func<T, bool>> CombineAnd<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        var parameter = Expression.Parameter(typeof(T));

        Expression? combined = null;
        foreach (var expression in expressions)
        {
            var replaced = ReplaceParameter(expression, parameter);
            if (replaced == null)
            {
                throw new ArgumentNullException($"Не удалось получить выражение для параметра {parameter.Name}");
            }

            combined = combined != null
                ? Expression.AndAlso(combined, replaced)
                : replaced;
        }

        if (combined == null)
        {
            return x => true;
        }

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    private static Expression? ReplaceParameter(LambdaExpression expression, Expression parameter)
    {
        var visitor = new CustomExpressionVisitor(expression.Parameters[0], parameter);
        return visitor.Visit(expression.Body);
    }

    private class CustomExpressionVisitor(Expression oldValue, Expression newValue) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            return node == oldValue
                ? newValue
                : base.Visit(node);
        }
    }
}