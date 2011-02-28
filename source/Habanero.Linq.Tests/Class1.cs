using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Habanero.Base;
using Habanero.BO;
using Habanero.BO.ClassDefinition;
using Habanero.Smooth;
using NUnit.Framework;

namespace Habanero.Linq.Tests
{
    [TestFixture]
    public class Class1
    {

        [Test]
        public void SimpleSelect()
        {
            var mapper = new ClassAutoMapper(typeof (Person));
            IClassDef classDef = mapper.Map();
            ClassDef.ClassDefs.Add(classDef);

            string hsql = "select * from Person";

            SelectQuery query = DoMagic(hsql);

            Assert.AreEqual("select from Person", query.ToString());
        }

        private SelectQuery DoMagic(string hsql)
        {
            ICharStream stream = new ANTLRStringStream(scenarioText);
            ams_scenarioLexer lexer = new ams_scenarioLexer(stream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            ams_scenarioParser parser = new ams_scenarioParser(tokenStream);

            parser.scenario();
        }


        public class Person : BusinessObject<Person>
        {
            
        }

    }
}
