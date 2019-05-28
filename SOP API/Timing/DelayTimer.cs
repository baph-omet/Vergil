using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SOPAPI.Timing {
    /// <summary>
    /// Class for repeating actions at fixed intervals. Actions are run on their own threads.
    /// </summary>
    [Obsolete("Use DelayTimer instead.")]
    public class DelayTimer : Timeable {
        private bool running;

        /// <summary>
        /// Creates a new timer with a single action. Allows specifying Debug mode, which will run all actions, then terminate the timer.
        /// </summary>
        /// <param name="method">Void method to run at fixed interval.</param>
        /// <param name="delay">The number of milliseconds to wait between runs.</param>
        /// <param name="debug">Whether or not to run in Debug mode.</param>
        public DelayTimer(Action method, int delay, bool debug = false) : base(new ActionRegistry() { new TimerAction(method,delay) }, debug) { }
        /// <summary>
        /// Create a new timer with a registry of actions.
        /// </summary>
        /// <param name="registry">A list of actions to perform at specific intervals.</param>
        /// <param name="debug">Whether or not to run in debug mode.</param>
        public DelayTimer(ActionRegistry registry, bool debug = false) : base(registry, debug) { }

        /// <summary>
        /// Begins running the timer.
        /// </summary>
        public override void Start() {
            running = true;
            if (Debug) foreach (TimerAction a in ActionRegistry) a.Action();
            else Timer();
        }

        /// <summary>
        /// Stops running the timer.
        /// </summary>
        public void Stop() {
            running = false;
        }

        private void Timer() {
            while (running) {
                foreach (TimerAction a in ActionRegistry) {
                    if (Debug || a.Timer.ElapsedMilliseconds >= a.Delay) {
                        a.TimeStamp();
                        a.Start();
                    }
                } if (Debug) Stop();
            }
        }
    }

    /// <summary>
    /// Class for repeating actions at fixed intervals. Actions are run on their own threads.
    /// </summary>
    public class DelayTimerNew : Timeable {
        private bool running = false;

        /// <summary>
        /// Creates a new timer with a single action. Allows specifying Debug mode, which will run all actions, then terminate the timer.
        /// </summary>
        /// <param name="method">Void method to run at fixed interval.</param>
        /// <param name="delay">The number of milliseconds to wait between runs.</param>
        /// <param name="debug">Whether or not to run in Debug mode.</param>
        public DelayTimerNew(Action method, int delay, bool debug = false) : base(new ActionRegistry() { new TimerAction(method, delay) }, debug) { }
        /// <summary>
        /// Create a new timer with a registry of actions.
        /// </summary>
        /// <param name="registry">A list of actions to perform at specific intervals.</param>
        /// <param name="debug">Whether or not to run in debug mode.</param>
        public DelayTimerNew(ActionRegistry registry, bool debug = false) : base(registry,debug) { }

        /// <summary>
        /// Begins running the timer.
        /// </summary>
        public override void Start() {
            running = true;
            foreach (TimerAction a in ActionRegistry) {

                if (Debug) {
                    a.Start();
                    continue;
                }
                Task task = new Task(a.Start);

                task.Wait();
                /*Thread t = new Thread(new ThreadStart(delegate () {
                    while(running) {
                        a.Start();
                        Thread.Sleep(...);
                    }
                }));
                t.Start();*/
            }
        }

        /// <summary>
        /// Stops the timer. 
        /// </summary>
        public void Stop() {
            running = false;
        }
    }
}
