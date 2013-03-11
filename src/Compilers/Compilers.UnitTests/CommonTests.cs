using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.DataStructures;

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
    }
}
