namespace Core.Cameras
{
    using SharpDX;

    public class OrbitCamera : Base.Camera
    {
        public OrbitCamera()
        {
            this.Eye = new Vector3(0, 0, -1);
            this.Target = new Vector3(0, 0, 0);
            this.Up = new Vector3(0, 1, 0);

            this.SetView(Eye, Target, Up);
        }

        public void Zoom(int value)
        {
           
        }
    }
}