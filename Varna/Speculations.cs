using System;

namespace Varna
{
    class SpeculativeTests 
    {

        public void Disjuncts()
        {
            var x = new Var("x");
            var y = new Var("x");

            var x1 = (x == 3 | 4 | 5);
            var x2 = (x == 3 | x == 4 | x == 5);




        }
        
        
        
        
        // [Test]
        public void TestyTest()
        {
            var x = new Var("x");
            var y = new Var("y");
            
            var prog =
                (x == 13)
                | (x == 7) & (y == 1);
            
            
            var exp4 =
                (x == 13|7)
                & (x == 13 | y == 1);
            
            var exp5 =
                (x==13 | x==7)
                & (x==13 | y==1);

            var exp6 =
                x == 13
                | x == 13 & y == 1 
                | x == 7 & y == 1;
            
            
            var exp7 =
                ( x == 1 | 2 | 3 | 4 | 5 | 6
                    & y == 1 | 2 | 3
                    & x > y
                )
                | y == 7;

            
            
            Console.WriteLine("boo");
        }
        
        // bringing all choices to the top is just a case of 
        // going through the graph and making choices one by one
        // each time we find a choice, we either choose left or right
        //
        //
        // though a graph like above includes all information, the actual reading of the graph must accumulate a context 
        // but an uninterpreted graph is just a graph with an empty context; a lazy structure still to be fully read
        //
        // 
        //
        //
        //
        
        
        
        // maybe it's always possible to bring all the choices to the top
        // so all we can do shuffle relations around based on movements we know are legal
        // as we shuffle them around we will also find lobes that locally don't make sense
        // and can be immediately dropped
        //
        // 
        //
        //
        //
        //
        
        
        
        
        
        //
        // but first we want to minimize the graph, and remove any impossibilities that can never survive its internal tensions
        // once this is done, we want to be able to make simple stepwise choices pertaining to single variables
        // 
        // what x's can be? how can we separate that out, given a tangled graph?
        // by crawling through the graph and trying all the paths
        //
        // as you come out of a visit, you return with you a fractured context; choices should be at the top
        
        
        
        
        
        
        //
        // the structure of the expression is the structure of the expression, end of
        // all the information is in it, pristine
        //
        // when we operate on it, what are we doing?
        // well, to list the particular values of a certain variable (its choices) we are rearranging the graph
        // the above graph is already in the perfect form for enumerating 'x'
        //
        //

        
        
        
        
        
        // so an Exp reads itself? I'm not sure how that can be true given lazy resolutions
        // it seems like the expression tree should always be retained anyway
        // and then through reading it we build up the bind context
        //
        // 

        
        
        
        public void Test1()
        {
            var x = new Var("x");
            var y = new Var("y");
            
            var prog = Ops.And(
                Ops.Bind(x, 13),
                Ops.Bind(y, 2),
                Ops.Or(
                    Ops.Bind(y, 2),
                    Ops.Bind(x, Ops.Or(666, 13))
                )
            );
        }

        
        
        

