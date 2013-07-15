using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Data.Xml.Dom;

namespace UnitTestLibrary1
{
    
    [TestClass]
    public class UnitTest1
    {   
        [TestMethod]
        public void TestMethod1()
        {
            var hitBodies = new[] {1, 2, 3, 4};
            hitBodies = hitBodies.Zip(hitBodies.Skip(1), (a, b) => 
                a + b
            ).ToArray();
            foreach (var i in hitBodies)
            {
                Debug.WriteLine(i);
            }
        }
    }
}
