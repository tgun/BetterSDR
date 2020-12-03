using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BetterSDR.Common;
using BetterSDR.Controls;
using BetterSDR.Properties;
using BetterSDR.RTLSDR;
using MathNet.Numerics;
using ScottPlot;
using ScottPlot.Drawing;
using Complex = System.Numerics.Complex;
using Fourier = BetterSDR.Common.Fourier;

namespace BetterSDR {
    public partial class MainForm : Form {
        private ISampleProvider _rtlDevice;
        private double[] _qFft;
        private Complex[] _rawPcm;
        private readonly object _pcmLock = new object();
        private Thread _renderThread;
        private bool _canRender = true;

        public MainForm() {
            InitializeComponent();
            InitPlot();
            frequencyEdit1.Frequency = 0100300000L;
        }

        private void InitPlot() {
            if (this.InvokeRequired) {
                this.Invoke(new BlankEventArgs(InitPlot));
                return;
            }

            if (_qFft == null)
                return;

            formsPlot1.plt.Clear();
            double fftSpacing = _rtlDevice.SampleRate / _qFft.Length;
            var sig = formsPlot1.plt.PlotSignal(_qFft, 48000, markerSize: 0, useParallel: false);
            sig.fillType = FillType.FillBelow;
            sig.fillColor1 = Color.DodgerBlue;
            sig.gradientFillColor1 = Color.Transparent;
            formsPlot1.plt.PlotHLine(0, Color.Black);
            formsPlot1.plt.YLabel("Power");
            formsPlot1.plt.XLabel("Frequency");
            formsPlot1.plt.Style(Style.Gray1);
            formsPlot1.plt.Colorset(Colorset.OneHalfDark);
            formsPlot1.Render();
        }
        private void button1_Click(object sender, EventArgs e) {
                var myDevice = new RtlDevice(0);
                
                _rtlDevice = myDevice;
                _rtlDevice.SampleRate = (uint)(2.048 * 1000000.0);
                myDevice.UseOffsetTuning = false;
                myDevice.SamplingMode = 0;
                myDevice.FrequencyCorrection = 0;
                myDevice.UseRtlAGC = true;
                //myDevice.UseLookupTable = true;
                myDevice.UseTunerAGC = false;
                myDevice.TunerGain = 496;
                _rtlDevice.DataAvailable += MyDevice_DataAvailable;
                // 1.090.000.000
                // 0.100.300.000
                myDevice.Frequency = (uint)(0100300000L);
                myDevice.Start();

            if (_renderThread == null) {
                _renderThread = new Thread(Render);
            }

            _renderThread.Start();
            timer1.Enabled = true;
            startButton.Enabled = false;
        }

        private void Render() {
            while (true) {
                if (_rtlDevice == null) {
                    Thread.Sleep(2);
                    continue;
                }

                UpdateFft();
                
                if (formsPlot1.plt.GetPlottables().Count == 0)
                    InitPlot();

                Thread.Sleep(1);
            }
        }

        private void MyDevice_DataAvailable() {
            var readLength = (int)(_rtlDevice.SampleRate / 2);

            if (readLength > _rtlDevice.Buffer.Length)
                readLength = _rtlDevice.Buffer.Length;

            Complex[] mySample = _rtlDevice.Buffer.Read(readLength);

            lock (_pcmLock) {
                _rawPcm = mySample;
            }
        }

        public delegate void BlankEventArgs();
        private void UpdateFft() {
            if (this.InvokeRequired) {
                this.Invoke(new BlankEventArgs(UpdateFft));
                return;
            }

            if (_rawPcm == null)
                return;

            Complex[] myCopy;
            double[] mags;

            lock (_pcmLock) {
                double[] blackWindow = Window.Blackman(_rawPcm.Length);
                myCopy = new Complex[_rawPcm.Length];
                mags = new double[_rawPcm.Length];
                for (var i = 0; i < _rawPcm.Length; i++) {
                    double newReal = _rawPcm[i].Real * blackWindow[i];
                    double newImag = _rawPcm[i].Imaginary * blackWindow[i];

                    myCopy[i] = new Complex(newReal, newImag);
                    mags[i] = newReal;
                }
            }
            var fftGain = (float)(10.0 * Math.Log10((double)myCopy.Length / 2));
            float compensation = 24.0f - fftGain + -120.0f;

            if (_qFft == null)
                _qFft = new double[myCopy.Length];

            Fourier.ForwardTransform(myCopy, myCopy.Length);
            Fourier.SpectrumPower(myCopy, ref _qFft, myCopy.Length, compensation);


            var temp = new double[_rawPcm.Length];
            Fourier.SmoothMaxCopy(_qFft, ref temp, 1.0f, 0);

            var scaledPower = new byte[_rawPcm.Length];
            Fourier.ScaleFFT(temp, ref scaledPower, scaledPower.Length, -130, 0);


            for (var i = 0; i < _qFft.Length; i++) {
                // -- ? Attack : Decay
                double ratio = _qFft[i] < scaledPower[i] ? 0.9 : 0.3;
                _qFft[i] = Math.Round(_qFft[i] * (1 - ratio) + scaledPower[i] * ratio);
            }
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void frequencyEdit1_FrequencyUpdated(long frequency) {
            if (_rtlDevice != null)
                _rtlDevice.Frequency = (uint) frequency;

        }

        private void btnSettings_Click(object sender, EventArgs e) {
            var setsForm = _rtlDevice.GetSettingsForm();
            setsForm.Show();
        }

        private void trackBar1_Scroll(object sender, EventArgs e) {
        }

        private void frequencyEdit1_Load(object sender, EventArgs e) {

        }

        private void MainForm_ResizeBegin(object sender, EventArgs e) {
            _canRender = false;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e) {
            _canRender = true;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (_canRender)
                formsPlot1.Render();
        }
    }
}
