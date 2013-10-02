namespace DnDCS.Server
{
    partial class ColorOptionsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbxColor = new System.Windows.Forms.GroupBox();
            this.lblB = new System.Windows.Forms.Label();
            this.btnColor = new System.Windows.Forms.Button();
            this.lblG = new System.Windows.Forms.Label();
            this.lblA = new System.Windows.Forms.Label();
            this.lblR = new System.Windows.Forms.Label();
            this.tbAlpha = new System.Windows.Forms.TrackBar();
            this.gbxColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbAlpha)).BeginInit();
            this.SuspendLayout();
            // 
            // gbxColor
            // 
            this.gbxColor.Controls.Add(this.tbAlpha);
            this.gbxColor.Controls.Add(this.lblB);
            this.gbxColor.Controls.Add(this.btnColor);
            this.gbxColor.Controls.Add(this.lblG);
            this.gbxColor.Controls.Add(this.lblA);
            this.gbxColor.Controls.Add(this.lblR);
            this.gbxColor.Location = new System.Drawing.Point(3, 3);
            this.gbxColor.Name = "gbxColor";
            this.gbxColor.Size = new System.Drawing.Size(184, 81);
            this.gbxColor.TabIndex = 0;
            this.gbxColor.TabStop = false;
            this.gbxColor.Text = "Title";
            // 
            // lblB
            // 
            this.lblB.AutoSize = true;
            this.lblB.Location = new System.Drawing.Point(96, 16);
            this.lblB.Name = "lblB";
            this.lblB.Size = new System.Drawing.Size(38, 13);
            this.lblB.TabIndex = 0;
            this.lblB.Text = "B: 000";
            // 
            // btnColor
            // 
            this.btnColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.btnColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColor.Location = new System.Drawing.Point(6, 32);
            this.btnColor.Name = "btnColor";
            this.btnColor.Size = new System.Drawing.Size(70, 43);
            this.btnColor.TabIndex = 0;
            this.btnColor.UseVisualStyleBackColor = true;
            this.btnColor.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // lblG
            // 
            this.lblG.AutoSize = true;
            this.lblG.Location = new System.Drawing.Point(51, 16);
            this.lblG.Name = "lblG";
            this.lblG.Size = new System.Drawing.Size(39, 13);
            this.lblG.TabIndex = 0;
            this.lblG.Text = "G: 000";
            // 
            // lblA
            // 
            this.lblA.AutoSize = true;
            this.lblA.Location = new System.Drawing.Point(140, 16);
            this.lblA.Name = "lblA";
            this.lblA.Size = new System.Drawing.Size(38, 13);
            this.lblA.TabIndex = 0;
            this.lblA.Text = "A: 000";
            // 
            // lblR
            // 
            this.lblR.AutoSize = true;
            this.lblR.Location = new System.Drawing.Point(6, 16);
            this.lblR.Name = "lblR";
            this.lblR.Size = new System.Drawing.Size(39, 13);
            this.lblR.TabIndex = 0;
            this.lblR.Text = "R: 000";
            // 
            // tbAlpha
            // 
            this.tbAlpha.Location = new System.Drawing.Point(82, 32);
            this.tbAlpha.Maximum = 255;
            this.tbAlpha.Name = "tbAlpha";
            this.tbAlpha.Size = new System.Drawing.Size(95, 45);
            this.tbAlpha.TabIndex = 1;
            this.tbAlpha.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbAlpha.Scroll += new System.EventHandler(this.tbAlpha_Scroll);
            // 
            // ColorOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbxColor);
            this.Name = "ColorOptionsControl";
            this.Size = new System.Drawing.Size(191, 89);
            this.gbxColor.ResumeLayout(false);
            this.gbxColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbAlpha)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxColor;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Label lblB;
        private System.Windows.Forms.Label lblG;
        private System.Windows.Forms.Label lblA;
        private System.Windows.Forms.Label lblR;
        private System.Windows.Forms.TrackBar tbAlpha;
    }
}
