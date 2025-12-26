using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMF.SimpleTest
{
	public class InterfaceTest : SimpleTestBase
	{
		public interface ITest
		{
			void WriteLog();
		}

		public abstract class ModuleCoreBase
		{
			public abstract string GetName { get; }
		}

		public class BB : ModuleCoreBase, ITest
		{
			public override string GetName => "AA";

			public void WriteLog()
			{
				LogTest( GetName );
			}
		}

		public class CC : ModuleCoreBase, ITest
		{
			public override string GetName => "CC";

			public void WriteLog()
			{
				LogTest( GetName );
			}
		}

		List<ModuleCoreBase> mModuleList = new List<ModuleCoreBase>();

		//------------------------------------------------------------------------
		public List<T> FindModuleInterface<T>() where T : class
		{
			List<T> list = null;
			Type find_type = typeof( T );
			foreach( ModuleCoreBase module in mModuleList )
			{
				if( module.GetType().GetInterfaces().Contains(find_type) )
				{
					if( list == null )
						list = new List<T>();

					list.Add( module as T );
				}
			}

			return list;
		}


		protected override void TestLogic()
		{			
			mModuleList.Add( new BB() );
			mModuleList.Add( new CC() );

			List<ITest> list = FindModuleInterface<ITest>();
			if( list != null )
			{
				foreach( ITest it in list )
					it.WriteLog();
			}
		}
	}
}
