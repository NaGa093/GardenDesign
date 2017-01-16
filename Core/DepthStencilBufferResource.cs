using SharpDX.Direct3D12;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D12.Resource;

namespace Core
{
    using Device = SharpDX.Direct3D12.Device;
    public class DepthStencilBufferResource
    {
        public static Resource New(Device device, long clientWidth, int clientHeight, int msaaCount, int msaaQuality, Format depthStencilFormat)
        {
            var depthStencilDesc = new ResourceDescription
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

            var optClear = new ClearValue
            {
                Format = depthStencilFormat,
                DepthStencil = new DepthStencilValue
                {
                    Depth = 1.0f,
                    Stencil = 0
                }
            };

            return device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                depthStencilDesc,
                ResourceStates.Common,
                optClear);
        }
    }
}
