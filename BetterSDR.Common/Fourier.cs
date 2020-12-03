using System;
using System.Numerics;

namespace BetterSDR.Common {
    public class Fourier {
        private const int MaxLutBits = 16; // 64k
        public const int MaxLutBins = 1 << MaxLutBits;
        public const int LutSize = MaxLutBins / 2;

        public static Complex[] _lut = new Complex[LutSize];

        static Fourier() {
            const double angle = (Math.PI * 2.0) / MaxLutBins;
            for (var i = 0; i < LutSize; i++)
                _lut[i] = FromAngle(angle * i).Conjugate();
        }

        public static void SpectrumPower(Complex[] buffer, ref double[] power, int length, float offset) {
            for (var i = 0; i < length; i++) {
                var m = buffer[i].Real * buffer[i].Real + buffer[i].Imaginary * buffer[i].Imaginary;
                var strength = (float)(10.0 * Math.Log10(1e-60 + m)) + offset;
                power[i] = strength;
            }
        }

        public static void ScaleFFT(float[] src, ref byte[] dest, int length, float minPower, float maxPower) {
            var scale = Byte.MaxValue / (maxPower - minPower);
            for (var i = 0; i < length; i++) {
                var magnitude = src[i];
                if (magnitude < minPower) {
                    magnitude = minPower;
                }
                else if (magnitude > maxPower) {
                    magnitude = maxPower;
                }
                dest[i] = (byte)((magnitude - minPower) * scale);
            }
        }


        public static void ScaleFFT(double[] src, ref byte[] dest, int length, float minPower, float maxPower) {
            var scale = Byte.MaxValue / (maxPower - minPower);
            for (var i = 0; i < length; i++) {
                var magnitude = src[i];
                if (magnitude < minPower) {
                    magnitude = minPower;
                }
                else if (magnitude > maxPower) {
                    magnitude = maxPower;
                }
                dest[i] = (byte)((magnitude - minPower) * scale);
            }
        }

        public static void SmoothCopy(byte[] srcPtr, ref byte[] dstPtr, float scale, int offset) {
            int sourceLength = srcPtr.Length;
            int destinationLength = dstPtr.Length;

            var r = sourceLength / scale / destinationLength;
            if (r > 1.0f) {
                var n = (int)Math.Ceiling(r);
                for (var i = 0; i < destinationLength; i++) {
                    var k = (int)(i * r - n * 0.5f);
                    var max = (byte)0;
                    for (var j = 0; j < n; j++) {
                        var index = k + j + offset;
                        if (index >= 0 && index < sourceLength) {
                            if (max < srcPtr[index]) {
                                max = srcPtr[index];
                            }
                        }
                    }
                    dstPtr[i] = max;
                }
            }
            else {
                for (var i = 0; i < destinationLength; i++) {
                    var index = (int)(r * i + offset);
                    if (index >= 0 && index < sourceLength) {
                        dstPtr[i] = srcPtr[index];
                    }
                }
            }
        }
        public static void SmoothMinCopy(float[] source, ref float[] destination, float zoom = 1f, float offset = 0f) {
            int sourceLength = source.Length;
            int destinationLength = destination.Length;

            if (zoom < 1f) zoom = 1f;

            float num = sourceLength / (zoom * destinationLength);
            float num2 = sourceLength * (offset + 0.5f * (1f - 1f / zoom));

            if (num > 1f) {
                var num3 = (int)Math.Ceiling(num * 0.5);
                int num4 = -1;

                for (var i = 0; i < destinationLength; i++) {
                    var num5 = 600f;
                    for (int j = -num3; j <= num3; j++) {
                        var num6 = (int)Math.Round(num2 + num * i + j);
                        if (num6 > num4 && num6 >= 0 && num6 < sourceLength && num5 > source[num6]) num5 = (float)source[num6];
                        num4 = num6;
                    }
                    destination[i] = num5;
                }

                return;
            }

            for (var k = 0; k < destinationLength; k++) {
                var num7 = (int)(num * k + num2);
                if (num7 >= 0 && num7 < sourceLength) destination[k] = source[num7];
            }
        }

