namespace DirectXView
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class DirectXControl : UserControl
    {
        private readonly Core.Screen screen;

        public DirectXControl()
        {
            InitializeComponent();

            this.screen = new Core.Screen(Handle);

            this.Resize += DirectXControl_Resize;
            this.Paint += DirectXControl_Paint;
            this.Disposed += DirectXControl_Disposed;
        }

        public void AddMesh(float startX, float startY, float stopX, float stopY)
        {
            this.screen?.AddMesh(startX, startY, stopX, stopY);
        }

        public void SetCameraPosition(Point point)
        {
            this.screen?.SetCameraPosition(point.X, point.Y);
        }

        public void CameraZoom(int delta)
        {
            this.screen?.CameraZoom(delta);
        }

        private void DirectXControl_Disposed(object sender, System.EventArgs e)
        {
            this.screen?.Dispose();
        }

        private void DirectXControl_Paint(object sender, PaintEventArgs e)
        {
            this.screen?.Run();
        }

        private void DirectXControl_Resize(object sender, System.EventArgs e)
        {
            this.screen?.Resize(Width, Height);
        }
    }
}