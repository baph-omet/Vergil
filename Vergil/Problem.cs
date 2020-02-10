using System;
using System.Collections.Generic;
using System.Reflection;
using Vergil.Utilities;

namespace Vergil {
    /// <summary>
    /// Wrapper for List of type Problem.
    /// </summary>
    public class ProblemList : List<Problem> {
        private readonly Log Log;

        /// <summary>
        /// The email address from which problem emails are sent.
        /// </summary>
        public string EmailAddress;
        /// <summary>
        /// Initialize a new ProblemList
        /// </summary>
        /// <param name="log">The Log object that will be used for logging.</param>
        public ProblemList(Log log = null) {
            Log = log;
        }

        /// <summary>
        /// Add a problem to this list and write it to the Log.
        /// </summary>
        /// <param name="p">The Problem to add.</param>
        public new void Add(Problem p) {
            base.Add(p);
            if (Log != null) Log.Write(p);
        }
        /// <summary>
        /// Add a problem to this list and write it to the Log.
        /// </summary>
        /// <param name="severity">The Severity level of this Problem</param>
        /// <param name="message">The message that will be shown for this Problem</param>
        /// <param name="exception">An exception to attach to this Problem</param>
        public void Add(Severity severity, string message, Exception exception = null) {
            Add(new Problem(severity, message, exception));
        }

        /// <summary>
        /// Send an email to recipients containing details of the issues in this list.
        /// </summary>
        /// <param name="recipients">Group of recipient names and/or emails to receive this message.</param>
        /// <param name="log">Log file to attach with further details.</param>
        /// <param name="attachmentFilepaths">Any additional files to attach.</param>
        /// <param name="isHtml">If true, the body of the email will be formatted in HTML, plain text if false.</param>
        /// <param name="useOpeners">If true, the body of the email will be preceded by comedic openers.</param>
        public void Send(IEnumerable<string> recipients, Log log = null, bool useOpeners = true, IEnumerable<string> attachmentFilepaths = null, bool isHtml = false) {
            if (Count < 1) return;
            try {
                Mail.SendEmail(
                    EmailAddress,
                    recipients,
                    Assembly.GetEntryAssembly().GetName().Name + " Issue" + (Count > 1 ? "s" : ""), this.Join("\n"),
                    log ?? Log,
                    useOpeners,
                    attachmentFilepaths,
                    isHtml
                );
                Clear();
            } catch (Exception e) {
                Add(Severity.SEVERE, "Could not send problems.", e);
            }
        }
        /// <summary>
        /// Send an email to the specified recipient group containing details of the issues in this list.
        /// </summary>
        /// <param name="recipientGroup">The name of the recipient group as defined in your program's Recipients.xml file.</param>
        /// <param name="log">Log file to attach with further details.</param>
        /// <param name="attachmentFilepaths">Any additional files to attach.</param>
        /// <param name="useOpeners">If true, the body of the email will be preceded by comedic openers.</param>
        public void Send(string recipientGroup, bool useOpeners = true, Log log = null, IEnumerable<string> attachmentFilepaths = null) {
            RecipientList group = RecipientList.Get(recipientGroup);
            if (group == null) throw new ArgumentException("\"" + recipientGroup + "\" is not a valid recipient group.");
            Send(group, log ?? Log, useOpeners, attachmentFilepaths);
        }

        /// <summary>
        /// Convert this list to a string by using a newline as the delimiter.
        /// </summary>
        /// <returns>This list joined over a newline</returns>
        public override string ToString() {
            return this.Join("\n");
        }
    }

    /// <summary>
    /// A class for encapsulating details of runtime issues.
    /// </summary>
    public class Problem {
        /// <summary>
        /// The severity level of this problem.
        /// </summary>
        public Severity Severity;
        /// <summary>
        /// The description of this problem.
        /// </summary>
        public string Message;
        /// <summary>
        /// An Exception associated with this problem.
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// Initialize a new Problem object.
        /// </summary>
        /// <param name="severity">The severity level of this problem.</param>
        /// <param name="message">The description of this problem.</param>
        /// <param name="exception">An optional Exception to associate with this problem.</param>
        public Problem(Severity severity, string message, Exception exception = null) {
            Severity = severity;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Returns string representation of this Problem.
        /// </summary>
        /// <returns>String representation of this Problem.</returns>
        public override string ToString() {
            return "[" + Severity.ToString() + "] " + Message + (Exception != null ? " See log file for exception details." : "");
        }
    }
}
