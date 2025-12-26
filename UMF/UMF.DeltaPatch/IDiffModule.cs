using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMF.DeltaPatch
{
	public interface IDiffModule
	{
		string OutputPath { get; }
		string DeltaExt { get; }

		void Make( string in_file_path, int version );
		void Patch( string input, string output, int version );
	}
}
