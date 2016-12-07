namespace DirectXView
{
    using Core;

    using System.Windows.Forms;

    public partial class DirectXControl : UserControl
    {
        private D3DApp _d3DApp;
        private bool _isDragging;

        public DirectXControl()
        {
            InitializeComponent();

            this._d3DApp = new D3DApp(Handle, Width, Height);

            this.Resize += DirectXControl_Resize;
            this.Paint += DirectXControl_Paint;
            this.Disposed += DirectXControl_Disposed;
            this.MouseMove += DirectXControl_MouseMove;
            this.MouseWheel += DirectXControl_MouseWheel;
            this.MouseUp += DirectXControl_MouseUp;
            this.MouseDown += DirectXControl_MouseDown;
        }

        private void DirectXControl_MouseDown(object sender, MouseEventArgs e)
        {
            this._isDragging = true;
            this._d3DApp.Camera.startX = e.X;
            this._d3DApp.Camera.startY = e.Y;
        }

        private void DirectXControl_MouseUp(object sender, MouseEventArgs e)
        {
            this._isDragging = false;
        }

        private void DirectXControl_MouseWheel(object sender, MouseEventArgs e)
        {
            this._d3DApp.Camera.Zoom(e.Delta);
        }

        private void DirectXControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                int currentX = e.X;
                this._d3DApp.Camera.deltaX = this._d3DApp.Camera.startX - currentX;
                this._d3DApp.Camera.startX = currentX;

                int currentY = e.Y;
                this._d3DApp.Camera.deltaY = this._d3DApp.Camera.startY - currentY;
                this._d3DApp.Camera.startY = currentY;

                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    this._d3DApp.Camera.RotateY(-this._d3DApp.Camera.deltaX);
                    this._d3DApp.Camera.RotateOrtho(this._d3DApp.Camera.deltaY);
                }
            }
        }

        private void DirectXControl_Disposed(object sender, System.EventArgs e)
        {
            this._d3DApp.Dispose();
        }

        private void DirectXControl_Paint(object sender, PaintEventArgs e)
        {
            this._d3DApp.Run();
        }

        private void DirectXControl_Resize(object sender, System.EventArgs e)
        {
            if (this._d3DApp != null)
            {
                this._d3DApp.Resize(Width, Height);
            }
        }
    }
}