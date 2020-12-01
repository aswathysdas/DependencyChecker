using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PackageChecker.Files
{
	internal static class FilesHelper
	{
        internal static string PickConfigFileDialog()
        {
            using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog())
            {
                fileDialog.Filters.Add(new CommonFileDialogFilter("config", ".config"));
                fileDialog.EnsurePathExists = true;
                fileDialog.Multiselect = false;

                CommonFileDialogResult result = fileDialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                {
                    return fileDialog.FileName;
                }

                return string.Empty;
            }
		}

		internal static string PickZipDialog()
		{
			using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog())
			{
				fileDialog.Filters.Add(new CommonFileDialogFilter("Zip Archive", ".zip"));
				fileDialog.EnsurePathExists = true;
				fileDialog.Multiselect = false;

				CommonFileDialogResult result = fileDialog.ShowDialog();
				if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fileDialog.FileName))
				{
					return fileDialog.FileName;
				}

				return string.Empty;
			}
		}

		internal static string PickFolderDialog()
		{
			using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog())
			{
				fileDialog.IsFolderPicker = true;
				fileDialog.EnsurePathExists = true;
				fileDialog.Multiselect = false;

				CommonFileDialogResult result = fileDialog.ShowDialog();
				if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fileDialog.FileName))
				{
					return fileDialog.FileName;
				}

				return string.Empty;
			}
		}

		internal static bool IsFolder(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			return attributes.HasFlag(FileAttributes.Directory);
		}

		internal static bool IsZipFile(string path)
		{
			if (IsFolder(path))
			{
				return false;
			}

			FileInfo info = new FileInfo(path);
			return !string.IsNullOrEmpty(info.Extension) && info.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
		}

		internal static void OpenFileExplorer(string rootFolder, string relativePath)
		{
			string fullPath = Path.Combine(rootFolder, relativePath);
			fullPath = ReplaseAltSeparators(fullPath);

			if (!File.Exists(fullPath))
			{
				return;
			}

			const string explorerArgsFormat = "/select, \"{0}\"";
			string argument = string.Format(CultureInfo.InvariantCulture, explorerArgsFormat, fullPath);

			Process.Start("explorer.exe", argument);
		}

		internal static string GetRelativePath(string fullPath, string rootPath)
		{
			if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				rootPath += Path.DirectorySeparatorChar;
			}
			Uri fileUri = new Uri(fullPath);
			Uri referenceUri = new Uri(rootPath);
			return referenceUri.MakeRelativeUri(fileUri).ToString().Replace("%20", " ");
		}

		internal static string ReplaseAltSeparators(string path)
		{
			return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}

		internal static bool IsPathAbsolute(string path)
		{
			return Path.IsPathRooted(path) &&
				!Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
		}
	}
}
