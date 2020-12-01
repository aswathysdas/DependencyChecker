using PackageChecker.BindingBase;
using PackageChecker.Files;
using PackageChecker.WindowManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PackageChecker.Models
{
	internal class WindowModel : BindableBase
	{
		#region Private Properties
		private string _pathValue = string.Empty;
		private Visibility _pathPanelVisibility;
		private Visibility _choosePanelVisibility;

		IProgressBarManager _progressBarManager;
		IFilesListManager _filesListManager;

		private MainWindowState _windowState;
		#endregion //Private Properties

		#region Binding Properties

		public string PathValue
		{
			get
			{
				return _pathValue;
			}
			set
			{
				_pathValue = value;
				OnPropertyChanged("PathValue");
			}
		}

		public Visibility PathPanelVisibility
		{
			get
			{
				return _pathPanelVisibility;
			}
			set
			{
				_pathPanelVisibility = value;
				OnPropertyChanged("PathPanelVisibility");
			}
		}

		public Visibility ChoosePanelVisibility
		{
			get
			{
				return _choosePanelVisibility;
			}
			set
			{
				_choosePanelVisibility = value;
				OnPropertyChanged("ChoosePanelVisibility");
			}
		}
		#endregion //Binding Properties

		#region Public Properties
		public MainWindowState WindowState
		{
			get
			{
				return _windowState;
			}
		}
		#endregion

		internal WindowModel(IProgressBarManager progressBarManager, IFilesListManager filesListManager)
		{
			_progressBarManager = progressBarManager;
			_filesListManager = filesListManager;
		}

		#region Commands Implementation

        public void SetConfigFileState(string configFilePath)
        {
            ConfigHelper.ConfigFilePathValue = configFilePath;
        }

		public void SetZipState(string path)
		{
			SetProgressMode();

			PathValue = path;

			Task task = UpdateFilesList(MainWindowState.ZipFile);
			if (task != null)
			{
				task.ContinueWith(t => DispatcherInvoke(() => SetPathMode(MainWindowState.ZipFile)));
			}
		}

		public void SetFolderState(string path)
		{
			SetProgressMode();

			PathValue = path;

			Task task = UpdateFilesList(MainWindowState.Folder);
			if (task != null)
			{
				task.ContinueWith(t => DispatcherInvoke(() => SetPathMode(MainWindowState.Folder)));
			}
		}

		public void SetEmptyState()
        {
            ConfigHelper.ResetPath();
			PathValue = string.Empty;

			_filesListManager.ClearList();

			SetChooseMode();
		}

		public void ProcessDragAndDrop(string[] files)
		{
			if (files.Length != 1)
			{
				WindowHelper.ShowError("Drag-and-Drop support only one record.");
			}

			string path = files[0];

			if (FilesHelper.IsFolder(path))
			{
				SetFolderState(path);
			}
			else if (FilesHelper.IsZipFile(path))
			{
				SetZipState(path);
			}
			else
			{
				WindowHelper.ShowError("File format isn't supported.");
			}
		}
		#endregion

		#region Private Methods
		private Task UpdateFilesList(MainWindowState transitionState)
		{
			switch (transitionState)
			{
				case MainWindowState.Folder:
					return _filesListManager.UpdateFileRecords(PathValue, FileSearchType.Folder);
				case MainWindowState.ZipFile:
					return _filesListManager.UpdateFileRecords(PathValue, FileSearchType.Zip);
				default:
					throw new ArgumentException("Unknown UpdateFilesList argument.");
			}
		}

		private void SetChooseMode()
		{
			_windowState = MainWindowState.None;

			ChoosePanelVisibility = Visibility.Visible;
			PathPanelVisibility = Visibility.Collapsed;
			_progressBarManager.SetVisibility(Visibility.Collapsed);
		}

		private void SetPathMode(MainWindowState state)
		{
			_windowState = state;

			PathPanelVisibility = Visibility.Visible;
			_progressBarManager.SetVisibility(Visibility.Collapsed);
			ChoosePanelVisibility = Visibility.Collapsed;
		}

		private void SetProgressMode()
		{
			_windowState = MainWindowState.ProgressPanel;

			_progressBarManager.SetVisibility(Visibility.Visible);
			PathPanelVisibility = Visibility.Collapsed;
			ChoosePanelVisibility = Visibility.Collapsed;
		}

		private void DispatcherInvoke(Action action)
		{
			App.Current.Dispatcher.Invoke(action);
		}
		#endregion Private Methods
	}
}
