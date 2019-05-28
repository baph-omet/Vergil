using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace SOPAPI.Timing {
    /// <summary>
    /// Wrapper class for a List of TimingAction objects.
    /// </summary>
    public class ActionRegistry : List<TimingAction> { }
    
    /// <summary>
    /// Abstract class that represents any action that is executed on a time loop.
    /// </summary>
    public abstract class TimingAction {
        /// <summary>
        /// A void method that this action will run.
        /// </summary>
        public Action Action;

        /// <summary>
        /// Number of milliseconds before this action is considered to be unresponsive and will be terminated.
        /// </summary>
        public int Timeout;

        /// <summary>
        /// Starts running this action.
        /// </summary>
        public void Start() {
            Thread t = new Thread(new ThreadStart(Action));
            Thread monitor = null;
            if (Timeout > 0) {
                monitor = new Thread(new ThreadStart(delegate () {
                    Thread.Sleep(Timeout);
                    if (t.IsAlive) {
                        t.Abort();
                        t.Join();
                        throw new TimerException("Action " + Action.GetType().Name + " timed out.");
                    }
                }));
            } t.Start();
            if (monitor != null) monitor.Start();
        }
    }

    /// <summary>
    /// Action object for TimeLoop objects.
    /// </summary>
    public class TimeLoopAction : TimingAction {
        /// <summary>
        /// The minute of the hour in which to run.
        /// </summary>
        public int Time;

        /// <summary>
        /// Create a new TimeLoopAction object.
        /// </summary>
        /// <param name="action">A void method to run.</param>
        /// <param name="time">The minute of the hour in which to run the method.</param>
        /// <param name="timeout">The number of milliseconds before this action times out. Default: 3300000 (55 minutes)</param>
        public TimeLoopAction(Action action, int time, int timeout = 3300000) {
            Action = action;
            Time = time;
            Timeout = timeout;
        }
    }

    /// <summary>
    /// Specifies action registries for a SOPTimer.
    /// </summary>
    public class TimerAction : TimingAction {
        /// <summary>
        /// This action's timer. Will run independently of other actions.
        /// </summary>
        public Stopwatch Timer;

        /// <summary>
        /// The number of milliseconds to wait between running Method.
        /// </summary>
        public int Delay;

        /// <summary>
        /// Initilizes a new TimerAction with the specified method and delay (in milliseconds).
        /// </summary>
        /// <param name="action">A void method that this action will run.</param>
        /// <param name="delay">The number of milliseconds to wait between running Method.</param>
        /// <param name="timeout">The numbe rof milliseconds before this action times out. Default: Same as delay.</param>
        public TimerAction(Action action, int delay, int timeout = 0) {
            Action = action;
            if (delay < 0) throw new ArgumentException("Delay must be greater than or equal to zero.");
            Delay = delay;
            if (timeout > 0) Timeout = timeout;
            else Timeout = delay;
            Timer = new Stopwatch();
            Timer.Start();
        }

        /// <summary>
        /// Reset this action's timer and start it again.
        /// </summary>
        public void TimeStamp() {
            Timer.Reset();
            Timer.Start();
        }
    }
}
