using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PackageChecker.Files
{
    internal static class ConfigHelper
    {
        private static string _configFilePathValue;

        internal static string ConfigFilePathValue
        {
            get
            {
                return _configFilePathValue;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception($"ConfigFilePathValue should not be null or empty or whitespace");
                }

                if (!File.Exists(value))
                {
                    throw new FileNotFoundException($"Can not find file {value}");
                }
                _configFilePathValue = value;
            }
        }

        internal static void ResetPath()
        {
            _configFilePathValue = string.Empty;
        }

        internal static string GetBindingRedirectFullName(string assemblyName, ref bool bindingRedirectConfigured)
        {
            var filePath = _configFilePathValue;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                bindingRedirectConfigured = false;
                return null;
            }
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
    }
}
