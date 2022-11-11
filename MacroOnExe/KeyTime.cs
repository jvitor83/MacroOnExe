using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacroOnExe
{
    public class KeyTime
    {
        public Keys Key { get; set; }
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(0);

    }
}
