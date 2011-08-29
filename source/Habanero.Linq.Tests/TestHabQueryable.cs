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
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class TestHabQueryable
    {

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            ClassDef.ClassDefs.Clear();
            var typeSource = new AssemblyTypeSource(typeof (Person));
            ClassDef.ClassDefs.Add(new AllClassesAutoMapper(typeSource).Map());
        }

        [SetUp]
        public void Setup()
        {
            BORegistry.DataAccessor = new DataAccessorInMemory();
        }

        [Test]
        public void _FirstTestEver()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter");
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(1, b.ToList().Count);
        }

        [Test]
        public void WhereClause_Operator_Equals()
        {
            //---------------Set up test pack-------------------
            CreatePersons("Peter", "Bob", "Peter");
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where p.Name == "Peter"
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void WhereClause_Operator_GreaterThan()
        {
            //---------------Set up test pack-------------------
            CreatePersons(12, 15, 18);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where p.Age > 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(1, b.ToList().Count);
        }

        [Test]
        public void WhereClause_Operator_GreaterThanOrEqual()
        {
            //---------------Set up test pack-------------------
            CreatePersons(12, 15, 18);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where p.Age >= 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void WhereClause_Operator_LessThan()
        {
            //---------------Set up test pack-------------------
            CreatePersons(12, 15, 18);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where p.Age < 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(1, b.ToList().Count);
        }

        [Test]
        public void WhereClause_Operator_LessThanOrEqual()
        {
            //---------------Set up test pack-------------------
            CreatePersons(12, 15, 18);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where p.Age <= 15
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void WhereClause_Operator_NotEqual()
        {
            //---------------Set up test pack-------------------
            CreatePersons(12, 15, 18, 20);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where p.Age != 12
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(3, b.ToList().Count);
        }

        [Test]
        public void WhereClause_LogicalOperator_AndAlso()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12); CreatePerson("Peter", 15);
            CreatePerson("Peter", 18); CreatePerson("Bob", 13);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where (p.Age < 16 && p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void WhereClause_LogicalOperator_And()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12); CreatePerson("Peter", 15);
            CreatePerson("Peter", 18); CreatePerson("Bob", 13);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where (p.Age < 16 & p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(2, b.ToList().Count);
        }

        [Test]
        public void WhereClause_LogicalOperator_Or()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12); CreatePerson("Peter", 15);
            CreatePerson("Bob", 18); CreatePerson("Bob", 13);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where (p.Age < 14 | p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(3, b.ToList().Count);
        }

        [Test]
        public void WhereClause_LogicalOperator_OrElse()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12); CreatePerson("Peter", 15);
            CreatePerson("Bob", 18); CreatePerson("Bob", 13);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where (p.Age < 14 || p.Name == "Peter")
                    select p; 
            //---------------Test Result -----------------------
            Assert.AreEqual(3, b.ToList().Count);
        }

        [Test]
        public void SelectClause_SingleField()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12); CreatePerson("Peter", 15);
            CreatePerson("Bob", 18); CreatePerson("Bob", 13);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where (p.Age < 14)
                    select p.Name; 
            //---------------Test Result -----------------------
            var names = b.ToList();
            Assert.AreEqual(2, names.Count);
            CollectionAssert.Contains(names, "Peter");
            CollectionAssert.Contains(names, "Bob");
        }

        [Test]
        public void SelectClause_TwoFields_NoCalculations()
        {
            //---------------Set up test pack-------------------
            CreatePerson("Peter", 12); CreatePerson("Peter", 15);
            CreatePerson("Bob", 18); CreatePerson("Bob", 13);
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    where (p.Age < 14)
                    select new {p.Name, p.Age}; 
            //---------------Test Result -----------------------
            var peoples = b.ToList();
            Assert.AreEqual(2, peoples.Count);
            Assert.AreEqual("Bob", peoples[0].Name);
            Assert.AreEqual(13, peoples[0].Age);
            Assert.AreEqual("Peter", peoples[1].Name);
            Assert.AreEqual(12, peoples[1].Age);
        }

        [Test]
        public void OrderClause_OnProperty()
        {
            //---------------Set up test pack-------------------
            CreatePersons("Bob", "Peter", "Alice");
            //---------------Assert Precondition----------------
            var b = from p in MyDB.Persons
                    orderby p.Name
                    select p; 
            //---------------Execute Test ----------------------
            var people = b.ToList();
            //---------------Test Result -----------------------
            Assert.AreEqual("Alice", people[0].Name);
            Assert.AreEqual("Bob", people[1].Name);
            Assert.AreEqual("Peter", people[2].Name);
        }

        [Test]
        public void OrderClause_OnProperty_Descending()
        {
            //---------------Set up test pack-------------------
            CreatePersons("Bob", "Peter", "Alice");
            //---------------Assert Precondition----------------
            var b = from p in MyDB.Persons
                    orderby p.Name descending 
                    select p;
            //---------------Execute Test ----------------------
            var people = b.ToList();
            //---------------Test Result -----------------------
            Assert.AreEqual("Peter", people[0].Name);
            Assert.AreEqual("Bob", people[1].Name);
            Assert.AreEqual("Alice", people[2].Name);
        }

        [Test]
        public void JoinClause_SimpleCase()
        {
            //---------------Set up test pack-------------------
            CreatePersonsWithAddress("Bob","South Africa");
            CreatePersonsWithAddress("Paul","Zimbabwe");
            
            //---------------Execute Test ----------------------
            var b = from p in MyDB.Persons
                    join a in MyDB.Addresses on p.Address equals a
                    where a.Country == "South Africa"
                    select p;
            //---------------Test Result -----------------------
            var people = b.ToList();
            Assert.AreEqual(1,people.Count);
            Assert.AreEqual("Bob", people[0].Name);

        }

        private void CreatePersonsWithAddress(string name, string country)
        {
            var person = CreatePerson(name);
            var address = new Address();
            address.Country = country;
            person.Address = address;
            address.Save();

        }

        private Person CreatePerson(string name)
        {
            var p1 = new Person { Name = name }; p1.Save();
            return p1;
        }

        private void CreatePerson(int age)
        {
            var p1 = new Person { Age = age, Name = "Person1"}; p1.Save();
        }

        private void CreatePerson(string name, int age)
        {
            var p1 = new Person { Age = age, Name = name }; p1.Save();
        }

        private void CreatePersons(params string[] names)
        {
            names.ToList().ForEach(s => CreatePerson(s));
        }

        private void CreatePersons(params int[] ages)
        {
            ages.ToList().ForEach(CreatePerson);
        }


    }

    public static class MyDB
    {
        public static IQueryable<Person> Persons
        {
            get { return new HabQueryable<Person>(); }
        }

        public static IQueryable<Address> Addresses
        {
            get { return new HabQueryable<Address>(); }
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

        public Address Address
        {
            get { return this.Relationships.GetRelatedObject<Address>("Address"); }
            set { this.Relationships.SetRelatedObject("Address", value); }
        }
    }

    public class Address : BusinessObject<Address>
    {
        public string AddressLine
        {
            get { return this.GetPropertyValue(address => address.AddressLine); }
            set { this.SetPropertyValue(address => address.AddressLine, value); }
        }

        public string Country
        {
            get { return this.GetPropertyValue(address => address.Country); }
            set { this.SetPropertyValue(address => address.Country, value); }
        }

        public BusinessObjectCollection<Person> Persons
        {
            get { return this.Relationships.GetRelatedCollection<Person>("Persons"); }
        }
        
    }
}
