namespace Core.Cameras.Base
{
    using SharpDX;

    public class Camera
    {
        private Matrix viewMatrix;
        private Matrix perspectiveMatrix;
        private Matrix orthographicMatrix;

        public Vector3 Eye;
        protected Vector3 Target;
        protected Vector3 Up;

        public Camera()
        {
            this.viewMatrix = Matrix.Identity;
            this.perspectiveMatrix = Matrix.Identity;
            this.orthographicMatrix = Matrix.Identity;
        }

        public Matrix ViewMatrix => viewMatrix;

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
    }
}