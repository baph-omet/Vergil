using Vergil.Forms;
using Vergil.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Vergil.Timing {
    /// <summary>
    /// Framework for programs that run on a timer.
    /// </summary>
    public class TimingProgram : BasicProgram {

        /// <summary>
        /// The list of actions to perform. Must be set before running.
        /// </summary>
        protected static ActionRegistry Registry;

        /// <summary>
        /// Set up the fields for this program.
        /// </summary>
        public static new void Initialize() {
            Registry = new ActionRegistry();
            BasicProgram.Initialize();
        }

        /// <summary>
        /// Run this program.
        /// </summary>
        public static void Run() {
            if (!Initialized) Initialize();
            try {
                bool continueOnce = false;
                if (Debug) {
                    DebugModeDialog dmd = new DebugModeDialog();
                    dmd.ShowDialog();
                    switch (dmd.Result) {
                        case DebugModeDialogResult.Quit:
                            Log.Write("Program quit via Debug dialog.");
                            return;
                        case DebugModeDialogResult.ContinueProduction:
                            Log.Write("Disabling Debug mode.");
                            Config.Debug = false;
                            break;
                        case DebugModeDialogResult.ContinueDebugOnce:
                            Log.Write("Continuing in Debug mode once.");
                            continueOnce = true;
                            break;
                    }
                }

                Timeable timeable;
                if (Registry.Count == 0) {
                    MessageBox.Show("No actions found. " + Assembly.GetEntryAssembly().GetName().Name + " will now quit.");
                    return;
                }
                Type firstType = Registry.First().GetType();
                if (firstType == typeof(TimeLoopAction)) timeable = new HourlyTimer(Registry, Config.Debug);
                else if (firstType == typeof(TimerAction)) timeable = new DelayTimer(Registry, Config.Debug);
                else {
                    MessageBox.Show("Unknown action types found in action registry. " + Assembly.GetEntryAssembly().GetName().Name + " cannot continue and will now quit.");
                    return;
                }
                while (true) {
                    try {
                        timeable.Start();
                        if (continueOnce) {
                            continueOnce = false;
                            Config.Debug = false;
                            timeable.Debug = false;
                            continue;
                        }
                        break;
                    } catch (TimerException e) {
                        Problems.Add(Severity.WARNING, e.Message, e);
                        Problems.Send(GetRecipients());
                        Thread.Sleep(60000);
                    }
                }
            } catch (Exception e) {
                Problems.Add(Severity.SEVERE, Assembly.GetEntryAssembly().GetName().Name + " Crash", e);
                if (!Config.Debug) Problems.Send(GetRecipients());
            }
        }

        private static IEnumerable<string> GetRecipients() {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Recipients.xml")) return RecipientList.Get("errors");
            else return Config.Get("recipients").Split(',');
        }
    }
}
