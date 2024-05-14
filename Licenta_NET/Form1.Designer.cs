namespace Licenta {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            richTextData = new RichTextBox();
            connectDriver = new Button();
            buttonMCP = new Button();
            buttonPIC = new Button();
            checkBoxPressure = new CheckBox();
            SuspendLayout();
            // 
            // richTextData
            // 
            richTextData.Location = new Point(10, 59);
            richTextData.Name = "richTextData";
            richTextData.Size = new Size(266, 294);
            richTextData.TabIndex = 2;
            richTextData.Text = "";
            // 
            // connectDriver
            // 
            connectDriver.Location = new Point(10, 359);
            connectDriver.Name = "connectDriver";
            connectDriver.Size = new Size(128, 28);
            connectDriver.TabIndex = 3;
            connectDriver.Text = "Connect Driver";
            connectDriver.UseVisualStyleBackColor = true;
            connectDriver.Click += connectDriver_Click;
            // 
            // buttonMCP
            // 
            buttonMCP.Location = new Point(12, 28);
            buttonMCP.Name = "buttonMCP";
            buttonMCP.Size = new Size(129, 25);
            buttonMCP.TabIndex = 4;
            buttonMCP.Text = "MCP2221";
            buttonMCP.UseVisualStyleBackColor = true;
            buttonMCP.Click += buttonMCP_Click;
            // 
            // buttonPIC
            // 
            buttonPIC.Location = new Point(147, 28);
            buttonPIC.Name = "buttonPIC";
            buttonPIC.Size = new Size(129, 25);
            buttonPIC.TabIndex = 5;
            buttonPIC.Text = "PICkit Serial";
            buttonPIC.UseVisualStyleBackColor = true;
            buttonPIC.Click += buttonPIC_Click;
            // 
            // checkBoxPressure
            // 
            checkBoxPressure.AutoSize = true;
            checkBoxPressure.Checked = true;
            checkBoxPressure.CheckState = CheckState.Checked;
            checkBoxPressure.Location = new Point(147, 365);
            checkBoxPressure.Name = "checkBoxPressure";
            checkBoxPressure.Size = new Size(99, 19);
            checkBoxPressure.TabIndex = 6;
            checkBoxPressure.Text = "Read Pressure";
            checkBoxPressure.UseVisualStyleBackColor = true;
            checkBoxPressure.CheckedChanged += checkBoxPressure_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(299, 423);
            Controls.Add(checkBoxPressure);
            Controls.Add(buttonPIC);
            Controls.Add(buttonMCP);
            Controls.Add(connectDriver);
            Controls.Add(richTextData);
            Name = "Form1";
            Text = "Ecran Tactil - GUI";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private RichTextBox richTextData;
        private Button connectDriver;
        private Button buttonMCP;
        private Button buttonPIC;
        private CheckBox checkBoxPressure;
    }
}