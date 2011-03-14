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
    public class TestHabQueryable
    {
        private IClassDef _personClassDef;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            ClassDef.ClassDefs.Clear();
            var mapper = new ClassAutoMapper(typeof(Person));
            _personClassDef = mapper.Map();
            ClassDef.ClassDefs.Add(_personClassDef);
        }

        [SetUp]
        public void Setup()
        {
            BORegistry.DataAccessor = new DataAccessorInMemory();
        }

        //[Test]
        //[Ignore]
        //public void SimpleSelect()
        //{
        //    string hsql = "select * from Person";

        //    ISelectQuery query = DoMagic(hsql);
        //    Assert.AreSame(_personClassDef, query.ClassDef);
        //    Assert.IsNull(query.Criteria);
        //}

        //private ISelectQuery DoMagic(string hsql)
        //{
        //    ICharStream stream = new ANTLRStringStream(hsql);
        //    HqlLexer lexer = new HqlLexer(stream);
        //    ITokenStream tokenStream = new CommonTokenStream(lexer);
        //    HqlParser parser = new HqlParser(tokenStream) {TreeAdaptor = new ASTTreeAdaptor(), Filter = true};

        //    HqlParser.statement_return statementReturn = parser.statement();
        //    ASTNode tree = (ASTNode) statementReturn.Tree;

        //    IASTNode queryNode = tree.GetChild(0);

            
        //    IASTNode selectNode = queryNode.GetChild(1);
            
        //    IASTNode fromNode = queryNode.GetChild(0);
        //    IASTNode fromRangeNode = fromNode.GetChild(0);
        //    IASTNode classNode = fromRangeNode.GetChild(0);
        //    string className = classNode.Text;

        //     MyDB db = new MyDB();
        //    var result = from p in db.Persons
        //                 where p.Name == "Peter"
        //                 select p;

        //    IClassDef classDef = ClassDef.ClassDefs[typeof(Person).Assembly.GetName().Name, className];
        //    return QueryBuilder.CreateSelectQuery(classDef);
        //}

        [Test]
        public void Test_SimpleLinqQuery()
        {
            //---------------Set up test pack-------------------
            Person p1 = new Person();
            p1.Name = "Peter";
            p1.Save();
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(1, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_Operator_Equals()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter");
            CreatePerson("Bob");
            CreatePerson("Peter");
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where p.Name == "Peter"
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_Operator_GreaterThan()
        {
            //---------------Set up test pack-------------------
            CreatePerson(12);
            CreatePerson(15);
            CreatePerson(18);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where p.Age > 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(1, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_Operator_GreaterThanOrEqual()
        {
            //---------------Set up test pack-------------------
            CreatePerson(12);
            CreatePerson(15);
            CreatePerson(18);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where p.Age >= 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_Operator_LessThan()
        {
            //---------------Set up test pack-------------------
            CreatePerson(12);
            CreatePerson(15);
            CreatePerson(18);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where p.Age < 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(1, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_Operator_LessThanOrEqual()
        {
            //---------------Set up test pack-------------------
            CreatePerson(12);
            CreatePerson(15);
            CreatePerson(18);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where p.Age <= 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_Operator_NotEqual()
        {
            //---------------Set up test pack-------------------
            CreatePerson(12);
            CreatePerson(15);
            CreatePerson(18);
            CreatePerson(20);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where p.Age != 12
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(3, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_LogicalOperator_AndAlso()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12);
            CreatePerson("Peter", 15);
            CreatePerson("Peter", 18);
            CreatePerson("Bob", 13);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where (p.Age < 16 && p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_LogicalOperator_And()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12);
            CreatePerson("Peter", 15);
            CreatePerson("Peter", 18);
            CreatePerson("Bob", 13);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where (p.Age < 16 & p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_LogicalOperator_Or()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12);
            CreatePerson("Peter", 15);
            CreatePerson("Bob", 18);
            CreatePerson("Bob", 13);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where (p.Age < 14 | p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(3, b.ToList().Count);
        }

        [Test]
        public void Test_WhereClause_LogicalOperator_OrElse()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12);
            CreatePerson("Peter", 15);
            CreatePerson("Bob", 18);
            CreatePerson("Bob", 13);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where (p.Age < 14 || p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(3, b.ToList().Count);
        }

        [Test]
        public void Test_SelectClause()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12);
            CreatePerson("Peter", 15);
            CreatePerson("Bob", 18);
            CreatePerson("Bob", 13);
            MyDB db = new MyDB();
            //---------------Execute Test ----------------------
            var b = from p in db.Persons
                    where (p.Age < 14)
                    select p.Name; 
            //---------------Test Result -----------------------
            var names = b.ToList();
            Assert.AreEqual(2, names.Count);
            CollectionAssert.Contains("Peter", names);
            CollectionAssert.Contains("Bob", names);
        }

        private void CreatePerson(string name)
        {
            var p1 = new Person { Name = name }; p1.Save();
        }

        private void CreatePerson(int age)
        {
            var p1 = new Person { Age = age, Name = "Person1"}; p1.Save();
        }

        private void CreatePerson(string name, int age)
        {
            var p1 = new Person { Age = age, Name = name }; p1.Save();
        }


    }

    public class MyDB
    {
        public IQueryable<Person> Persons
        {
            get { return new HabQueryable<Person>(); }
        }
    }

    public class Person : BusinessObject<Person>
    {
        public string Name
        {
            get { return this.GetPropertyValue(person => person.Name); }
            set { this.SetPropertyValue(person => person.Name, value); }
        }      
        public int Age
        {
            get { return this.GetPropertyValue(person => person.Age); }
            set { this.SetPropertyValue(person => person.Age, value); }
        }
    }
}
