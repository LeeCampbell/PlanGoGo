using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlanGoGo.ProjectPlan.Tests
{
    [TestClass]
    public class GoalFixture
    {
        [TestMethod]        
        public void TestMethod1()
        {
            var eventRaised = false;
            var sut = new Goal();
            sut.PropertyChanged += (s, e) => { eventRaised = true; };
            sut.ManDays = 2;

            Assert.IsTrue(eventRaised);
        }
    }
}
