namespace Vergil.Forms {
    partial class EmailPrompt {
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
            this.TextBoxPrompt = new System.Windows.Forms.TextBox();
            this.RichTextBoxDescription = new System.Windows.Forms.RichTextBox();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextBoxPrompt
            // 
            this.TextBoxPrompt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBoxPrompt.Location = new System.Drawing.Point(12, 44);
            this.TextBoxPrompt.Name = "TextBoxPrompt";
            this.TextBoxPrompt.Size = new System.Drawing.Size(335, 20);
            this.TextBoxPrompt.TabIndex = 0;
            // 
            // RichTextBoxDescription
            // 
            this.RichTextBoxDescription.BackColor = System.Drawing.SystemColors.Control;
            this.RichTextBoxDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RichTextBoxDescription.Location = new System.Drawing.Point(12, 13);
            this.RichTextBoxDescription.Name = "RichTextBoxDescription";
            this.RichTextBoxDescription.ReadOnly = true;
            this.RichTextBoxDescription.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.RichTextBoxDescription.Size = new System.Drawing.Size(422, 35);
            this.RichTextBoxDescription.TabIndex = 1;
            this.RichTextBoxDescription.Text = "Type an email address or domain recipient name in the form \"firstname last" +
    "name\". To specify multiple recipients, separate each with a comma.";
            // 
            // ButtonOK
            // 
            this.ButtonOK.Location = new System.Drawing.Point(359, 45);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(75, 23);
            this.ButtonOK.TabIndex = 2;
            this.ButtonOK.Text = "Ok";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // EmailPrompt
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 76);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.RichTextBoxDescription);
            this.Controls.Add(this.TextBoxPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EmailPrompt";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select Email";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxPrompt;
        private System.Windows.Forms.RichTextBox RichTextBoxDescription;
        private System.Windows.Forms.Button ButtonOK;
    }
}