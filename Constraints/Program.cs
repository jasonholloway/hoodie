using System;
using System.Collections.Generic;

namespace probs
{
    using static Ops;

    public static class Ops
    {
        public static void Assert(Node node)
        {
            //crawls through graph constraining variables
            //...
        }
    }

    public class Variable
    {
        public readonly List<Constraint> Constraints = new List<Constraint>();

        internal void AddConstraint(Constraint constraint)
        {
            var result = new Variable();
            Constraints.Add(constraint);
            result.Constraints.Add(constraint);
        }
        
        public static Node operator ==(Variable me, object val)
            => new EqualsNode(new VariableNode(me), new ConstantNode(val));

        public static Node operator !=(Variable me, object val)
            => new NotNode(new EqualsNode(new VariableNode(me), new ConstantNode(val)));
        
        public static Node operator &(Variable me, Node other) => default;
        public static Node operator ^(Variable me, Node other) => default;

        public object Sample() => default;
    }

    public static class VariableExtensions
    {
        public static Node In(this Variable variable, params object[] vals)
            => default;
    }


    public abstract class Constraint {
    }
    
    //so, constraints... they are added together as per checkout kata, but they affect not just single variables, they span multiple ones in fact
    //in fact, each time we try to accumulate constraints, we do so speculatively, to see if we can whittle them to a possible simplicity
    //...
    //
    //though some constraints aren't either/or, but are mergeable
    //you could say such constraints are joined by 'ands'
    //...
    //
    //as we crawl along, we accumulate constraints, but centred on nodes

    public class Equals : Constraint
    {
        public Variable Left { get; }
        public object Right { get; }
        public Variable Result { get; }

        public Equals(Variable left, object right, Variable result)
        {
            Left = left;
            Right = right;
            Result = result;
        }
    }

    
    internal class Program
    {
        public static void Main(string[] args)
        {
            var country = new Variable();
            Assert(country == "AU");
            
            Console.WriteLine();
            Console.WriteLine($"country: {country}");
            Console.WriteLine();
        }
        
        public static void _Main(string[] args)
        {
            var env = new Variable();
            Assert(env.In("F1", "F2", "PreProd"));

            var country = new Variable();
            Assert(country.In("UK", "GB", "US", "DE", "AU"));

            var carrier = new Variable();
            Assert(carrier.In("AUSPO", "UPS", "HERMES", "TNT"));
            
            Assert((country == "AU") ^ (country == "US"));
            
            Assert((country == "AU").Therefore(carrier == "AUSPO"));
            Assert((country == "US").Therefore(carrier == "UPS"));
            
            Assert(country == "AU");
            
            Console.WriteLine();
            Console.WriteLine($"country: {country}");
            Console.WriteLine();
        }
        
        //the question is, do we have both Nodes and Variables?
        //nodes are AST components; asserting on these binds them to variables
        //which means each kind of relation requires both an AST link and an actual constraining relation
        //
        
        
        
        
        //an Assert applies directly
        //propagating through the network
        //an assert isn't just saying 'this is true'
        //
        //just doing x==3 doesn't assert anything (here we have a rule, not a goal)
        //its constructive, but not binding to any nodes
        //so the actual updating of the node graph isn't done by the operator, which only builds up an AST
        //but by the Assert() itself, which goes through the AST applying constraints to the nodes
        //and if any of the constraint accumulations returns 'nope', then the assertion must fail
        //
        //so assertions accumulate, each time finessing the graph
        //
        //but - it's not just the individual nodes that accumulate; its also the edges of the graph itself
        //
        //
        //
        
        
        
        
        
        //each assertion is conjunctive goal, in the lingo
        //and each assertion adds new relations to the graph
        //meaning that each node can consistently represent only certain values
        //
        //but then there's the process of sampling these, by which choosing one value has follow-on effects on others
        //sampling is effectively layering on another assertion
        //
        //but - as well as sampling, which must proceed incrementally across the graph,
        //there's also the node-by-node independent enumerating of possiblities
        //
        //so - sample asserts and gets back a single value from a node; enumerating lists all possible local values,
        //with no accounting of links between values of different nodes
        //this latter is possible because, on each graph change, constraints propagate in all directions
        //
        //asserting against one node must invalidate cached values elsewhere
        //if there were no caches, evaluation would be done at point of asking -
        //at a certain node, starting unconstrained, 
        //
        //constraints would be gradually uncovered, firstly an assertion of 'stringiness'
        //the stringiness would know it was at loggerheads with non-subtypes or supertypes
        //it's a constraint of type 'ClrType', with its own logic of interaction with other
        //ClrTypes; such constraints would be implcitly created across the board: a numeric constraint would 
        //implicate (==decimal & > 5M) | (==int & > 5) | ...
        //
        //so, a constraint (x is string) is found, and then a constraint of (x is int) - these constraints, accumulated into
        //a common splodge, would not accumulate happily
        //
        //how about (x is int) & (x < 3) & (x > 10)?
        //the first would accumulate happily, the others would have explicit (x is ints) as guards;
        //then the first constraint would be of type number range (almost like certain types of value can have certain kinds of constraint imposed)
        //
        //then an IntConstraint added to another IntConstraint would not sit together happily
        //each constraint also has the power of enumerating values - or rather of sampling values
        //
        //but an IntConstraint must sit below the more generic TypeConstraint - which exists in a system with other TypeConstraints
        //such a constraint is itself of a certain type, and can recognise others by this type
        //as constraints are accumulated (how?) such constraints will find each other and interact
        //
        //generically, constraints either sit well with each other, or they don't
        //in such cases, we must backtrack; ors and exclusive ors are where we can backtrack - here is the disjuntion
        
        //because the network is even its basic cases going to be cyclical, we need to deal with this
        //propogation will continue narrowing until it changes nothing, then it can stop expanding
        //at each expansion, a trail is gathered: we go forward until we have to go back
        //we don't need to detect recursions, because we want recursions; we could have a limit on recursion depth, though there's no need, as we can limit by time
        
        //
        
    }

}