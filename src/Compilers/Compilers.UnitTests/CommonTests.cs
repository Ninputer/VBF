using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Common;

namespace Compilers.UnitTests
{
    [TestFixture]
    public class CommonTests
    {
        [Test]
        public void EditDistanceEqualTest()
        {
            const string str1 = "abc";
            const string str2 = "abc";

            var ed = StringHelpers.EditDistance(str1, str2);

            Assert.AreEqual(0, ed);
        }

        [Test]
        public void EditDistanceAdd1Test()
        {
            const string str1 = "abc";
            const string str2 = "abcd";

            var ed = StringHelpers.EditDistance(str1, str2);

            Assert.AreEqual(1, ed);
        }

        [Test]
        public void EditDistanceRemove1Test()
        {
            const string str1 = "abc";
            const string str2 = "ac";

            var ed = StringHelpers.EditDistance(str1, str2);

            Assert.AreEqual(1, ed);
        }

        [Test]
        public void EditDistanceReplace1Test()
        {
            const string str1 = "abc";
            const string str2 = "ebc";

            var ed = StringHelpers.EditDistance(str1, str2);

            Assert.AreEqual(1, ed);
        }

        [Test]
        public void EditDistanceGeneralTest1()
        {
            const string str1 = "snowy";
            const string str2 = "sunny";

            var ed = StringHelpers.EditDistance(str1, str2);

            Assert.AreEqual(3, ed);
        }

        [Test]
        public void EditDistanceGeneralTest2()
        {
            const string str1 = "exponential";
            const string str2 = "polynomial";

            var ed = StringHelpers.EditDistance(str1, str2);

            Assert.AreEqual(6, ed);
        }

        [Test]
        public void EditDistanceZeroLengthTest()
        {
            var ed = StringHelpers.EditDistance(String.Empty, String.Empty);

            Assert.AreEqual(0, ed);
        }

        [Test]
        public void PriorityQueueMinTest()
        {
            var minpq = new PriorityQueue<int>(new[] { 3, 7, 1, 4, 5, 2, 9, 6 }, ExtremeType.Minimum);

            int last = 0;

            while (!minpq.IsEmpty)
            {
                int curr = minpq.DeleteExtreme();

                Assert.Greater(curr, last);

                last = curr;
            }
        }

        [Test]
        public void PriorityQueueMaxTest()
        {
            var maxpq = new PriorityQueue<int>(new[] { 3, 7, 1, 4, 5, 2, 9, 6 }, ExtremeType.Maximum);

            int last = Int32.MaxValue;

            while (!maxpq.IsEmpty)
            {
                int curr = maxpq.DeleteExtreme();

                Assert.Less(curr, last);

                last = curr;
            }
        }

        [Test]
        public void PriorityQueueMinFloatTest()
        {
            var minpq = new PriorityQueue<float>(new[] { 0.3f, 0.7f, 0.1f, 0.4f, 0.5f, 0.2f, 0.9f, 0.6f }, ExtremeType.Minimum);

            float last = 0.0f;

            while (!minpq.IsEmpty)
            {
                float curr = minpq.DeleteExtreme();

                Assert.Greater(curr, last);

                last = curr;
            }
        }

        [Test]
        public void PriorityQueueDecreaseKeyTest()
        {
            var minpq = new PriorityQueue<float>(new[] { 0.3f, 0.7f, 0.1f, 0.4f, 0.5f, 0.2f, 0.9f, 0.6f }, ExtremeType.Minimum);


            Assert.AreEqual(0.1f, minpq.PeekExtreme());

            minpq.ModifyValue(0.4f, 0.01f);

            Assert.AreEqual(0.01f, minpq.PeekExtreme());

            minpq.ModifyValue(0.01f, 0.001f);

            Assert.AreEqual(0.001f, minpq.PeekExtreme());
            
            float last = 0.0f;

            while (!minpq.IsEmpty)
            {
                float curr = minpq.DeleteExtreme();

                Assert.Greater(curr, last);

                last = curr;
            }
        }

        [Test]
        public void PriorityQueueIncreaseKeyTest()
        {
            var minpq = new PriorityQueue<float>(new[] { 0.3f, 0.7f, 0.1f, 0.4f, 0.5f, 0.2f, 0.9f, 0.6f }, ExtremeType.Minimum);


            Assert.AreEqual(0.1f, minpq.PeekExtreme());

            minpq.ModifyValue(0.4f, 0.55f);

            Assert.AreEqual(0.1f, minpq.PeekExtreme());

            minpq.ModifyValue(0.1f, 0.22f);

            Assert.AreEqual(0.2f, minpq.PeekExtreme());

            minpq.ModifyValue(0.5f, 500f);
            minpq.Insert(100f);
            minpq.Insert(501f);

            float last = 0.0f;

            while (!minpq.IsEmpty)
            {
                float curr = minpq.DeleteExtreme();

                Assert.AreNotEqual(0.4f, curr);
                Assert.Greater(curr, last);

                last = curr;
            }
        }

        [Test]
        public void PriorityQueueInsertTest()
        {
            var values = new[] { 3, 7, 1, 4, 5, 2, 9, 6 };
            var maxpq = new PriorityQueue<int>(ExtremeType.Maximum);

            Assert.IsTrue(maxpq.IsEmpty);

            for (int i = 0; i < values.Length; i++)
            {
                maxpq.Insert(values[i]);
            }

            Assert.AreEqual(9, maxpq.DeleteExtreme());

            maxpq.Insert(0);
            maxpq.Insert(8);

            int last = Int32.MaxValue;

            while (!maxpq.IsEmpty)
            {
                int curr = maxpq.DeleteExtreme();

                Assert.Less(curr, last);

                last = curr;
            }
        }
    }
}
