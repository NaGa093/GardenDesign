namespace Core.Cameras
{
    using Core.Cameras.Base;

    using SharpDX;

    public class OrbitCamera : Camera
    {
        private const float _maxZoom = 3.0f;

        public OrbitCamera()
        {
            this.eye = new Vector3(4, 2, 0);
            this.target = new Vector3(0, 0, 0);
            this.up = new Vector3(0, 1, 0);

            this.SetView(eye, target, up);
        }

        public void RotateY(int value)
        {
            var rotY = (value / 100.0f);
            var rotMat = Matrix.RotationY(rotY);

            this.eye = Vector3.TransformCoordinate(eye, rotMat);
            this.SetView(eye, target, up);
        }

        public void RotateOrtho(int value)
        {
            var viewDir = target - eye;
            var orhto = Vector3.Cross(viewDir, up);

            var rotOrtho = (value / 100.0f);
            var rotOrthoMat = Matrix.RotationAxis(orhto, rotOrtho);

            var eyeLocal = eye - target;
            eyeLocal = Vector3.TransformCoordinate(eyeLocal, rotOrthoMat);

            var newEye = eyeLocal + target;
            var newViewDir = target - newEye;
            var cosAngle = Vector3.Dot(newViewDir, up) / (newViewDir.Length() * up.Length());

            if (cosAngle < 0.9f && cosAngle > -0.9f)
            {
                this.eye = eyeLocal + target;
                this.SetView(eye, target, up);
            }
        }

        public void Zoom(int value)
        {
            float scaleFactor = 1.0f;

            if (value > 0)
            {
                scaleFactor = 1.1f;
            }
            else
            {
                if ((eye - target).Length() > _maxZoom)
                    scaleFactor = 0.9f;
            }

            var scale = Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor);
            eye = Vector3.TransformCoordinate(eye, scale);

            this.SetView(eye, target, up);
        }
    }
}