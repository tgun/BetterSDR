using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterSDR.Controls {
    public partial class Waterfall : UserControl {
        private const float TimestampFontSize = 14.0f;

        public const int CursorSnapDistance = 4;
        public const int RightClickSnapDistance = 500; // Snap distance in Hz, for Ellie

        public Waterfall() {
            InitializeComponent();
        }
    }
}
