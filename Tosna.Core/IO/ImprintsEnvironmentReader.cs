using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tosna.Core.Imprints;
using Tosna.Core.Problems;

namespace Tosna.Core.IO
{
	public static class ImprintsEnvironmentReader
	{
		public static ImprintsEnvironment Read(ImprintsSerializer serializer, string[] rootFilesToRead)
		{
			var result = new List<Imprint>();
			var filesToRead = new Queue<string>(rootFilesToRead);
			var filesAlreadyRead = new HashSet<string>();

			while (filesToRead.Any())
			{
				var file = filesToRead.Dequeue();

				var rootImprints = serializer.LoadRootImprints(XDocument.Load(file, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri), file).ToArray();

				var allImprints = rootImprints.GetNestedImprintsRecursively();
				var dependencies = rootImprints.GetExternalDependenciesRecursively();

				var dependentFilesPaths = dependencies.Select(dependency => dependency.FilePath).Distinct();
				foreach (var dependentFilePath in dependentFilesPaths)
				{
					if (!filesAlreadyRead.Contains(dependentFilePath))
					{
						filesToRead.Enqueue(dependentFilePath);
					}
				}

				result.AddRange(allImprints);
				filesAlreadyRead.Add(file);
			}

			foreach (var imprint in result)
			{
				if (imprint.TryGetInfo(out var info) && info.Problems.Any(problem => problem.IsProblemCritical()))
				{
					var problemMessageWithLocation =
						info.Problems.First(problem => problem.IsProblemCritical()).GetProblemMessageWithLocation();
					throw new Exception($"Error in {info.FilePath}:{Environment.NewLine}{problemMessageWithLocation}");
				}
			}

			return new ImprintsEnvironment(result);
		}
	}
}
