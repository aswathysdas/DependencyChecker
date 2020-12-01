using PackageChecker.BindingBase;
using PackageChecker.Files;
using PackageChecker.Models;
using PackageChecker.WindowManagement;
using System;
using System.Windows;
using System.Windows.Input;

namespace PackageChecker.ViewModels
{
	internal class WindowViewModel : BindableBase
	{
		private readonly WindowModel _model;

		internal WindowViewModel(IProgressBarManager progressBarManager, IFilesListManager filesListManager)
		{
			_model = new WindowModel(progressBarManager, filesListManager);
			_model.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
			_model.SetEmptyState();

            SetConfigFileState = new BindCommand(param =>
            {
                string fileName = FilesHelper.PickConfigFileDialog();
                if (!string.IsNullOrEmpty(fileName))
                {
                    _model.SetConfigFileState(fileName);
                }
            });

                SetZipState = new BindCommand(param =>
			{
				string fileName = FilesHelper.PickZipDialog();
				if (!string.IsNullOrEmpty(fileName))
				{
					_model.SetZipState(fileName);
				}
			});

			SetFolderState = new BindCommand(param =>
			{
				string folderPath = FilesHelper.PickFolderDialog();
				if (!string.IsNullOrEmpty(folderPath))
				{
					_model.SetFolderState(folderPath);
				}
			});

            RefreshState = new BindCommand(param =>
            {
                _model.RefreshState();
            });

			SetEmptyState = new BindCommand(param =>
			{
				_model.SetEmptyState();
			});

			ProcessDragAndDrop = new BindCommand(param =>
			{
				string[] files = (string[])param;

				if (files == null || files.Length != 1)
				{
					WindowHelper.ShowError("Drag-and-Drop support only one record.");
				}
				
				string path = files[0];
				
				if (FilesHelper.IsFolder(path))
				{
					_model.SetFolderState(path);
				}
				else if (FilesHelper.IsZipFile(path))
				{
					_model.SetZipState(path);
				}
				else
				{
					WindowHelper.ShowError("File format isn't supported.");
				}
			});
		}
		public ICommand SetConfigFileState { get; }
		public ICommand SetZipState { get; }
		public ICommand SetFolderState { get; }
		public ICommand RefreshState { get; }
		public ICommand SetEmptyState { get; }
		public ICommand ProcessDragAndDrop { get; }

		public string PathValue { get { return _model.PathValue; } set { _model.PathValue = value; } }
		public Visibility ChoosePanelVisibility => _model.ChoosePanelVisibility;
		public Visibility PathPanelVisibility => _model.PathPanelVisibility;
		public MainWindowState WindowState => _model.WindowState;
	}
}
