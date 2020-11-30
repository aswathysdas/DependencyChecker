using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PackageChecker.WindowManagement;

namespace PackageChecker.Files
{
	internal static class AssemblyManager
	{
		internal static AssemblyName GetAssemblyName(string filePath)
		{
			return AssemblyName.GetAssemblyName(filePath);
		}

		internal static Assembly GetAssemblyByFile(string fileName)
		{
			AssemblyName assemblyName = GetAssemblyName(fileName);

			Assembly assembly = GetFirstOrDefaultAssembly(assemblyName.FullName);
			if (assembly == null)
			{
				assembly = Assembly.Load(File.ReadAllBytes(fileName));
			}

			return assembly;
		}

		internal static Assembly GetAssemblyByName(AssemblyName assemblyName)
		{
			Assembly assembly = GetFirstOrDefaultAssembly(assemblyName.FullName);
			if (assembly == null)
			{
				assembly = Assembly.Load(assemblyName);
			}

			return assembly;
		}

		internal static bool IsAssemblyInGAC(AssemblyName assemblyName)
		{
			try
			{
				return GetAssemblyByName(assemblyName).GlobalAssemblyCache;
			}
			catch(Exception ex)
			{
				WindowHelper.ShowError(ex.ToString());
				return false;
			}
		}

		private static Assembly GetFirstOrDefaultAssembly(string fullName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			return assemblies.FirstOrDefault(a => a.FullName == fullName);
		}
	}
}
