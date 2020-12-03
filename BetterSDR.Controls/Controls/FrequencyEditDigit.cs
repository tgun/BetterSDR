using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BetterSDR.Controls {
    public partial class FrequencyEditDigit : UserControl, IRenderable {
        public bool IsCursorInside { get; private set; }
        public long Weight { get; set; }
        public int DigitIndex { get; set; }

        private int _displayedDigit;
        public int DisplayedDigit {
            get => _displayedDigit;
            set {
                if (value < 0 || value > 9) return;
                if (_displayedDigit == value) return;

                _displayedDigit = value;
                _isRenderNeeded = true;
            }
        }
        public bool Masked {
            get => _masked;
            set {
                if (_masked == value) return;
                _masked = value;
                _isRenderNeeded = true;
            }
        }
        private bool _highlight;

        public bool Highlight {
            get => _highlight;
            set {
                _highlight = value;
                _isRenderNeeded = true;
            }
        }
        // -- State tracking fields
        private bool _isRenderNeeded;
        private bool _isUpperHalf;
        private bool _masked;
        private int _lastMouseY;
        private bool _isLastUpperHalf;


        private readonly ImageAttributes _maskedAttributes = new ImageAttributes();
        public Bitmap[] ImageList { get; set; }

        public FrequencyEditDigit(int digitIndex) {
            InitializeComponent();
            DigitIndex = digitIndex;
            _displayedDigit = 0;
            var cm = new ColorMatrix {Matrix33 = Constants.MaskedDigitTransparency};
            _maskedAttributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }

        #region Mouse Events

        private void FrequencyEditDigit_MouseDown(object sender, MouseEventArgs e) {
            _isUpperHalf = (e.Y <= ClientRectangle.Height / 2);
            var eventArgs = new DigitClickEventArgs(_isUpperHalf, e.Button);
            OnDigitClick?.Invoke(this, eventArgs);
            tickTimer.Interval = 300;
            tickTimer.Enabled = true;
        }

        private void FrequencyEditDigit_MouseEnter(object sender, EventArgs e) {
            IsCursorInside = true;
            _isRenderNeeded = true;
            Focus();
        }

        private void FrequencyEditDigit_MouseLeave(object sender, EventArgs e) {
            IsCursorInside = false;
            _isRenderNeeded = true;
        }

        private void FrequencyEditDigit_MouseMove(object sender, MouseEventArgs e) {
            _isLastUpperHalf = e.Y <= ClientRectangle.Height / 2;
            _lastMouseY = e.Y;

            if (_isUpperHalf != _isLastUpperHalf) {
                _isRenderNeeded = true;
            }

            _isLastUpperHalf = _isUpperHalf;
        }

        private void FrequencyEditDigit_MouseUp(object sender, MouseEventArgs e) {
            tickTimer.Enabled = false;
        }

        private void FrequencyEditDigit_Scroll(object sender, ScrollEventArgs e) {
            var args = new DigitClickEventArgs((e.NewValue - e.OldValue > 0), MouseButtons.Left);
            OnDigitClick?.Invoke(this, args);
        }

        #endregion
        #region Rendering
        private void FrequencyEditDigit_Paint(object sender, PaintEventArgs e) {
            DrawNumber(e);
            DrawMouseover(e);
            DrawHighlight(e);
        }

        private void DrawHighlight(PaintEventArgs e) {
            if (!_highlight) return;

            var transparentColor = new SolidBrush(Color.FromArgb(25, Color.Red));
            e.Graphics.FillRectangle(transparentColor, new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height));
        }

        private void DrawNumber(PaintEventArgs e) {
            if (ImageList == null || DisplayedDigit >= ImageList.Length)
                return;

            Bitmap img = ImageList[DisplayedDigit];
            ImageAttributes attributes = ((_masked && !IsCursorInside) || !Parent.Enabled) ? _maskedAttributes : null;
            var rectangle = new Rectangle(0, 0, img.Width, img.Height);
            e.Graphics.DrawImage(img, rectangle, 0.0f, 0.0f, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
        }

        private void DrawMouseover(PaintEventArgs e) {
            if (!IsCursorInside || ((FrequencyEdit)base.Parent).EntryModeActive)
                return;

            bool isUpperHalf = (_lastMouseY <= ClientRectangle.Height / 2);
            Color transparentColor = Color.FromArgb(100, isUpperHalf ? Color.Red : Color.Blue);

            using (var transparentBrush = new SolidBrush(transparentColor)) {
                Rectangle rect;

                if (isUpperHalf)
                    rect = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height / 2);
                else
                    rect = new Rectangle(0, ClientRectangle.Height / 2, ClientRectangle.Width, ClientRectangle.Height);

                e.Graphics.FillRectangle(transparentBrush, rect);
            }
        }

        public void Render()
        {
            if (!_isRenderNeeded) return;
            Invalidate();
            _isRenderNeeded = false;
        }
        #endregion

        #region Event Emitters

        public event DigitClickEvent OnDigitClick;

        #endregion

        private void tickTimer_Tick(object sender, EventArgs e) {
            var args = new DigitClickEventArgs(_isLastUpperHalf, MouseButtons.Left);
            OnDigitClick?.Invoke(this, args);
            tickTimer.Tag = ((int) tickTimer.Tag) + 1;
            switch ((int) tickTimer.Tag) {
                case 10:
                    tickTimer.Interval = 200;
                    break;
                case 20:
                    tickTimer.Interval = 100;
                    break;
                case 50:
                    tickTimer.Interval = 50;
                    break;
                case 100:
                    tickTimer.Interval = 20;
                    break;
            }
        }
    }
}
