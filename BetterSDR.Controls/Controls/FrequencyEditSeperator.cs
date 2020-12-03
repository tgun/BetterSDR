using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BetterSDR.Controls {
    public partial class FrequencyEditSeperator : UserControl, IRenderable {
        public Image Icon { get; set; }
        public bool Masked { get; set; }

        private bool _renderNeeded;
        private readonly ImageAttributes _maskedAttributes = new ImageAttributes();

        public FrequencyEditSeperator() {
            InitializeComponent();
          
        }

        public void Render() {
            if (!_renderNeeded)
                return;

            Invalidate();
            _renderNeeded = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (Icon == null)
                return;

            ImageAttributes attributes = (Masked || !Parent.Enabled) ? _maskedAttributes : null;
            e.Graphics.DrawImage(Icon, new Rectangle(0, 0, Width, Height), 0.0f, 0.0f, Icon.Width, Icon.Height, GraphicsUnit.Pixel, attributes);
        }

        private void FrequencyEditSeperator_Load(object sender, EventArgs e) {
            UpdateStyles();

            var cm = new ColorMatrix {Matrix33 = Constants.MaskedDigitTransparency};
            _maskedAttributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
    }
}
