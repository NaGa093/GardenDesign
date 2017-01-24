namespace Core.Meshes.Base
{
    using Core.Helpers;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Device = SharpDX.Direct3D12.Device;
    using Resource = SharpDX.Direct3D12.Resource;

    public class Mesh<TVertex, TIndex> : IDisposable where TVertex : struct where TIndex : struct
    {
        private int meshIndex;
        private List<IDisposable> toDispose;

        protected GraphicsCommandList CommandList;

        public Mesh()
        {
            this.World = Matrix.Identity;
        }

        public Mesh(Device device, GraphicsCommandList commandList, PrimitiveTopology primitiveTopology, ref int index,
            IEnumerable<TVertex> vertices = null, IEnumerable<TIndex> indices = null, string name = "Default")
        {
            this.CommandList = commandList;
            this.PrimitiveTopology = primitiveTopology;

            this.Name = name;
            this.World = Matrix.Identity;
            this.Initialize(device, ref index, vertices, indices);
        }

        public string Name { get; set; }

        public Resource VertexBufferGpu { get; set; }

        public Resource IndexBufferGpu { get; set; }

        public int VertexByteStride { get; set; }

        public int VertexBufferByteSize { get; set; }

        public Format IndexFormat { get; set; }

        public int IndexBufferByteSize { get; set; }

        public int IndexCount { get; set; }

        public Matrix World { get; set; }

        public PrimitiveTopology PrimitiveTopology { get; protected set; }

        public VertexBufferView VertexBufferView => new VertexBufferView
        {
            BufferLocation = this.VertexBufferGpu.GPUVirtualAddress,
            StrideInBytes = this.VertexByteStride,
            SizeInBytes = this.VertexBufferByteSize
        };

        public IndexBufferView IndexBufferView => new IndexBufferView
        {
            BufferLocation = this.IndexBufferGpu.GPUVirtualAddress,
            Format = this.IndexFormat,
            SizeInBytes = this.IndexBufferByteSize
        };

        public void Draw(int objectConstantsSize, long gpuVirtualAddress)
        {
            this.CommandList.SetVertexBuffer(0, this.VertexBufferView);
            this.CommandList.SetIndexBuffer(this.IndexBufferView);
            this.CommandList.PrimitiveTopology = this.PrimitiveTopology;

            var objCbAddress = gpuVirtualAddress + this.meshIndex*objectConstantsSize;

            this.CommandList.SetGraphicsRootConstantBufferView(0, objCbAddress);

            this.CommandList.DrawIndexedInstanced(this.IndexCount, 1, 0, 0, 0);
        }

        public void Dispose()
        {
            foreach (var disposable in toDispose)
            {
                disposable.Dispose();
            }
        }

        protected void Initialize(Device device, ref int index, IEnumerable<TVertex> vertices = null,
            IEnumerable<TIndex> indices = null)
        {
            this.meshIndex = index;
            index++;

            var vertexArray = vertices?.ToArray();
            var indexArray = indices?.ToArray();

            var vertexBufferByteSize = Utilities.SizeOf(vertexArray);
            Resource vertexBufferUploader;
            var vertexBuffer = BufferHelper.CreateDefaultBuffer(device, CommandList, vertexArray, vertexBufferByteSize,
                out vertexBufferUploader);

            var indexBufferByteSize = Utilities.SizeOf(indexArray);
            Resource indexBufferUploader;
            var indexBuffer = BufferHelper.CreateDefaultBuffer(device, CommandList, indexArray, indexBufferByteSize,
                out indexBufferUploader);

            this.VertexByteStride = Utilities.SizeOf<TVertex>();
            this.VertexBufferByteSize = vertexBufferByteSize;
            this.VertexBufferGpu = vertexBuffer;

            if (indexArray != null)
            {
                this.IndexCount = indexArray.Length;
                this.IndexFormat = GetIndexFormat();
                this.IndexBufferByteSize = indexBufferByteSize;
                this.IndexBufferGpu = indexBuffer;
            }

            this.toDispose = new List<IDisposable>
            {
                vertexBuffer,
                vertexBufferUploader,
                indexBuffer,
                indexBufferUploader
            };
        }

        private Format GetIndexFormat()
        {
            var format = Format.Unknown;

            if (typeof (TIndex) == typeof (int))
            {
                format = Format.R32_UInt;
            }
            else if (typeof (TIndex) == typeof (short))
            {
                format = Format.R16_UInt;
            }

            return format;
        }
    }
}