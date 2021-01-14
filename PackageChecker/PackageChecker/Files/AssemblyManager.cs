using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            var assembly = Assembly.ReflectionOnlyLoadFrom(fileName);
            if (assembly == null)
            {
                throw new FileNotFoundException(fileName);
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
			catch
			{
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
