using System.Collections.Generic;
using System.IO;
using Tosna.Core.Stamps;

namespace Tosna.Core.IO
{
	public class StampsEnvironment
	{
		public IReadOnlyCollection<Stamp> Stamps { get; }

		public FileInfo DefaultFilePath { get; }

		public StampsEnvironment(IReadOnlyCollection<Stamp> stamps, FileInfo defaultFilePath)
		{
			Stamps = stamps;
			DefaultFilePath = defaultFilePath;
		}
	}
}
