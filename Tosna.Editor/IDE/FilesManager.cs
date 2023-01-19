using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tosna.Core;
using Tosna.Core.Documents;
using Tosna.Core.Helpers.Xml;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Editor.IDE
{
	public class FilesManager
	{
		private readonly IDocumentReader documentReader;
		private readonly IDocumentWriter documentWriter;
		private readonly IDictionary<string, SingleFileManager> fileManagers = new Dictionary<string, SingleFileManager>();

		public IReadOnlyCollection<SingleFileManager> FileManagers => fileManagers.Values.ToArray();

		public ImprintsSerializer Serializer { get; }

		public FilesManager(ISerializingElementsManager serializingElementsManager,
			ISerializingTypesResolver serializingTypesResolver, IDocumentReaderFactory readerFactory,
			IDocumentWriterFactory writerFactory)
		{
			documentReader = readerFactory.CreateReader();
			documentWriter = writerFactory.CreateWriter();
			Serializer = new ImprintsSerializer(serializingElementsManager, serializingTypesResolver);
		}

		public async Task<IReadOnlyCollection<SingleFileManager>> AddFiles(IEnumerable<string> files)
		{
			var result = new List<SingleFileManager>();
			foreach (var file in files)
			{
				if (fileManagers.ContainsKey(file))
				{
					continue;
				}

				var singleFileManager = new SingleFileManager(file, Serializer, documentReader, documentWriter);
				fileManagers[file] = singleFileManager;
				result.Add(singleFileManager);
			}
			await VerifyDependencies();
			return result;
		}

		public async Task<IReadOnlyCollection<SingleFileManager>> AddFilesWithDependencies(IEnumerable<string> files)
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
					var singleFileManager = new SingleFileManager(file, Serializer, documentReader, documentWriter);
					fileManagers[file] = singleFileManager;
					result.Add(singleFileManager);

					newDependencies.AddRange(singleFileManager.DependenciesFiles);
				}

				filesToAdd = newDependencies;
			}
			
			await VerifyDependencies();
			
			return result;
		}

		public async Task DeleteFiles(IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				fileManagers.Remove(file);
			}
			await VerifyDependencies();
		}

		public async Task Clear()
		{
			fileManagers.Clear();
			await VerifyDependencies();
		}

		public async Task VerifyDependencies()
		{
			var singleFileManagers = FileManagers;

			var imprints = singleFileManagers.SelectMany(fileManager => fileManager.Imprints).ToArray();

			await Task.WhenAll(singleFileManagers.Select(singleFileManager =>
				Task.Run(() => singleFileManager.VerifyDependencies(imprints))));
		}

		public async Task<SingleFileManager> AddNewFile(string newFileName)
		{
			var document = XmlCompletor.GetEmptyDocument();
			document.Save(newFileName);
			var newFiles = await AddFiles(new[] {newFileName});
			return newFiles.Single();
		}
	}
}
