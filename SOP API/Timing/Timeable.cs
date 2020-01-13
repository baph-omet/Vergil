namespace Vergil.Timing {
    /// <summary>
    /// Framework for classes that run actions on a timer.
    /// </summary>
    public abstract class Timeable {
        /// <summary>
        /// List of actions to take.
        /// </summary>
        protected ActionRegistry ActionRegistry;
        /// <summary>
        /// Whether or not to run in debug mode.
        /// </summary>
        public bool Debug;

        /// <summary>
        /// Create a new instace of this class.
        /// </summary>
        /// <param name="registry">The registry of actions this timer will perform.</param>
        /// <param name="debug">Whether or not to run in debug mode.</param>
        public Timeable(ActionRegistry registry, bool debug) {
            ActionRegistry = registry;
            Debug = debug;
        }

        /// <summary>
        /// Start running this timer.
        /// </summary>
        public abstract void Start();
    }
}
