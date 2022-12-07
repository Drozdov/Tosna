using System;
using System.IO;
using Tosna.Core.Helpers;

namespace Tosna.Core.Imprints
{
	public sealed class ReferenceImprint : Imprint
	{
		public string ReferenceId { get; }

		public string ReferenceRelativePath { get; }

		public ReferenceImprint(Type type, string publicName, string id, ImprintInfo info, string referenceId, string referenceRelativePath) : base(type, publicName, id, info)
		{
			ReferenceId = referenceId;
			ReferenceRelativePath = referenceRelativePath;
		}

		public override void Accept(IImprintVisitor visitor)
		{
			visitor.Visit(this);
		}

		public string AbsoluteReferencePath
		{
			get
			{
				if (!TryGetInfo(out var info))
				{
					throw new InvalidOperationException("Cannot get absolute path for reference. No info");
				}

				if (string.IsNullOrEmpty(ReferenceRelativePath))
				{
					return info.FilePath;
				}

				var directoryName = Path.GetDirectoryName(info.FilePath);

				if (directoryName == null)
				{
					throw new InvalidOperationException("Cannot get directory name form imprint info.");
				}

				return PathUtils.GetAbsolutePath(ReferenceRelativePath, new DirectoryInfo(directoryName));
			}
		}
	}
}