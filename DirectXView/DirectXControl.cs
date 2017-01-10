namespace DirectXView
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class DirectXControl : UserControl
    {
        private readonly Core.Screen screen;
        private bool isDragging;
        private Point startPoint;
        private Point deltaPoint;

        public DirectXControl()
        {
            InitializeComponent();

            this.screen = new Core.Screen(Handle);

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
            this.isDragging = true;
            this.startPoint = new Point(e.X, e.Y);
        }

        private void DirectXControl_MouseUp(object sender, MouseEventArgs e)
        {
            this.isDragging = false;
        }

        private void DirectXControl_MouseWheel(object sender, MouseEventArgs e)
        {
            this.screen.CameraZoom(e.Delta);
        }

        private void DirectXControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.deltaPoint = new Point(startPoint.X - e.X, startPoint.Y - e.Y);
                this.startPoint = new Point(e.X, e.Y);

                if (e.Button == MouseButtons.Left)
                {
                    this.screen.CameraRotationY(-this.deltaPoint.X);
                    this.screen.CameraRotationOrtho(this.deltaPoint.Y);
                }
            }
        }

        private void DirectXControl_Disposed(object sender, System.EventArgs e)
        {
            this.screen.Dispose();
        }

        private void DirectXControl_Paint(object sender, PaintEventArgs e)
        {
            this.screen.Run();
        }

        private void DirectXControl_Resize(object sender, System.EventArgs e)
        {
            this.screen?.Resize(Width, Height);
        }
    }
}