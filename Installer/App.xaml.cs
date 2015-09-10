using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            AppDomain.CurrentDomain.AssemblyResolve +=new ResolveEventHandler(ResolveAssembly);
        }

        static Assembly ResolveAssembly(object sender, ResolveEventArgs args) {
            var parentAssembly = Assembly.GetExecutingAssembly();

            string name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll",
                resourceName = parentAssembly.GetManifestResourceNames()
                .First(s => s.EndsWith(name));

            using(var stream = parentAssembly.GetManifestResourceStream(resourceName)) {
                var block = new byte[stream.Length];
                stream.Read(block, 0, block.Length);
                return Assembly.Load(block);
            }
        }
    }
}
