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
            int stride = getStride(pixWidth: 5, bytesPerPix: 3);
            Assert.That(stride, Is.EqualTo(16));
        }

        [Test]
        public void GetMinimumStride_JustAboveBarrier_RoundsUp()
        {
            int stride = getStride(pixWidth: 3, bytesPerPix: 3);
            Assert.That(stride, Is.EqualTo(12));
        }

        [Test]
        public void GetMinimumStride_ExactlyAtBarrier_ReturnsExact()
        {
            int stride = getStride(pixWidth: 6, bytesPerPix: 4);
            Assert.That(stride, Is.EqualTo(24));
        }

        [Test]
        public void GetMinimumStride_HalfwayBetweenBarriers_RoundsUp()
        {
            int stride = getStride(pixWidth: 5, bytesPerPix: 2);
            Assert.That(stride, Is.EqualTo(12));
        }

        private static int getStride(int pixWidth, int bytesPerPix)
        {
            return FluentBitmap.GetMinimumStride(pixWidth, bytesPerPix);
        }
    }
}