        // [Test]
        public void Test2()
        {
            var x = new Var("x");
            var y = new Var("y");
            
            var exp1 = Ops.Or(
                Ops.Bind(x, 13),
                Ops.Bind(x, 7)
            );
            //(x=13) v (x=7)
            //x=(13 v 7)
            
            var exp2 = Ops.Or(
                Ops.Bind(x, 13),
                Ops.And(Ops.Bind(x, 7), Ops.Bind(y, 1))
            );
            //(x=13) v ((x=7) ^ (y=1))
            //x=(13 v 7) ^ (y=1 v x=13) 
            //

            var exp3 =
                (x == 13)
                | (x == 7) & (y == 1);
            
            
            var exp4 =
                (x == 13|7)
                & (x == 13 | y == 1);
            

            var exp5 =
                ( x == 1 | 2 | 3 | 4 | 5 | 6
                    & y == 1 | 2 | 3
                    & x > y
                )
                | y == 7;


            
            var exp6 = x > 10;
            // reading the above, no choices to make
            // `>` would make x 'comparable' but nothing more
            // and we'd know that numbers are comparable, and that 2 is a number, so...
            //
            // the reader would separate out all disjunctions, would absorb allof them, thus making its work of deduction easier
            // all choices would be preserved, but at the top level (all expressions have a hierarchy, though graphs don't)
            // a graph at rest has a hierarchy, though this hierarchy shouldn't affect the content of the graph- it's just an organisation of it
            //
            // in reading/reordering the graph, we always want the choice at the top
            // then the per-variable possibles are just a matter of filtering and extracting from the choices
            //
            // this seems wasteful, as it is - but it simplifies the job a tad
            // possibilities are exploded
            // and in choosing from there on, disjuncts can't be introduced, just removed, so all the action is at the top level
            //
            // but what about subjunctive conditions? if this, then this? if they are out in the open then they canbe exploded as before
            // maybe this is what grounding kind of is... simplified for easy computation
            // ------
            //
            // but after mangling, there'd be no point in having bindings sitting around, as they'd always be structured just as the expression itself
            // binding context is per delve; it is used as a temporary tracking of consistency, alongside a reference to last backtrackable choice point
            // expressions are graphs in their pristine form at rest (ie hierarchical, so not as perfect as could be - leaning a certain way)
            //
            // but one of our approaches was decomposing the delving of a composite expression into atoms; this would give us a set of simple cases for which we need program
            // a Delving should be composite, just as an Exp is composite in its own way
            // a Delving is a function taking a bind context and returning a set of possible bind contexts, derived from delving into an expression
            // in fact, the Delving isn'tthen composite, as it will be a monolithic function, taking composite parameters
            //
            // what happens when there are cycles?
            // such as...

            var exp7 =
                (x > 5 & y == 1) & x == 2;
            // in the above, x will first be bound to 5+, y to 1, but then x will be bound to the product of 5 and 2, which is Never
            // the presence of a single Never means inconsistency; as soon as we have this, we must give up on this disjunct and backtrack

            
            var exp8 =
                (x > 5 & y == 1) & (x == 2 | x == 7);
            // here, the disjunction will have been met before the inconsistent x==2, and so we will be able to go back to that point to continue

            
            var exp9 =
                (x > 5 & y == 1) | (x == 2 & y == 7);
            // and here we see we don't just want the expression as output, as the expression might be inconsistent in some of its branches
            // cleaning away this inconsistency takes effort that shouldn't be duplicated
            // but, the original reading will in fact remove impossible disjuncts from the expression
            // all the context therefore gives us is a nice index into the consistent bindings (this is still useful)


            var exp10 = x == 9;
            // just the writing of the above should accumulate a nice x=9 binding

            var exp11 = x == 9 & y == 3;
            // here the actual interpretation and simplification of the graph is done as soon as the nodes are constructed
            // the expression actually becomes {x:9}; {y:3}; {x:9, y:3}
            // this is the very eager way of computation
            // and befits not storing the expression, but a simpler structure of bindings and choices

            var exp12 = 
                x == 1 | 2 | 3 | 5 | 8 | 9 | 12 | 20;
            //here though we have a big set of possibles
            //if we eagerly multiplied out all disjuncts, we'd quickly run out of space
            //the solution to this is laziness; so we don't cache and investigate each disjunct, but instead a simplification
            //of the possible types; but we would eagerly cache and multiply out this simplified type.

            var exp13 = 
                exp12 & x == (3|-7) & y == (9 | 7);
            // if x were a hefty set of possibles, we wouldn't want to multiply out the above
            // if x were 'int', then no never would be realised, but we'd get the below

            var exp13_2 =
                (exp12 & x == 3 & y == 9) 
                | (exp12 & x == 3 & y == 7) 
                | (exp12 & x == -7 & y == 9) 
                | (exp12 & x == -7 & y == 7);
            
            // the impossibility of x==-7 wouldn't be known up front
            // in interpreting it, exp12 would be *sent* the bind info, and a new view expected
            // exp12 would then return a resolver
            
            // smaller simpler expressions could have the same basic interface as a fallback - a resolver
            // to receive a bind and return a new one
            // but an expression including a disjunct would return a disjunct of binds

            // so Exp(x == 7) is actually a function that takes a BindContext and returns a set of possible ones
            // just writing x == 7 would actually then dissolve into {x:7} with certain behaviour attached
            // an Exp is a means of resolving some binds
            
            var exp14 =
                exp12 & exp13_2;
            // here both subexpressions are 'lazy' 
            // we have the full bindings for neither up front
            // in fact the `&` can't do anything more with these
            // until we try and resolve everything, which must be a final operation
            //
            // how does the `&` know to be lazy in this case? the leaves must have some flag denoting strictness
            // though an exp might have strict and lazy parts
            
            // a World is then a combination of actual var bindings and a set of resolvables
            // 
            
            var exp15 =
                (exp12 & x == 3 & y == 9) 
                | (exp12 & x == 3 & y == 7) 
                | (exp12 & x == -7 & y == 9) 
                | (exp12 & x == -7 & y == 7);
            
            // but the above form isn't perfect
            // commonalities of expressions should be factored out to avoid interpreting lazy expressions repeatedly
            // and in evaluating an expression, it would always be best to provide it with as much resolved context
            // as possible
            
            // though we can't do that if we're passing in expressions to the resolver
            // AND lists should be ordered by weight before evaluating (though how do you order without evaluating?)
            // a BINDING is special - it is eager, no further evaluation needed
            
            // in vars terms, an exp might be lazy because it requires gpg decoding    
            // 
            
            // @apiKey = map @custRef {
            //    123123) apiKey21
            //    999999) apiKey4
            //    *) json {
            //       hello: "whoomp"
            //       blah: @schnooo|null
            //     }
            // }
            
            // the above is evidently a lazy evaluable
            // or rather, this is:
            // @x = json {
            //   whoomp: @y
            // }
            //
            // or is it lazy? it depends on how we set it. json values would default normally to lazy i reckon
            
            var exp16 =
                (x == 1 & y == x + 3 & x == 0);
            // above is legitimate but problematic
            // by the time we get to y, we can't eagerly resolve the domain (as it is relative to x, which changes in the next part)

            var z = new Var("z");

            var exp17 =
                (x == 1 & y == x + 3 & x == y + z);
            // here interestingly z is constrained to be -3
            // y is bound to a lazy exp (lazy because x is not final
            // and then the plus somehow deduces, again lazily
            //
            // though, when it is final, it can be narrowed
            // at the end of the pass, y will be bound to an expression
            // though x will be too; problems yup

            // @custRef = 999999|111111
            //
            // @apiKey = map @custRef {
            //    123123) apiKey21
            //    999999) apiKey4
            //    *) json {
            //       hello: "whoomp"
            //       blah: @schnooo|null
            //     }
            // }
            //
            //
            //
        }

    }
}