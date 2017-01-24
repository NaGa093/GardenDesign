using System;
using Core.Primitives;
using Core.Utilities;
using SharpDX.Direct3D12;

namespace Core
{
    internal class FrameResource : IDisposable
    {
        public FrameResource(Device device, int passCount, int objectCount)
        {
            CmdListAlloc = device.CreateCommandAllocator(CommandListType.Direct);

            PassCB = new UploadBuffer<PassConstants>(device, passCount, true);
            ObjectCB = new UploadBuffer<ObjectConstants>(device, objectCount, true);
        }

        public CommandAllocator CmdListAlloc { get; }
        public UploadBuffer<PassConstants> PassCB { get; }
        public UploadBuffer<ObjectConstants> ObjectCB { get; }
        public long Fence { get; set; }

        public void Dispose()
        {
            ObjectCB.Dispose();
            PassCB.Dispose();
            CmdListAlloc.Dispose();
        }
    }
}