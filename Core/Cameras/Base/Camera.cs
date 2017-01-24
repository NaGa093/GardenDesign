namespace Core.Cameras.Base
{
    using SharpDX;

    public class Camera
    {
        private Vector3 position;
        private Matrix viewMatrix;
        private Matrix perspectiveMatrix;
        private Matrix orthographicMatrix;

        protected Vector3 Target;
        protected Vector3 Up;

        public Camera()
        {
            this.viewMatrix = Matrix.Identity;
            this.perspectiveMatrix = Matrix.Identity;
            this.orthographicMatrix = Matrix.Identity;
            this.position = new Vector3(0,0,0);
        }

        public Vector3 Eye { get; protected set; }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        public Matrix PerspectiveMatrix => perspectiveMatrix;

        public Matrix OrthographicMatrix => orthographicMatrix;

        public Matrix ViewPerspectiveMatrix => viewMatrix * perspectiveMatrix;

        public Matrix ViewOrthographicMatrix => viewMatrix * orthographicMatrix;

        public Matrix ProjectionMatrix => Projection == Enums.Projections.Orthographic ? OrthographicMatrix : PerspectiveMatrix;

        public Enums.Projections Projection
        {
            get; set;
        }

        public void SetPerspective(float fov, float aspect, float znear, float zfar)
        {
            perspectiveMatrix = Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
        }

        public void SetOrthographic(float width, float height, float znear, float zfar)
        {
            orthographicMatrix = Matrix.OrthoLH(width, height, znear, zfar);
        }

        public void SetView(Vector3 eyeView, Vector3 targetView, Vector3 upView)
        {
            viewMatrix = Matrix.LookAtLH(eyeView, targetView, upView);
        }

        public void SetPosition(Vector3 cameraPosition)
        {
            this.position = cameraPosition;

            this.ViewMatrix = new Matrix(
               ViewMatrix.M11, ViewMatrix.M12, ViewMatrix.M13, ViewMatrix.M14,
               ViewMatrix.M21, ViewMatrix.M22, ViewMatrix.M23, ViewMatrix.M24,
               ViewMatrix.M31, ViewMatrix.M32, ViewMatrix.M33, ViewMatrix.M34,
               position.X, position.Y, position.Z, 1.0f);
        }
    }
}