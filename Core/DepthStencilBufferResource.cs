namespace Core
{
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    using Resource = SharpDX.Direct3D12.Resource;
    using Device = SharpDX.Direct3D12.Device;

    public class DepthStencilBufferResource
    {
        public static Resource New(Device device, long clientWidth, int clientHeight, int msaaCount, int msaaQuality, Format depthStencilFormat)
        {
            var depthStencilDesc = BuildResourceDescription(clientWidth, clientHeight, msaaCount, msaaQuality);
            var optClear = BuildClearValue(depthStencilFormat);

            return device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                depthStencilDesc,
                ResourceStates.Common,
                optClear);
        }

        private static ResourceDescription BuildResourceDescription(long clientWidth, int clientHeight, int msaaCount, int msaaQuality)
        {
            return new ResourceDescription
            {
                Dimension = ResourceDimension.Texture2D,
                Alignment = 0,
                Width = clientWidth,
                Height = clientHeight,
                DepthOrArraySize = 1,
                MipLevels = 1,
                Format = Format.R24G8_Typeless,
                SampleDescription = new SampleDescription
                {
                    Count = msaaCount,
                    Quality = msaaQuality
                },
                Layout = TextureLayout.Unknown,
                Flags = ResourceFlags.AllowDepthStencil
            };
        }

        private static ClearValue BuildClearValue(Format depthStencilFormat)
        {
            return new ClearValue
            {
                Format = depthStencilFormat,
                DepthStencil = new DepthStencilValue
                {
                    Depth = 1.0f,
                    Stencil = 0
                }
            };
        }
    }
}
