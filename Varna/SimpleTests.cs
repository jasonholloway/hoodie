using System.Collections.Immutable;
using NUnit.Framework;

namespace Varna
{
    using static Ops;
    
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
        public void Disjunctions_BubbleBinds()
        {
            var x = new Var("x");
            var y = new Var("y");

            var exp = (x == 3 & y == 1 | x == 3 & y == 2);
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<OrExp>());

            var or = (OrExp)scope.Exp;
            Assert.That(or.Left.Exp, Is.TypeOf<True>());
            Assert.That(or.Left.Get("x").Raw(), Is.EqualTo(3));
            Assert.That(or.Left.Get("y").Raw(), Is.EqualTo(1));
            
            Assert.That(or.Right.Exp, Is.TypeOf<True>());
            Assert.That(or.Right.Get("x").Raw(), Is.EqualTo(3));
            Assert.That(or.Right.Get("y").Raw(), Is.EqualTo(2));
            
            // only common binds bubble into the or
            Assert.That(scope.Get("x").Raw(), Is.EqualTo(3));
            Assert.That(scope.Get("y").Raw(), Is.Null);
        }
        
        [Test]
        public void Disjunctions_Flatten_SuperSimple()
        {
            var exp = (Exp)1 | (Exp)2 | (Exp)2 | (Exp)1 | (Exp)1 | (Exp)2;
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<OrExp>());
            var or1 = (OrExp)scope.Exp;
            Assert.That(or1.Left.Exp, Is.TypeOf<Int>());
            Assert.That(or1.Left.Raw(), Is.EqualTo(1));
            
            Assert.That(or1.Right.Exp, Is.TypeOf<Int>());
            Assert.That(or1.Right.Raw(), Is.EqualTo(2));
        }
        
        [Test]
        public void Disjunctions_Flatten_SimpleButOutOfOrder()
        {
            var exp = (Exp)1 | (Exp)2 | (Exp)1 | (Exp)3;
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<OrExp>());
            var or1 = (OrExp)scope.Exp;
            Assert.That(or1.Right.Exp, Is.TypeOf<Int>());
            Assert.That(or1.Right.Raw(), Is.EqualTo(3));
            
            Assert.That(or1.Left.Exp, Is.TypeOf<OrExp>());
            var or2 = (OrExp)or1.Left.Exp;
            Assert.That(or2.Left.Exp, Is.TypeOf<Int>());
            Assert.That(or2.Left.Raw(), Is.EqualTo(2));
            Assert.That(or2.Right.Exp, Is.TypeOf<Int>());
            Assert.That(or2.Right.Raw(), Is.EqualTo(1));
        }
        
        [Test]
        public void Disjunctions_Flatten_SimpleButDeeplyOutOfOrder()
        {
            var exp = (Exp) 1 | (Exp) 2 | (Exp) 1 | (Exp) 3 | (Exp) 2;
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<OrExp>());
            var or1 = (OrExp)scope.Exp;
            Assert.That(or1.Right.Exp, Is.TypeOf<Int>());
            Assert.That(or1.Right.Raw(), Is.EqualTo(3));
            
            Assert.That(or1.Left.Exp, Is.TypeOf<OrExp>());
            var or2 = (OrExp)or1.Left.Exp;
            Assert.That(or2.Left.Exp, Is.TypeOf<Int>());
            Assert.That(or2.Left.Raw(), Is.EqualTo(2));
            Assert.That(or2.Right.Exp, Is.TypeOf<Int>());
            Assert.That(or2.Right.Raw(), Is.EqualTo(1));
        }
        
        [Test]
        public void Disjunctions_Flatten()
        {
            var x = new Var("x");
            var y = new Var("y");

            var exp = ((x == (1 | (1 | 2))) | (x == (1 | 2)) | x == 1 & (x == 1));
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<OrExp>());
            
            var or = (OrExp)scope.Exp;
            Assert.That(scope.Get("x").Raw(), Is.Null);

            Assert.That(or.Left.Exp, Is.TypeOf<True>());
            Assert.That(or.Left.Get("x").Raw(), Is.EqualTo(1));
            
            Assert.That(or.Right.Exp, Is.TypeOf<True>());
            Assert.That(or.Right.Get("x").Raw(), Is.EqualTo(2));
        }


        [Test]
        public void CompareTest()
        {
            var x = new Var("x");
            var y = new Var("y");

            var s1 = x == 3 & y == 2;
            var s2 = x == 3 & y == 2;
            
            Assert.That(s1, Is.EqualTo(s2).Using(ScopeComparer.Exp));
        }


        [Test]
        public void Disjunctions_SimplifiesNevers()
        {
            var x = new Var("x");

            var exp = (x == 3 & x == 1 | x == 4 & x == 4);
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<True>());
            // Assert.That(scope.Get("x").Raw(), Is.EqualTo(4));
        }
        
        [Test]
        public void BadConjunction()
        {
            var x = new Var("x");

            var exp = (x == 3 & x == 1);
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<Never>());
            Assert.That(scope.Binds, Is.Empty);
        }
        
        [Test]
        public void Conjunction_Annihilable()
        {
            var x = new Var("x");

            var exp = (x == 3 & Never());
            var scope = Reader.Read(exp).Complete();
            
            Assert.That(scope.Exp, Is.TypeOf<Never>());
            Assert.That(scope.Binds, Is.Empty);
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
        
        // There's a problem with going depth first: it's thorough but inefficient
        // - could do lazy Ands
        // - read by iterations

        [Test]
        public void Conjunction()
        {
            var x = new Var("x");
            var y = new Var("y");

            var exp1 = (x == 7 & y == 3);

            var result = Reader.Read(exp1).Complete();
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