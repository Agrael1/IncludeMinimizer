using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncludeMinimizer
{
    internal static class Output
    {
        static private OutputWindowPane pane;
        static public async Task InitializeAsync()
        {
            pane = await VS.Windows.CreateOutputWindowPaneAsync("Include Minimizer");
        }
        static public async Task WriteLineAsync(string str)
        {
            await pane.WriteLineAsync(str);
        }
        static public void WriteLine(string str)
        {
            pane.WriteLine(str);
        }
    }
}