        public static void SmoothMinCopy(double[] source, ref double[] destination, float zoom = 1f, float offset = 0f) {
            int sourceLength = source.Length;
            int destinationLength = destination.Length;

            if (zoom < 1f) zoom = 1f;

            float num = sourceLength / (zoom * destinationLength);
            float num2 = sourceLength * (offset + 0.5f * (1f - 1f / zoom));
            
            if (num > 1f) {
                var num3 = (int)Math.Ceiling(num * 0.5);
                int num4 = -1;
                
                for (var i = 0; i < destinationLength; i++) {
                    var num5 = 600f;
                    for (int j = -num3; j <= num3; j++) {
                        var num6 = (int)Math.Round(num2 + num * i + j);
                        if (num6 > num4 && num6 >= 0 && num6 < sourceLength && num5 > source[num6]) num5 = (float)source[num6];
                        num4 = num6;
                    }
                    destination[i] = num5;
                }

                return;
            }

            for (var k = 0; k < destinationLength; k++) {
                var num7 = (int)(num * k + num2);
                if (num7 >= 0 && num7 < sourceLength) destination[k] = source[num7];
            }
        }

        public static void SmoothMaxCopy(float[] source, ref float[] dest, float zoom = 1f, float offset = 0f) {
            var sourceLength = source.Length;
            var destLength = dest.Length;

            if (zoom < 1f)
                zoom = 1f;

            float r = sourceLength / (zoom * destLength);
            float t = sourceLength * (offset + 0.5f * (1f - 1f / zoom));
            if (r > 1f) {
                var halfed = (int)Math.Ceiling(r * 0.5);
                var k = -1;
                for (var i = 0; i < destLength; i++) {
                    var d = -600f;
                    for (var j = -halfed; j <= halfed; j++) {
                        int meh = (int)Math.Round(t + r * i + j);
                        if (meh > k && meh >= 0 && meh < sourceLength && d < source[meh])
                            d = source[meh];
                        k = meh;
                    }

                    dest[i] = d;
                }

                return;
            }

            int num7 = (int)Math.Ceiling(1f / r);
            float num8 = 1f / num7;
            int num9 = 0;
            int num10 = (int)t;
            int num11 = num10 + 1;
            int num12 = sourceLength - 1;
            for (int k = 0; k < destLength; k++) {
                int num13 = (int)(t + k * r);
                if (num13 > num10) {
                    num9 = 0;
                    if (num13 >= num12) {
                        num10 = num12;
                        num11 = num12;
                    }
                    else {
                        num10 = num13;
                        num11 = num13 + 1;
                    }
                }

                dest[k] = (source[num10] * (num7 - num9) + source[num11] * num9) * num8;
                num9++;
            }
        }


        public static void SmoothMaxCopy(double[] source, ref double[] dest, float zoom = 1f, float offset = 0f) {
            var sourceLength = source.Length;
            var destLength = dest.Length;

            if (zoom < 1f)
                zoom = 1f;

            float r = sourceLength / (zoom * destLength);
            float t = sourceLength * (offset + 0.5f * (1f - 1f / zoom));
            if (r > 1f) {
                var halfed = (int) Math.Ceiling(r * 0.5);
                var k = -1;
                for (var i = 0; i < destLength; i++) {
                    var d = -600d;
                    for (var j = -halfed; j <= halfed; j++) {
                        int meh = (int) Math.Round(t + r * i + j);
                        if (meh > k && meh >= 0 && meh < sourceLength && d < source[meh])
                            d = source[meh];
                        k = meh;
                    }

                    dest[i] = d;
                }

                return;
            }

            int num7 = (int) Math.Ceiling(1f / r);
            float num8 = 1f / num7;
            int num9 = 0;
            int num10 = (int) t;
            int num11 = num10 + 1;
            int num12 = sourceLength - 1;
            for (int k = 0; k < destLength; k++) {
                int num13 = (int) (t + k * r);
                if (num13 > num10) {
                    num9 = 0;
                    if (num13 >= num12) {
                        num10 = num12;
                        num11 = num12;
                    }
                    else {
                        num10 = num13;
                        num11 = num13 + 1;
                    }
                }

                dest[k] = (source[num10] * (num7 - num9) + source[num11] * num9) * num8;
                num9++;
            }
        }


