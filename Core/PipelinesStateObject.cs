namespace Core
{
    using Helpers;

    using SharpDX.DXGI;
    using SharpDX.Direct3D12;

    using Device = SharpDX.Direct3D12.Device;

    public class PipelinesStateObject
    {
        public static PipelineState New(Device device, RootSignature rootSignature, int msaaCount, int msaaQuality, Format depthStencilFormat, Format backBufferFormat)
        {
            var graphicsPipelineStateDescription = BuildGraphicsPipelineStateDescription(rootSignature, msaaCount, msaaQuality, depthStencilFormat);
            graphicsPipelineStateDescription.RenderTargetFormats[0] = backBufferFormat;

            return device.CreateGraphicsPipelineState(graphicsPipelineStateDescription);
        }

        private static InputLayoutDescription BuildInputLayout()
        {
            return new InputLayoutDescription(new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
            });
        }

        private static GraphicsPipelineStateDescription BuildGraphicsPipelineStateDescription(RootSignature rootSignature, int msaaCount, int msaaQuality, Format depthStencilFormat)
        {
            var inputLayout = BuildInputLayout();

            return new GraphicsPipelineStateDescription
            {
                InputLayout = inputLayout,
                RootSignature = rootSignature,
                VertexShader = ShaderHelper.CompileShader("Shaders\\Color.hlsl", "VS", "vs_5_0"),
                PixelShader = ShaderHelper.CompileShader("Shaders\\Color.hlsl", "PS", "ps_5_0"),
                RasterizerState = RasterizerStateDescription.Default(),
                BlendState = BlendStateDescription.Default(),
                DepthStencilState = DepthStencilStateDescription.Default(),
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                SampleDescription = new SampleDescription(msaaCount, msaaQuality),
                DepthStencilFormat = depthStencilFormat
            };
        }
    }
}
