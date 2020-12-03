using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BetterSDR.Common;

namespace BetterSDR.RTLSDR {
    public enum SamplingMode {
        Quadrature = 0,
        DirectSamplingI,
        DirectSamplingQ
    }

    /// <summary>
    /// C# Wrapper for a friendly interface with an RTL-SDR Dongle
    /// </summary>
    public sealed class RtlDevice : IDisposable, ISampleProvider {
        private const uint DefaultFrequency = 1090000000;
        private const int DefaultSampleRate = 2000000;

        private IntPtr _dev;
        public uint Index { get; }
        public string Name { get; }
        #region Feature Toggles
        private bool _useTunerAgc = true;
        public bool UseTunerAGC {
            get => _useTunerAgc;
            set {
                _useTunerAgc = value;
                
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_tuner_gain_mode(_dev, _useTunerAgc ? 0 : 1);
            }
        }

        private bool _useRtlAgc; 
        public bool UseRtlAGC {
            get => _useRtlAgc;
            set {
                _useRtlAgc = value;
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_agc_mode(_dev, _useRtlAgc ? 1 : 0);
            }
        }

        private int _tunerGain;
        public int TunerGain {
            get => _tunerGain;
            set {
                _tunerGain = value;
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_tuner_gain(_dev, _tunerGain);
            }
        }

        private uint _centerFrequency = DefaultFrequency;
        public uint Frequency {
            get => _centerFrequency;
            set {
                _centerFrequency = value;
                if (_dev == IntPtr.Zero) return;

                LibraryWrapper.rtlsdr_set_center_freq(_dev, _centerFrequency);
            }
        }

        public Form GetSettingsForm() {
            return _settingsForm;
        }

        private uint _sampleRate = DefaultSampleRate;

        public uint SampleRate {
            get => _sampleRate;
            set {
                _sampleRate = value;
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_sample_rate(_dev, _sampleRate);
            }
        }

        private bool _useBiasTee;
        public bool BiasTee {
            get => _useBiasTee;
            set {
                _useBiasTee = value;
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_bias_tee(_dev, value ? 1 : 0);
            }
        }

        private int _frequencyCorrection;
        public int FrequencyCorrection {
            get => _frequencyCorrection;
            set {
                _frequencyCorrection = value;
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_freq_correction(_dev, _frequencyCorrection);
            }
        }

        private SamplingMode _samplingMode;
        public SamplingMode SamplingMode {
            get => _samplingMode;
            set {
                _samplingMode = value;
                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_direct_sampling(_dev, (int) _samplingMode);
            }
        }

        private bool _useOffsetTuning;
        public bool UseOffsetTuning {
            get => _useOffsetTuning;
            set {
                _useOffsetTuning = value;

                if (_dev != IntPtr.Zero) 
                    LibraryWrapper.rtlsdr_set_offset_tuning(_dev, _useOffsetTuning ? 1 : 0);
            }
        }

        public bool UseLookupTable { get; set; }
        #endregion

        #region Supported Features
        public RtlSdrTunerType TunerType => _dev == IntPtr.Zero ? RtlSdrTunerType.Unknown : LibraryWrapper.rtlsdr_get_tuner_type(_dev);
        public bool SupportsOffsetTuning { get; }
        public int[] SupportedGains { get; }
        #endregion

        #region Incoming Data management
        public ComplexBuffer Buffer { get; set; }
        private Thread _worker;
        public static readonly uint ReadLength = (16 * 16384);   /* 256k */
        #endregion

        private static readonly float[] LookUpTable;
        private SettingsForm _settingsForm;
        static RtlDevice() {
            LookUpTable = new float[256];
            const float scale = 1.0f / 127.0f;

            for (var i = 0; i < 256; i++) {
                LookUpTable[i] = (i - 128) * scale;
            }
            
        }
        public RtlDevice(int index) {
            Index = (uint)index;
            Buffer = new ComplexBuffer();
            UseLookupTable = false;
            _settingsForm = new SettingsForm();

            int r = LibraryWrapper.rtlsdr_open(out _dev, Index);
            
            if (r != 0)
                throw new ApplicationException("Cannot open RTL device. Is the device locked somewhere?");

            int count = _dev == IntPtr.Zero ? 0 : LibraryWrapper.rtlsdr_get_tuner_gains(_dev, null);
            
            if (count < 0) count = 0;

            SupportsOffsetTuning = LibraryWrapper.rtlsdr_set_offset_tuning(_dev, 0) != -2;
            SupportedGains = new int[count];
            
            if (count >= 0) 
                LibraryWrapper.rtlsdr_get_tuner_gains(_dev, SupportedGains);

            Name = LibraryWrapper.rtlsdr_get_device_name(Index);
        }