        public static void ForwardTransform(Complex[] buffer, int length) {
            if (length <= MaxLutBins)
                ForwardTransformLut(buffer, length);
            else
                ForwardTransformRot(buffer, length);
        }

        private static void ForwardTransformLut(Complex[] buffer, int length) {
            int nm1 = length - 1;
            int nd2 = length / 2;
            int i, j, jm1, k, l, m, n, le, le2, ip, nd4;
            Complex u, t;

            m = 0;
            i = length;
            while (i > 1) {
                ++m;
                i = (i >> 1);
            }

            j = nd2;

            for (i = 1; i < nm1; ++i) {
                if (i < j) {
                    t = buffer[j];
                    buffer[j] = buffer[i];
                    buffer[i] = t;
                }

                k = nd2;

                while (k <= j) {
                    j = j - k;
                    k = k / 2;
                }

                j += k;
            }

            for (l = 1; l <= m; ++l) {
                le = 1 << l;
                le2 = le / 2;

                n = MaxLutBits - l;

                for (j = 1; j <= le2; ++j) {
                    jm1 = j - 1;

                    u = _lut[jm1 << n];

                    for (i = jm1; i <= nm1; i += le) {
                        ip = i + le2;

                        t = u * buffer[ip];
                        buffer[ip] = buffer[i] - t;
                        buffer[i] += t;
                    }
                }
            }

            nd4 = nd2 / 2;
            for (i = 0; i < nd4; i++) {
                t = buffer[i];
                buffer[i] = buffer[nd2 - i - 1];
                buffer[nd2 - i - 1] = t;

                t = buffer[nd2 + i];
                buffer[nd2 + i] = buffer[nd2 + nd2 - i - 1];
                buffer[nd2 + nd2 - i - 1] = t;
            }
        }

        public static Complex FromAngle(double angle) {
            Complex result = new Complex(Math.Cos(angle), Math.Sin(angle));
            return result;
        }

        private static void ForwardTransformRot(Complex[] buffer, int length) {
            int nm1 = length - 1;
            int nd2 = length / 2;
            int i, j, jm1, k, l, m, le, le2, ip, nd4;
            Complex u, t;

            m = 0;
            i = length;
            while (i > 1) {
                ++m;
                i = (i >> 1);
            }

            j = nd2;

            for (i = 1; i < nm1; ++i) {
                if (i < j) {
                    t = buffer[j];
                    buffer[j] = buffer[i];
                    buffer[i] = t;
                }

                k = nd2;

                while (k <= j) {
                    j = j - k;
                    k = k / 2;
                }

                j += k;
            }

            for (l = 1; l <= m; ++l) {
                le = 1 << l;
                le2 = le / 2;

                var angle = Math.PI / le2;

                for (j = 1; j <= le2; ++j) {
                    jm1 = j - 1;

                    u = FromAngle(angle * jm1).Conjugate();

                    for (i = jm1; i <= nm1; i += le) {
                        ip = i + le2;

                        t = u * buffer[ip];
                        buffer[ip] = buffer[i] - t;
                        buffer[i] += t;
                    }
                }
            }

            nd4 = nd2 / 2;
            for (i = 0; i < nd4; i++) {
                t = buffer[i];
                buffer[i] = buffer[nd2 - i - 1];
                buffer[nd2 - i - 1] = t;

                t = buffer[nd2 + i];
                buffer[nd2 + i] = buffer[nd2 + nd2 - i - 1];
                buffer[nd2 + nd2 - i - 1] = t;
            }
        }
    }
}
