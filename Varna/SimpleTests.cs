using System;
using NUnit.Framework;

namespace Varna
{
    class SimpleTests
    {
        [Test]
        public void Disjunction()
        {
            var x = new Var("x");

            var exp = (x == ((Exp)3 | 9));
            var scope = Reader.Read(exp).Complete();
            
            // reading should bring all disjuncts to the top
            Assert.That(scope.Exp, Is.TypeOf<OrExp>());

            // and branches should be digested
            var or = (OrExp)scope.Exp;
            Assert.That(or.Left.Exp, Is.TypeOf<True>());
            Assert.That(or.Right.Exp, Is.TypeOf<True>());
            
            // and binds bubbled
            Assert.That(or.Left.Get("x").Raw(), Is.EqualTo(3));
            Assert.That(or.Right.Get("x").Raw(), Is.EqualTo(9));
        }
        
        
        [Test]
        public void IfThen()
        {
            var x = new Var("x");
            var y = new Var("y");

            var exp = (x == 3 & y == 9);
            var result = Reader.Read(exp).Complete();

            Assert.That(result.Exp, Is.TypeOf<True>());
            Assert.That(result.Get("x").Raw(), Is.EqualTo(3));
            Assert.That(result.Get("y").Raw(), Is.EqualTo(9));
        }

        [Test]
        public void IfNotThenNot()
        {
            var y = new Var("y");

            var exp = (new Never() & y == 9);
            var result = Reader.Read(exp).Complete();

            Assert.That(result.Exp, Is.TypeOf<Never>());
            Assert.That(result.Get("y").Exp, Is.TypeOf<Never>());
        }

        [Test]
        public void IfNotThenNot2()
        {
            var x = new Var("x");

            var exp = (x == 3 & new Never());
            var result = Reader.Read(exp).Complete();

            Assert.That(result.Exp, Is.TypeOf<Never>());
            Assert.That(result.Get("x").Exp, Is.TypeOf<Never>());
        }


        [Test]
        public void Assignment()
        {
            var x = new Var("x");

            var exp1 = (x == 7);

            var result = Reader.Read(exp1);
            Assert.That(result.Get("x").Raw(), Is.EqualTo(7));
        }

        [Test]
        public void Conjunction()
        {
            var x = new Var("x");
            var y = new Var("y");

            var exp1 = (x == 7 & y == 3);

            var result = Reader.Read(exp1);
            Assert.That(result.Get("x").Raw(), Is.EqualTo(7));
            Assert.That(result.Get("y").Raw(), Is.EqualTo(3));
        }

        [Test]
        public void And_Values()
        {
            var exp = ((Exp) 1 & 5);
            var result = Reader.Read(exp);
            Assert.That(result.Get().Exp, Is.TypeOf<Never>());
        }

        [Test]
        public void And_Values2()
        {
            var exp = ((Exp) 13 & 13);
            var result = Reader.Read(exp);
            Assert.That(result.Get().Exp, Is.TypeOf<Int>());
        }
    }
}