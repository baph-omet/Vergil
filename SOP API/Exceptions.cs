using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vergil {
    /// <summary>
    /// An exception for when unexpected data is found in a configuration file
    /// </summary>
    public class InvalidConfigException : Exception {
        /// <summary>
        /// Initialize a new instance of this exception.
        /// </summary>
        /// <param name="message">The message to attach to this exception</param>
        public InvalidConfigException(string message) : base(message) { }
        /// <summary>
        /// Initialize a new instance of this exception.
        /// </summary>
        /// <param name="message">The message to attach to this exception</param>
        /// <param name="innerException">The underlying exception</param>
        public InvalidConfigException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception type for unexpected conditions in timing threads.
    /// </summary>
    public class TimerException : Exception {
        /// <summary>
        /// Initialize a new instance of this exception.
        /// </summary>
        /// <param name="message">The message to attach to this exception</param>
        public TimerException(string message) : base(message) { }
        /// <summary>
        /// Initialize a new instance of this exception.
        /// </summary>
        /// <param name="message">The message to attach to this exception</param>
        /// <param name="innerException">The underlying exception</param>
        public TimerException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception for erroneous conditions from Energy Accounting
    /// </summary>
    public class EAException : Exception {
        /// <summary>
        /// Initialize a new instance of this exception.
        /// </summary>
        /// <param name="message">The message to attach to this exception</param>
        public EAException(string message) : base(message) { }
        /// <summary>
        /// Initialize a new instance of this exception.
        /// </summary>
        /// <param name="message">The message to attach to this exception</param>
        /// <param name="innerException">The underlying exception</param>
        public EAException(string message, Exception innerException) : base(message, innerException) { }
    }
}
