using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tosna.Core;
using Tosna.Core.Imprints;
using Tosna.Core.Problems;
using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.ProblemsDetailing;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE
{
	public class SingleFileManager
	{
		#region Fields & Properties

		private readonly ImprintsSerializer serializer;
		private readonly XmlProblemsDetailsGetter xmlProblemsDetailsGetter;

		private SingleFileManagerState state;

		private string content = string.Empty;

		public string FileName { get; }
		
		public IReadOnlyCollection<string> DependenciesFiles { get; private set; } = new string[] { };

		public IReadOnlyCollection<Imprint> Imprints { get; private set; } = new Imprint[] { };

		public IReadOnlyCollection<VerificationNotification> Notifications { get; private set; } = new VerificationNotification[] { };

		public IReadOnlyCollection<VerificationNotification> DependenciesErrors { get; private set; } = new VerificationNotification[] { };

		public IReadOnlyCollection<FieldsConfiguratorManager> DescriptorFileManagers { get; private set; } = new FieldsConfiguratorManager[] { };

		public IReadOnlyCollection<VerificationNotification> AllErrors => Notifications.Union(DependenciesErrors).ToArray();

		public IReadOnlyCollection<VerificationNotification> AllVerificationNotifications => AllErrors;

		public SingleFileManagerState State
		{
			get => state;
			private set
			{
				state = value;
				StateChanged(this, EventArgs.Empty);
			}
		}
		
		public string Content
		{
			get => content;
			private set
			{
				content = value;
				ContentUpdated(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Events

		public event EventHandler VerificationFinished = delegate { };

		public event EventHandler StateChanged = delegate { };

		public event EventHandler ContentUpdated = delegate { };

		#endregion

		#region Ctor

		public SingleFileManager(string fileName, ImprintsSerializer serializer, XmlProblemsDetailsGetter xmlProblemsDetailsGetter)
		{
			this.serializer = serializer;
			this.xmlProblemsDetailsGetter = xmlProblemsDetailsGetter;
			FileName = fileName;

			ReloadFromDisk();
			Verify();
		}

		#endregion

		#region Methods

		public void ReloadFromDisk()
		{
			try
			{
				Content = File.ReadAllText(FileName);
				State = SingleFileManagerState.FileInSavedState;
			}
			catch (Exception e)
			{
				Content = string.Empty;
				Notifications = new[] { new VerificationError(FileName, new FullDocumentCoordinates(), e.Message) };
				DependenciesFiles = new string[] { };
				Imprints = new Imprint[] { };
				State = SingleFileManagerState.FileInvalid;
				DescriptorFileManagers = new FieldsConfiguratorManager[] { };
			}
		}

		public void MarkStateAsSaved()
		{
			State = SingleFileManagerState.FileInSavedState;
		}

		public void SaveToDisk()
		{
			try
			{
				File.WriteAllText(FileName, Content);
				State = SingleFileManagerState.FileInSavedState;
			}
			catch
			{
				State = SingleFileManagerState.FileInvalid;
			}
		}

		public void Edit(string newContent)
		{
			var oldLinesCount = Content.Count(c => c == '\n');
			var newLinesCount = newContent.Count(c => c == '\n');

			if (oldLinesCount != newLinesCount)
			{
				DependenciesErrors = new VerificationError[] { };
			}

			Content = newContent;
			State = SingleFileManagerState.FileWithUnsavedChanges;
		}

		public void Verify()
		{
			XDocument xDocument;

			try
			{
				xDocument = XDocument.Parse(Content, LoadOptions.SetLineInfo);
			}
			catch (XmlException e)
			{
				Notifications = CreateVerificationErrors(e).ToArray();
				DependenciesFiles = new string[] { };
				DescriptorFileManagers = new FieldsConfiguratorManager[] { };
				return;
			}
			catch (Exception e)
			{
				Notifications = new[] {new VerificationError(FileName, new FullDocumentCoordinates(), e.Message)};
				DependenciesFiles = new string[] { };
				Imprints = new Imprint[] { };
				DescriptorFileManagers = new FieldsConfiguratorManager[] { };
				return;
			}

			try
			{
				Imprints = serializer.LoadRootImprints(xDocument, FileName).ToArray();
				Notifications = GetVerificationErrors(Imprints).ToArray();
				DescriptorFileManagers = GetDescriptedManagers(Imprints).ToArray();

				DependenciesFiles = Imprints.GetExternalDependenciesRecursively().Select(id => id.FilePath).Distinct()
					.Except(new[] { FileName }).ToArray();
			}
			catch (Exception e)
			{
				Imprints = new Imprint[] { };
				Notifications = new[] {new VerificationError(FileName, new FullDocumentCoordinates(), e.Message)};
				DependenciesFiles = new string[] { };
			}

			VerificationFinished(this, EventArgs.Empty);
		}

		public void VerifyDependencies(IReadOnlyCollection<Imprint> allStamps)
		{
			var knownIdentifiers = new HashSet<ImprintIdentifier>(
				allStamps.GetNestedImprintsRecursively().SelectMany(stamp => stamp.TryGetId(out var id)
					? new[] {new ImprintIdentifier(id, stamp.FilePath)}
					: new ImprintIdentifier[] { })
			);
			var dependenciesErrors = new List<VerificationError>();

			foreach (var stamp in Imprints.GetNestedImprintsRecursively())
			{
				foreach (var dependency in stamp.GetExternalDependencies())
				{
					if (knownIdentifiers.Contains(dependency)) continue;
					var coordinates = stamp.TryGetInfo(out var info) ? (ITextIntervalCoordinates)new FullLineCoordinates(info.Line) : new FullDocumentCoordinates();
					dependenciesErrors.Add(new VerificationError(FileName, coordinates,
						$"Unresolved dependency {dependency.Id} in {dependency.FilePath}"));
				}
			}

			DependenciesErrors = dependenciesErrors;

			VerificationFinished(this, EventArgs.Empty);
		}

		public bool TryGetImprint(int line, int column, out Imprint foundImprint, out bool isRootImprint)
		{
			foundImprint = null;
			var foundColumn = -1;
			isRootImprint = false;
			foreach (var imprint in Imprints.GetNestedImprintsRecursively())
			{
				if (imprint.TryGetInfo(out var info) && info.Line == line && foundColumn <= info.Column && info.Column <= column)
				{
					foundImprint = imprint;
					foundColumn = info.Column;
					isRootImprint = Imprints.Contains(foundImprint);
				}
			}
			return foundImprint != null;
		}

		#endregion

		#region Private

		private IEnumerable<VerificationError> CreateVerificationErrors(XmlException e)
		{
			yield return new VerificationError(FileName,
				e.LineNumber > 0
					? (ITextIntervalCoordinates) new StartEndCoordinates(e.LineNumber, e.LinePosition, e.LineNumber,
						e.LineNumber + 100)
					: new FullDocumentCoordinates(), e.Message);


			if (xmlProblemsDetailsGetter.TryCreateVerificationError(Content, FileName, e, out var verificationError))
			{
				yield return verificationError;
			}
		}

		private static IEnumerable<VerificationNotification> GetVerificationErrors(IEnumerable<Imprint> imprints)
		{
			foreach (var imprint in imprints.GetNestedImprintsRecursively())
			{
				if (!imprint.TryGetInfo(out var info)) continue;
				foreach (var problem in info.Problems)
				{
					if (problem.IsProblemCritical())
					{
						yield return new VerificationError(info.FilePath, new FullLineCoordinates(problem.GetProblemLineNumber()), problem.GetProblemMessage(),
							ComplexSerializerProviderFactory.GetProvider(problem));
					}
					else
					{
						yield return new VerificationWarning(info.FilePath, new FullLineCoordinates(problem.GetProblemLineNumber()), problem.GetProblemMessage(),
							ComplexSerializerProviderFactory.GetProvider(problem));
					}

				}
			}
		}

		private IEnumerable<FieldsConfiguratorManager> GetDescriptedManagers(IEnumerable<Imprint> imprints)
		{
			foreach (var imprint in imprints)
			{
				var descriptorAttribute =
					imprint.Type.GetCustomAttributes(typeof(FieldsConfiguratorAttribute), false).FirstOrDefault() as
						FieldsConfiguratorAttribute;

				if (descriptorAttribute == null)
				{
					continue;
				}

				var contextType = descriptorAttribute.ContextType;

				var context = (FieldsConfiguratorContext)Activator.CreateInstance(contextType);

				var publicName = imprint.GetCompactDescription();
				yield return DescriptorFileManagers.FirstOrDefault(manager => Equals(manager.PublicName, publicName)) ?? new FieldsConfiguratorManager(this, context, publicName, serializer);
			}
		}

		#endregion
	}

	public enum SingleFileManagerState
	{
		FileInSavedState,
		FileWithUnsavedChanges,
		FileInvalid
	}
}
