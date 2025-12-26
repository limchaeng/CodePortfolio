//////////////////////////////////////////////////////////////////////////
//
// ScriptTemplateCreate
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

using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace UMF.Unity.EditorUtil
{
	public class ScriptTemplateCreate
	{
		public class DoCreateCode : EndNameEditAction
		{
			public override void Action( int instanceId, string pathName, string resourceFile )
			{
				Object o = ScriptTemplateCreate.CreateScript( pathName, resourceFile );
				if( o != null )
					ProjectWindowUtil.ShowCreatedAsset( o );
			}
		}

		private static Texture2D scriptIcon = ( EditorGUIUtility.IconContent( "cs Script Icon" ).image as Texture2D );

		//------------------------------------------------------------------------	
		[MenuItem( "UMF/Script/Create Empty(Default)" )]
		[MenuItem( "Assets/UMF/Script/Create Empty Script(Default)", false, 0 )]
		static void CreateEmptyScriptDefault()
		{
			List<string> template_path_list = new List<string>()
			{
				"Assets/Plugins/UMF.Unity/Editor/_ScriptTemplate.cs.txt",
				"Assets/UMF.Unity/Editor/_ScriptTemplate.cs.txt",
				"Packages/com.fournext.umf.unity/Editor/_ScriptTemplate.cs.txt",
			};
			string valid_path = "";
			foreach( string path in template_path_list )
			{
				if( File.Exists( path ) )
				{
					valid_path = path;
					break;
				}
			}

			string script_name = "NewScript.cs";

			CreateScriptFrom( script_name, valid_path, TemplateReplaceFunc );
			string TemplateReplaceFunc( string source )
			{
				string replaced = source;
				replaced = replaced.Replace( "#AUTHORNAME#", "LCY." );
				replaced = replaced.Replace( "#COPYRIGHT#", $"Copyright {System.DateTime.Now.Year} FourNext" );
				return replaced;
			}
		}

		//------------------------------------------------------------------------
		/// <summary>
		///   <paramref name="_template_path"/> 템플릿 파일 경로
		///   <paramref name="replace_fun"/> 템플릿에서 변경해야하는 항목
		/// </summary>
		/// 
		static System.Func<string, string> mLastReplaceFunc = null;			

        public static void CreateScriptFrom( string _name, string _template_path, System.Func<string,string> replace_func )
		{
			mLastReplaceFunc = replace_func;
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists( 0, ScriptableObject.CreateInstance<DoCreateCode>(), _name, scriptIcon, _template_path );
		}

		//------------------------------------------------------------------------
		internal static Object CreateScript( string path_name, string template_path )
		{
			string class_name = Path.GetFileNameWithoutExtension( path_name ).Replace( " ", System.String.Empty );
			string template_text = System.String.Empty;

			if( File.Exists( template_path ) )
			{
				UTF8Encoding encoding = new UTF8Encoding( true, false );

				StreamReader sr = new StreamReader( template_path );
				template_text = sr.ReadToEnd();
				sr.Close();

				template_text = template_text.Replace( "#SCRIPTNAME#", class_name );
				if( mLastReplaceFunc != null )
					template_text = mLastReplaceFunc( template_text );

				StreamWriter sw = new StreamWriter( Path.GetFullPath( path_name ), false, encoding );
				sw.Write( template_text );
				sw.Close();

				AssetDatabase.ImportAsset( path_name );
				return AssetDatabase.LoadAssetAtPath( path_name, typeof( Object ) );
			}
			else
			{
				Debug.LogError( $"template file not found : {template_path}" );
				return null;
			}
		}
	}
}
