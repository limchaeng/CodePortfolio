using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UMF.Core;

namespace UMTools.TBLExport
{
	public class TBLExportTranslate
	{
		readonly string SUPPORT_LANGUAGE_FILE = "_support_language.xml";

		StreamWriter mLogWriter = null;
		StreamWriter mErrorWriter = null;
		StreamWriter mTagIssueWriter = null;
		StreamWriter mMergeEqualWriter = null;
		StreamWriter mDuplicatedWriter = null;

		public class LanguageCultureData
		{
			public string language;
			public string culture_code;

			public LanguageCultureData( string lan, string c_code )
			{
				language = lan;
				culture_code = c_code;
			}

			public override string ToString()
			{
				return language + "(" + culture_code + ")";
			}
		}

		List<LanguageCultureData> mSupportedLanguages = new List<LanguageCultureData>();
		public List<LanguageCultureData> SupportedLanguages { get { return mSupportedLanguages; } }

		FormTBLExport mMain = null;
		public FormTBLExport Main { get { return mMain; } }

		string MAKE_DATE = "";
		string LOG_PATH = "";
		string LOG_SUFFIX = "";

		//------------------------------------------------------------------------	
		public TBLExportTranslate( FormTBLExport main_form )
		{
			mMain = main_form;
		}

		//------------------------------------------------------------------------
		public void SetLog( string logpath, string suffix )
		{
			MAKE_DATE = System.DateTime.Now.ToString( "yyyyMMdd_HHmmss" );
			LOG_PATH = logpath;
			LOG_SUFFIX = suffix;
		}

		//------------------------------------------------------------------------	
		public void UnSetLog()
		{
			if( mLogWriter != null )
			{
				mLogWriter.Flush();
				mLogWriter.Close();
			}

			if( mErrorWriter != null )
			{
				mErrorWriter.Flush();
				mErrorWriter.Close();
			}

			if( mTagIssueWriter != null )
			{
				mTagIssueWriter.Flush();
				mTagIssueWriter.Close();
			}

			if( mMergeEqualWriter != null )
			{
				mMergeEqualWriter.Flush();
				mMergeEqualWriter.Close();
			}

			if( mDuplicatedWriter != null )
			{
				mDuplicatedWriter.Flush();
				mDuplicatedWriter.Close();
			}

			mLogWriter = null;
			mErrorWriter = null;
			mTagIssueWriter = null;
			mMergeEqualWriter = null;
			mDuplicatedWriter = null;
		}

		//------------------------------------------------------------------------	
		public void Log( string fmt, params object[] parms )
		{
			string log = "";
			if( parms == null || parms.Length <= 0 )
				log = fmt;
			else
				log = string.Format( fmt, parms );

			if( mLogWriter == null )
			{
				try
				{
					string logfilename = Path.Combine( LOG_PATH, MAKE_DATE + "_" + LOG_SUFFIX + ".log" );
					if( File.Exists( logfilename ) )
						File.Delete( logfilename );

					FileStream fs = new FileStream( logfilename, FileMode.CreateNew, FileAccess.Write );
					mLogWriter = new StreamWriter( fs, System.Text.Encoding.UTF8 );
				}
				catch( System.Exception ex )
				{
					mLogWriter = null;
					mMain.LogWrite_2_List( FormTBLExport.eLogType.Error, ex.ToString() );
				}
			}

			if( mLogWriter != null )
			{
				mLogWriter.WriteLine( log );
				mLogWriter.Flush();
			}
		}
		public void Error( string fmt, params object[] parms )
		{
			string log = "";
			if( parms == null || parms.Length <= 0 )
				log = fmt;
			else
				log = string.Format( fmt, parms );

			if( mErrorWriter == null )
			{
				try
				{
					string logfilename_e = Path.Combine( LOG_PATH, MAKE_DATE + "_" + LOG_SUFFIX + "_error.log" );
					if( File.Exists( logfilename_e ) )
						File.Delete( logfilename_e );
					FileStream fs_e = new FileStream( logfilename_e, FileMode.CreateNew, FileAccess.Write );
					mErrorWriter = new StreamWriter( fs_e, System.Text.Encoding.UTF8 );
				}
				catch( System.Exception ex )
				{
					mErrorWriter = null;
					mMain.LogWrite_2_List( FormTBLExport.eLogType.Error, ex.ToString() );
				}
			}

			if( mErrorWriter != null )
			{
				mErrorWriter.WriteLine( log );
				mErrorWriter.Flush();
			}
		}

