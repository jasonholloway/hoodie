using NUnit.Framework;

namespace Kraka
{
    [TestFixture]
    public class KrakaTests
    {
        [Test]
        public void Test()
        {
            var refs = "blah v nope v krrrumpt";


            var l = "len(refs)";

            var z = @"
                case refs
                  blah: whoomp v schnoo
                  nope: plops
             ";
            
            // z = plops v whoomp v schnoo v nil
            //
            // but the relations we've added don't predeterminethis; we need to rediscover this obvious fact
            //
            // z has a case relation 
            // vars have to be taken as nil by default
            // otherwise the above relation is not enough to sample
            // 
            // but we'd look at node z, and it would have an edge leading to the case relation
            // z begins as any 
            //
            // and it actually has four relations: whoomp, schnoo, plops and never
            // whoomp and schnoo sit beside each other as possibles
            //   (whoomp v schnoo) v plops  
            //
            // its a structure of nested disjunctions
            // 
            // Assert(z == plops v (whoomp v schnoo))
            // Assert((z==plops) v ((z==whoomp) v (z==schnoo)))
            //
            // the above assertion would create graph links either one per linked var
            // each variable is itself just a named expression
            //
            // z means: plops v (whoomp v schnoo)
            //
            // and, as more things are pinned to it,
            // the name/expression comes to mean more and more
            // 
            // Assert(x==1)
            // Assert(x==2)
            // accumulates this:  x = 1 ^ 2 = never
            // 
            // 
            // so how do we accumulate ors? into an alternate realilty of assertions
            // we can pile up assertions in one block; but then me must yield
            //
            //true
            //^ Assert(x==this)
            //v Assert(x==that)
            //
            // i dunno - we seem to want to be able to piile on constraints later
            // 
            //true
            //^ Assert(x==this)
            //^ Assert(x==that)
            //^ Assert(x==another)
            //
            //which is basically building one big expression
            //of lots of nested relations
            //which we then need to search
            //
            //exceptit also involves binds
            //binds in two lobes joined by an AND become joined by ANDs, and ORs similarly
            
            //so in my simple language, i need ands and ors, but also binds
            //exp: 
            //  const
            //| var = exp
            //| exp ^ exp
            //| exp v exp
            //
            //const:
            //
            //var:
            //  \w+
            //
            //though i can start with the actual ast in C#...
            
            

            
            
            
            var xx = @"
                


            ";
            
            
            
            var custRefs = Set("C1", "C2", "C3");

            var keys = MapFrom(custRefs,
                ("C1", "KEY1"),
                ("C2", "KEY2"),
                ("C3", "KEY3")
                );
            
            var serviceRefs = Set("123", "234", "345");

        }

        public static Set<T> Set<T>(params T[] elems)
            => new Set<T>();

        public static Set<B> MapFrom<A, B>(Set<A> in1, params (A, B)[] maps)
        {
            return new Set<B>();
        }
    }

    public class Set<A>
    {

        public Set<B> Switch<B>(params (A, B)[] cases)
        {
            return new Set<B>();
        }
        
    }
    
}