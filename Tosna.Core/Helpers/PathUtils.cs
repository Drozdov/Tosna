using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tosna.Core.Helpers
{
	// TODO: move to another location

	public static class PathUtils
	{
		public static string GetRelativePath(DirectoryInfo destinationDirectory, DirectoryInfo currentDirectory)
		{
			var destinationHierarchy = GetHierarchy(destinationDirectory);
			var currentHierarchy = GetHierarchy(currentDirectory);

			while (destinationHierarchy.Any() && currentHierarchy.Any() &&
				   destinationHierarchy.First() == currentHierarchy.First())
			{
				destinationHierarchy.RemoveFirst();
				currentHierarchy.RemoveFirst();
			}

			var path = new List<string>();

			while (currentHierarchy.Any())
			{
				path.Add("..");
				currentHierarchy.RemoveFirst();
			}

			while (destinationHierarchy.Any())
			{
				path.Add(destinationHierarchy.First());
				destinationHierarchy.RemoveFirst();
			}

			return Path.Combine(path.ToArray());
		}

		public static string GetCommonPath(IEnumerable<DirectoryInfo> directoryInfos)
		{
			var hierarchies = directoryInfos.Select(GetHierarchy).Select(h => h.ToArray()).ToArray();

			if (!hierarchies.Any())
			{
				return string.Empty;
			}

			var path = new List<string>();

			for (var i = 0; ; i++)
			{
				string item = null;

				foreach (var hierarchy in hierarchies)
				{
					if (hierarchy.Length <= i || (item != null && hierarchy[i] != item))
					{
						return Path.Combine(path.ToArray());
					}
					item = hierarchy[i];
				}

				path.Add(item);
			}

		}
		
		public static string GetRelativePath(FileInfo destinationFile, DirectoryInfo currentDirectory)
		{
			var directory = GetRelativePath(destinationFile.Directory, currentDirectory);
			return Path.Combine(directory, destinationFile.Name);
		}

		public static string GetAbsolutePath(string relativePath, DirectoryInfo currentDirectory)
		{
			var relativePathItems = relativePath.Split('\\', '/');

			var currentHierarchy = GetHierarchy(currentDirectory);

			foreach (var item in relativePathItems)
			{
				switch (item)
				{
					case ".":
						break;

					case "..":
						currentHierarchy.RemoveLast();
						break;

					default:
						currentHierarchy.AddLast(item);
						break;
				}
			}

			return Path.Combine(currentHierarchy.ToArray());
		}

		private static LinkedList<string> GetHierarchy(DirectoryInfo directoryInfo)
		{
			var result = new LinkedList<string>();

			var currentDirectory = directoryInfo;

			while (currentDirectory.Parent != null)
			{
				result.AddFirst(currentDirectory.Name);
				currentDirectory = currentDirectory.Parent;
			}

			result.AddFirst(currentDirectory.Root.FullName);

			return result;
		}
	}
}
