// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
/*******************************************************************************/
/* Test:    BaseFinal
/* Purpose: 1. if finalize() is called before the objects are GCed.
/*      2. resurrect the object while the finalize() method is call.
/*******************************************************************************/

namespace DefaultNamespace {
    using System;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;

    public class BaseFinal
    {
// disabling unused variable warning
#pragma warning disable 0414
        internal static Object StObj;
#pragma warning restore 0414

        [Fact]
        public static int TestEntryPoint()
        {
            Console.WriteLine("Test should return with ExitCode 100 ...");
            CreateObj temp = new CreateObj();
            if (temp.RunTest())
            {
                Console.WriteLine( "Test passed!" );
                return 100;
            }
            else
            {
                Console.WriteLine( "Test failed!" );
                return 1;
            }
        }


        ~BaseFinal()
        {
            BaseFinal.StObj = ( this );
            Console.WriteLine( "Main class Finalize().");
        }

        public void CreateNode( int i )
        {
            BNode rgobj = new BNode( i );
        }
    }


    internal class BNode
    {
        public static int icCreateNode = 0;
        public static int icFinalNode = 0;
        internal static List<object> rlNode = new List<object>( );
        public int [] mem;

        public BNode( int i )
        {
            icCreateNode++;
            mem = new int[i];
            mem[0] = 99;
            if(i > 1 )
            {
                mem[mem.Length-1] = mem.Length-1;
            }
        }

        ~BNode()
        {
            icFinalNode++;
            rlNode.Add(this);  //resurrect objects
        }
    }


    internal class CreateObj
    {

        public BaseFinal mv_Obj;

// disabling unused variable warning
#pragma warning disable 0414
        public BNode obj;
#pragma warning restore 0414


        public CreateObj()
        {

            mv_Obj = new BaseFinal();
            Console.WriteLine("before starting the test, heap size is {0}", GC.GetTotalMemory(false));

            for( int i=1; i< 1000; i++)
            {
                obj = new BNode(i);     //create new one and delete the last one.
                mv_Obj.CreateNode( i ); //create locate objects in createNode().
            }
            Console.Write(BNode.icCreateNode);
             Console.WriteLine(" Nodes were created.");

            //Console.WriteLine("after all objects were created, the heapsize is " + GC.GetTotalMemory(false));
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public void DestroyNode()
        {
            obj = null;
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public void ResurrectNodes()
        {
            for(int i=0; i< BNode.rlNode.Count; i++)
            {
                BNode oldNode = (BNode)BNode.rlNode[ i ];
                if ( oldNode.mem[0] != 99 )
                {
                    Console.WriteLine( "One Node is not resurrected correctly.");
                }
                oldNode = null;
                BNode.rlNode[ i ] = null;
            }
        }

        public bool RunTest()
        {
            DestroyNode();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.Write(BNode.icFinalNode);
            Console.WriteLine(" Nodes were finalized and resurrected.");
            //Console.WriteLine("after all objects were deleted and resurrected in Finalize() , the heapsize is " + GC.GetTotalMemory(false));

            ResurrectNodes(); 

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            //Console.WriteLine("after all objects were deleted , the heapsize is " + GC.GetTotalMemory(false));

            return ( BNode.icCreateNode == BNode.icFinalNode );


        }
    }


}
