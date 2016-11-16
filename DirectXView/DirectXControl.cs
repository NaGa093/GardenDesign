namespace DirectXView
{
    using Core;

    using System.Windows.Forms;

    public partial class DirectXControl : UserControl
    {
        private D3DApp _d3DApp;

        public DirectXControl()
        {
            InitializeComponent();

            _d3DApp = new D3DApp(Handle, Width, Height);

            Resize += DirectXControl_Resize;
            Paint += DirectXControl_Paint;
            Disposed += DirectXControl_Disposed;
        }

        private void DirectXControl_Disposed(object sender, System.EventArgs e)
        {
            _d3DApp.Dispose();
        }

        private void DirectXControl_Paint(object sender, PaintEventArgs e)
        {
            _d3DApp.Run();
        }

        private void DirectXControl_Resize(object sender, System.EventArgs e)
        {
            if (_d3DApp != null)
            {
                _d3DApp.Resize(Width, Height);
            }
        }
    }
}