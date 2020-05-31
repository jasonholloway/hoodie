using NUnit.Framework;
using static Hoodie.GroupMaps.Tests.MapLang.Runner;

namespace Hoodie.GroupMaps.Tests
{
    public class ClumpTests
    {
        [Test]
        public void MapEquality1()
            => Test(@"
                    A | A
                    A = A
                ");

        [Test]
        public void MapEquality2()
            => Test(@"
                    A . | . A
                    A B = B A
                ");

        [Test]
        public void MapEquality3()
            => Test(@"
                    A . | . A
                    A B = B A
                    C . | C .
                ");
    }

    public class CropTests
    {
        [Test]
        public void Crop1()
            => Test(@"
                A |  _
                A #> A
            ");
        
        [Test]
        public void Crop2()
            => Test(@"
                A B |  _ _
                A B #> A B
            ");
        
        [Test]
        public void Crop3()
            => Test(@"
                A B |  _
                A . #> A
            ");
        
        [Test]
        public void Crop4()
            => Test(@"
                A B |  _
                . . #> .
            ");
    }
    
    

    public class HitTests
    {
        [Test]
        public void Simple()
            => Test(@"
                A => A
            ");
        
        [Test]
        public void Simple_WithMask1()
            => Test(@"
                A |  .
                B => B
            ");

        [Test]
        public void Simple_Disjunct()
            => Test(@"
                A B => A ^ B
            ");
        
        
        [Test]
        public void Complicated1()
            => Test(@"
                A  |   _
                A  =>  A
            ");
        
        [Test]
        public void Complicated2_1()
            => Test(@"
                A Z  |   _ _ _ 
                A .  =>  A ^ . 
            ");
        
        [Test]
        public void Complicated2_2()
            => Test(@"
               Z A  |   _ _ _ 
               . A  =>  A ^ . 
            ");
        
        [Test]
        public void Complicated3()
            => Test(@"
                A Z .  |   _ _ _
                A . C  |   _ _ _
                A . C  =>  A ^ C 
            ");
        
        [Test]
        public void Complicated3_2()
            => Test(@"
                A Z .  |   _ _ _
                A . C  =>  A ^ C 
            ");
        
        [Test]
        public void Complicated4_1()
            => Test(@"
                A . Y  |   _ _ _
                A B .  =>  A ^ B
            ");
        
        [Test]
        public void Complicated4_2()
            => Test(@"
                A Y .  |   _ _ _
                A . B  =>  A ^ B
            ");
        
        [Test]
        public void Complicated4_3()
            => Test(@"
                Y A .  |   _ _ _
                . A B  =>  A ^ B
            ");
        
        [Test]
        public void Complicated5()
            => Test(@"
                A . Y Z  |   _ _ _
                . B . .  =>  . ^ B
                A B . .  =>  A | B
            ");
        
        [Test]
        public void Complicated6()
            => Test(@"
                A . Y Z  |   _ _ _ _ _
                . B . Z  |   _ _ _ _ _
                A B . .  =>  A ^ B ^ .
            ");
        
        [Test]
        public void Empty_MeansAvailableDisjunct1()
            => Test(@"
                .  =>  . 
            ");
        
        [Test]
        public void Empty_MeansAvailableDisjunct2()
            => Test(@"
                Y Z  |   _ 
                . .  =>  . 
            ");
    }
}