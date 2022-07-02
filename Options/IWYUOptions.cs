using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IncludeMinimizer
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class XIWYUOptions : BaseOptionPage<IWYUOptions> { }
    }

    public class IWYUOptions : BaseOptionModel<IWYUOptions>
    {
        string exe = "";
        Comment comm = Comment.Default;
        uint verbose = 2;

        [Browsable(false)]
        public bool Dirty { get; private set; }

        [Category("General")]
        [DisplayName("IWYU Executable")]
        [Description("A path to IWYU executable.")]
        [DefaultValue("")]
        public string Executable
        {
            get { return exe; }
            set
            {
                Dirty = true;
                exe = value;
            }
        }



        [Category("General")]
        [DisplayName("Print Commentaries")]
        [Description("Tells IWYU to show or hide individual commentaries to headers.")]
        [DefaultValue(Comment.Default)]
        [TypeConverter(typeof(EnumConverter))]
        public Comment Comms { get { return comm; } set { Dirty = true; comm = value; } }
        
        [Category("General")]
        [DisplayName("Output Verbosity")]
        [Description("Determines how much output needs to be printed. May help in case of error. Max value is 7.")]
        [DefaultValue(2)]
        public uint Verbosity{ get { return verbose; } set { Dirty = true; verbose = Math.Min(value, 7); } }

        public void ClearFlag()
        {
            Dirty = false;
        }
    }

    public enum Comment
    {
        Default,
        Always,
        Never
    }
}
