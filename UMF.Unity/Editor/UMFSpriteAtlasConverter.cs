#define NO_UNITY_2D_SPRITES
#if !NO_UNITY_2D_SPRITES
//////////////////////////////////////////////////////////////////////////
//
// UMFSpriteAtlasConverter
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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using System.IO;
using System;
using UnityEditor.Sprites;
using System.Linq;

namespace UMF.Unity.EditorUtil
{
	public class UMFSpriteAtlasConverter
	{
		[MenuItem( "Assets/UMF/Sprite/SpriteAtlas to SpriteSheet", true )]
		static bool Sprite_Atlas_2_SpriteSheet()
		{
			return Selection.activeObject.GetType() == typeof( SpriteAtlas );
		}

		//------------------------------------------------------------------------
		public class UMFSpriteAtlasRectData
		{
			// in
			public Sprite source_sprite;

			// out
			public Texture2D readable_texture;
			public string out_name = "";
			public Rect rect;

			public UMFSpriteAtlasRectData( Sprite sprite )
			{
				source_sprite = sprite;
				readable_texture = GetReadableTexture( source_sprite.texture );
				out_name = source_sprite.name.Replace( "(Clone)", "" );
			}

			public void UpdateFromPackRect( Texture2D pack_texture, Rect pack_rect )
			{
				rect = new Rect( pack_rect.x * pack_texture.width, pack_rect.y * pack_texture.height, pack_rect.width * pack_texture.width, pack_rect.height * pack_texture.height );
			}
		}
		[MenuItem( "Assets/UMF/Sprite/SpriteAtlas to SpriteSheet" )]
		static void Sprite_Atlas_2_SpriteSheet( MenuCommand command )
		{
			SpriteAtlas atlas = Selection.activeObject as SpriteAtlas;
			int padding = atlas.GetPackingSettings().padding;

			Debug.Log( $"# SpriteAtlas to SpriteSheet : {atlas.name}" );
			Debug.Log( $"- sprite count = {atlas.spriteCount}" );

			string export_path = Path.GetDirectoryName( AssetDatabase.GetAssetPath( atlas ) ).NormalizeSlashPath();

			//Pack sprites
			Sprite[] sprites = new Sprite[atlas.spriteCount];
			atlas.GetSprites( sprites );

			UMFSpriteAtlasRectData[] sprite_datas = new UMFSpriteAtlasRectData[sprites.Length];
			for( int i = 0; i < sprites.Length; i++ )
			{
				sprite_datas[i] = new UMFSpriteAtlasRectData( sprites[i] );
			}

			Texture2D sp_sheet_texture = new Texture2D( 1, 1, TextureFormat.RGBA32, false );
			Rect[] pack_rects = sp_sheet_texture.PackTextures( sprite_datas.Select( a => a.readable_texture ).ToArray(), padding, 4096 );
			SpriteRect[] sp_sheet_rects = new SpriteRect[pack_rects.Length];

			for( int i = 0; i < pack_rects.Length; i++ )
			{
				sprite_datas[i].UpdateFromPackRect( sp_sheet_texture, pack_rects[i] );

				SpriteRect sp_rect = new SpriteRect();
				sp_rect.alignment = SpriteAlignment.Center;
				sp_rect.name = sprite_datas[i].out_name;
				sp_rect.rect = sprite_datas[i].rect;
				sp_rect.pivot = new Vector2( 0.5f, 0.5f );
				sp_rect.spriteID = GUID.Generate();

				sp_sheet_rects[i] = sp_rect;
			}

			//Save image
			var path = AssetDatabase.GetAssetPath( atlas );
			var png_path = path.Replace( ".spriteatlas", "_sheet.png" );

			Debug.Log( $"Create sprite from atlas: {atlas.name} path: {path}" );

			byte[] bytes = sp_sheet_texture.EncodeToPNG();

			var temp_png_path = png_path;//To prevent bug in number of naming
			int j = 1;
			while( File.Exists( temp_png_path ) )
			{
				temp_png_path = Path.Combine( Path.GetDirectoryName( png_path ), $"{Path.GetFileNameWithoutExtension( png_path )} {j}{Path.GetExtension( png_path )}" );
				j++;
			}
			png_path = temp_png_path;

			File.WriteAllBytes( png_path, bytes );

			//Update sprite settings
			AssetDatabase.Refresh();

			TextureImporter ti = AssetImporter.GetAtPath( png_path ) as TextureImporter;
			ti.textureType = TextureImporterType.Sprite;
			ti.spriteImportMode = SpriteImportMode.Multiple;

			SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
			factory.Init();
			ISpriteEditorDataProvider data_provider = factory.GetSpriteEditorDataProviderFromObject( ti );
			data_provider.InitSpriteEditorDataProvider();
			data_provider.SetSpriteRects( sp_sheet_rects );
			data_provider.Apply();

			EditorUtility.SetDirty( ti );
			ti.SaveAndReimport();
		}

		//------------------------------------------------------------------------
		private static Texture2D GetReadableTexture( Texture2D source )
		{
			RenderTexture tmp = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.ARGB32,
				RenderTextureReadWrite.sRGB );

			Graphics.Blit( source, tmp );
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = tmp;
			Texture2D result = new Texture2D( source.width, source.height );
			result.ReadPixels( new Rect( 0, 0, tmp.width, tmp.height ), 0, 0 );
			result.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary( tmp );
			return result;
		}
	}
}
#endif