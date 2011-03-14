using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Remotion.Data.Linq;

namespace Habanero.Linq
{
        /// <summary>
        /// Provides the main entry point to a LINQ query.
        /// </summary>
        public class HabQueryable<T> : QueryableBase<T>
        {
            // This constructor is called by our users
            public HabQueryable()
                : base(new HabQueryExecutor())
            {
            }

            // This constructor is called indirectly by LINQ's query methods, just pass to base.
            public HabQueryable(IQueryProvider provider, Expression expression)
                : base(provider, expression)
            {
            }
        }
}
