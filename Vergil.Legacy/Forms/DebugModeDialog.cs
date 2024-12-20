using System;
using System.Windows.Forms;
using System.Reflection;

namespace Vergil.Forms {
    /// <summary>
    /// Enum to encapsulate the result states of this dialog.
    /// </summary>
    public enum DebugModeDialogResult {
        /// <summary>
        /// Quit without continuing.
        /// </summary>
        Quit = 0,
        /// <summary>
        /// Continue in Debug mode.
        /// </summary>
        ContinueDebug = 1,
        /// <summary>
        /// Disable Debug mode, then continue.
        /// </summary>
        ContinueProduction = 2,
        /// <summary>
        /// Continue in Debug mode, then disable Debug mode and continue.
        /// </summary>
        ContinueDebugOnce = 3
    }

    public partial class DebugModeDialog : Form {
        private readonly string[] resultMapping = new[] {
            "Continue in Debug mode.",
            "Disable Debug mode, then continue.",
            "Continue in Debug mode, then disable Debug mode and continue."
        };

        /// <summary>
        /// The result state of this dialog.
        /// </summary>
        public DebugModeDialogResult Result { get; private set; } = DebugModeDialogResult.Quit;

        private bool ignoreClosing = false;

        /// <summary>
        /// Create a new dialog window.
        /// </summary>
        public DebugModeDialog() {
            InitializeComponent();
            BodyText.Text = "[Warning] This program is currently configured to run in Debug mode. This means all basic features of the program will run immediately, then the program will terminate. Please select an option to proceed.";
            ActionComboBox.Items.Clear();
            ActionComboBox.Items.AddRange(resultMapping);
            ActionComboBox.SelectedIndex = 0;
            BodyText.SelectionProtected = true;
        }

        private void DebugModeDialog_FormClosing(object sender, FormClosingEventArgs e) {
            if (ignoreClosing) return;
            if (MessageBox.Show(Assembly.GetEntryAssembly().GetName().Name + " will now quit.", "Warning", MessageBoxButtons.OKCancel) == DialogResult.Cancel) e.Cancel = true;
            Result = DebugModeDialogResult.Quit;
        }

        private void QuitButton_Click(object sender, EventArgs e) {
            Result = DebugModeDialogResult.Quit;
            ignoreClosing = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, EventArgs e) {
            Result = (DebugModeDialogResult)(ActionComboBox.SelectedIndex + 1);
            ignoreClosing = true;
            Close();
        }
    }
}
