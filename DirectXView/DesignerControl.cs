namespace DirectXView
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class DesignerControl : UserControl
    {
        private bool isDragging;
        private Point startPoint;

        public DesignerControl()
        {
            InitializeComponent();

            this.SizeChanged += DesignerControl_SizeChanged;

            this.directXControl1.MouseMove += DirectXControl1_MouseMove;
            this.directXControl1.MouseUp += DirectXControl1_MouseUp;
            this.directXControl1.MouseDown += DirectXControl1_MouseDown;
        }

        private void DirectXControl1_MouseDown(object sender, MouseEventArgs e)
        {
            this.isDragging = true;
            this.startPoint = new Point(e.X, e.Y);
        }

        private void DirectXControl1_MouseUp(object sender, MouseEventArgs e)
        {
            this.isDragging = false;
        }

        private void DirectXControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isDragging)
            {
                if (e.Button == MouseButtons.Right)
                {
                    var newPoint = new Point(startPoint.X - e.X, startPoint.Y - e.Y);

                    if (this.TopRulerControl.StartValue + newPoint.X > 0)
                    {
                        this.TopRulerControl.StartValue += newPoint.X;
                    }
                    else
                    {
                        this.TopRulerControl.StartValue = 0;
                    }

                    if (this.LeftRulerControl.StartValue + newPoint.Y > 0)
                    {
                        this.LeftRulerControl.StartValue += newPoint.Y;
                    }
                    else
                    {
                        this.LeftRulerControl.StartValue = 0;
                    }

                    this.directXControl1.SetCameraPosition(newPoint);
                    this.startPoint = e.Location;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                this.directXControl1.AddMesh(startPoint.X, startPoint.Y, e.X, e.Y);
            }
        }

        private void DesignerControl_SizeChanged(object sender, EventArgs e)
        {
            this.TopRulerControl.Size = new Size(this.Size.Width,this.TopRulerControl.Size.Height);
            this.LeftRulerControl.Size = new Size(this.LeftRulerControl.Size.Width, this.Size.Height);
            this.directXControl1.Size = new Size(this.Size.Width, this.Size.Height);
        }
    }
}
