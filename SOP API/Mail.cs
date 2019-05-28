using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.IO;
using System.Reflection;
using SOPAPI.XML;

namespace SOPAPI {
    /// <summary>
    /// Static class with email sending methods. Use the methods of this class to send quick emails.
    /// </summary>
    public static class Mail {

        /*private static string[] Openers = {
            "Oh shoot",
            "Shucks",
            "Da'gum",
            "D'oh",
            "Aww, tartar sauce",
            "Ouch",
            "Well that's not right",
            "Ya dun goof'd",
            "It won't let me",
            "Houston, we have a problem",
            "A thing happened",
            "Oye, tenemos una problema",
            "Not again!",
            "You've gotta be kidding me",
            "What'd you do that for?",
            "I trusted you!",
            "Nope.avi",
            "Well, at least I tried",
            "Danger, Will Robinson",
            "I hate it when that happens",
            "OHHHHH NOOOOOOO!",
            "Where's the N-1?",
            "Hey, who turned out the lights?",
            "Help me, Obi-Wan Kenobi, you're my only hope",
            "Doctor, we're losing him!",
            "*Sigh* I'll go get the shovel",
            "Son, I am dissapoint",
            "Curse your sudden, but inevitable betrayal",
            "You found paradise in America. You had a good trade, you made a good living. The police protected you and there were courts of law. So you didn't need a friend"
                + "like me. Now you come and say \"Don Corleone, give me justice.\" But you don't ask with respect. You don't offer friendship. You don't even think to call me "
                + "\"Godfather.\" You come into my house on the day my daughter is to be married and you ask me to do murder - for money.",
            "Did you try restarting it?",
            "Don't try this at home",
            "I'm gonna need to see some ID",
            "It's not working and I'd like it to stop doing that",
            "They say it's the squeaky wheel that gets the grease. Well, squeak squeak.",
            "This is just between you and me. And everyone else who got this email.",
            "We need to go deeper.",
            "Did someone forget the Seven P's?",
            "Blame Canada!",
            "'Murica",
            "What kind of Mickey Mouse operation is this?",
            "Look Dave, I can see you're really upset about this. I honestly think you ought to sit down calmly, take a stress pill, and think things over.",
            "Where's my Megawatt?",
            "Don't blink. Blink, and you're dead.",
            "My leg!",
            "UNACCEPTABLE!!!!",
            "So it's treason, then.",
            "I tried, and therefore no one can judge me."
        };*/

