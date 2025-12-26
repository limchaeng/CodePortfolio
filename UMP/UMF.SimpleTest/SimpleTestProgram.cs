using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using UMF.Core;

namespace UMF.SimpleTest
{
	class SimpleTestProgram
	{
		static void Main( string[] args )
		{
			new Test01();
		
			//new DelegateTest();
			//new EnvConfigTest();
			//new ParallelTest();
			//new ReloadInfoTest();
			//new InterfaceTest();
		}
	}

	//------------------------------------------------------------------------
	public class Test01 : SimpleTestBase
	{
		
		public abstract class AA
		{
			public const int ID_1 = 1;

			public virtual IEnumerator Coroutine1()
			{
				LogTest( "coroutine 1" );
				yield return null;
				LogTest( "coroutine 1-1" );
			}
		}

		public class BB : AA
		{
			public const int ID_100 = 100;

			public int idx = 0;

			public override IEnumerator Coroutine1()
			{
				LogTest( $"coroutine {idx}" );
				yield return null;
				LogTest( $"coroutine {idx}-1" );

				IEnumerator base_routine = base.Coroutine1();
				while( base_routine.MoveNext() )
					yield return null;
			}
		}

		public class MultiEnumerator
		{
			Stack<IEnumerator> mCurrStack = new Stack<IEnumerator>();
			IEnumerator mCurr = null;

			public MultiEnumerator(IEnumerator curr)
			{
				mCurr = curr;
			}

			public bool MoveNext()
			{
				if( mCurr.MoveNext() == false )
				{
					mCurr = null;
					if( mCurrStack.Count == 0 )
						return false;

					mCurr = mCurrStack.Pop();
				}
				else
				{
					if( mCurr.Current != null && mCurr.Current is IEnumerator )
					{
						mCurrStack.Push( mCurr );
						mCurr = (IEnumerator)mCurr.Current;							
					}
				}

				return true;
			}
		}

		IEnumerator ListRoutine(List<BB> blist)
		{
			foreach( BB b in blist )
			{
				yield return b.Coroutine1();
			}
		}

		public class CC
		{
			List<IEnumerator> list = new List<IEnumerator>();
			public bool IsFinish { get { return list.Count <= 0; } }

			IEnumerator Routine(int _idx)
			{
				int idx = _idx;
				yield return null;
				LogTest( "[0] routine : {0}", idx );
				yield return null;
				LogTest( "[1] routine : {0}", idx );
			}

			public void AddRoutine(int _idx)
			{
				list.Add( Routine( _idx ) );
			}

			public void Update()
			{
				if( list.Count > 0 )
				{
					List<IEnumerator> new_list = new List<IEnumerator>();
					foreach( IEnumerator routine in list )
					{
						if( routine.MoveNext() )
							new_list.Add( routine );
					}
					list = new_list;
				}
			}
		}

		public class DD : CC
		{

		}

		[System.Flags]
		public enum eEnumFlags : int
		{
			None	= 0x0000,
			AA		= 0x0001,
			BB		= 0x0002,
		}

		public bool HasEnumFlag(int value, int check)
		{
			return ( ( value & check ) != 0 );
		}
			

		protected override void TestLogic()
		{
			eEnumFlags flags = eEnumFlags.AA;

			if( HasEnumFlag( (int)flags, (int)eEnumFlags.BB ) )
				LogTest( "+==========" );

			/*/
			CC cc = new CC();
			for( int i = 1; i < 5; i++ )
				cc.AddRoutine( i );

			while(cc.IsFinish == false)
			{
				cc.Update();
			}

			/**/

			/*/
			List<BB> bList = new List<BB>();
			for( int i = 0; i < 5; i++ )
				bList.Add( new BB() { idx = UMFRandom.Instance.NextRange( 1, 100 ) } );

			foreach( BB b in bList )
				LogTest( b.idx.ToString() );

			Sep();

			bList.Sort( ( a, b ) =>
			 {
				 return b.idx.CompareTo( a.idx );
			 } );

			foreach( BB b in bList )
				LogTest( b.idx.ToString() );


			/**/

			/*/
			List<BB> bList = new List<BB>();
			for( int i = 0; i < 5; i++ )
				bList.Add( new BB() { idx = i } );

			MultiEnumerator routine = new MultiEnumerator( ListRoutine( bList ) );
			while( routine.MoveNext() )
			{
				LogTest( "AAA" );
			}

			LogTest( "BBB" );

			/**/

			/*/
			BB b = new BB();

			IEnumerator routine = b.Coroutine1();
			while( routine.MoveNext() )
			{
				LogTest( "AAA" );
			}	
			/**/
		}
	}
}