        public static Dictionary<int, string> GetAvailableDevices() {
            var result = new Dictionary<int, string>();
            var count = (int)LibraryWrapper.rtlsdr_get_device_count();

            for (var i = 0; i < count; i++) {
                string name = LibraryWrapper.rtlsdr_get_device_name((uint)i);
                result.Add(i, i + "]" + name);
            }

            return result;
        }

        public void Dispose() {
            Stop();
            LibraryWrapper.rtlsdr_close(_dev);
            _dev = IntPtr.Zero;
        }

        /// <summary>
        /// Setup the device and begin reading for samples
        /// </summary>
        public void Start() {
            if (_worker != null)
                throw new ApplicationException("Already running");

            int r = LibraryWrapper.rtlsdr_set_center_freq(_dev, _centerFrequency);

            if (r != 0)
                throw new ApplicationException("Cannot access RTL device");

            r = LibraryWrapper.rtlsdr_set_tuner_gain_mode(_dev, _useTunerAgc ? 0 : 1);
            if (r != 0)
                throw new ApplicationException("Cannot access RTL device");

            if (!_useTunerAgc) {
                r = LibraryWrapper.rtlsdr_set_tuner_gain(_dev, _tunerGain);

                if (r != 0)
                    throw new ApplicationException("Cannot access RTL device");
            }

            LibraryWrapper.rtlsdr_set_freq_correction(_dev, 52);
            
            if (_useRtlAgc) 
                LibraryWrapper.rtlsdr_set_agc_mode(_dev, 1);

            LibraryWrapper.rtlsdr_set_center_freq(_dev, DefaultFrequency);
            LibraryWrapper.rtlsdr_set_sample_rate(_dev, SampleRate);
            r = LibraryWrapper.rtlsdr_reset_buffer(_dev);

            if (r != 0)
                throw new ApplicationException("Cannot access RTL device");

            _worker = new Thread(StreamProc) {Priority = ThreadPriority.Highest};
            _worker.Start();
        }

        /// <summary>
        /// Stop reading for samples.
        /// </summary>
        public void Stop() {
            if (_worker == null) {
                return;
            }
            LibraryWrapper.rtlsdr_cancel_async(_dev);
            _worker.Join(100);
            _worker = null;
        }

        public bool IsStreaming => _worker != null;

        #region Streaming methods

        /// <summary>
        /// Initiate a read of samples from the RTL Dongle. The device will call our callback once the given read length is reached.
        /// </summary>
        private void StreamProc() {
            LibraryWrapper.rtlsdr_read_async(_dev, RtlSdrSamplesAvailable, IntPtr.Zero, 0, ReadLength);
        }

        /// <summary>
        /// Callback for handling the incoming data.
        /// </summary>
        /// <param name="buf">Pointer to a unmanaged memory buffer containing our samples</param>
        /// <param name="len">The length of the samples received</param>
        /// <param name="ctx">Pointer to the device giving us this data.</param>
        private void RtlSdrSamplesAvailable(IntPtr buf, uint len, IntPtr ctx) {
            if (len > ReadLength) len = ReadLength;

            var actualBuffer = new byte[len];
            Marshal.Copy(buf, actualBuffer, 0, (int)len);

            var myList = new Complex[len / 2];
            var j = 0;

            for (var i = 0; i < len; i+=2) {
                byte real = UseLookupTable ? (byte)LookUpTable[actualBuffer[i]] : actualBuffer[i];
                byte imaginary = UseLookupTable ? (byte)LookUpTable[actualBuffer[i+1]] : actualBuffer[i+1];

                var myComplex = new Complex(real, imaginary);
                myList[j++] = (myComplex);
            }

            Buffer.Add(myList);

            DataAvailable?.Invoke(); // -- Let any consumers know that we have data available for consumption.
        }

        public event EmptyEventDelegate DataAvailable;

        #endregion
    }


}
