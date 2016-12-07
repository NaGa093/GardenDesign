namespace Core.Cameras.Base
{
    using SharpDX;

    public class Camera
    {
        public Vector3 eye;
        public Vector3 target;
        public Vector3 up;

        public Matrix view = Matrix.Identity;
        public Matrix perspective = Matrix.Identity;
        public Matrix viewPerspective = Matrix.Identity;

        public Matrix View
        {
            get { return view; }
        }

        public void SetPerspective(float fov, float aspect, float znear, float zfar)
        {
            perspective = Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
        }

        public void SetView(Vector3 eye, Vector3 target, Vector3 up)
        {
            view = Matrix.LookAtLH(eye, target, up);
        }

        public Matrix Perspective
        {
            get { return perspective; }
        }

        public Matrix ViewPerspective
        {
            get { return view * perspective; }
        }

        public bool dragging = false;
        public int startX = 0;
        public int deltaX = 0;

        public int startY = 0;
        public int deltaY = 0;
    }
}