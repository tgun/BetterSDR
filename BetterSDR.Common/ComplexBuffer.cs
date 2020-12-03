using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterSDR.Common {
    public class ComplexBuffer : IDisposable {
        public int Length {
            get {
                lock (_opLocker) {
                    return _buffer.Length;
                }
            }
        }

        private Complex[] _buffer;
        private readonly object _opLocker = new object();

        //public event EventArgs DataAvailable;
        public ComplexBuffer() {
            _buffer = new Complex[0];
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the first object in the buffer without advancing the buffer position.
        /// </summary>
        /// <returns></returns>
        public Complex Peek() {
            lock (_opLocker) {
                return _buffer[0];
            }
        }

        /// <summary>
        /// Read a single item off the buffer.
        /// </summary>
        /// <returns></returns>
        public Complex ReadOne() {
            lock (_opLocker) {
                Complex value = _buffer[0];
                RemoveItems(1);
                return value;
            }
        }

        /// <summary>
        /// Read more than one item off the top of the buffer
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Complex[] Read(int length) {
            lock (_opLocker) {
                var myValue = new Complex[length];
                Array.Copy(_buffer, 0, myValue, 0, length);
                RemoveItems(length);
                return myValue;
            }
        }

        #region Buffer Control
        /// <summary>
        /// Add a single Complex item to the buffer
        /// </summary>
        /// <param name="data">The data to append to the buffer</param>
        public void Add(Complex data) {
            var myArray = new Complex[1] {data};
            Add(myArray);
        }

        /// <summary>
        /// Add an array of Complex data to the buffer
        /// </summary>
        /// <param name="data">The data to be appended to the buffer</param>
        public void Add(Complex[] data) {
            if (data == null || data.Length == 0)
                return;

            lock (_opLocker) {
                if (_buffer.Length == 0) {
                    _buffer = data;
                    return;
                }

                int tempLength = _buffer.Length + data.Length;
                var tempBuff = new Complex[tempLength];
                Array.Copy(_buffer, 0, tempBuff, 0, _buffer.Length);
                Array.Copy(data, 0, tempBuff, _buffer.Length, data.Length);

                _buffer = tempBuff;
            }
        }

        private void RemoveItems(int length) {
            int tempLength = _buffer.Length - length;
            var tempBuff = new Complex[tempLength];

            Array.Copy(_buffer, length, tempBuff, 0, tempLength);

            _buffer = tempBuff;
        }
        #endregion
    }
}
