//////////////////////////////////////////////////////////////////////////
//
// DeltaPatch
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMF.DeltaPatch.Octodiff;

namespace UMF.DeltaPatch
{
	public class DeltaPatch
	{
		public enum eDiffType
		{
			Octodiff,
		}

		string mBasePath = "";
		eDiffType mDiffType = eDiffType.Octodiff;
		IDiffModule mDiffModule = null;

		//------------------------------------------------------------------------
		public DeltaPatch( string base_path, eDiffType diff_type = eDiffType.Octodiff )
		{
			mBasePath = base_path;
			mDiffType = diff_type;

			switch( mDiffType )
			{
				case eDiffType.Octodiff:
					mDiffModule = new DiffModule_Octodiff();
					break;

				default:
					throw new Exception( $"Diff type({mDiffType}) module invalid" );
			}
		}

		//------------------------------------------------------------------------
		public void MAKE( string in_file, int version )
		{
			if( version < 0 )
				throw new Exception( $"version invalid. -{version}" );

			string in_file_path = Path.Combine( mBasePath, in_file );
			if( File.Exists( in_file_path ) == false )
				throw new Exception( $"Make file not found. - {in_file_path}" );

			mDiffModule.Make( Path.Combine( mBasePath, in_file ), version );
		}

		//------------------------------------------------------------------------
		public void PATCH( string in_file, string out_file, int version )
		{
			mDiffModule.Patch( in_file, out_file, version );
		}

	}
}
