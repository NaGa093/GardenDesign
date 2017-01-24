namespace Core
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class Screen : IDisposable
    {
        private int clientWidth;
        private int clientHeight;
        private float xPosition;
        private float yPosition;
        private bool paused;
        private bool running;
        private int frameCount;
        private readonly Stopwatch fpsTimer;

        private readonly D3DApp d3DApp;

        public Screen(IntPtr handleInstance)
        {
            this.d3DApp = new D3DApp(handleInstance);
            this.fpsTimer = new Stopwatch();
            this.running = true;
        }

        public void Resize(int width, int height)
        {
            this.paused = true;

            this.clientWidth = width;
            this.clientHeight = height;
            this.d3DApp.Resize(clientWidth, clientHeight);
            this.SetCameraPosition();

            this.paused = false;
        }

        public void CameraZoom(int zoomValue)
        {
            this.d3DApp.CameraZoom(zoomValue);
        }

        public void AddMesh(float startX, float startY, float stopX, float stopY)
        {
            this.d3DApp.AddMesh(startX,startY,stopX,stopY);
        }

        public void SetCameraPosition(float x, float y)
        {
            if (this.xPosition + x > 0)
            {
                this.xPosition += x;
            }
            else
            {
                this.xPosition = 0;
            }

            if (this.yPosition + y > 0)
            {
                this.yPosition += y;
            }
            else
            {
                this.yPosition = 0;
            }

            this.SetCameraPosition();
        }

        private void SetCameraPosition()
        {
            var x = - (this.clientWidth - 2) / 2 - xPosition;
            var y = (this.clientHeight) / 2 + yPosition;
            this.d3DApp.CameraPosition(x, y, 1.0f);
        }

        public void Run()
        {
            this.fpsTimer.Start();
            Task.Factory.StartNew(() =>
            {
                while (this.running)
                {
                    if (!this.paused)
                    {
                        this.CalculateFrameRateStats();
                        this.d3DApp.Update(this.clientWidth, this.clientHeight);
                        this.d3DApp.Draw(this.paused);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        public void Dispose()
        {
            this.running = false;
            this.paused = true;
            this.d3DApp.Dispose();
        }

        private void CalculateFrameRateStats()
        {
            this.frameCount++;
            if (this.fpsTimer.ElapsedMilliseconds > 1000L)
            {
                var fps = 1000.0 * this.frameCount / this.fpsTimer.ElapsedMilliseconds;
                var ms = (float)this.fpsTimer.ElapsedMilliseconds / this.frameCount;
                var text = $"FPS: {fps:F2} ({ms:F2}ms)";
                Console.WriteLine(text);

                this.fpsTimer.Reset();
                this.fpsTimer.Stop();
                this.fpsTimer.Start();
                this.frameCount = 0;
            }
        }
    }
}
