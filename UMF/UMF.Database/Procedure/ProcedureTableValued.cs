//////////////////////////////////////////////////////////////////////////
//
// ProcedureTableValued
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

using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UMF.Database.MSSql;

namespace UMF.Database
{
	//------------------------------------------------------------------------	
	public interface IProcedureTableValued
	{
		int Count { get; }
		void GetDumpRecord( System.Action<int, int, object> call );
	}

	//------------------------------------------------------------------------
	public class MetaFieldData
	{
		public bool IsPrimitive = false;
		public bool IsString = false;
		public FieldInfo[] field_list = null;
		public SqlMetaData[] sql_metadata = null;

		public MetaFieldData( System.Type type )
		{
			if( type.IsPrimitive )
			{
				IsPrimitive = true;
				sql_metadata = new SqlMetaData[1];
				sql_metadata[0] = new SqlMetaData( "value", SqlCommand_MSSql.ConvertToSqlDbType( type ) );
			}
			else
			{
				field_list = type.GetFields();
				sql_metadata = new SqlMetaData[field_list.Length];
				for( int i = 0; i < field_list.Length; i++ )
				{
					FieldInfo field = field_list[i];

					TableValuedAttribute attr = field.GetCustomAttribute<TableValuedAttribute>();
					if( attr != null && attr.MaxLength != -1 )
						sql_metadata[i] = new SqlMetaData( field.Name, SqlCommand_MSSql.ConvertToSqlDbType( field.FieldType ), attr.MaxLength );
					else
						sql_metadata[i] = new SqlMetaData( field.Name, SqlCommand_MSSql.ConvertToSqlDbType( field.FieldType ) );
				}
			}
		}
	}

	//------------------------------------------------------------------------
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class TableValuedAttribute : System.Attribute
	{
		public int MaxLength { get; set; } = -1;
	}

	//------------------------------------------------------------------------	
	class TABLEVALUEDMETA<T>
	{
		static MetaFieldData mMetadataCached;
		static object _lock_obj = new object();
		public static MetaFieldData MetadataCached
		{
			get
			{
				if( mMetadataCached == null )
				{
					lock( _lock_obj )
					{
						if( mMetadataCached == null )
						{
							mMetadataCached = new MetaFieldData( typeof( T ) );
						}
					}
				}

				return mMetadataCached;
			}
		}

	}

	//------------------------------------------------------------------------	
	/// <summary>
	///   Only for MSSql
	///   - MySql does not support
	/// </summary>
	public class ProcedureTableValuedBase<T> : IEnumerable<SqlDataRecord>, IEnumerator<SqlDataRecord>, IProcedureTableValued
	{
		public List<T> list;
		protected int index = -1;

		protected System.Type mType = null;

		public int Count
		{
			get
			{
				if( list == null )
					return 0;
				return list.Count;
			}
		}

		public ProcedureTableValuedBase( List<T> list )
		{
			mType = typeof( T );

			this.list = new List<T>();
			if( list != null )
				this.list.AddRange( list );

			Reset();
		}

		public ProcedureTableValuedBase()
		{
			Reset();
		}

		public void Dispose()
		{
			Reset();
		}

		public virtual SqlDataRecord Current
		{
			get
			{
				if( index >= list.Count )
					return null;

				MetaFieldData meta_cache = TABLEVALUEDMETA<T>.MetadataCached;

				SqlDataRecord outrec = new SqlDataRecord( meta_cache.sql_metadata );
				object list_obj = list[index];
				if( meta_cache.IsPrimitive )
				{
					outrec.SetValue( 0, list_obj );
				}
				else
				{
					for( int i = 0; i < meta_cache.sql_metadata.Length; i++ )
					{
						outrec.SetValue( i, meta_cache.field_list[i].GetValue( list_obj ) );
					}
				}
				return outrec;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		public bool MoveNext()
		{
			return ++index < list.Count;
		}

		public void Reset()
		{
			index = -1;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this;
		}

		public System.Collections.Generic.IEnumerator<SqlDataRecord> GetEnumerator()
		{
			return this;
		}

		public void GetDumpRecord( System.Action<int, int, object> call )
		{
			for( int i = 0; i < list.Count; i++ )
			{
				index = i;
				SqlDataRecord record = Current;

				for( int r = 0; r < record.FieldCount; r++ )
				{
					call( i, r, record.GetValue( r ) );
				}
			}

			Reset();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	///
	public class tv_bigint_list : ProcedureTableValuedBase<long>
	{
		public tv_bigint_list( List<long> list ) : base( list ) { }
	}

	public class tv_int_list : ProcedureTableValuedBase<int>
	{
		public tv_int_list( List<int> list ) : base( list ) { }
	}

	public class tv_smallint_list : ProcedureTableValuedBase<short>
	{
		public tv_smallint_list( List<short> list ) : base( list ) { }
	}

	public class tv_tinyint_list : ProcedureTableValuedBase<byte>
	{
		public tv_tinyint_list( List<byte> list ) : base( list ) { }
	}

	//------------------------------------------------------------------------
	public class db_tinystr_list
	{
		public byte n;
		public string str;
	}
	public class tv_tinystr_list : ProcedureTableValuedBase<db_tinystr_list>
	{
		public tv_tinystr_list( List<db_tinystr_list> list ) : base( list ) { }
	}
}
