//////////////////////////////////////////////////////////////////////////
//
// UIImageSelector
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
using UnityEngine.UI;

[System.Obsolete("대신 UISpriteSelector 를 사용하세요")]
[RequireComponent(typeof(Image))]
public class UIImageSelector : MonoBehaviour
{
	public string m_SpritePrefix = "";
	public List<Sprite> m_SpriteList = new List<Sprite>();

	Image mImage = null;
	public Image GetImage { get { return mImage; } }

	//------------------------------------------------------------------------
	private void Awake()
	{
		mImage = gameObject.GetComponent<Image>();
	}

	//------------------------------------------------------------------------
	public void Select( int idx )
	{
		if( mImage == null )
			return;

		if( m_SpriteList.Count == 0 || m_SpriteList.Count <= idx )
			return;

		mImage.sprite = m_SpriteList[idx];
	}

	//------------------------------------------------------------------------
	public void Select( string _name )
	{
		if( mImage == null )
			return;

		string full_name = string.Format( "{0}{1}", m_SpritePrefix, _name );

		Sprite sprite = m_SpriteList.Find( a => a.name == full_name );
		if( sprite != null )
			mImage.sprite = sprite;
	}
}
