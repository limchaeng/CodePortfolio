using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace UMF.SimpleTest
{
	public class DelegateTest : SimpleTestBase
	{
		public delegate void _PacketHandler<ST>( ST session, object packet );

		[AttributeUsage( System.AttributeTargets.Method )]
		public class PacketHandlerAttribute : Attribute
		{
			public int PacketId { get; set; }
			public Type PacketType { get; set; }

			public virtual string GetID<ST>() where ST : Session
			{
				return PacketId.ToString();
			}
		}

		[AttributeUsage( System.AttributeTargets.Method )]
		public class DBPacketHandlerAttribute : PacketHandlerAttribute
		{
			public int DBID { get; set; }

			public override string GetID<ST>()
			{
				return DBID.ToString() + ":" + typeof( ST ).Name; 
			}
		}

		[AttributeUsage( System.AttributeTargets.Method )]
		public class DB2PacketHandlerAttribute : DBPacketHandlerAttribute
		{
			public int DBID_2 { get; set; }

			public override string GetID<ST>()
			{
				return DBID_2.ToString();
			}
		}

		public class Session
		{

		}

		public class Peer : Session
		{
			public string id = "peer id";
		}

		public abstract class HandlerBase
		{
			public abstract void OnHandle( Session session, object pakcet );
		}

		public class Handler<ST> : HandlerBase where ST : Peer
		{
			_PacketHandler<ST> _handler;

			public Handler( _PacketHandler<ST> handler )
			{
				_handler = handler;
			}

			public override void OnHandle( Session session, object packet )
			{
				_handler( (ST)session, packet );
				//_handler.Invoke( (ST)session, packet );
				//_handler.DynamicInvoke( (ST)session, packet );
			}
		}

		public class HandlerManagerBase
		{
			protected Dictionary<int, HandlerBase> mReflectionCallHandlerDic = new Dictionary<int, HandlerBase>();
			protected Dictionary<int, HandlerBase> mDirectCallHandlerDic = new Dictionary<int, HandlerBase>();

			public void OnHandle(Session session, int packet_id, object packet, int calltype)
			{
				Dictionary<int, HandlerBase> call_dic = null;

				switch(calltype)
				{
					case 0: call_dic = mReflectionCallHandlerDic; break;
					case 1: call_dic = mDirectCallHandlerDic; break;
				}

				HandlerBase handler;
				if( call_dic.TryGetValue( packet_id, out handler ) )
				{
					handler.OnHandle( session, packet );
				}
			}
		}

		public class HandlerManager<ST> : HandlerManagerBase where ST : Peer
		{
			public int call_count = 0;

			public HandlerManager()
			{
				//MethodInfo[] handler_methdos = GetType().GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
				MethodInfo[] handler_methdos = GetAllMethods().ToArray();
				if( handler_methdos != null )
				{
					foreach( MethodInfo method in handler_methdos )
					{
						PacketHandlerAttribute handler_attr = method.GetCustomAttribute<PacketHandlerAttribute>();
						if( handler_attr != null )
						{
							//LogTest( $"============= method : {method.Name} = {handler_attr.GetType().Name} = Get: {handler_attr.GetID<ST>()}" );

// 							if( handler_attr.GetType().Equals( typeof( DBPacketHandlerAttribute )))
// 								LogTest( "####################" );

// 							if( handler_attr.GetType().IsSubclassOf( typeof( PacketHandlerAttribute ) ) )
// 							{
// 								LogTest( "=>" );
// 							}

							int packet_id = handler_attr.PacketId;
							Type packet_type = handler_attr.PacketType;

							Type del_type = typeof( _PacketHandler<ST> );
							_PacketHandler<ST> del = (_PacketHandler<ST>)Delegate.CreateDelegate( del_type, this, method );
							Handler<ST> handler = new Handler<ST>( del );

							mReflectionCallHandlerDic.Add( packet_id, handler );
							LogTest( $"### packet add {packet_id} {packet_type.Name} : dic count : {mReflectionCallHandlerDic.Count}" );

						}
					}
				}

				{
					Handler<ST> handler = new Handler<ST>( OnTestPacket );
					mDirectCallHandlerDic.Add( 1, handler );
				}
			}

			bool CheckMethodEqual( MethodInfo a, MethodInfo b )
			{
				if( a.Name != b.Name )
					return false;

				ParameterInfo[] param_a = a.GetParameters();
				ParameterInfo[] param_b = b.GetParameters();
				if( param_a.Length != param_b.Length )
					return false;

				for(int i=0; i<param_a.Length; i++ )
				{
					if( param_a[i].ParameterType != param_b[i].ParameterType )
						return false;
				}

				if( a.ReturnParameter.ParameterType != b.ReturnParameter.ParameterType )
					return false;

				return true;
			}

			List<MethodInfo> GetAllMethods()
			{
				List<MethodInfo> list = new List<MethodInfo>();
				Type curr_type = GetType();
				while(true)
				{
					MethodInfo[] handler_methdos = curr_type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
					if( handler_methdos != null )
					{
						foreach (MethodInfo method in handler_methdos)
						{
							LogTest( $">>>> all : {method.Name} : {method.DeclaringType.Name} : {method.GetParameters().Length} : {method.ReturnParameter.ParameterType}" );

							if( list.Exists(a => CheckMethodEqual(a, method)) == false )
							{
								list.Add( method );
							}
							else
							{
								LogTest( $"			>>>> exist : {method.Name}" );
							}

						}
					}

					curr_type = curr_type.BaseType;
					if( curr_type == null )
						break;
				}

				return list;
			}


			[PacketHandler( PacketId = 1, PacketType = typeof(TestPacket))]
			public virtual void OnTestPacket(Peer session, object _packet )
			{
				TestPacket packet = _packet as TestPacket;
				//LogTest( $"{session.id} = {packet.id}, {packet.name}" );
				call_count++;
			}

			[DBPacketHandler( PacketId = 2, PacketType = typeof( TestPacket ), DBID = 99 )]
			public virtual void OnDBTestPacket( Peer session, object _packet )
			{
				TestPacket packet = _packet as TestPacket;
				//LogTest( $"{session.id} = {packet.id}, {packet.name}" );
				call_count++;
			}
			[DB2PacketHandler( PacketId = 3, PacketType = typeof( TestPacket ), DBID = 99, DBID_2 = 999 )]
			private void OnDBTestPacket2( Peer session, object _packet )
			{
				TestPacket packet = _packet as TestPacket;
				//LogTest( $"{session.id} = {packet.id}, {packet.name}" );
				call_count++;
			}

			[DB2PacketHandler( PacketId = 5, PacketType = typeof( TestPacket ), DBID = 99, DBID_2 = 999 )]
			protected virtual void OnDBTestPacket5( Peer session, object _packet )
			{
				LogTest( "pacekt id 5" );
			}
		}

		public class TestPacket
		{
			public int id = 1;
			public string name = "test packet";
		}

		DateTime begin_time = DateTime.Now;
		public void BeginTime()
		{
			begin_time = DateTime.Now;
		}
		public TimeSpan EndTime()
		{
			return ( DateTime.Now - begin_time );
		}

		public class HandlerManager2<ST> : HandlerManager<ST> where ST : Peer
		{
			public HandlerManager2() : base()
			{
			}

			[DB2PacketHandler( PacketId = 4, PacketType = typeof( TestPacket ), DBID = 99, DBID_2 = 999 )]
			private void OnPrivateMethod( Peer session, object _packet )
			{
				TestPacket packet = _packet as TestPacket;
				//LogTest( $"{session.id} = {packet.id}, {packet.name}" );
				call_count++;
			}

			protected override void OnDBTestPacket5( Peer session, object _packet )
			{
				//base.OnDBTestPacket2( session, _packet, aa );
				LogTest( "pacekt id 5 child" );
			}
		}

		protected override void TestLogic()
		{
			Peer peer = new Peer();
			TestPacket packet = new TestPacket();

			HandlerManager2<Peer> manager = new HandlerManager2<Peer>();

			int test_count = 1;

			BeginTime();
			for( int i = 0; i < test_count; i++ )
			{
				manager.OnHandle( peer, 5, packet, 0 );
			}
			LogTest( $"reflection call count : {manager.call_count} time : {EndTime()} ms" );

			manager.call_count = 0;
// 			BeginTime();
// 			for( int i = 0; i < test_count; i++ )
// 			{
// 				manager.OnHandle( peer, 1, packet, 1 );
// 			}
// 			LogTest( $"direct call count : {manager.call_count} time : {EndTime()} ms" );
		}
	}
}
