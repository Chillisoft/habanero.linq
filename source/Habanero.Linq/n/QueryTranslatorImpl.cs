using System;
using System.Collections;
using System.Collections.Generic;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Iesi.Collections.Generic;

using NHibernate.Engine;
using NHibernate.Engine.Query;
using NHibernate.Hql.Ast.ANTLR.Exec;
using NHibernate.Hql.Ast.ANTLR.Loader;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NHibernate.Hql.Ast.ANTLR.Util;
using NHibernate.Param;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Hql.Ast.ANTLR
{
	

        //private HqlSqlTranslator Analyze(string collectionRole)
        //{
        //    var translator = new HqlSqlTranslator(_stageOneAst, this, _factory, _tokenReplacements, collectionRole);

        //    translator.Translate();

        //    return translator;
        //}


    public class HqlParseEngine
	{
		private readonly string _hql;
		private CommonTokenStream _tokens;
		private readonly bool _filter;

		public HqlParseEngine(string hql, bool filter)
		{
			_hql = hql;
			_filter = filter;
		}

		public IASTNode Parse()
		{
            // Parse the query string into an HQL AST.
            var lex = new HqlLexer(new CaseInsensitiveStringStream(_hql));
            _tokens = new CommonTokenStream(lex);

		    var parser = new HqlParser(_tokens) {TreeAdaptor = new ASTTreeAdaptor(), Filter = _filter};

		   
            try
            {
                var ast = (IASTNode) parser.statement().Tree;

                var walker = new NodeTraverser(new ConstantConverter());
                walker.TraverseDepthFirst(ast);

                return ast;
            }
            finally
            {
                parser.ParseErrorHandler.ThrowQueryException();
            }
		}

		class ConstantConverter : IVisitationStrategy
		{
			public void Visit(IASTNode node)
			{
			    node.Text = "MYCONSTANT";
			}
		}
	}

    internal class HqlSqlTranslator
	{
		private readonly IASTNode _inputAst;
		private readonly IDictionary<string, string> _tokenReplacements;
		private readonly string _collectionRole;
		private IStatement _resultAst;

		public HqlSqlTranslator(IASTNode ast, IDictionary<string, string> tokenReplacements, string collectionRole)
		{
			_inputAst = ast;
			_tokenReplacements = tokenReplacements;
			_collectionRole = collectionRole;
		}

		public IStatement SqlStatement
		{
			get { return _resultAst; }
		}

		public IStatement Translate()
		{
            if (_resultAst == null)
            {
                var nodes = new HqlSqlWalkerTreeNodeStream(_inputAst);

                var hqlSqlWalker = new HqlSqlWalker(_qti, _sfi, nodes, _tokenReplacements, _collectionRole);
                hqlSqlWalker.TreeAdaptor = new HqlSqlWalkerTreeAdaptor(hqlSqlWalker);

                try
                {
                    // Transform the tree.
                    _resultAst = (IStatement) hqlSqlWalker.statement().Tree;
                }
                finally
                {
                    hqlSqlWalker.ParseErrorHandler.ThrowQueryException();
                }
            }

		    return _resultAst;
		}
	}

	internal class HqlSqlGenerator
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof(HqlSqlGenerator));

		private readonly IASTNode _ast;
		private readonly ISessionFactoryImplementor _sfi;
		private SqlString _sql;
		private IList<IParameterSpecification> _parameters;

		public HqlSqlGenerator(IStatement ast, ISessionFactoryImplementor sfi)
		{
			_ast = (IASTNode)ast;
			_sfi = sfi;
		}

		public SqlString Sql
		{
			get { return _sql; }
		}

		public IList<IParameterSpecification> CollectionParameters
		{
			get { return _parameters; }
		}

		public SqlString Generate()
		{
            if (_sql == null)
            {
                var gen = new SqlGenerator(_sfi, new CommonTreeNodeStream(_ast));

                try
                {
                    gen.statement();

                    _sql = gen.GetSQL();

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("SQL: " + _sql);
                    }
                }
                finally
                {
                    gen.ParseErrorHandler.ThrowQueryException();
                }

                _parameters = gen.GetCollectedParameters();
            }

		    return _sql;
		}
	}
}
