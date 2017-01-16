namespace Core
{
    using System;

    using SharpDX.Direct3D12;

    using Device = SharpDX.Direct3D12.Device;

    public class DescriptorHeapObjects : IDisposable
    {
        public DescriptorHeapObjects(Device device, int renderTargetViewDescriptorCount, int depthStencilViewDescriptorCount)
        {
            var rtvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = renderTargetViewDescriptorCount,
                Type = DescriptorHeapType.RenderTargetView
            };

            this.RenderTargetViewDescriptorHeap = device.CreateDescriptorHeap(rtvHeapDesc);

            var dsvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = depthStencilViewDescriptorCount,
                Type = DescriptorHeapType.DepthStencilView
            };

            this.DepthStencilViewDescriptorHeap = device.CreateDescriptorHeap(dsvHeapDesc);

            var cbvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = 1,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                Flags = DescriptorHeapFlags.ShaderVisible,
                NodeMask = 0
            };

            this.ConstantBufferViewShaderResourceViewUnorderedAccessViewDescriptorHeap = device.CreateDescriptorHeap(cbvHeapDesc);

            this.DescriptorHeaps = new[] { ConstantBufferViewShaderResourceViewUnorderedAccessViewDescriptorHeap };
        }

        public DescriptorHeap RenderTargetViewDescriptorHeap
        {
            get;
        }

        public DescriptorHeap DepthStencilViewDescriptorHeap
        {
            get;
        }

        public DescriptorHeap ConstantBufferViewShaderResourceViewUnorderedAccessViewDescriptorHeap
        {
            get;
        }

        public DescriptorHeap[] DescriptorHeaps
        {
            get;
        }

        public void Dispose()
        {
            this.RenderTargetViewDescriptorHeap.Dispose();
            this.DepthStencilViewDescriptorHeap.Dispose();
        }
    }
}
