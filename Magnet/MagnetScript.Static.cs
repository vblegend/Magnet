using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    public sealed partial class MagnetScript
    {

        private static List<String> ExistScripts = new List<string>();

        private readonly static Dictionary<String, MagnetScript> Instances = new Dictionary<string, MagnetScript>();


        public static Boolean Exists(String scriptName)
        {
            var scriptModule = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetCustomAttributes(typeof(ScriptAssemblyAttribute), false).Length > 0)
                .Where(assembly =>
            {
                return assembly.ManifestModule.ScopeName == scriptName;
            }).FirstOrDefault();
            return scriptModule != null;
        }


        public static Boolean GetMagnet(String magnetName, out MagnetScript magnet)
        {
            magnet = Instances[magnetName];
            return magnet != null;
        }

        private static void AddCache(MagnetScript magnet)
        {
            Instances.Add(magnet.Name, magnet);
        }

        private static void RemoveCache(MagnetScript magnet)
        {
            Instances.Remove(magnet.Name);
        }
    }
}
