namespace Core.Utilities
{
    using SharpDX.Direct3D12;
    using System;
    using System.Runtime.InteropServices;

    public class UploadBuffer<T> : IDisposable where T : struct
    {
        private readonly int _elementByteSize;
        private readonly IntPtr _resourcePointer;

        public UploadBuffer(Device device, int elementCount, bool isConstantBuffer)
        {
            _elementByteSize = isConstantBuffer
                ? Helpers.BufferHelper.CalcConstantBufferByteSize<T>()
                : Marshal.SizeOf(typeof(T));

            Resource = device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(_elementByteSize * elementCount),
                ResourceStates.GenericRead);

            _resourcePointer = Resource.Map(0);
        }

        public Resource Resource { get; }

        public void CopyData(int elementIndex, ref T data)
        {
            Marshal.StructureToPtr(data, _resourcePointer + elementIndex * _elementByteSize, true);
        }

        public void Dispose()
        {
            Resource.Unmap(0);
            Resource.Dispose();
        }
    }
}