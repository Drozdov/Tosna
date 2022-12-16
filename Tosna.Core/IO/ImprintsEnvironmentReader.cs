using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Documents;
using Tosna.Core.Documents.Xml;
using Tosna.Core.Imprints;
using Tosna.Core.Problems;

namespace Tosna.Core.IO
{
	public static class ImprintsEnvironmentReader
	{
		public static ImprintsEnvironment Read(ImprintsSerializer serializer, IDocumentReader reader, string[] rootFilesToRead)
		{
			var result = new List<Imprint>();
			var filesToRead = new Queue<string>(rootFilesToRead);
			var filesAlreadyRead = new HashSet<string>();

			while (filesToRead.Any())
			{
				var file = filesToRead.Dequeue();

				var document = reader.ReadDocument(file);
				var rootImprints = serializer.LoadRootImprints(document).ToArray();

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
				if (imprint.TryGetInfo(out var info) && info.Problems.Any(problem => problem.IsCritical))
				{
					var criticalProblem = info.Problems.First(problem => problem.IsCritical);
					var location = criticalProblem.Location;
					throw new Exception(
						$"Error in {info.FilePath}:{Environment.NewLine}{criticalProblem.Description} at {location.LineStart}:{location.ColumnStart}");
				}
			}

			return new ImprintsEnvironment(result);
		}
	}
}
