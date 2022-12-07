using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Tosna.Core.Helpers;
using Tosna.Core.Problems;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Vm;

namespace Tosna.Editor.Wpf.XmlEditor.AutoCompletion
{
	public class CompletionDataFactory : ICompletionDataProviderVisitor
	{
		private readonly FilesManagerInteractionVm filesManagerInteractionVm;

		private IEnumerable<ICompletionData> result;
		
		private CompletionDataFactory(FilesManagerInteractionVm filesManagerInteractionVm)
		{
			this.filesManagerInteractionVm = filesManagerInteractionVm;
		}

		public static IEnumerable<ICompletionData> CreateCompletionDatas(ICompletionDataProvider problem, FilesManagerInteractionVm filesManagerInteractionVm)
		{
			var factory = new CompletionDataFactory(filesManagerInteractionVm);
			problem.Visit(factory);
			return factory.result;
		}

		public void Visit(CommonProblem problem)
		{
			result = new ICompletionData[] { };
		}

		public void Visit(NoneCompletionDataProvider provider)
		{
			result = new ICompletionData[] { };
		}

		public void Visit(MissingMembersCompletionDataProvider provider)
		{
			result = new[] { new AddMissingMembersCompletionData(provider) };
		}

		public void Visit(UnfinishedTypeCompletionDataProvider provider)
		{
			var list = new List<ICompletionData>();

			var prefix = provider.UnfinishedPrefix.ToLowerInvariant();

			foreach (var knownImprint in filesManagerInteractionVm.FindImprintsByType(provider.Type))
			{
				if (knownImprint.TryGetId(out var id) &&
					knownImprint.TryGetInfo(out var info) &&
					provider.TypesResolver.TryGetName(knownImprint.Type, out var typeName) &&
					(typeName.ToLowerInvariant().StartsWith(prefix) || id.ToLowerInvariant().StartsWith(prefix)))
				{
					string referenceFilePath = null;
					if (filesManagerInteractionVm.FileName != info.FilePath)
					{
						referenceFilePath =
							PathUtils.GetRelativePath(new FileInfo(info.FilePath), new FileInfo(filesManagerInteractionVm.FileName).Directory);
					}

					list.Add(new RefUnfinishedTypeCompletionData(provider, typeName, id, referenceFilePath));
				}
			}

			var derivedTypes = (
				from assemblyType in provider.TypesResolver.GetAllTypes()
				where provider.Type.IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract
				select assemblyType).ToArray();

			foreach (var derivedType in derivedTypes)
			{
				if (provider.TypesResolver.TryGetName(derivedType, out var name) && name.ToLowerInvariant().StartsWith(prefix))
				{
					list.Add(new NewUnfinishedTypeCompletionData(provider, derivedType, name));
				}
			}

			list.Sort((data1, data2) => (int)(data1.Priority - data2.Priority));
			result = list;
		}
	}
}
