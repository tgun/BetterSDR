using System.Windows.Forms;

namespace BetterSDR.RTLSDR {
    public partial class SettingsForm : Form {
        public SettingsForm() {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            var devices = RtlDevice.GetAvailableDevices();
            var items = new string[devices.Count];
            var i = 0;
            foreach (var device in devices)
            {
                items[i++] = $"{device.Key}) {device.Value}";
            }
            deviceDropdown.Items.AddRange(items);
        }
    }
}
