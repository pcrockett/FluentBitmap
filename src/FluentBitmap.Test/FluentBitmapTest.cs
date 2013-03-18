using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FluentBitmap.Test
{
    [TestFixture]
    public class FluentBitmapTest
    {
        [Test]
        public void GetMinimumStride_JustBelowBarrier_RoundsUp()
        {
            int stride = getStride(pixWidth: 5, bitsPerPix: 24);
            Assert.That(stride, Is.EqualTo(16));
        }

        [Test]
        public void GetMinimumStride_JustAboveBarrier_RoundsUp()
        {
            int stride = getStride(pixWidth: 3, bitsPerPix: 24);
            Assert.That(stride, Is.EqualTo(12));
        }

        [Test]
        public void GetMinimumStride_ExactlyAtBarrier_ReturnsExact()
        {
            int stride = getStride(pixWidth: 6, bitsPerPix: 32);
            Assert.That(stride, Is.EqualTo(24));
        }

        [Test]
        public void GetMinimumStride_HalfwayBetweenBarriers_RoundsUp()
        {
            int stride = getStride(pixWidth: 5, bitsPerPix: 16);
            Assert.That(stride, Is.EqualTo(12));
        }

        [Test]
        public void GetMinimumStride_SingleBitPerPixel_IsCorrect()
        {
            int stride = getStride(pixWidth: 100, bitsPerPix: 1);
            Assert.That(stride, Is.EqualTo(16));
        }

        [Test]
        public void GetMinimumStride_TwoBitsPerPixel_IsCorrect()
        {
            int stride = getStride(pixWidth: 20, bitsPerPix: 2);
            Assert.That(stride, Is.EqualTo(8));
        }

        private static int getStride(int pixWidth, int bitsPerPix)
        {
            return FluentBitmap.GetMinimumStride(pixWidth, bitsPerPix);
        }
    }
}
