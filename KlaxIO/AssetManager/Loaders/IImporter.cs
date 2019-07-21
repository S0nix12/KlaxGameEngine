using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxIO.AssetManager.Loaders
{
	public interface IImporter
	{
		bool Import(string filename, string importPath, bool bAlwaysImport = false);
		bool ImportAsync(string filename, string importPath, bool bAlwaysImport = false);
		string[] GetSupportedFormats();
	}
}
