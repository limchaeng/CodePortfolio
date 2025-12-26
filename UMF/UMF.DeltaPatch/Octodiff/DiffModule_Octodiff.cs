using Octodiff.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMF.DeltaPatch.Octodiff
{
	internal class DiffModule_Octodiff : IDiffModule
	{
		const string OUTPUT_PATH = "_octodiff";
		const string SIG_EXT = ".octosig";
		const string DELTA_EXT = ".octodelta";

		public string OutputPath => OUTPUT_PATH;
		public string DeltaExt => DELTA_EXT;

		//------------------------------------------------------------------------
		public void Make( string in_file_path, int version )
		{
			string output_root = Path.Combine( Path.GetDirectoryName( in_file_path ), OUTPUT_PATH );

			// 0. check
			string prev_version_sig_path = "";
			if( version > 0 )
			{
				int prev_version = version - 1;
				prev_version_sig_path = Path.Combine( output_root, prev_version.ToString(), $"{prev_version}{SIG_EXT}" );
				if( File.Exists( prev_version_sig_path ) == false )
					throw new Exception( $"not found previous version({prev_version}) signature file. -{prev_version_sig_path}" );
			}

			// 1. create new signature
			string output_dir = Path.Combine( output_root, version.ToString() );
			if( Directory.Exists( output_dir ) == false )
				Directory.CreateDirectory( output_dir );

			string signature_file_path = Path.Combine( output_dir, $"{version}{SIG_EXT}" );

			SignatureBuilder signature_builder = new SignatureBuilder();
			using( FileStream basis_fs = new FileStream( in_file_path, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				using( FileStream signature_fs = new FileStream( signature_file_path, FileMode.Create, FileAccess.Write, FileShare.Read ) )
				{
					signature_builder.Build( basis_fs, new SignatureWriter( signature_fs ) );
				}
			}

			// check
			if( File.Exists( signature_file_path ) == false )
				throw new Exception( $"signature build invalid. - {signature_file_path}" );

			// 2. create delta
			if( version > 0 )
			{
				string delta_file_path = Path.Combine( output_dir, $"{version}{DELTA_EXT}" );
				DeltaBuilder delta_builder = new DeltaBuilder();
			}
		}

		//------------------------------------------------------------------------
		public void Patch( string input, string output, int version )
		{
			
		}

	}
}
