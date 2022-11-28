using System.IO;

namespace Tosna.Core.Common.Stamps
{
	public class StampImprint
	{
		private readonly FileInfo defaultFilePath;

		public Stamp Stamp { get; }

		public StampImprint Parent { get; set; }

		public FileInfo FilePath => Parent != null ? Parent.FilePath : defaultFilePath;

		public string Id { get; set; }

		public bool Inlined { get; set; }

		public StampImprint(Stamp stamp, FileInfo defaultFilePath)
		{
			Stamp = stamp;
			Id = stamp.UserId;
			this.defaultFilePath = defaultFilePath;
		}
	}
}