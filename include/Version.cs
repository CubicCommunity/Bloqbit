using System.ComponentModel;
using System.Reflection;

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