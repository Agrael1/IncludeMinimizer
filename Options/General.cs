using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IncludeMinimizer
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class XMapGenOptions : BaseOptionPage<General> { }
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
                if (map_path == value) return;
                map_path = value;

                map.Load(map_path);
            }
        }

        [Browsable(false)]
        public MapManager Map { get { return map; } }

        public async Task<bool> IsInMapAsync()
        {
            var doc = await VS.Documents.GetActiveDocumentViewAsync();
            var str = Util.GetRelativePath(doc.FilePath, Prefix).Replace('\\', '/');
            return map.Map.ContainsKey(str);
        }

        [Category("General")]
        [DisplayName("Relative File Prefix")]
        [Description("Prefix for relative file path stored into map. e.g. C:\\users\\map\\a.h with prefix C:\\users will write <map/a.h> to the final map.")]
        [DefaultValue("")]
        public string Prefix { get; set; } = "";
    }
}
