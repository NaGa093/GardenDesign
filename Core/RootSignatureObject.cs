namespace Core
{
    using SharpDX.Direct3D12;

    using Device = SharpDX.Direct3D12.Device;

    public class RootSignatureObject
    {
        public static RootSignature New(Device device)
        {
            var rootSigDesc = BuildRootSignatureDescription();

            return device.CreateRootSignature(rootSigDesc.Serialize());
        }

        private static RootDescriptor[] BuildRootDescriptors()
        {
            return new[]
            {
                new RootDescriptor(0, 0),
                new RootDescriptor(1, 0),
                new RootDescriptor(2, 0)
            };
        }

        private static RootParameter[] BuildRootParameters()
        {
            var rootDescriptors = BuildRootDescriptors();

            return new[]
            {
                new RootParameter(ShaderVisibility.Vertex, rootDescriptors[0], RootParameterType.ConstantBufferView),
                new RootParameter(ShaderVisibility.Pixel, rootDescriptors[1], RootParameterType.ConstantBufferView),
                new RootParameter(ShaderVisibility.All, rootDescriptors[2], RootParameterType.ConstantBufferView),
            };
        }

        private static RootSignatureDescription BuildRootSignatureDescription()
        {
            var rootParameters = BuildRootParameters();

            return new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                rootParameters);
        }
    }
}
