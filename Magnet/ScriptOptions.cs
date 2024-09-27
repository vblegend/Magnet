using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    public class ScriptOptions
    {

        public String  Name { get; set; }


        public Boolean Debug { get; set; }

        public String BaseDirectory { get; set; }

        public String ScriptFilePattern { get; set; } = "*.cs";

        /// <summary>
        /// add using xxxx;
        /// </summary>
        public IEnumerable<String> Using { get; set; } = [];

        /// <summary>
        /// import Assemblys
        /// </summary>
        public IEnumerable<Assembly> Imports { get; set; } = [];

    }
}
