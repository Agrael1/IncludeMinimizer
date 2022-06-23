using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IncludeMinimizer
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class General1Options : BaseOptionPage<General1> { }
    }

    public class General1 : BaseOptionModel<General1>
    {
        [Category("General")]
        [DisplayName("Mapping File")]
        [Description("Set the mapping file to output to.")]
        [DefaultValue("")]
        public string MapFile { get; set; } = "";
        [Category("General")]
        [DisplayName("Relative File Prefix")]
        [Description("Prefix for relative file path stored into map. e.g. C:\\users\\map\\a.h with prefix C:\\users will write <map/a.h> to the final map.")]
        [DefaultValue("")]
        public string Prefix { get; set; } = "";
    }
}