		//------------------------------------------------------------------------	
		public void LogTranslateTagIssue( string fmt, params object[] parms )
		{
			string log = "";
			if( parms == null || parms.Length <= 0 )
				log = fmt;
			else
				log = string.Format( fmt, parms );

			if( mTagIssueWriter == null )
			{
				try
				{
					string filename = Path.Combine( LOG_PATH, MAKE_DATE + "_" + LOG_SUFFIX + "_TAGISSUE.log" );
					if( File.Exists( filename ) )
						File.Delete( filename );

					FileStream fs = new FileStream( filename, FileMode.CreateNew, FileAccess.Write );
					mTagIssueWriter = new StreamWriter( fs, System.Text.Encoding.UTF8 );

				}
				catch( System.Exception ex )
				{
					mTagIssueWriter = null;
					mMain.LogWrite_2_List( FormTBLExport.eLogType.Error, ex.ToString() );
				}
			}

			if( mTagIssueWriter != null )
			{
				mTagIssueWriter.WriteLine( log );
				mTagIssueWriter.Flush();
			}
		}

		//------------------------------------------------------------------------	
		public void LogTranslateEqual( string fmt, params object[] parms )
		{
			string log = "";
			if( parms == null || parms.Length <= 0 )
				log = fmt;
			else
				log = string.Format( fmt, parms );

			if( mMergeEqualWriter == null )
			{
				try
				{
					string equal_log_filename = Path.Combine( LOG_PATH, MAKE_DATE + "_" + LOG_SUFFIX + "_EQUAL.log" );
					if( File.Exists( equal_log_filename ) )
						File.Delete( equal_log_filename );

					FileStream fs = new FileStream( equal_log_filename, FileMode.CreateNew, FileAccess.Write );
					mMergeEqualWriter = new StreamWriter( fs );
				}
				catch( System.Exception ex )
				{
					mMergeEqualWriter = null;
					mMain.LogWrite_2_List( FormTBLExport.eLogType.Error, ex.ToString() );
				}
			}

			if( mMergeEqualWriter != null )
			{
				mMergeEqualWriter.WriteLine( log );
				mMergeEqualWriter.Flush();
			}
		}

		//------------------------------------------------------------------------	
		public void LogDuplicated( string fmt, params object[] parms )
		{
			string log = "";
			if( parms == null || parms.Length <= 0 )
				log = fmt;
			else
				log = string.Format( fmt, parms );

			if( mDuplicatedWriter == null )
			{
				try
				{
					string dup_log_filename = Path.Combine( LOG_PATH, MAKE_DATE + "_" + LOG_SUFFIX + "_DUPLICATED.log" );
					if( File.Exists( dup_log_filename ) )
						File.Delete( dup_log_filename );

					FileStream fs = new FileStream( dup_log_filename, FileMode.CreateNew, FileAccess.Write );
					mDuplicatedWriter = new StreamWriter( fs );
				}
				catch( System.Exception ex )
				{
					mDuplicatedWriter = null;
					mMain.LogWrite_2_List( FormTBLExport.eLogType.Error, ex.ToString() );
				}
			}

			if( mDuplicatedWriter != null )
			{
				mDuplicatedWriter.WriteLine( log );
				mDuplicatedWriter.Flush();
			}
		}


		//------------------------------------------------------------------------	
		public void RefreshSupportedLanguage( string tbl_path )
		{
			mSupportedLanguages.Clear();

			string support_lan_file = Path.Combine( tbl_path, SUPPORT_LANGUAGE_FILE );
			if( File.Exists( support_lan_file ) )
			{
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load( support_lan_file );

					XmlNode root = doc.SelectSingleNode( "SupportLanguages" );

					foreach( XmlNode data_node in root.ChildNodes )
					{
						if( data_node.NodeType == XmlNodeType.Comment )
							continue;

						string lan = XMLUtil.ParseAttribute<string>( data_node, "Language", "" );
						if( string.IsNullOrEmpty( lan ) == false )
						{
							string culture = XMLUtil.ParseAttribute<string>( data_node, "Culture", "" );
							LanguageCultureData data = new LanguageCultureData( lan, culture );
							mSupportedLanguages.Add( data );
						}
					}

				}
				catch( System.Exception ex )
				{
					mMain.LogWrite_2_List( FormTBLExport.eLogType.Error, ex.ToString() );
				}
			}
		}
	}
}