        /// <summary>
        /// Sends an email to a single recipient.
        /// </summary>
        /// <returns>Returns void</returns>
        /// <param name="from">The email address from which to send the email</param>
        /// <param name="recipient">The email address to which to send the email</param>
        /// <param name="subject">The text to go in the subject line of the email</param>
        /// <param name="message">The text to go in the body of the email</param>
        /// <param name="isHtml">If true, the email will be sent as HTML, else plain text</param>
        [Obsolete("Use an overload instead")]
        public static void SendEmail(string from, string recipient, string subject, string message, bool isHtml = false) {
            SendEmail(from, new string[] { recipient }, subject, message, null, attachmentFilepaths: new string[] { }, isHtml: isHtml);
        }
        /// <summary>
        /// Send an email to a specified email group.
        /// </summary>
        /// <param name="from">The email address from which the email is sent.</param>
        /// <param name="groupName">The email group to which the email will be sent, as defined in your program's Recipients.xml file.</param>
        /// <param name="subject">The text to go in the subject line of the email.</param>
        /// <param name="message">The text to go in the body of the email.</param>
        /// <param name="log">The Log file to send.</param>
        /// <param name="useOpeners">If true, the email body will be preceded by comedic openers.</param>
        /// <param name="attachmentFilepaths">An array containing the filepaths of the attachments.</param>
        /// <param name="isHtml">If true, the email will be sent as HTML, else plain text.</param>
        public static void SendEmail(string from, string groupName, string subject, string message, Log log=null, bool useOpeners = false, IEnumerable<string> attachmentFilepaths = null, bool isHtml = false) {
            SendEmail(from, RecipientList.Get(groupName), subject, message, log, useOpeners, attachmentFilepaths, isHtml);
        }
        /// <summary>
        /// Sends an email to one or more recipients with a program's Log attached, as well as other attachments. Sends each email separately.
        /// </summary>
        /// <param name="from">The email address from which the email is sent.</param>
        /// <param name="recipients">A string array containing the email addresses to which to send the emails.</param>
        /// <param name="subject">The text to go in the subject line of the email.</param>
        /// <param name="message">The text to go in the body of the email.</param>
        /// <param name="log">The Log file to send.</param>
        /// <param name="useOpeners">If true, the email body will be preceded by comedic openers.</param>
        /// <param name="attachmentFilepaths">An array containing the filepaths of the attachments.</param>
        /// <param name="isHtml">If true, the email will be sent as HTML, else plain text.</param>
        /// <exception cref="ArgumentException">A passed recipient could not be converted into a valid email address.</exception>
        /// <exception cref="SmtpException">Could not send email.</exception>
        public static void SendEmail(string from, IEnumerable<string> recipients, string subject, string message, Log log = null, bool useOpeners = false, IEnumerable<string> attachmentFilepaths = null, bool isHtml = false) {
            if (useOpeners) {
                Random rand = new Random();
                message = GetOpenerMessage() + "\n----------------\n" + message;
            }
            if (log != null) {
                if (attachmentFilepaths == null) attachmentFilepaths = new string[] { };
                File.WriteAllLines(log.GetPath() + @"\ProgramLog.txt", log.GetLines());
                List<string> fps = attachmentFilepaths.ToList();
                fps.Insert(0, log.GetPath() + @"\ProgramLog.txt");
                attachmentFilepaths = fps.ToArray();
            }
            SmtpClient SMTP = new SmtpClient("smtp.santeecooper.com");
            recipients = Mail.GetEmailAddresses(recipients);
            foreach (string rec in recipients) {
                if (!IsValidAddress(rec)) throw new ArgumentException("Recipient \"" + rec + "\" is not a valid email address.");
                MailMessage email = new MailMessage(from, rec) {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = isHtml
                };
                if (attachmentFilepaths == null) attachmentFilepaths = new string[] { };
                foreach (string attachmentFilepath in attachmentFilepaths)
                    if (attachmentFilepath.Length > 0 && File.Exists(attachmentFilepath))
                        email.Attachments.Add(new Attachment(attachmentFilepath));

                DateTime startTime = DateTime.Now;
                while (true) {
                    try {
                        SMTP.Send(email);
                        break;
                    } catch (SmtpException e) {
                        if ((DateTime.Now - startTime).TotalMinutes > 10) throw e;
                    }
                }
                email.Dispose();
            }
            if (log != null && File.Exists(log.GetPath() + @"\ProgramLog.txt")) File.Delete(log.GetPath() + @"\ProgramLog.txt");
        }

        //======================================================================================================

        /// <summary>
        /// Sends an email to a single recipient. Includes comedic email opening lines.
        /// </summary>
        /// <param name="from">The email address from which to send the email</param>
        /// <param name="recipient">The email address to which to send the email</param>
        /// <param name="subject">The text to go in the subject line of the email</param>
        /// <param name="message">The text to go in the body of the email</param>
        [Obsolete("Functionality has been included in SendMessage().")]
        public static void SendEmailWithOpeners(string from, string recipient, string subject, string message) {
            SendEmailWithOpeners(from, new string[] { recipient }, subject, message, null, new string[] { }, false);
        }
        /// <summary>
        /// Sends an email to one or more recipients with a program's Log and specified files attached. Includes comedic email opening lines. Sends each email separately.
        /// </summary>
        /// <param name="from">The email address from which the email is sent</param>
        /// <param name="recipients">A string array containing the email addresses to which to send the emails</param>
        /// <param name="subject">The text to go in the subject line of the email</param>
        /// <param name="message">The text to go in the body of the email</param>
        /// <param name="log">The Log file to send</param>
        /// <param name="attachmentFilepaths">An array of file paths for attachments</param>
        /// <param name="isHtml">If true, the email will be sent as HTML, else plain text</param>
        [Obsolete("Functionality has been included in SendMessage().")]
        public static void SendEmailWithOpeners(string from, IEnumerable<string> recipients, string subject, string message, Log log = null, IEnumerable<string> attachmentFilepaths = null, bool isHtml = false) {
            SendEmail(from, recipients, subject, message, log, true, attachmentFilepaths, false);
        }

        //======================================================================================================

