namespace Core.Helpers
{
    using SharpDX;
    using SharpDX.Direct3D12;

    using System;

    public class BufferHelper
    {
        public static int CalcConstantBufferByteSize<T>() where T : struct => (System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)) + 255) & ~255;

        public static Resource CreateDefaultBuffer<T>(
            Device device,
            GraphicsCommandList cmdList,
            T[] initData,
            long byteSize,
            out Resource uploadBuffer) where T : struct
        {
            Resource defaultBuffer = device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                ResourceDescription.Buffer(byteSize),
                ResourceStates.Common);

            uploadBuffer = device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(byteSize),
                ResourceStates.GenericRead);

            IntPtr ptr = uploadBuffer.Map(0);
            Utilities.Write(ptr, initData, 0, initData.Length);
            uploadBuffer.Unmap(0);

            cmdList.ResourceBarrierTransition(defaultBuffer, ResourceStates.Common, ResourceStates.CopyDestination);
            cmdList.CopyResource(defaultBuffer, uploadBuffer);
            cmdList.ResourceBarrierTransition(defaultBuffer, ResourceStates.CopyDestination, ResourceStates.GenericRead);

            return defaultBuffer;
        }
    }
}