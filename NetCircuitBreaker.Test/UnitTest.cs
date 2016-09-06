using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCircuitBreaker.Core;
using NetCircuitBreaker.CircuitStates;
using System.Threading;
using NetCircuitBreaker.Exceptions;

namespace cb.Test
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void should_keep_circuit_closed_because_no_error_happened()
        {
            var cb = new CircuitBreaker(5, 3, TimeSpan.FromSeconds(15));

            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);

            Assert.AreEqual(CircuitStatus.Closed, cb.GetState());
        }

        [TestMethod]
        public void should_keep_circuit_closed_beucase_not_reach_max_errors()
        {
            var cb = new CircuitBreaker(5, 3, TimeSpan.FromSeconds(15));

            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);

            Assert.AreEqual(CircuitStatus.Closed, cb.GetState());
        }

        [TestMethod]
        public void should_open_circuit_because_reach_max_errors()
        {
            var cb = new CircuitBreaker(5, 3, TimeSpan.FromSeconds(15));

            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);

            Assert.AreEqual(CircuitStatus.Open, cb.GetState());
        }

        [TestMethod]
        [ExpectedException(typeof(OpenCircuitException))]
        public void keep_circuit_open_because_not_reach_the_timeout()
        {
            var cb = new CircuitBreaker(
                maxErrors: 5, 
                maxSuccess: 3, 
                circuitReset: TimeSpan.FromSeconds(15)
                );

            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);
            ExecuteErrorAction(cb);

            Thread.Sleep(1000);

            ExecuteSucessAction(cb);
        }

        [TestMethod]
        public void should_half_open_circuit_because_reach_the_timeout()
        {
            var cb = new CircuitBreaker(5, 3, TimeSpan.FromSeconds(2));

            cb.Open("test");

            Thread.Sleep(3000);

            ExecuteSucessAction(cb);

            Assert.AreEqual(CircuitStatus.HalfOpen, cb.GetState());
        }

        [TestMethod]
        public void should_kepp_circuit_half_open_because_not_reach_max_success()
        {
            var cb = new CircuitBreaker(5, 3, TimeSpan.FromSeconds(15));

            cb.HalfOpen();

            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);

            Assert.AreEqual(CircuitStatus.HalfOpen, cb.GetState());
        }

        [TestMethod]
        public void should_close_circuit_because_reach_max_success()
        {
            var cb = new CircuitBreaker(5, 3, TimeSpan.FromSeconds(15));

            cb.HalfOpen();

            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);
            ExecuteSucessAction(cb);

            Assert.AreEqual(CircuitStatus.Closed, cb.GetState());
        }

        #region aux

        private void ExecuteSucessAction(CircuitBreaker cb)
        {
            cb.Execute(() => { });
        }

        private void ExecuteErrorAction(CircuitBreaker cb)
        {
            try
            {
                cb.Execute(() => { throw new Exception(); });
            }
            catch { }
        }

        #endregion
    }
}
