using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace Bloqbit.Include
{
    public static class Version
    {
        private static readonly string VERSION = "1.0.0";

        /// <summary>
        /// Gets the string of the current version of Bloqbit 
        /// </summary>
        public static string Get()
        {
            var vc = new VersionConverter();

            try
            {
                var entryPath = Assembly.GetEntryAssembly()?.Location;
                if (!string.IsNullOrEmpty(entryPath))
                {
                    var dir = Path.GetDirectoryName(entryPath);
                    string? csproj = null;
                    var current = dir;

                    while (!string.IsNullOrEmpty(current))
                    {
                        csproj = Directory.GetFiles(current, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (!string.IsNullOrEmpty(csproj)) break;

                        var parent = Directory.GetParent(current);
                        if (parent == null) break;
                        if (parent.FullName == current) break;
                        current = parent.FullName;
                    }

                    if (!string.IsNullOrEmpty(csproj))
                    {
                        var xdoc = XDocument.Load(csproj);
                        var versionElem = xdoc.Descendants().FirstOrDefault(e =>
                            e.Name.LocalName.Equals("Version", StringComparison.OrdinalIgnoreCase) ||
                            e.Name.LocalName.Equals("PackageVersion", StringComparison.OrdinalIgnoreCase));

                        if (versionElem != null)
                        {
                            var val = versionElem.Value?.Trim();
                            if (!string.IsNullOrEmpty(val) && vc.IsValid(val))
                                return val;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            if (vc.IsValid(VERSION))
            {
                return VERSION;
            }
            else
            {
                var version = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

                return version ?? "1.0.0";
            }
        }
    }
}