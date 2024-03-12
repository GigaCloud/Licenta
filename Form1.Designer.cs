namespace Licenta
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            absoluteButton = new Button();
            touchpadButton = new Button();
            richTextData = new RichTextBox();
            SuspendLayout();
            // 
            // absoluteButton
            // 
            absoluteButton.Location = new Point(10, 12);
            absoluteButton.Name = "absoluteButton";
            absoluteButton.Size = new Size(75, 23);
            absoluteButton.TabIndex = 0;
            absoluteButton.Text = "Absolute";
            absoluteButton.UseVisualStyleBackColor = true;
            absoluteButton.Click += absoluteButton_Click;
            // 
            // touchpadButton
            // 
            touchpadButton.Location = new Point(91, 12);
            touchpadButton.Name = "touchpadButton";
            touchpadButton.Size = new Size(75, 23);
            touchpadButton.TabIndex = 1;
            touchpadButton.Text = "Touchpad";
            touchpadButton.UseVisualStyleBackColor = true;
            touchpadButton.Click += touchpadButton_Click;
            // 
            // richTextData
            // 
            richTextData.Location = new Point(10, 59);
            richTextData.Name = "richTextData";
            richTextData.Size = new Size(156, 294);
            richTextData.TabIndex = 2;
            richTextData.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(190, 409);
            Controls.Add(richTextData);
            Controls.Add(touchpadButton);
            Controls.Add(absoluteButton);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button absoluteButton;
        private Button touchpadButton;
        private RichTextBox richTextData;
    }
}