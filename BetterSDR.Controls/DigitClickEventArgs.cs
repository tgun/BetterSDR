using System.Windows.Forms;

namespace BetterSDR.Controls {
    public delegate void DigitClickEvent(object sender, DigitClickEventArgs args);

    public class DigitClickEventArgs {
        public bool IsUpperHalf { get; set; }
        public MouseButtons Button { get; set; }
        public DigitClickEventArgs(bool isUpper, MouseButtons buttons) => (IsUpperHalf, Button) = (isUpper, buttons);

    }
}
