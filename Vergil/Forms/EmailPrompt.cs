using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Vergil.Forms {
    /// <summary>
    /// Form that prompts the user to enter one or more email addresses for use elsewhere.
    /// </summary>
    public partial class EmailPrompt : Form {
        /// <summary>
        /// The list of addresses entered into this form. If the form is cancelled or no valid email addresses are entered, this will be empty.
        /// </summary>
        public List<string> Addresses;
        /// <summary>
        /// Whether or not this form accepts multiple email addresses. If false, only the first email will be added to the Addresses list.
        /// </summary>
        public bool AcceptMultipleAddresses;

        /// <summary>
        /// Initialize a new instance of this form.
        /// </summary>
        /// <param name="multiple">Optional, default false. If true, this form will parse multiple email addresses.</param>
        public EmailPrompt(bool multiple = true) {
            AcceptMultipleAddresses = multiple;
            Addresses = new List<string>();
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, EventArgs e) {
            if (TextBoxPrompt.Text.Length == 0) {
                MessageBox.Show("Please specify at least one email address or recipient name.", "Warning");
                return;
            }
            List<string> invalid = new List<string>();
            foreach (string s in TextBoxPrompt.Text.Split(',')) {
                string address;
                try {
                    address = Mail.ConvertNameToAddress(s);
                } catch (FormatException) {
                    address = s;
                }
                if (!Mail.IsValidAddress(address)) invalid.Add(address);
                if (!AcceptMultipleAddresses) break;
            }

            if (invalid.Count > 0) {
                MessageBox.Show("Invalid address(es) found:\n" + String.Join("\n", invalid) + "\nPlease correct invalid address(es).");
                return;
            }
            if (!AcceptMultipleAddresses) Addresses.Add(Mail.GetEmailAddresses(new[] { TextBoxPrompt.Text.Split(',')[0] }).ElementAt(0));
            else Addresses.AddRange(Mail.GetEmailAddresses(TextBoxPrompt.Text.Split(',')));
            TextBoxPrompt.Clear();
            Close();
        }
    }
}
