//////////////////////////////////////////////////////////////////////////
//
// CSVReader
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

using System.IO;
using System.Collections.Generic;

namespace UMF.Core
{
	public class CSVReader
	{
		Dictionary<string, int> m_ColumnHeaders = null;
		public Dictionary<string, int> ColumnHeaders { get { return m_ColumnHeaders; } }

		public class RowData
		{
			public List<string> datas = new List<string>();

            public override string ToString()
            {
				return string.Join( ",", datas );
            }
        }
		List<RowData> m_Rows = new List<RowData>();
		public int RowCount { get { return m_Rows.Count; } }
        public List<RowData> Rows { get { return m_Rows; } }

        public int GetFieldIndex( string field_name )
		{
			int index = -1;
			m_ColumnHeaders.TryGetValue( field_name, out index );
			return index;
		}

		public string GetData( int row_index, int column_index )
		{
			if( row_index >= m_Rows.Count )
				throw new System.Exception( $"over row : {row_index} > {m_Rows.Count}" );

			if( column_index >= m_Rows[row_index].datas.Count )
                throw new System.Exception( $"over column : {column_index} > {m_Rows[row_index].datas.Count}" );

            return m_Rows[row_index].datas[column_index];
		}

		public CSVReader()
		{
		}

		public CSVReader( string filepath )
		{
			Load( filepath );
		}

		public CSVReader( string filepath, int check_row_count )
		{
			Load( filepath );
			if( RowCount != check_row_count )
				throw new System.Exception( string.Format( "{0} RowCount{1} Error should be {2}", filepath, RowCount, check_row_count ) );
		}

        readonly char[] delimiter = ",\"".ToCharArray();

		public void Load( string filepath )
		{
			m_ColumnHeaders = null;
			m_Rows = null;

			StreamReader reader = new StreamReader( filepath );

			string strLine;
			int findIndex;

			while( reader.EndOfStream == false )
			{
				strLine = reader.ReadLine();
				if( string.IsNullOrEmpty( strLine.Trim() ) == true )
					break;

				string data;
				RowData _RowData = new RowData();

				for( int lastFindIndex = 0; lastFindIndex < strLine.Length; )
				{
					findIndex = strLine.IndexOfAny( delimiter, lastFindIndex );
					if( findIndex == -1 )
					{
						data = strLine.Substring( lastFindIndex );
						_RowData.datas.Add( data );
						break;
					}
					else
					{
						if( strLine[findIndex] == '\"' )
						{
							do
							{
								findIndex = strLine.IndexOf( '\"', findIndex + 1 );
							} while( strLine[findIndex - 1] != '\\' );
							findIndex = strLine.IndexOf( 'm' );
						}
						data = strLine.Substring( lastFindIndex, findIndex - lastFindIndex );
						lastFindIndex = findIndex + 1;
					}

					_RowData.datas.Add( data );
				}
				if( m_ColumnHeaders == null )
				{
					m_ColumnHeaders = new Dictionary<string, int>();
					m_Rows = new List<RowData>();

					for( int i = 0; i < _RowData.datas.Count; ++i )
					{
						m_ColumnHeaders.Add( _RowData.datas[i], i );
					}
				}
				else
					m_Rows.Add( _RowData );
			}
		}
	}
}