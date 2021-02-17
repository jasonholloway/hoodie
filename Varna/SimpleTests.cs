using NUnit.Framework;

namespace Varna
{
    class SimpleTests
    {
        [Test]
        public void Disjunction()
        {
            var x = new Var("x");

            var exp = (x == 3 | 9);
            var scope = Reader.Read(exp).Complete();
            
            // reading should bring all disjuncts to the top
            // because it will in effect be doing backtracking
            // if all disjuncts are at the top then we can enumerate bindings

            Assert.That(scope.Exp, Is.TypeOf<True>());
            Assert.That(scope.Get("x").Raw(), Is.EqualTo(3));
            Assert.That(scope.Get("y").Raw(), Is.EqualTo(9));
        }
        
        // it's like bindings only come into play when we've squashed together the graph
        // either there's an expression, or there's a value and a binding
        // but the value is itself an expression
        // 
        // it's an or, or it's an and, or its a nested expression doing something else
        // layers of interpretation
        // an Or has no need of Bindings
        // whereas in an And, bindings do make sense
        // in fact it is the duty of the And to combine all bindings, and if there are any Nevers, to discount the lot
        // (I like the thought of the Or being distinct and at the head of the pack - not just a binary exp)
        // 
        // an assignment is simply readable into a binding
        // would an And even ever retain its structure?
        //
        // vague pre-returned types have to be consumed properly when we do dredge them out
        // the structure should stay then, though we can offer simpified versions up front
        // the proper structure lives on in the background via the More functions
        // while on top things are simpler, with simple leafexps and bindings
        //
        // a leafexp and a binding is a happy simple thing, it's own kind of node
        // Ors then don't have any bindings; and no Mores either; as the structure itself is there
        // if there is a structured exp, we don't need no Mores and no Binds!!!   <<<<<<<<<<<<<<<<<<<<<<<<<<< THIS!
        //
        // but if we have a simple Exp, with a bind, a structure of truth can still lurk behind via its More continuation
        // 
        // Binds and Mores are then not normal, but belong only to a certain type of node
        // A Context has a Bind and a More and a simple Exp 
        // it can only be the simplest Exp to have a bind next to it...
        // as has been said, the structure that we felt we should retain, still sits there, but only in the background
        //
        //
        
        
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