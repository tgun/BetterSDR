namespace BetterSDR.Controls {
    partial class FrequencyEditDigit {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tickTimer
            // 
            this.tickTimer.Interval = 300;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // FrequencyEditDigit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.DoubleBuffered = true;
            this.Name = "FrequencyEditDigit";
            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.FrequencyEditDigit_Scroll);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FrequencyEditDigit_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FrequencyEditDigit_MouseDown);
            this.MouseEnter += new System.EventHandler(this.FrequencyEditDigit_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.FrequencyEditDigit_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FrequencyEditDigit_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FrequencyEditDigit_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tickTimer;
    }
}
