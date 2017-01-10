namespace Core.Cameras
{
    using SharpDX;

    public class OrbitCamera : Base.Camera
    {
        private const float MaxZoom = 3.0f;

        public OrbitCamera()
        {
            this.Eye = new Vector3(4, 2, 0);
            this.Target = new Vector3(0, 0, 0);
            this.Up = new Vector3(0, 1, 0);

            this.SetView(Eye, Target, Up);
        }

        public void RotateY(int value)
        {
            var rotY = (value / 100.0f);
            var rotMat = Matrix.RotationY(rotY);

            this.Eye = Vector3.TransformCoordinate(Eye, rotMat);
            this.SetView(Eye, Target, Up);
        }

        public void RotateOrtho(int value)
        {
            var viewDir = Target - Eye;
            var orhto = Vector3.Cross(viewDir, Up);

            var rotOrtho = (value / 100.0f);
            var rotOrthoMat = Matrix.RotationAxis(orhto, rotOrtho);

            var eyeLocal = Eye - Target;
            eyeLocal = Vector3.TransformCoordinate(eyeLocal, rotOrthoMat);

            var newEye = eyeLocal + Target;
            var newViewDir = Target - newEye;
            var cosAngle = Vector3.Dot(newViewDir, Up) / (newViewDir.Length() * Up.Length());

            if (cosAngle < 0.9f && cosAngle > -0.9f)
            {
                this.Eye = eyeLocal + Target;
                this.SetView(Eye, Target, Up);
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
                if ((Eye - Target).Length() > MaxZoom)
                    scaleFactor = 0.9f;
            }

            var scale = Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor);
            Eye = Vector3.TransformCoordinate(Eye, scale);

            this.SetView(Eye, Target, Up);
        }
    }
}