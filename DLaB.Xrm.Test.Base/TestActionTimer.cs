using System;
using System.Diagnostics;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{

    /// <summary>
    /// Timer class for timing actions during a test
    /// </summary>
#if !DEBUG_XRM_UNIT_TEST_CODE
    [DebuggerNonUserCode]
#endif
    public class TestActionTimer
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ITestLogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestActionTimer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TestActionTimer(ITestLogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Times the given action, and writes the message with the elapsed milliseconds to the trace listeners in
        /// the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="actionToTime"></param>
        /// <param name="actionDescription"></param>
        [DebuggerHidden]
        public void Time(Action actionToTime, string actionDescription)
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                actionToTime();
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error attempting to " + actionDescription + Environment.NewLine + Environment.NewLine + ex);
                throw;
            }
            finally
            {
                watch.Stop();
                Logger.WriteLine("Time to " + actionDescription + watch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Times the given action, and writes the message with the elapsed milliseconds to the trace listeners in
        /// the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="actionToTime"></param>
        /// <param name="actionDescriptionFormat"></param>
        /// <param name="values">Used for a String.Format.  The ElapsedMilliseconds will be added to the end of the array</param>
        [DebuggerHidden]
        public void Time(Action actionToTime, string actionDescriptionFormat, params object[] values)
        {
            Array.Resize(ref values, values.Length + 1);
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                actionToTime();
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error attempting to " + actionDescriptionFormat + Environment.NewLine + Environment.NewLine + ex, values);
                throw;
            }
            finally
            {
                watch.Stop();
                values[values.Length - 1] = watch.ElapsedMilliseconds;
                Logger.WriteLine("Time to " + actionDescriptionFormat, values);
            }
        }

        /// <summary>
        /// This overload was created so that lamda expressions wouldn't cause the debugger to break within the hidden methods
        /// Times the given action, and writes the message with the elapsed milliseconds to the trace listeners in
        /// the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="actionToTime"></param>
        /// <param name="entity">Entity to pass to actionToTime</param>
        /// <param name="actionDescriptionFormat"></param>
        /// <param name="values">Used for a String.Format.  The ElapsedMilliseconds will be added to the end of the array</param>
        [DebuggerHidden]
        public TResult Time<TInput, TResult>(Func<TInput, TResult> actionToTime, TInput entity, String actionDescriptionFormat, params object[] values)
        {
            Array.Resize(ref values, values.Length + 1);
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                return actionToTime(entity);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error attempting to " + actionDescriptionFormat + Environment.NewLine + Environment.NewLine + ex, values);
                throw;
            }
            finally
            {
                watch.Stop();
                values[values.Length - 1] = watch.ElapsedMilliseconds;
                Logger.WriteLine("Time to " + actionDescriptionFormat, values);
            }
        }
    }
}
