//////////////////////////////////////////////////////////////////////////
//
// CommandManager
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;

namespace UMF.Server
{
	[System.Flags]
	public enum eCommandAuthority
	{
		None		= 0x0000,

		View		= 0x0001,
		Reload		= 0x0002,
		Server		= 0x0004,
		GamePlay	= 0x0008,

		Develop		= 0x0010,
	}

	public class Command
	{
		public virtual bool IsParamCommand { get { return false; } }

		OnCommand _OnCommand;
		public virtual Delegate GetOnCommand { get { return _OnCommand; } }
		Dictionary<string, Command> subs;
		public Dictionary<string, Command> SubCommands { get { return subs; } }
		protected bool _isConsole = true;
		protected string _CommandName = "";
		public string CommandName { get { return _CommandName; } }
		protected string _lastOutput = "";

		eCommandAuthority mAuthority = eCommandAuthority.None;
		public void SetAuthority( eCommandAuthority authority )
		{
			mAuthority = authority;
		}

		public bool AllowAuthority( eCommandAuthority req_auth )
		{
			if( _isConsole )
				return true;

			if( mAuthority == eCommandAuthority.None )
				return true;

			return ( ( req_auth & mAuthority ) != 0 );
		}

		public bool IsConsole { set { _isConsole = value; } get { return _isConsole; } }
		public void SetCommandName(string _name) { _CommandName = _name; }
		public string LastOutput
		{
			get
			{
				string ret = _lastOutput;
				if( subs != null )
				{
					foreach( Command sub_cmd in subs.Values )
					{
						string _last_output = sub_cmd.LastOutput;
						if( string.IsNullOrEmpty( _last_output ) == false )
							ret += string.Format( "{0}\n", _last_output );
					}
				}

				_lastOutput = "";

				return ret;
			}
		}

		public Command( OnCommand onCommand )
		{
			this._OnCommand = onCommand;
		}

		protected void WriteConsole( string str, bool is_line = true )
		{
			_lastOutput += str;
			if( _isConsole )
			{
				if( is_line )
					Console.WriteLine( str );
				else
					Console.Write( str );
			}
		}

		public Command AddCommand( string command, eCommandAuthority authority = eCommandAuthority.None )
		{
			return AddCommand( command, null, authority );
		}

		public Command AddParamCommand( string command, ParamCommandBase paramCommand, eCommandAuthority authority = eCommandAuthority.None )
		{
			if( subs == null )
				subs = new Dictionary<string, Command>();

			paramCommand.SetAuthority( authority == eCommandAuthority.None ? mAuthority : authority );
			paramCommand.SetCommandName( command );
			paramCommand.IsConsole = _isConsole;
			subs.Add( command.ToLower(), paramCommand );

			return paramCommand;
		}

		public Command AddCommand( string command, OnCommand onCommand, eCommandAuthority authority = eCommandAuthority.None )
		{
			if( subs == null )
				subs = new Dictionary<string, Command>();

			Command newCommand = new Command( onCommand );
			newCommand.SetAuthority( authority == eCommandAuthority.None ? mAuthority : authority );
			newCommand.SetCommandName( command );
			newCommand.IsConsole = _isConsole;
			subs.Add( command.ToLower(), newCommand );

			return newCommand;
		}

		public virtual bool RunCommand( string[] cmds, int index, eCommandAuthority req_auth )
		{
			if( subs == null && cmds.Length == index )
			{
				if( _OnCommand == null )
					WriteConsole( "delegate is null" );
				else
					_OnCommand( _CommandName );

				return true;
			}

			if( subs == null )
			{
				WriteConsole( "invalid param : " + cmds[index] );
				return false;
			}

			if( cmds.Length <= index )
			{
				WriteConsole( "need param : ", false );
				bool bFirst = true;
				foreach( string cmdKey in subs.Keys )
				{
					if( bFirst == false )
						WriteConsole( ", ", false );
					else
						bFirst = false;
					WriteConsole( cmdKey, false );
				}
				WriteConsole( "\n", false );
				return false;
			}

			string cmd = cmds[index];
			Command command;
			if( subs.TryGetValue( cmd.ToLower(), out command ) == false )
			{
				if( index == 0 )
					WriteConsole( "Unknown command : " + cmd );
				else
					WriteConsole( "Unknown param : " + cmd );
				return false;
			}

			if( command.AllowAuthority( req_auth ) == false )
			{
				WriteConsole( "Access denied!" );
				return false;
			}

			return command.RunCommand( cmds, index + 1, req_auth );
		}

