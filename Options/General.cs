using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace IncludeMinimizer
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class General1Options : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        private MapManager map = new();
        string map_path = "";

        [Category("General")]
        [DisplayName("Mapping File")]
        [Description("Set the mapping file to output to.")]
        [DefaultValue("")]
        public string MapFile
        {
            get { return map_path; }
            set
            {
                if(map_path == value || !File.Exists(value))return;
                map_path = value;

                map.Load(map_path);
            }
        }

        public MapManager GetMap()
        { return map; }

        [Category("General")]
        [DisplayName("Relative File Prefix")]
        [Description("Prefix for relative file path stored into map. e.g. C:\\users\\map\\a.h with prefix C:\\users will write <map/a.h> to the final map.")]
        [DefaultValue("")]
        public string Prefix { get; set; } = "";
    }
}
