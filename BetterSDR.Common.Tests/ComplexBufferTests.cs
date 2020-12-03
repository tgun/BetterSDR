using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterSDR.Common.Tests {
    [TestClass]
    public class ComplexBufferTests {
        private ComplexBuffer _testBuffer;

        [TestInitialize]
        public void Setup() {
            _testBuffer = new ComplexBuffer();
        }

        [TestMethod]
        public void AddItemsTest() {
            var givenData = new Complex[3] {
                new Complex(1, -1),
                new Complex(2, -2),
                new Complex(3, -3)
            };

            _testBuffer.Add(givenData);

            var expectedBufferLength = 3;
            var actualBufferLength = _testBuffer.Length;

            Assert.AreEqual(expectedBufferLength, actualBufferLength);
        }

        [TestMethod]
        public void AddItemsNullTest() {
            var givenData = new Complex[3] {
                new Complex(1, -1),
                new Complex(2, -2),
                new Complex(3, -3)
            };

            _testBuffer.Add(givenData);
            _testBuffer.Add(null);

            var expectedBufferLength = 3;
            var actualBufferLength = _testBuffer.Length;

            Assert.AreEqual(expectedBufferLength, actualBufferLength);
        }

        [TestMethod]
        public void AddOneItemTest() {
            var givenData = new Complex(1, -1);

            _testBuffer.Add(givenData);

            var expectedBufferLength = 1;
            var actualBufferLength = _testBuffer.Length;

            Assert.AreEqual(expectedBufferLength, actualBufferLength);
        }

        [TestMethod]
        public void PeekItemTest() {
            var givenData = new Complex[3] {
                new Complex(1, -1),
                new Complex(2, -2),
                new Complex(3, -3)
            };

            _testBuffer.Add(givenData);

            var expectedItem = givenData[0];
            var actualItem = _testBuffer.Peek();

            var expectedLength = 3;
            var actualLength = _testBuffer.Length;

            Assert.AreEqual(expectedItem, actualItem);
            Assert.AreEqual(expectedLength, actualLength);
        }

        [TestMethod]
        public void ReadItemTest() {
            var givenData = new Complex[3] {
                new Complex(1, -1),
                new Complex(2, -2),
                new Complex(3, -3)
            };

            _testBuffer.Add(givenData);

            var expectedItem = givenData[0];
            var actualItem = _testBuffer.ReadOne();

            var expectedLength = 2;
            var actualLength = _testBuffer.Length;

            Assert.AreEqual(expectedItem, actualItem);
            Assert.AreEqual(expectedLength, actualLength);
        }

        [TestMethod]
        public void ReadMultipleTest() {
            var givenData = new Complex[3] {
                new Complex(1, -1),
                new Complex(2, -2),
                new Complex(3, -3)
            };

            var expectedData = new Complex[2] {
                new Complex(1, -1),
                new Complex(2, -2)
            };

            _testBuffer.Add(givenData);

            var actualData = _testBuffer.Read(2);

            var expectedLength = 1;
            var actualLength = _testBuffer.Length;

            for (var i = 0; i < 2; i++) {
                Assert.AreEqual(expectedData[i], actualData[i]);
            }

            Assert.AreEqual(expectedLength, actualLength);
        }
    }
}
