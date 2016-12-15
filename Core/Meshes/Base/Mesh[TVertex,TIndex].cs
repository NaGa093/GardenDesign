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
        private List<IDisposable> _toDispose;
        protected GraphicsCommandList _commandList;
        protected PrimitiveTopology _primitiveTopology;

        public Mesh()
        {
        }

        public Mesh(Device device,
           GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology,
           IEnumerable<TVertex> vertices = null,
           IEnumerable<TIndex> indices = null,
           string name = "Default")
        {
            _commandList = commandList;
            _primitiveTopology = primitiveTopology;

            this.Name = name;

            this.Initialize(device, vertices, indices);
        }

        public string Name
        {
            get; set;
        }

        public Resource VertexBufferGPU
        {
            get; set;
        }

        public Resource IndexBufferGPU
        {
            get; set;
        }

        public object VertexBufferCPU
        {
            get; set;
        }

        public object IndexBufferCPU
        {
            get; set;
        }

        public int VertexByteStride
        {
            get; set;
        }

        public int VertexBufferByteSize
        {
            get; set;
        }

        public Format IndexFormat
        {
            get; set;
        }

        public int IndexBufferByteSize
        {
            get; set;
        }

        public int IndexCount
        {
            get; set;
        }

        public VertexBufferView VertexBufferView => new VertexBufferView
        {
            BufferLocation = this.VertexBufferGPU.GPUVirtualAddress,
            StrideInBytes = this.VertexByteStride,
            SizeInBytes = this.VertexBufferByteSize
        };

        public IndexBufferView IndexBufferView => new IndexBufferView
        {
            BufferLocation = this.IndexBufferGPU.GPUVirtualAddress,
            Format = this.IndexFormat,
            SizeInBytes = this.IndexBufferByteSize
        };

        protected void Initialize(Device device, IEnumerable<TVertex> vertices = null, IEnumerable<TIndex> indices = null)
        {
            TVertex[] vertexArray = vertices.ToArray();
            TIndex[] indexArray = indices.ToArray();

            int vertexBufferByteSize = Utilities.SizeOf(vertexArray);
            Resource vertexBufferUploader;
            Resource vertexBuffer = BufferHelper.CreateDefaultBuffer(device, _commandList, vertexArray, vertexBufferByteSize, out vertexBufferUploader);

            int indexBufferByteSize = Utilities.SizeOf(indexArray);
            Resource indexBufferUploader;
            Resource indexBuffer = BufferHelper.CreateDefaultBuffer(device, _commandList, indexArray, indexBufferByteSize, out indexBufferUploader);

            VertexByteStride = Utilities.SizeOf<TVertex>();
            VertexBufferByteSize = vertexBufferByteSize;
            VertexBufferGPU = vertexBuffer;
            VertexBufferCPU = vertexArray;
            IndexCount = indexArray.Length;
            IndexFormat = GetIndexFormat<TIndex>();
            IndexBufferByteSize = indexBufferByteSize;
            IndexBufferGPU = indexBuffer;
            IndexBufferCPU = indexArray;

            this._toDispose = new List<IDisposable>()
            {
                vertexBuffer, vertexBufferUploader,
                indexBuffer, indexBufferUploader
            };
        }

        public void Draw()
        {
            _commandList.SetVertexBuffer(0, VertexBufferView);
            _commandList.SetIndexBuffer(IndexBufferView);
            _commandList.PrimitiveTopology = _primitiveTopology;
            _commandList.DrawIndexedInstanced(IndexCount, 1, 0, 0, 0);
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _toDispose)
                disposable.Dispose();
        }

        private static Format GetIndexFormat<TIndex>()
        {
            var format = Format.Unknown;

            if (typeof(TIndex) == typeof(int))
            {
                format = Format.R32_UInt;
            }
            else if (typeof(TIndex) == typeof(short))
            {
                format = Format.R16_UInt;
            }

            return format;
        }
    }
}