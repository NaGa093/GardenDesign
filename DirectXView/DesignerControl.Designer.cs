namespace DirectXView
{
    partial class DesignerControl
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
            this.rulerControl2 = new Assets.Controls.RulerControl();
            this.rulerControl1 = new Assets.Controls.RulerControl();
            this.directXControl1 = new DirectXView.DirectXControl();
            this.SuspendLayout();
            // 
            // rulerControl2
            // 
            this.rulerControl2.ActualSize = true;
            this.rulerControl2.DivisionMarkFactor = 5;
            this.rulerControl2.Divisions = 10;
            this.rulerControl2.Dock = System.Windows.Forms.DockStyle.Left;
            this.rulerControl2.ForeColor = System.Drawing.Color.Black;
            this.rulerControl2.Location = new System.Drawing.Point(0, 23);
            this.rulerControl2.MajorInterval = 100;
            this.rulerControl2.MiddleMarkFactor = 3;
            this.rulerControl2.MouseTrackingOn = false;
            this.rulerControl2.Name = "rulerControl2";
            this.rulerControl2.Orientation = Assets.Enums.Orientations.orVertical;
            this.rulerControl2.RulerAlignment = Assets.Enums.RulerAlignments.raBottomOrRight;
            this.rulerControl2.ScaleMode = Assets.Enums.ScaleModes.smPoints;
            this.rulerControl2.Size = new System.Drawing.Size(24, 477);
            this.rulerControl2.StartValue = 0D;
            this.rulerControl2.TabIndex = 1;
            this.rulerControl2.Text = "rulerControl2";
            this.rulerControl2.VerticalNumbers = true;
            this.rulerControl2.ZoomFactor = 1D;
            // 
            // rulerControl1
            // 
            this.rulerControl1.ActualSize = true;
            this.rulerControl1.DivisionMarkFactor = 5;
            this.rulerControl1.Divisions = 10;
            this.rulerControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rulerControl1.ForeColor = System.Drawing.Color.Black;
            this.rulerControl1.Location = new System.Drawing.Point(0, 0);
            this.rulerControl1.MajorInterval = 100;
            this.rulerControl1.MiddleMarkFactor = 3;
            this.rulerControl1.MouseTrackingOn = false;
            this.rulerControl1.Name = "rulerControl1";
            this.rulerControl1.Orientation = Assets.Enums.Orientations.orHorizontal;
            this.rulerControl1.RulerAlignment = Assets.Enums.RulerAlignments.raBottomOrRight;
            this.rulerControl1.ScaleMode = Assets.Enums.ScaleModes.smPoints;
            this.rulerControl1.Size = new System.Drawing.Size(500, 23);
            this.rulerControl1.StartValue = 0D;
            this.rulerControl1.TabIndex = 0;
            this.rulerControl1.Text = "rulerControl1";
            this.rulerControl1.VerticalNumbers = true;
            this.rulerControl1.ZoomFactor = 1D;
            // 
            // directXControl1
            // 
            this.directXControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.directXControl1.Location = new System.Drawing.Point(24, 23);
            this.directXControl1.Name = "directXControl1";
            this.directXControl1.Size = new System.Drawing.Size(476, 477);
            this.directXControl1.TabIndex = 2;
            // 
            // DesignerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.directXControl1);
            this.Controls.Add(this.rulerControl2);
            this.Controls.Add(this.rulerControl1);
            this.Name = "DesignerControl";
            this.Size = new System.Drawing.Size(500, 500);
            this.ResumeLayout(false);

        }

        #endregion

        private Assets.Controls.RulerControl rulerControl1;
        private Assets.Controls.RulerControl rulerControl2;
        private DirectXControl directXControl1;
    }
}
