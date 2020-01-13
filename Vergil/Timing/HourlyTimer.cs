using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Vergil.Timing {
    /// <summary>
    /// Class for creating an unthreaded loop that loops forever and executes an action at a specified minute of the hour.
    /// </summary>
    public class HourlyTimer : Timeable {
        /// <summary>
        /// Creates a new loop that will execute the specified action at the specified minute.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="checkMinute">The minute of the hour to perform the action.</param>
        /// <param name="debug">Whether or not to run in debug mode.</param>
        public HourlyTimer(Action action, int checkMinute, bool debug = false) : base(new ActionRegistry() { new TimeLoopAction(action, checkMinute) },debug) { }
        /// <summary>
        /// Creates a new loop that will run the specified actions.
        /// </summary>
        /// <param name="registry">The actions to perform.</param>
        /// <param name="debug">Whether or not to run in debug mode.</param>
        public HourlyTimer(ActionRegistry registry, bool debug = false) : base(registry, debug) { }


        /// <summary>
        /// Begin execution of the loop
        /// </summary>
        public override void Start() {
            if (this.ActionRegistry.Count == 0) return;
            if (Debug) DoThingDebug();
            else if (ActionRegistry.Count > 1) DoThingMultiThread();
            else DoThing();
        }

        private void DoThing() {
            while (true) {
                TimeLoopAction action = (TimeLoopAction)this.ActionRegistry[0];
                if (Debug || (Math.Abs(DateTime.Now.Minute - action.Time) < 1)) action.Action();
                if (Debug) break;
                else Thread.Sleep(60000);
            }
        }

        private void DoThingMultiThread() {
            while(true) {
                foreach (TimeLoopAction action in this.ActionRegistry) if (Debug || (Math.Abs(DateTime.Now.Minute - action.Time) < 1)) action.Start();
                if (Debug) break;
                else Thread.Sleep(60000);
            }
        }

        private void DoThingDebug() {
            foreach (TimeLoopAction action in this.ActionRegistry) action.Action();
        }
    }
}
