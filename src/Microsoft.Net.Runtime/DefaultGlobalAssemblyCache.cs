using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Microsoft.Net.Runtime
{
    public class DefaultGlobalAssemblyCache : IGlobalAssemblyCache
    {
        public bool TryResolvePartialName(string name, out string assemblyLocation)
        {
#if DESKTOP
            if (PlatformHelper.IsMono)
            {
                // REVIEW: What about mono on windows?
                assemblyLocation = Path.Combine(@"/usr/local/lib/mono/4.5/" + name + ".dll");
                return File.Exists(assemblyLocation);
            }

            var gacFolders = new[] { Environment.Is64BitProcess ? "GAC_64" : "GAC_32", "GAC_MSIL" };

            foreach (var folder in gacFolders)
            {
                string gacPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\assembly",
                    folder,
                    name);

                var di = new DirectoryInfo(gacPath);

                if (!di.Exists)
                {
                    continue;
                }

                var match = di.EnumerateFiles("*.dll", SearchOption.AllDirectories)
                                .FirstOrDefault(d => Path.GetFileNameWithoutExtension(d.Name).Equals(name, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    assemblyLocation = match.FullName;
                    return true;
                }
            }

            assemblyLocation = null;
            return false;
#else
            assemblyLocation = null;
            return false;
#endif
        }
    }
}
