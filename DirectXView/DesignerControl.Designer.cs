using System.Drawing;
using System.Windows.Forms;

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
            this.LeftRulerControl = new Assets.Controls.RulerControl();
            this.CenterPanel = new System.Windows.Forms.Panel();
            this.TopRulerControl = new Assets.Controls.RulerControl();
            this.directXControl1 = new DirectXView.DirectXControl();
            this.SuspendLayout();
            // 
            // LeftRulerControl
            // 
            this.LeftRulerControl.ActualSize = false;
            this.LeftRulerControl.DivisionMarkFactor = 5;
            this.LeftRulerControl.Divisions = 10;
            this.LeftRulerControl.ForeColor = System.Drawing.Color.Black;
            this.LeftRulerControl.Location = new System.Drawing.Point(0, 23);
            this.LeftRulerControl.MajorInterval = 100;
            this.LeftRulerControl.MiddleMarkFactor = 3;
            this.LeftRulerControl.MouseTrackingOn = false;
            this.LeftRulerControl.Name = "LeftRulerControl";
            this.LeftRulerControl.Orientation = Assets.Enums.Orientations.orVertical;
            this.LeftRulerControl.RulerAlignment = Assets.Enums.RulerAlignments.raBottomOrRight;
            this.LeftRulerControl.ScaleMode = Assets.Enums.ScaleModes.smPixels;
            this.LeftRulerControl.Size = new System.Drawing.Size(25, 150);
            this.LeftRulerControl.StartValue = 0D;
            this.LeftRulerControl.TabIndex = 1;
            this.LeftRulerControl.Text = "LeftRulerControl";
            this.LeftRulerControl.VerticalNumbers = true;
            this.LeftRulerControl.ZoomFactor = 1D;
            // 
            // CenterPanel
            // 
            this.CenterPanel.Location = new System.Drawing.Point(0, 0);
            this.CenterPanel.Name = "CenterPanel";
            this.CenterPanel.BackColor = Color.White;
            this.CenterPanel.BorderStyle = BorderStyle.FixedSingle;
            this.CenterPanel.Size = new System.Drawing.Size(23, 24);
            this.CenterPanel.TabIndex = 0;
            // 
            // HeaderRulerControl
            // 
            this.TopRulerControl.ActualSize = true;
            this.TopRulerControl.DivisionMarkFactor = 5;
            this.TopRulerControl.Divisions = 10;
            this.TopRulerControl.ForeColor = System.Drawing.Color.Black;
            this.TopRulerControl.Location = new System.Drawing.Point(23, 0);
            this.TopRulerControl.MajorInterval = 100;
            this.TopRulerControl.MiddleMarkFactor = 3;
            this.TopRulerControl.MouseTrackingOn = false;
            this.TopRulerControl.Name = "TopRulerControl";
            this.TopRulerControl.Orientation = Assets.Enums.Orientations.orHorizontal;
            this.TopRulerControl.RulerAlignment = Assets.Enums.RulerAlignments.raBottomOrRight;
            this.TopRulerControl.ScaleMode = Assets.Enums.ScaleModes.smPixels;
            this.TopRulerControl.Size = new System.Drawing.Size(150, 25);
            this.TopRulerControl.StartValue = 0D;
            this.TopRulerControl.TabIndex = 1;
            this.TopRulerControl.Text = "TopRulerControl";
            this.TopRulerControl.VerticalNumbers = true;
            this.TopRulerControl.ZoomFactor = 1D;
            // 
            // directXControl1
            // 
            this.directXControl1.Location = new System.Drawing.Point(this.CenterPanel.Size.Width, this.CenterPanel.Size.Height);
            this.directXControl1.Name = "directXControl1";
            this.directXControl1.BackColor = Color.Black;
            this.directXControl1.Size = new System.Drawing.Size(293, 233);
            this.directXControl1.TabIndex = 2;
            // 
            // DesignerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.directXControl1);
            this.Controls.Add(this.TopRulerControl);
            this.Controls.Add(this.CenterPanel);
            this.Controls.Add(this.LeftRulerControl);
            this.Name = "DesignerControl";
            this.Size = new System.Drawing.Size(500, 500);
            this.ResumeLayout(false);
        }

        #endregion

        private Assets.Controls.RulerControl TopRulerControl;
        private Assets.Controls.RulerControl LeftRulerControl;
        private System.Windows.Forms.Panel CenterPanel;
        private DirectXControl directXControl1;
    }
}
