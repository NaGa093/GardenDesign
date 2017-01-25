namespace Core.Cameras
{
    using SharpDX;

    public class OrbitCamera : Base.Camera
    {
        public OrbitCamera() : base()
        {
            this.Eye = new Vector3(0, 0, -1);
            this.Target = new Vector3(0, 0, 0);
            this.Up = new Vector3(0, 1, 0);

            this.SetView(Eye, Target, Up);
        }

        public void CameraZoom(int value)
        {
            if (value > 0)
            {
                base.Zoom += 0.1f;
            }
            else
            {
                base.Zoom -= 0.1f;
            }
        }
    }
}