		public virtual void AutomationCommand( string[] cmds, int index, ref string rCommand )
		{
			if( index == 0 )
				rCommand = "";

			if( subs == null && cmds.Length <= index )
				return;

			if( subs == null )
			{
				WriteConsole( "invalid param : " + cmds[index] );
				return;
			}

			List<string> _params = null;

			string cmd = "";
			if( cmds.Length <= index )
				_params = new List<string>( subs.Keys );
			else if( cmds.Length == index + 1 )
			{
				cmd = cmds[index].ToLower();

				_params = new List<string>();
				foreach( string cmdKey in subs.Keys )
				{
					if( cmdKey.StartsWith( cmd ) == true )
						_params.Add( cmdKey );
				}
				if( _params.Count == 0 )
					_params = new List<string>( subs.Keys );
			}

			if( _params != null )
			{
				if( _params.Count == 1 )
				{
					if( index > 0 )
						rCommand += " ";
					cmd = _params[0];
					rCommand += cmd;
				}
				else
				{
					string compareParam = null;
					foreach( string strParam in _params )
					{
						if( compareParam == null )
							compareParam = strParam;
						else
						{
							int i = 0;
							for( i = 0; i < strParam.Length && i < compareParam.Length; ++i )
							{
								if( strParam[i] != compareParam[i] )
									break;
							}
							compareParam = compareParam.Substring( 0, i );
						}
					}
					if( compareParam.Length > 0 )
					{
						if( index > 0 )
							rCommand += " ";
						rCommand += compareParam;
					}
					else
						rCommand += " ";

					WriteConsole( "need param : ", false );
					bool bFirst = true;
					foreach( string strParam in _params )
					{
						if( bFirst == false )
							WriteConsole( ", ", false );
						else
							bFirst = false;
						WriteConsole( strParam, false );
					}
					WriteConsole( "\n", false );
					return;
				}
			}

			if( cmd.Length == 0 )
			{
				cmd = cmds[index];
				if( index > 0 )
					rCommand += " ";
				rCommand += cmd;
			}
			Command command;
			if( subs.TryGetValue( cmd.ToLower(), out command ) == false )
			{
				if( index == 0 )
					WriteConsole( "Unknown command : " + cmd );
				else
					WriteConsole( "Unknown param : " + cmd );
				return;
			}

			command.AutomationCommand( cmds, index + 1, ref rCommand );
		}
	}

	public abstract class ParamCommandBase : Command
	{
		abstract protected Type[] Types { get; }
		override public bool IsParamCommand { get { return true; } }
		public string GetParamString()
		{
			string parms = "";
			for( int i = 0; i < Types.Length; i++ )
			{
				if( i > 0 )
					parms += " ";

				if( Types[i].IsEnum == true )
				{
					parms += string.Format( "<{0}:", Types[i].Name );
					string[] names = Enum.GetNames( Types[i] );
					bool bFirstName = true;
					foreach( string name in names )
					{
						if( bFirstName == false )
							parms += ",";
						else
							bFirstName = false;

						parms += name;
					}
					parms += ">";
				}
				else
				{
					parms += string.Format( "<{0}>", Types[i].Name );
				}
			}

			return parms;
		}

		Delegate _OnParamCommand = null;
		public override Delegate GetOnCommand
		{
			get
			{
				return _OnParamCommand;
			}
		}

		public ParamCommandBase( Delegate onParamCommand )
			: base( null )
		{
			_OnParamCommand = onParamCommand;
		}

		public override bool RunCommand( string[] cmds, int index, eCommandAuthority req_auth )
		{
			if( cmds.Length != index + Types.Length )
				PrintParams();
			else if( _OnParamCommand == null )
				WriteConsole( "delegate is null" );
			else
			{
				ArrayList _params = new ArrayList();
				_params.Add( _CommandName );
				for( int i = index; i < cmds.Length; ++i )
				{
					Type type = Types[i - index];

					if( type.IsEnum == true )
					{
						try
						{
							_params.Add( Enum.Parse( type, cmds[i], true ) );
						}
						catch( System.Exception )
						{
							PrintParams();
							return false;
						}
					}
					else
					{
						TypeCode typeCode = Type.GetTypeCode( type );
						if( typeCode == TypeCode.String )
							_params.Add( cmds[i] );
						else
						{
							if( typeCode == TypeCode.Object && type != typeof( IPAddress ) )
							{
								PrintParams();
								return false;
							}

							System.Reflection.MethodInfo method = type.GetMethod( "Parse", new Type[] { typeof( string ) } );
							if( method == null )
							{
								PrintParams();
								return false;
							}

							try
							{
								_params.Add( method.Invoke( null, new string[1] { cmds[i] } ) );
							}
							catch( System.Exception )
							{
								PrintParams();
								return false;
							}
						}
					}
				}

				if( AllowAuthority( req_auth ) == false )
				{
					WriteConsole( "Not allowed authority!" );
					return false;
				}

				_OnParamCommand.DynamicInvoke( _params.ToArray() );
				return true;
			}

			return false;
		}

