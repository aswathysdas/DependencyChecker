using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace PackageChecker.Files
{
	internal class AssembliesReferences
	{
		private const string SuccessMessage = "Passed.";
		private const string ErrorMessage = "Failed. Missing assemblies:\n{0}.";

		private Dictionary<string, HashSet<string>> _assembliesInFolders;

		internal AssembliesReferences()
		{
			_assembliesInFolders = new Dictionary<string, HashSet<string>>();
		}

		internal void AddAssembly(string folder, AssemblyName assemblyName)
		{
			if (!_assembliesInFolders.ContainsKey(folder))
			{
				_assembliesInFolders[folder] = new HashSet<string>();
			}

			_assembliesInFolders[folder].Add(assemblyName.FullName);
		}

		internal string CheckAssembly(string folder, Assembly assembly)
		{
			HashSet<string> assembliesInFolder = _assembliesInFolders[folder];
			HashSet<string> additionalAssemblies = GetAdditionalAssemblies(folder, assembly);

			List<string> missingAssemblies = new List<string>();
			foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
            {
                var temp = referencedAssembly;

				bool notExist = AssemblyNotExist(assembliesInFolder, additionalAssemblies, ref temp);
				if (notExist)
				{
					missingAssemblies.Add(temp.FullName);
				}
			}

			if (missingAssemblies.Count == 0)
			{
				return SuccessMessage;
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, ErrorMessage, string.Join(";\n", missingAssemblies));
			}
		}

        private bool AssemblyNotExist(HashSet<string> assembliesInFolder, HashSet<string> additionalAssemblies,
            ref AssemblyName referencedAssembly)
		{
			//check bindingRedirect 
            bool bindingRedirectConfigured = false;

			var bindingRedirectFullName = GetBindingRedirectFullName(referencedAssembly.Name,ref bindingRedirectConfigured);
            if (bindingRedirectConfigured)
            {
                var bindingRedirectAssembly = new AssemblyName(bindingRedirectFullName);
                referencedAssembly = bindingRedirectAssembly;
			}

            string refAssemblyFullName = referencedAssembly.FullName;
			bool notExist = !(assembliesInFolder.Contains(refAssemblyFullName) || additionalAssemblies.Contains(refAssemblyFullName))
                            &&
                            !AssemblyManager.IsAssemblyInGAC(referencedAssembly);
            return notExist;
        }

        private string GetBindingRedirectFullName(string assemblyName,ref bool bindingRedirectConfigured)
        {
            var filePath = ConfigurationManager.AppSettings["ConfigFileName"];
            var root = XElement.Load(filePath);
            var root2 = RemoveAllNamespaces(root);
            var runtime = root2.Element("runtime");
            var assemblyBinding = runtime?.Element("assemblyBinding");
            var assemblyIdentity = assemblyBinding?.Elements("dependentAssembly")
                .Elements("assemblyIdentity").FirstOrDefault(x => x.Attribute("name")?.Value == assemblyName);
            if (assemblyIdentity != null)
            {
                bindingRedirectConfigured = true;
            }
            var dependentAssembly = assemblyIdentity?.Parent;
            var bindingRedirect = dependentAssembly?.Elements("bindingRedirect").FirstOrDefault();
            var newVersion = bindingRedirect?.Attribute("newVersion")?.Value;
            //Console.WriteLine(newVersion);
            var culture = assemblyIdentity?.Attribute("culture")?.Value;
            var publicKeyToken = assemblyIdentity?.Attribute("publicKeyToken")?.Value;
            var bindingRedirectFullName =
                $"{assemblyName}, Version={newVersion}, Culture={culture}, PublicKeyToken={publicKeyToken?.ToLower()}";
            //Console.WriteLine(bindingRedirectFullName);
            return bindingRedirectFullName;
        }

        /// <summary>
        /// https://stackoverflow.com/a/988325/13338936
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(RemoveAllNamespaces));
        }

		private HashSet<string> GetAdditionalAssemblies(string folder, Assembly assembly)
		{
			HashSet<string> additionalAssemblies = new HashSet<string>();

			string assemblyFileName = assembly.ManifestModule.ScopeName;
			if (!".exe".Equals(Path.GetExtension(assemblyFileName), System.StringComparison.OrdinalIgnoreCase))
			{
				return additionalAssemblies;
			}

			string configFilePath = Path.Combine(folder, assemblyFileName + ".config");
			if (!File.Exists(configFilePath))
			{
				return additionalAssemblies;
			}

			List<string> bindingPaths = GetConfigAssemblyBindingPaths(configFilePath);
			foreach (string path in bindingPaths)
			{
				string bindingFolder = string.Empty;
				if (FilesHelper.IsPathAbsolute(path))
				{
					bindingFolder = path;
				}
				else
				{
					bindingFolder = Path.Combine(folder, path);
				}

				if (_assembliesInFolders.ContainsKey(bindingFolder))
				{
					additionalAssemblies.UnionWith(_assembliesInFolders[bindingFolder]);
				}
			}

			return additionalAssemblies;
		}

		private List<string> GetConfigAssemblyBindingPaths(string configPath)
		{
			XmlDocument config = new XmlDocument();
			config.Load(configPath);

			XmlNodeList pathNodes = config.DocumentElement.SelectNodes("//*[local-name() = 'assemblyBinding']/*[local-name() = 'probing']/@privatePath");

			List<string> bindingPaths = new List<string>(pathNodes.Count);
			foreach (XmlAttribute path in pathNodes)
			{
				bindingPaths.Add(path.Value);
			}

			return bindingPaths;
		}
	}
}
