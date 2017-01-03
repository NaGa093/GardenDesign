namespace Core.Cameras.Base
{
    using Enums;
    using SharpDX;

    public class Camera
    {
        protected Vector3 eye;
        protected Vector3 target;
        protected Vector3 up;

        private Matrix viewMatrix;
        private Matrix worldMatrix;
        private Matrix perspectiveMatrix;
        private Matrix orthographicMatrix;

        public bool dragging;
        public int startX;
        public int deltaX;

        public int startY;
        public int deltaY;

        public Camera()
        {
            this.viewMatrix = Matrix.Identity;
            this.worldMatrix = Matrix.Identity;
            this.perspectiveMatrix = Matrix.Identity;
            this.orthographicMatrix = Matrix.Identity;
        }

        public Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
        }

        public Matrix ViewMatrix
        {
            get
            {
                return viewMatrix;
            }
        }

        public Matrix PerspectiveMatrix
        {
            get
            {
                return perspectiveMatrix;
            }
        }

        public Matrix OrthographicMatrix
        {
            get
            {
                return orthographicMatrix;
            }
        }

        public Matrix ViewPerspectiveMatrix
        {
            get
            {
                return viewMatrix * perspectiveMatrix;
            }
        }

        public Matrix ViewOrthographicMatrix
        {
            get
            {
                return viewMatrix * orthographicMatrix;
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                return Projection == Projections.Orthographic ? OrthographicMatrix : PerspectiveMatrix;
            }
        }

        public Projections Projection
        {
            get;
            set;
        }

        public void SetPerspective(float fov, float aspect, float znear, float zfar)
        {
            perspectiveMatrix = Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
        }

        public void SetOrthographic(float width, float height, float znear, float zfar)
        {
            orthographicMatrix = Matrix.OrthoLH(width, height, znear, zfar);
        }

        public void SetView(Vector3 eye, Vector3 target, Vector3 up)
        {
            viewMatrix = Matrix.LookAtLH(eye, target, up);
        }
    }
}