namespace Vergil.Forms {
    /// <summary>
    /// Dialog for determining what to do when a program is started in Debug mode.
    /// </summary>
    partial class DebugModeDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.BodyText = new System.Windows.Forms.RichTextBox();
            this.QuitButton = new System.Windows.Forms.Button();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.ActionComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // BodyText
            // 
            this.BodyText.BackColor = System.Drawing.SystemColors.Control;
            this.BodyText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BodyText.Location = new System.Drawing.Point(13, 13);
            this.BodyText.Name = "BodyText";
            this.BodyText.ReadOnly = true;
            this.BodyText.Size = new System.Drawing.Size(438, 51);
            this.BodyText.TabIndex = 0;
            this.BodyText.Text = "";
            // 
            // QuitButton
            // 
            this.QuitButton.Location = new System.Drawing.Point(12, 71);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.Size = new System.Drawing.Size(75, 23);
            this.QuitButton.TabIndex = 1;
            this.QuitButton.Text = "Quit";
            this.QuitButton.UseVisualStyleBackColor = true;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfirmButton.Location = new System.Drawing.Point(376, 71);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(75, 23);
            this.ConfirmButton.TabIndex = 4;
            this.ConfirmButton.Text = "Confirm";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // ActionComboBox
            // 
            this.ActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ActionComboBox.FormattingEnabled = true;
            this.ActionComboBox.Items.AddRange(new object[] {
            "Continue in Debug Mode",
            "Continue in Debug Mode, then disable Debug Mode",
            "Disable Debug Mode"});
            this.ActionComboBox.Location = new System.Drawing.Point(93, 71);
            this.ActionComboBox.Name = "ActionComboBox";
            this.ActionComboBox.Size = new System.Drawing.Size(277, 21);
            this.ActionComboBox.TabIndex = 5;
            this.ActionComboBox.Text = "Continue in Debug Mode";
            // 
            // DebugModeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 106);
            this.Controls.Add(this.ActionComboBox);
            this.Controls.Add(this.ConfirmButton);
            this.Controls.Add(this.QuitButton);
            this.Controls.Add(this.BodyText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DebugModeDialog";
            this.Text = "Debug Mode";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebugModeDialog_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox BodyText;
        private System.Windows.Forms.Button QuitButton;
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.ComboBox ActionComboBox;
    }
}