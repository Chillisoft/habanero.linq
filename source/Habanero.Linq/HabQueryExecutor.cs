using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Text;
using Habanero.Base;
using Habanero.BO;
using Habanero.BO.ClassDefinition;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;

namespace Habanero.Linq
{
    public class HabQueryExecutor : IQueryExecutor
    {
        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ? ExecuteCollection<T>(queryModel).SingleOrDefault() : ExecuteCollection<T>(queryModel).Single();
        }

        IEnumerable<T> IQueryExecutor.ExecuteCollection<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel);
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            ISelectQuery selectQuery = HabQueryModelVisitor.GenerateSelectQuery(queryModel);
            var businessObjectCollection = BORegistry.DataAccessor.BusinessObjectLoader.GetBusinessObjectCollection(ClassDef.ClassDefs[typeof(T)], selectQuery);
            return (from object obj in businessObjectCollection select (T)obj).ToList();
        }
    }

    public class HabQueryModelVisitor : QueryModelVisitorBase
    {
        private ISelectQuery _selectQuery = new SelectQuery();

        public static ISelectQuery GenerateSelectQuery(QueryModel queryModel)
        {
            var visitor = new HabQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            return visitor.GetSelectQuery();
        }

        private ISelectQuery GetSelectQuery()
        {
            return _selectQuery;
        }

        private void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            // queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            // VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            _selectQuery.Criteria = GetCriteria(whereClause.Predicate);

            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            var source = _selectQuery.Source;
            QueryBuilder.PrepareSource(ClassDef.ClassDefs["Habanero.Linq.Tests", "Person"], ref source);
            _selectQuery.Source = source;

            base.VisitSelectClause(selectClause, queryModel);
        }

        private Criteria GetCriteria(Expression expression)
        {
            var criteriaString = CriteriaGeneratorExpressionTreeVisitor.GetCriteriaString(expression, _selectQuery);
            return CriteriaParser.CreateCriteria(criteriaString);
        }
    }


    public class CriteriaGeneratorExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        readonly Dictionary<ExpressionType, string> operators = new Dictionary<ExpressionType, string>()
                                                                    { 
                                                                        { ExpressionType.Equal, "=" },
                                                                        { ExpressionType.LessThan, "<" },
                                                                        { ExpressionType.LessThanOrEqual, "<=" },
                                                                        { ExpressionType.GreaterThan, ">" },
                                                                        { ExpressionType.GreaterThanOrEqual, ">=" },
                                                                        { ExpressionType.AndAlso, "AND" },
                                                                        { ExpressionType.And, "AND" },
                                                                        { ExpressionType.OrElse, "OR" },
                                                                        { ExpressionType.Or, "OR" },
                                                                        { ExpressionType.NotEqual, "<>" }
                                                                    };
        private readonly ISelectQuery _selectQuery;
        private readonly StringBuilder _criteriaString = new StringBuilder();

        private CriteriaGeneratorExpressionTreeVisitor(ISelectQuery selectQuery)
        {
            _selectQuery = selectQuery;
        }

        public static string GetCriteriaString(Expression linqExpression, ISelectQuery selectQuery)
        {
            var visitor = new CriteriaGeneratorExpressionTreeVisitor(selectQuery);
            visitor.VisitExpression(linqExpression);
            return visitor.GetCriteriaString();
        }

     
        public string GetCriteriaString()
        {
            return _criteriaString.ToString();
        }

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            _criteriaString.Append("(");
            VisitExpression(expression.Left);
            _criteriaString.Append(" " + operators[expression.NodeType] + " ");
            VisitExpression(expression.Right);
            _criteriaString.Append(")");
            return expression;
        }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            _criteriaString.Append(expression.Value);
            return expression;
        }

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            VisitExpression(expression.Expression);
            //_criteriaString.AppendFormat(".{0}", expression.Member.Name);
            _criteriaString.AppendFormat("{0}", expression.Member.Name);

            return expression;
        }

        protected override Expression VisitQuerySourceReferenceExpression(Remotion.Data.Linq.Clauses.Expressions.QuerySourceReferenceExpression expression)
        {
            //_criteriaString.Append(expression.ReferencedQuerySource.ItemName);
            return expression;
        }

        // Called when a LINQ expression type is not handled above.
        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            string itemText = FormatUnhandledItem(unhandledItem);
            var message = string.Format("The expression '{0}' (type: {1}) is not supported by this LINQ provider.", itemText, typeof(T));
            return new NotSupportedException(message);
        }

        private string FormatUnhandledItem<T>(T unhandledItem)
        {
            var itemAsExpression = unhandledItem as Expression;
            return itemAsExpression != null ? FormattingExpressionTreeVisitor.Format(itemAsExpression) : unhandledItem.ToString();
        }

    }
}