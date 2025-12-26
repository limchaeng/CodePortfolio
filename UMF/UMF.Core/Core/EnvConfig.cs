//////////////////////////////////////////////////////////////////////////
//
// EnvConfig
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
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace UMF.Core
{
	public class EnvConfig : EnvironmentProperty
	{
		Dictionary<string, string> mConfigLogDic = new Dictionary<string, string>();

		public void ConfigLoad( string env_name, string config_text )
		{
			LoadProperty( env_name, config_text );
			ConfigLoad();
		}
		public void ConfigLoad( string env_file )
		{
			LoadPropertyFile( env_file );
			ConfigLoad();
		}

		//------------------------------------------------------------------------
		protected virtual void ConfigLoad()
		{
			Log.Write( "Load Config : " + mEnvName );

			mConfigLogDic.Clear();

			Type type = GetType();

			FieldInfo[] field_infos = type.GetFields();
			foreach( FieldInfo field in field_infos )
			{
				object value_obj = field.GetValue( this );

				string field_name = string.Format( "{0}({1})", field.Name, field.FieldType.Name );
				string field_value = "_DEF:null";
				if( value_obj != null )
					field_value = "_Def:" + value_obj.ToString();

				string str_value = GetEnvironmentValue( field.Name );
				if( string.IsNullOrEmpty( str_value ) )
				{
					mConfigLogDic.Add( field_name, field_value );
					continue;
				}

				try
				{
					value_obj = null;
					if( field.FieldType.IsEnum )
						value_obj = Enum.Parse( field.FieldType, str_value );
					else
						value_obj = Convert.ChangeType( str_value, field.FieldType );

					field.SetValue( this, value_obj );

					field_value = "_NULL";
					if( value_obj != null )
						field_value = value_obj.ToString();
				}
				catch( System.InvalidCastException )
				{
					// do nothing
					field_value = "_InvalidCast";
				}
				catch( System.Exception ex )
				{
					field_value = "_EX:" + ex.Message;
					Log.WriteWarning( "EnvConfig:{0} - {1}", mEnvName, ex.ToString() );
				}

				mConfigLogDic.Add( field_name, field_value );
			}
		}

		//------------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "========================================" );
			sb.AppendLine( string.Format( "Type:{0} Env:{0}", GetType().FullName, mEnvName ) );

			foreach( var kvp in mConfigLogDic )
			{
				sb.AppendLine( string.Format( "{0} = {1}", kvp.Key, kvp.Value ) );
			}

			return sb.ToString();
		}
	}
}
