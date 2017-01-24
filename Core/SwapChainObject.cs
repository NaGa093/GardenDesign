namespace Core
{
    using System;

    using SharpDX;
    using SharpDX.DXGI;

    public class SwapChainObject
    {
        public static SwapChain3 New(IntPtr handleIntPtr, Factory factory, ComObject comObject, Format backBufferFormat,int swapChainBufferCount,int msaaCount, int msaaQuality)
        {
            var swapChainDescription = BuildSwapChainDescription(handleIntPtr, backBufferFormat, swapChainBufferCount,
                msaaCount, msaaQuality);

            using (var tempSwapChain = new SwapChain(factory, comObject, swapChainDescription))
            {
                return tempSwapChain.QueryInterface<SwapChain3>();
            }
        }

        private static SwapChainDescription BuildSwapChainDescription(IntPtr handleIntPtr, Format backBufferFormat, int swapChainBufferCount, int msaaCount, int msaaQuality)
        {
            return new SwapChainDescription
            {
                ModeDescription = BuildModeDescription(backBufferFormat),
                SampleDescription = BuildSampleDescription(msaaCount, msaaQuality),
                Usage = Usage.RenderTargetOutput,
                BufferCount = swapChainBufferCount,
                SwapEffect = SwapEffect.FlipDiscard,
                Flags = SwapChainFlags.AllowModeSwitch,
                OutputHandle = handleIntPtr,
                IsWindowed = true
            };
        }

        private static ModeDescription BuildModeDescription(Format backBufferFormat)
        {
            return new ModeDescription
            {
                Format = backBufferFormat,
                RefreshRate = new Rational(60, 1),
                Scaling = DisplayModeScaling.Unspecified,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified
            };
        }

        private static SampleDescription BuildSampleDescription(int msaaCount, int msaaQuality)
        {
            return new SampleDescription
            {
                Count = msaaCount,
                Quality = msaaQuality
            };
        }
    }
}
