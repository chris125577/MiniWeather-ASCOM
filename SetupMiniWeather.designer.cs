namespace ASCOM.MiniWeather
{
    partial class SetupMiniWeather
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.comboBoxComPort = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.inputTtrim = new System.Windows.Forms.NumericUpDown();
            this.inputHtrim = new System.Windows.Forms.NumericUpDown();
            this.inputPtrim = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputTtrim)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputHtrim)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputPtrim)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(410, 273);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(6);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(118, 46);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(562, 273);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(6);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(118, 48);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(350, 60);
            this.label1.TabIndex = 2;
            this.label1.Text = "Arduino Mini Pro / BME settings";
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.MiniWeather.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(584, 17);
            this.picASCOM.Margin = new System.Windows.Forms.Padding(6);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(412, 171);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 25);
            this.label2.TabIndex = 5;
            this.label2.Text = "Com Port";
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(418, 23);
            this.chkTrace.Margin = new System.Windows.Forms.Padding(6);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(123, 29);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Trace on";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // comboBoxComPort
            // 
            this.comboBoxComPort.FormattingEnabled = true;
            this.comboBoxComPort.Location = new System.Drawing.Point(562, 165);
            this.comboBoxComPort.Margin = new System.Windows.Forms.Padding(6);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(108, 33);
            this.comboBoxComPort.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 100);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "Temp adjust";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 215);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 25);
            this.label4.TabIndex = 9;
            this.label4.Text = "hPa adjust";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 156);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 25);
            this.label5.TabIndex = 10;
            this.label5.Text = "%RH adjust";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inputTtrim
            // 
            this.inputTtrim.DecimalPlaces = 1;
            this.inputTtrim.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.inputTtrim.Location = new System.Drawing.Point(214, 96);
            this.inputTtrim.Margin = new System.Windows.Forms.Padding(6);
            this.inputTtrim.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.inputTtrim.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            -2147483648});
            this.inputTtrim.Name = "inputTtrim";
            this.inputTtrim.Size = new System.Drawing.Size(94, 31);
            this.inputTtrim.TabIndex = 11;
            // 
            // inputHtrim
            // 
            this.inputHtrim.DecimalPlaces = 1;
            this.inputHtrim.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.inputHtrim.Location = new System.Drawing.Point(214, 152);
            this.inputHtrim.Margin = new System.Windows.Forms.Padding(6);
            this.inputHtrim.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.inputHtrim.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            -2147483648});
            this.inputHtrim.Name = "inputHtrim";
            this.inputHtrim.Size = new System.Drawing.Size(94, 31);
            this.inputHtrim.TabIndex = 12;
            // 
            // inputPtrim
            // 
            this.inputPtrim.DecimalPlaces = 1;
            this.inputPtrim.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.inputPtrim.Location = new System.Drawing.Point(214, 212);
            this.inputPtrim.Margin = new System.Windows.Forms.Padding(6);
            this.inputPtrim.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.inputPtrim.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            -2147483648});
            this.inputPtrim.Name = "inputPtrim";
            this.inputPtrim.Size = new System.Drawing.Size(94, 31);
            this.inputPtrim.TabIndex = 13;
            // 
            // SetupMiniWeather
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 337);
            this.Controls.Add(this.inputPtrim);
            this.Controls.Add(this.inputHtrim);
            this.Controls.Add(this.inputTtrim);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxComPort);
            this.Controls.Add(this.chkTrace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupMiniWeather";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MiniWeather Setup V.2.4";
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputTtrim)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputHtrim)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputPtrim)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.ComboBox comboBoxComPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown inputTtrim;
        private System.Windows.Forms.NumericUpDown inputHtrim;
        private System.Windows.Forms.NumericUpDown inputPtrim;
    }
}