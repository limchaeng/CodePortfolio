//////////////////////////////////////////////////////////////////////////
//
// SpriteSelectorBase
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
using UnityEngine;

namespace UMF.Unity
{
	public abstract class SpriteSelectorBase : MonoBehaviour
	{
		public string m_SpritePrefix = "";

		[System.Serializable]
		public class SpriteData
		{
			public Sprite m_Sprite = null;
			public string m_FixName = "";
		}

		public List<SpriteData> m_SpriteDataList = new List<SpriteData>();
		public int Count { get { return m_SpriteDataList.Count; } }
		public int SelectedIDX { get; private set; } = 0;

		//------------------------------------------------------------------------
		public virtual void UpdateSprite( Sprite sprite ) { }
		public virtual Sprite GetSprite() { return null; }

		//------------------------------------------------------------------------
		public void Select( int idx )
		{
			if( m_SpriteDataList.Count == 0 || m_SpriteDataList.Count <= idx )
				return;

			SelectedIDX = idx;
			UpdateSprite( m_SpriteDataList[idx].m_Sprite );
		}

		//------------------------------------------------------------------------
		public void Select( string _name )
		{
			string full_name = string.Format( "{0}{1}", m_SpritePrefix, _name );

			SpriteData data = m_SpriteDataList.Find( a => a.m_Sprite != null && a.m_Sprite.name == full_name );
			if( data != null )
			{
				SelectedIDX = m_SpriteDataList.FindIndex( a => a == data );
				UpdateSprite( data.m_Sprite );
			}
		}

		//------------------------------------------------------------------------
		public void SelectKey( string _key )
		{
			SpriteData data = m_SpriteDataList.Find( a => a.m_FixName == _key );
			if( data != null )
			{
                SelectedIDX = m_SpriteDataList.FindIndex( a => a == data );
                UpdateSprite( data.m_Sprite );
			}
		}

		//------------------------------------------------------------------------
		public abstract float alpha { get; set; }

		//------------------------------------------------------------------------
		public abstract Color color { get; set; }
	}
}