		void PrintParams()
		{
			WriteConsole( "need param : ", false );
			bool bFirst = true;
			foreach( Type type in Types )
			{
				if( bFirst == false )
					WriteConsole( ", ", false );
				else
					bFirst = false;

				if( type.IsEnum == true )
				{
					WriteConsole( "<" + type.Name + " : ", false );
					string[] names = Enum.GetNames( type );
					bool bFirstName = true;
					foreach( string name in names )
					{
						if( bFirstName == false )
							WriteConsole( ", ", false );
						else
							bFirstName = false;
						WriteConsole( name, false );
					}
					WriteConsole( ">", false );
				}
				else
					WriteConsole( "<" + type.Name + ">", false );
			}
			WriteConsole( "\n", false );
		}

		override public void AutomationCommand( string[] cmds, int index, ref string rCommand )
		{
			rCommand += " ";
			PrintParams();
		}
	}

	public class ParamCommand1<T> : ParamCommandBase
	{
		override protected Type[] Types { get { return types; } }
		Type[] types = new Type[1] { typeof( T ) };

		public ParamCommand1( OnParamCommand1<T> onParamCommand ) : base( onParamCommand ) { }
	}

	public class ParamCommand2<T, U> : ParamCommandBase
	{
		override protected Type[] Types { get { return types; } }
		Type[] types = new Type[2] { typeof( T ), typeof( U ) };

		public ParamCommand2( OnParamCommand2<T, U> onParamCommand ) : base( onParamCommand ) { }
	}

	public class ParamCommand3<T, U, V> : ParamCommandBase
	{
		override protected Type[] Types { get { return types; } }
		Type[] types = new Type[3] { typeof( T ), typeof( U ), typeof( V ) };

		public ParamCommand3( OnParamCommand3<T, U, V> onParamCommand ) : base( onParamCommand ) { }
	}

	public delegate void OnCommand(string command);
	public delegate void OnParamCommand1<T>( string command, T t );
	public delegate void OnParamCommand2<T, U>( string command, T t, U u );
	public delegate void OnParamCommand3<T, U, V>( string command, T t, U u, V v );

	public class CommandManager
	{
		protected bool mIsConsole = true;
		protected Command root = new Command( null );

		public CommandManager()
			: this( true )
		{
		}

		public CommandManager( bool is_console )
		{
			mIsConsole = is_console;
			root.IsConsole = is_console;
		}

		string strCommand = "";
		public virtual void Update()
		{
			while( Console.KeyAvailable == true )
			{
				ConsoleKeyInfo _ConsoleKeyInfo = Console.ReadKey();
				switch( _ConsoleKeyInfo.Key )
				{
					case ConsoleKey.Tab:
						AutomationCommand();
						break;

					case ConsoleKey.Enter:
						RunCommandConsole();
						break;

					case ConsoleKey.Backspace:
						if( strCommand.Length > 0 )
						{
							strCommand = strCommand.Substring( 0, strCommand.Length - 1 );
							Console.Write( "\n" + strCommand );
						}
						break;

					default:
						strCommand += _ConsoleKeyInfo.KeyChar;
						break;
				}
			}
		}

		void AutomationCommand()
		{
			Console.Write( '\n' );

			string[] cmds = strCommand.Split( seperators, StringSplitOptions.RemoveEmptyEntries );
			root.AutomationCommand( cmds, 0, ref strCommand );
			Console.Write( "> " + strCommand );
		}

		protected char[] seperators = new char[1] { ' ' };
		void RunCommandConsole()
		{
			Console.Write( '\n' );
			if( strCommand.Length == 0 )
				return;

			Console.WriteLine( "> " + strCommand );

			string[] cmds = strCommand.Split( seperators, StringSplitOptions.RemoveEmptyEntries );
			if( root.RunCommand( cmds, 0, eCommandAuthority.None ) == true )
			{
				Console.WriteLine( "Run Command : " + strCommand );
			}

			strCommand = "";
		}

		//------------------------------------------------------------------------		
		public string RunCommandTool( string strCommand, eCommandAuthority req_auth )
		{
			if( string.IsNullOrEmpty( strCommand ) )
				return "Command Empty";

			string[] cmds = strCommand.Split( seperators, StringSplitOptions.RemoveEmptyEntries );
			if( root.RunCommand( cmds, 0, req_auth ) == true )
				return "";

			return root.LastOutput;
		}

		public Command AddRootCommand( string command, eCommandAuthority authority )
		{
			Command cmd = root.AddCommand( command, null, authority );
			cmd.SetAuthority( authority );
			cmd.SetCommandName( command );
			cmd.IsConsole = root.IsConsole;

			return cmd;
		}

		public Command AddRootCommand( string command, OnCommand onCommand, eCommandAuthority authority )
		{
			Command cmd = root.AddCommand( command, onCommand, authority );
			cmd.SetAuthority( authority );
			cmd.SetCommandName( command );
			cmd.IsConsole = root.IsConsole;

			return cmd;
		}

		public Command AddRootParamCommand( string command, ParamCommandBase paramCommand, eCommandAuthority authority )
		{
			Command cmd = root.AddParamCommand( command, paramCommand, authority );
			cmd.SetAuthority( authority );
			cmd.SetCommandName( command );
			cmd.IsConsole = root.IsConsole;

			return cmd;
		}
	}
}
