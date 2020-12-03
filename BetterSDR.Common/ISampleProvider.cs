using System.Windows.Forms;

namespace BetterSDR.Common {
    public delegate void EmptyEventDelegate();
    public interface ISampleProvider {
        event EmptyEventDelegate DataAvailable;
        ComplexBuffer Buffer { get; set; }
        uint SampleRate { get; set; }
        uint Frequency { get; set; }
        Form GetSettingsForm();
    }
}