        /// <summary>
        /// Converts an array of names in the form "John Doe" into valid email addresses in the form "JOHN.DOE@santeecooper.com".
        /// Assumes that only the recipient's first and last names are used, and that no numbers are present in the address.
        /// </summary>
        /// <param name="names">An array of names in the form "John Doe"</param>
        /// <returns>An array of email addresses in the form "JOHN.DOE@santeecooper.com"</returns>
        public static IEnumerable<string> GetEmailAddresses(IEnumerable<string> names) {
            List<string> addresses = new List<string>();
            for (int i = 0; i < names.Count(); i++) {
                string name = names.ElementAt(i).Trim();
                if (Util.IsSignificant(name)) {
                    if (IsValidAddress(name)) addresses.Add(name);
                    else {
                        try {
                            addresses.Add(ConvertNameToAddress(name));
                        } catch(FormatException) { }
                    }
                }
            }
            return addresses;
        }

        /// <summary>
        /// Validates an email address. Checks to see if it contains an ampersand and that there's a period in the last 4 characters.
        /// This won't catch sneaky mistakes, but it will catch things that are blatantly wrong.
        /// </summary>
        /// <param name="address">The email address to validate</param>
        /// <returns>True if address contains @ and has a period in the last 4 characters</returns>
        public static Boolean IsValidAddress(string address) {
            return Util.CountOf(address, '@') == 1 && address.Split('@')[1].Contains('.');
        }

        /// <summary>
        /// Attempts to convert the given name into a Santee Cooper email address based on firstname.lastname@santeecooper.com pattern.
        /// </summary>
        /// <param name="name">The name of a recipient. Must be in "firstname lastname" format.</param>
        /// <returns>An email address in format firstname.lastname@santeecooper.com</returns>
        /// <exception cref="FormatException">Name must be in firstname lastname format.</exception>
        public static string ConvertNameToAddress(string name) {
            if (name.Length > 1 && name.Contains(' ')) {
                string address = String.Join(".", name.ToLower().Split(' ')) + "@santeecooper.com";
                if (IsValidAddress(address)) return address;
            } throw new FormatException("Name must be in firstname lastname format.");
        }

        private static string[] GetOpeners() {
            return Properties.Resources.Openers.Split('\n');
        }

        private static string GetOpenerMessage() {
            Random rand = new Random();
            string[] openers = GetOpeners();
            return openers[rand.Next(openers.GetLength(0))];
        }
    }

    /// <summary>
    /// Wrapper class for automating recipient groups.
    /// </summary>
    public class RecipientList : List<string> {
        /// <summary>
        /// The name of this group, used to differentiate it.
        /// </summary>
        public string Name;

        /// <summary>
        /// Create a new RecipientList
        /// </summary>
        /// <param name="name">The name of this list</param>
        public RecipientList(string name = "") {
            Name = name;
        }

        /// <summary>
        /// Get all groups as defined in a file.
        /// </summary>
        /// <param name="filepath">The path to the XML file containing the groups. Default: Recipients.xml in current folder.</param>
        /// <returns>A List of RecipientLists that represent all groups defined in the specified file.</returns>
        public static List<RecipientList> GetAllGroups(string filepath = null) {
            List<RecipientList> groups = new List<RecipientList>();
            XMLFile file = new XMLFile(filepath ?? Directory.GetCurrentDirectory() + "\\Recipients.xml");
            foreach (XMLSection section in file.FindSection("groups").GetSections()) {
                RecipientList recipients = new RecipientList(section.Get("name"));
                foreach (XMLNode node in section.FindSection("recipients").GetChildren()) recipients.Add(node.Get());
                groups.Add(recipients);
            }
            return groups;
        }

        /// <summary>
        /// Gets a RecipientList of a specified name.
        /// </summary>
        /// <param name="name">The name of the recipient list to get, as defined in the file.</param>
        /// <param name="filepath">The path to the XML file. Default: Recipients.xml in current directory.</param>
        /// <returns>A RecipientList that matches the specified name.</returns>
        /// <exception cref="ArgumentException">No group of the specified name could be found.</exception>
        public static RecipientList Get(string name, string filepath = null) {
            foreach (RecipientList l in GetAllGroups(filepath)) if (l.Name.ToUpper() == name.ToUpper()) return l;
            throw new ArgumentException("Could not find group named \"" + name + "\".");
        }
    }
}
