namespace Core
{
    using System;

    using SharpDX;
    using SharpDX.DXGI;

    public class SwapChainObject
    {
        public static SwapChain3 New(IntPtr handleIntPtr, Factory factory, ComObject comObject, Format backBufferFormat,int swapChainBufferCount,int msaaCount, int msaaQuality)
        {
            var swapChainDescription = new SwapChainDescription
            {
                ModeDescription = new ModeDescription
                {
                    Format = backBufferFormat,
                    RefreshRate = new Rational(60, 1),
                    Scaling = DisplayModeScaling.Unspecified,
                    ScanlineOrdering = DisplayModeScanlineOrder.Unspecified
                },
                SampleDescription = new SampleDescription
                {
                    Count = msaaCount,
                    Quality = msaaQuality
                },
                Usage = Usage.RenderTargetOutput,
                BufferCount = swapChainBufferCount,
                SwapEffect = SwapEffect.FlipDiscard,
                Flags = SwapChainFlags.AllowModeSwitch,
                OutputHandle = handleIntPtr,
                IsWindowed = true
            };

            using (var tempSwapChain = new SwapChain(factory, comObject, swapChainDescription))
            {
                return tempSwapChain.QueryInterface<SwapChain3>();
            }
        }
    }
}
