using System.IO;

namespace Tosna.Core.Stamps
{
	public class StampImprint
	{
		private readonly FileInfo defaultFilePath;

		public Stamp Stamp { get; }

		public StampImprint Parent { get; set; }

		public FileInfo FilePath => Parent != null ? Parent.FilePath : defaultFilePath;

		public string Id { get; set; }
		
		public string PublicName { get; set; }

		public bool Inlined { get; set; }

		public StampImprint(Stamp stamp, FileInfo defaultFilePath)
		{
			Stamp = stamp;
			Id = stamp.UserId;
			PublicName = stamp.UserPublicName;
			this.defaultFilePath = defaultFilePath;
		}
	}
}