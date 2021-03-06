﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.Expressions;
using LinqToDB.SqlQuery;

namespace LinqToDB.Linq.Builder
{
	partial class TableBuilder
	{
#if !NET45
		private static MethodInfo _getArgumentMethodInfo =
			MemberHelper.MethodOf(() => ((FormattableString)null).GetArgument(0));
#endif

		static IBuildContext BuildRawSqlTable(ExpressionBuilder builder, BuildInfo buildInfo)
		{
			var methodCall = (MethodCallExpression)buildInfo.Expression;

			if (builder.MappingSchema.IsScalarType(methodCall.Method.GetGenericArguments()[0]))
				throw new LinqToDBException("Selection of scalar types not supported by FromSql method. Use mapping class with one column for scalar values");

			string                  format;
			IEnumerable<Expression> arguments;

			var sqlExpr = methodCall.Arguments[1];

			// Consider that FormattableString is used
			if (sqlExpr.NodeType == ExpressionType.Call)
			{
				var mc = (MethodCallExpression)sqlExpr;

				format    = (string)mc.Arguments[0].EvaluateExpression();
				arguments = ((NewArrayExpression)mc.Arguments[1]).Expressions;

			} 
			else
			{
				var evaluatedSql = sqlExpr.EvaluateExpression();
#if !NET45
				if (evaluatedSql is FormattableString formattable)
				{
					format    = formattable.Format;
					arguments = formattable.GetArguments().Select(Expression.Constant);
				}
				else
#endif
				{
					var rawSqlString = (RawSqlString)evaluatedSql;

					format        = rawSqlString.Format;
					var arrayExpr = methodCall.Arguments[2];
					var array     = (object[])arrayExpr.EvaluateExpression();
					arguments     = array.Select(Expression.Constant);
				}
			}

			var sqlArguments = arguments.Select(a => builder.ConvertToSql(buildInfo.Parent, a)).ToArray();

			return new RawSqlContext(builder, buildInfo, methodCall.Method.GetGenericArguments()[0], format, sqlArguments);
		}

		class RawSqlContext : TableContext	
		{
			public RawSqlContext(ExpressionBuilder builder, BuildInfo buildInfo, Type originalType, string sql, params ISqlExpression[] parameters) 
				: base(builder, buildInfo, new SqlRawSqlTable(builder.MappingSchema, originalType, sql, parameters))
			{
			}
		}
	}
}
