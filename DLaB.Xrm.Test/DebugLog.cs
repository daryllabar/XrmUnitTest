using System;
using System.Diagnostics;

namespace DLaB.Xrm.Test
{
    public class DebugLog
    {
        /// <summary>
        /// Times the given action, and writes the message with the elapsed milliseconds to the trace listeners in
        /// the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="actionToTime"></param>
        /// <param name="actionDescription"></param>
        [DebuggerHidden]
        public static void Time(Action actionToTime, String actionDescription)
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                actionToTime();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error attempting to " + actionDescription + Environment.NewLine + Environment.NewLine + ex);
                throw;
            }
            finally
            {
                watch.Stop();
                Debug.WriteLine("Time to " + actionDescription + watch.ElapsedMilliseconds);
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
        public static void Time(Action actionToTime, String actionDescriptionFormat, params object[] values)
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
                Debug.WriteLine("Error attempting to " + actionDescriptionFormat + Environment.NewLine + Environment.NewLine + ex, values);
                throw;
            }
            finally
            {
                watch.Stop();
                values[values.Length - 1] = watch.ElapsedMilliseconds;
                Debug.WriteLine("Time to " + actionDescriptionFormat, values);
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
        public static TResult Time<TInput, TResult>(Func<TInput, TResult> actionToTime, TInput entity, String actionDescriptionFormat, params object[] values)
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
                Debug.WriteLine("Error attempting to " + actionDescriptionFormat + Environment.NewLine + Environment.NewLine + ex, values);
                throw;
            }
            finally
            {
                watch.Stop();
                values[values.Length - 1] = watch.ElapsedMilliseconds;
                Debug.WriteLine("Time to " + actionDescriptionFormat, values);
            }
        }
    }
}
