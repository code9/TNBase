﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TNBase.DataStorage;
using TNBase.Objects;
using System.Collections.Generic;

namespace TNBase.Test
{
    [TestClass]
    public class GenericTester
    {
        [TestMethod]
        public void Generic_BasicTimeTest()
        {
            ModuleGeneric.SaveStartTime();
            ModuleGeneric.saveEndTime();
            ModuleGeneric.getStartTimeString();
            ModuleGeneric.getEndTimeString();
            Assert.AreEqual("00:00:00", ModuleGeneric.getElapsedTimeString());
        }

        /// <summary>
        /// Base for the weekly stat test methods
        /// </summary>
        /// <param name="newStatsWeek">Is it a new stats week?</param>
        /// <param name="expectingInOuts">Should the in out bits be updated?</param>
        private void WeekStatTest_Base(bool newStatsWeek)
        {
            Mock<IServiceLayer> mockServiceLayer = new Mock<IServiceLayer>();
            mockServiceLayer.Setup(x => x.GetCurrentWeekNumber()).Returns(100);
            mockServiceLayer.Setup(x => x.GetCurrentListenerCount()).Returns(10);
            mockServiceLayer.Setup(x => x.GetListenersByStatus(It.IsAny<ListenerStates>())).Returns(new List<Listener>() { });
            mockServiceLayer.Setup(x => x.WeeklyStatExistsForWeek(100)).Returns(newStatsWeek);
            mockServiceLayer.Setup(x => x.GetCurrentWeekStats()).Returns(new WeeklyStats { WeekNumber = 100 });

            ModuleGeneric.UpdateStatsWeek(mockServiceLayer.Object);

            // Check we add/update the week stats
            if (newStatsWeek)
            {
                mockServiceLayer.Verify(x => x.UpdateWeeklyStats(It.Is<WeeklyStats>(y => y.TotalListeners == 10 && y.WeekNumber == 100 && y.PausedCount == 0)), Times.Once);
            }
            else
            {
                mockServiceLayer.Verify(x => x.SaveWeekStats(It.Is<WeeklyStats>(y => y.TotalListeners == 10 && y.WeekNumber == 100 && y.PausedCount == 0)), Times.Once);
            }
        }

        [TestMethod]
        public void Generic_WeekStatSave_SaveWeekStats()
        {
            WeekStatTest_Base(false);
        }

        [TestMethod]
        public void Generic_WeekStatSave_UpdateWeeklyStats()
        {
            WeekStatTest_Base(true);
        }
    }
}
