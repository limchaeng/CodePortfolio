using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace UMF.SimpleTest
{
	public class ParallelTest : SimpleTestBase
	{
		public class TestClass
		{
			public int int_value;
		}

		protected override void TestLogic()
		{
			ConcurrentQueue<TestClass> c_queue = new ConcurrentQueue<TestClass>();
			for(int i=0; i<10; i++)
			{
				TestClass t = new TestClass();
				t.int_value = i;

				c_queue.Enqueue( t );
			}

			LogTest( "Concurrent Queue count:{0}", c_queue.Count );

			WaitKey();

			TestClass t2;
			while(c_queue.TryDequeue(out t2) )
			{
				LogTest( "- try dequeue : {0}", t2.int_value );
			}

			LogTest( "Concurrent Queue count:{0}", c_queue.Count );
		}
	}
}
