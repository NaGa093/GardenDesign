
namespace Core
{
    using System;

    using SharpDX.Direct3D12;

    using Device = SharpDX.Direct3D12.Device;

    public class CommandObjects : IDisposable
    {
        public CommandObjects(Device device)
        {
            var queueDesc = new CommandQueueDescription(CommandListType.Direct);

            this.GetCommandQueue = device.CreateCommandQueue(queueDesc);

            this.GetCommandAllocator = device.CreateCommandAllocator(CommandListType.Direct);

            this.GetGraphicsCommandList = device.CreateCommandList(
                0,
                CommandListType.Direct,
                 this.GetCommandAllocator,
                null);

            this.GetGraphicsCommandList.Close();
        }

        public CommandQueue GetCommandQueue
        {
            get;
        }

        public CommandAllocator GetCommandAllocator
        {
            get;
        }

        public GraphicsCommandList GetGraphicsCommandList
        {
            get;
        }

        public void Dispose()
        {
            this.GetCommandQueue.Dispose();
            this.GetCommandAllocator.Dispose();
            this.GetGraphicsCommandList.Dispose();
        }
    }
}
