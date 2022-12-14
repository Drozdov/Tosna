using System.Collections.Generic;
using System.Linq;
using Tosna.Core;
using Tosna.Core.Helpers.Xml;
using Tosna.Core.SerializationInterfaces;
using Tosna.Editor.IDE.ProblemsDetailing;

namespace Tosna.Editor.IDE
{
	public class FilesManager
	{
		private readonly IDictionary<string, SingleFileManager> fileManagers = new Dictionary<string, SingleFileManager>();
		private readonly XmlProblemsDetailsGetter xmlProblemsDetailsGetter;

		public IReadOnlyCollection<SingleFileManager> FileManagers => fileManagers.Values.ToArray();

		public ImprintsSerializer Serializer { get; }

		public FilesManager(ISerializingElementsManager serializingElementsManager,
			ISerializingTypesResolver serializingTypesResolver)
		{
			Serializer = new ImprintsSerializer(serializingElementsManager, serializingTypesResolver);
			xmlProblemsDetailsGetter = new XmlProblemsDetailsGetter(serializingElementsManager, serializingTypesResolver);
		}

		public IReadOnlyCollection<SingleFileManager> AddFiles(IEnumerable<string> files)
		{
			var result = new List<SingleFileManager>();
			foreach (var file in files)
			{
				if (fileManagers.ContainsKey(file))
				{
					continue;
				}

				var singleFileManager = new SingleFileManager(file, Serializer, xmlProblemsDetailsGetter);
				fileManagers[file] = singleFileManager;
				result.Add(singleFileManager);
			}
			VerifyDependencies();
			return result;
		}

		public IReadOnlyCollection<SingleFileManager> AddFilesWithDependencies(IEnumerable<string> files)
		{
			var result = new List<SingleFileManager>();
			var presentFiles = new HashSet<string>(fileManagers.Keys);
			var filesToAdd = new List<string>(files);

			while (filesToAdd.Any())
			{
				var newDependencies = new List<string>();

				foreach (var file in filesToAdd)
				{
					if (presentFiles.Contains(file))
					{
						continue;
					}

					presentFiles.Add(file);
					var singleFileManager = new SingleFileManager(file, Serializer, xmlProblemsDetailsGetter);
					fileManagers[file] = singleFileManager;
					result.Add(singleFileManager);

					newDependencies.AddRange(singleFileManager.DependenciesFiles);
				}

				filesToAdd = newDependencies;
			}
			VerifyDependencies();
			return result;
		}

		public void DeleteFiles(IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				fileManagers.Remove(file);
			}
			VerifyDependencies();
		}

		public void Clear()
		{
			fileManagers.Clear();
		}

		public void VerifyDependencies()
		{
			var singleFileManagers = FileManagers;

			var stamps = singleFileManagers.SelectMany(fileManager => fileManager.Imprints).ToArray();

			foreach (var singleFileManager in singleFileManagers)
			{
				singleFileManager.VerifyDependencies(stamps);
			}
		}

		public SingleFileManager AddNewFile(string newFileName)
		{
			var document = XmlCompletor.GetEmptyDocument();
			document.Save(newFileName);
			return AddFiles(new[] {newFileName}).Single();
		}
	}
}
