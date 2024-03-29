using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Tosna.Core;
using Tosna.Editor.Helpers;
using Tosna.Editor.IDE.Interfaces;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Vm.FileSystem;
using PathUtils = Tosna.Core.Helpers.PathUtils;

namespace Tosna.Editor.IDE.Vm
{
	public class FilesHierarchyVm : NotifyPropertyChanged, IDisposable
	{
		private readonly FilesManager filesManager;
		private readonly FilesViewerVm filesViewerVm;
		private readonly IFilesSelector filesSelector;
		private readonly VerificationService verificationService;
		private IFileSystemItemVm[] selectedItems;

		public FilesHierarchyVm(FilesManager filesManager, FilesViewerVm filesViewerVm, IFilesSelector filesSelector,
			VerificationService verificationService, ILogger logger)
		{
			Contract.Requires(filesManager != null);
			Contract.Requires(filesViewerVm != null);
			Contract.Requires(filesSelector != null);

			this.filesManager = filesManager;
			this.filesViewerVm = filesViewerVm;
			this.filesSelector = filesSelector;
			this.verificationService = verificationService;

			AddExistingFilesCommand = new ActionCommand(AddFiles, () => true, logger);
			AddExistingFilesWithDependenciesCommand = new ActionCommand(AddFilesWithDependencies, () => true, logger);
			CreateNewFileCommand = new ActionCommand(CreateNewFile, () => true, logger);
			DeleteFilesCommand =
				new ActionCommand(DeleteFiles, () => SelectedItems != null && SelectedItems.Any(), logger);
			ExcludeFilesCommand =
				new ActionCommand(ExcludeFiles, () => SelectedItems != null && SelectedItems.Any(), logger);
			LaunchEditorCommand = new ActionCommand(LaunchEditor, CanLaunchEditor, logger);
			ExcludeAllFilesCommand = new ActionCommand(ExcludeAllFiles, () => true, logger);

			ReloadAll();
		}

		public void Dispose()
		{
			TopDirectoryItemVm?.Dispose();
		}

		public void RefreshAll()
		{
			UpdateTopDirectoryItem();
		}

		#region Properties

		public IFileSystemItemVm[] SelectedItems
		{
			get => selectedItems;
			set
			{
				selectedItems = value;
				(ExcludeFilesCommand as ActionCommand)?.RaiseCanExecuteChanged();
				(DeleteFilesCommand as ActionCommand)?.RaiseCanExecuteChanged();
			}
		}

		public ICommand AddExistingFilesCommand { get; }

		public ICommand AddExistingFilesWithDependenciesCommand { get; }

		public ICommand CreateNewFileCommand { get; }

		public ICommand DeleteFilesCommand { get; }
		
		public ICommand ExcludeFilesCommand { get; }

		public ICommand ExcludeAllFilesCommand { get; }

		public ICommand LaunchEditorCommand { get; }

		public DirectoryItemVm TopDirectoryItemVm { get; set; } = new DirectoryItemVm(string.Empty);

		public ObservableCollection<FileItemVm> OpenItems { get; set; } = new ObservableCollection<FileItemVm>();
		
		#endregion

		#region Commands implementations

		private void AddFiles(bool loadDependencies)
		{
			if (!filesSelector.SelectFiles(GetInitialDirectory(), out var files))
			{
				return;
			}

			var newItems = loadDependencies
				? filesManager.AddFilesWithDependencies(files)
				: filesManager.AddFiles(files);
			UpdateTopDirectoryItem();
			foreach (var newItem in newItems)
			{
				TopDirectoryItemVm.Add(newItem);
			}
			
			verificationService.EnqueueDependenciesVerification();
		}

		private void AddFiles()
		{
			AddFiles(false);
		}

		private void AddFilesWithDependencies()
		{
			AddFiles(true);
		}

		private void CreateNewFile()
		{
			if (!filesSelector.CreateFile(GetInitialDirectory(), out var newFileName))
			{
				return;
			}

			var newItem = filesManager.AddNewFile(newFileName);
			UpdateTopDirectoryItem();
			
			TopDirectoryItemVm.Add(newItem);

			filesViewerVm.OpenDocument(newItem);
			
			verificationService.EnqueueDependenciesVerification();
		}

		private void DeleteFiles()
		{
			// TODO
		}

		private void ExcludeFiles()
		{
			filesManager.DeleteFiles(SelectedItems.OfType<FileItemVm>().Select(item => item.FileManager.FileName));
			UpdateTopDirectoryItem();
			foreach (var item in SelectedItems.OfType<FileItemVm>())
			{
				TopDirectoryItemVm.Remove(item.FileManager);
			}
			verificationService.EnqueueDependenciesVerification();
		}

		private void ExcludeAllFiles()
		{
			filesManager.Clear();
			ReloadAll();
			verificationService.EnqueueDependenciesVerification();
		}

		private void LaunchEditor()
		{
			if (SelectedItems == null)
			{
				return;
			}
			
			switch (SelectedItems.FirstOrDefault())
			{
				case FileItemVm fileItemVm:
					filesViewerVm.OpenDocument(fileItemVm.FileManager);
					return;

				case DescribedItemVm describedItemVm:
					filesViewerVm.OpenDocument(describedItemVm.FieldsConfiguratorManager);
					return;
			}
		}

		private bool CanLaunchEditor()
		{
			if (SelectedItems != null && SelectedItems.Length > 0)
			{
				var o = SelectedItems[0];
				return o is FileItemVm || o is DescribedItemVm;
			}

			return false;
		}

		#endregion

		#region Refreshing

		private string GetInitialDirectory()
		{
			if (SelectedItems != null && SelectedItems.Length == 1 && SelectedItems.Single().TryGetDirectory(out var directory))
			{
				return directory;
			}

			return TopDirectoryItemVm?.DirectoryPath;
		}

		private void UpdateTopDirectoryItem()
		{
			var fileManagers = filesManager.FileManagers;

			var filePaths = fileManagers.Select(manager => manager.FileName);

			var directories = filePaths.Select(Path.GetDirectoryName).Distinct().Select(directory => new DirectoryInfo(directory));

			var topDirectory = PathUtils.GetCommonPath(directories);

			if (string.Equals(TopDirectoryItemVm.DirectoryPath, topDirectory))
			{
				return;
			}

			TopDirectoryItemVm.Dispose();
			TopDirectoryItemVm = new DirectoryItemVm(topDirectory);

			foreach (var fileManager in fileManagers)
			{
				TopDirectoryItemVm.Add(fileManager);
			}

			RaisePropertyChanged(nameof(TopDirectoryItemVm));
		}

		private void ReloadAll()
		{
			TopDirectoryItemVm.Dispose();
			TopDirectoryItemVm = new DirectoryItemVm(string.Empty);
			RaisePropertyChanged(nameof(TopDirectoryItemVm));
			UpdateTopDirectoryItem();
		}

		#endregion
	}

}
