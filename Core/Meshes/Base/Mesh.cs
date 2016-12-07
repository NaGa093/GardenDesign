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

    public class Mesh : IDisposable
    {
        private readonly List<IDisposable> _toDispose = new List<IDisposable>();
        private static GraphicsCommandList _commandList;
        private static PrimitiveTopology _primitiveTopology;

        public string Name { get; set; }

        public Resource VertexBufferGPU { get; set; }
        public Resource IndexBufferGPU { get; set; }

        public object VertexBufferCPU { get; set; }
        public object IndexBufferCPU { get; set; }

        public int VertexByteStride { get; set; }
        public int VertexBufferByteSize { get; set; }

        public Format IndexFormat { get; set; }

        public int IndexBufferByteSize { get; set; }
        public int IndexCount { get; set; }

        public Dictionary<string, SubMesh> DrawArgs { get; } = new Dictionary<string, SubMesh>();

        public VertexBufferView VertexBufferView => new VertexBufferView
        {
            BufferLocation = VertexBufferGPU.GPUVirtualAddress,
            StrideInBytes = VertexByteStride,
            SizeInBytes = VertexBufferByteSize
        };

        public IndexBufferView IndexBufferView => new IndexBufferView
        {
            BufferLocation = IndexBufferGPU.GPUVirtualAddress,
            Format = IndexFormat,
            SizeInBytes = IndexBufferByteSize
        };

        public static Mesh Create<TVertex, TIndex>(
           Device device,
           GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology,
           IEnumerable<TVertex> vertices,
           IEnumerable<TIndex> indices,
           string name = "Default")
           where TVertex : struct
           where TIndex : struct
        {
            _commandList = commandList;
            _primitiveTopology = primitiveTopology;

            TVertex[] vertexArray = vertices.ToArray();
            TIndex[] indexArray = indices.ToArray();

            int vertexBufferByteSize = Utilities.SizeOf(vertexArray);
            Resource vertexBufferUploader;
            Resource vertexBuffer = BufferHelper.CreateDefaultBuffer(device, commandList, vertexArray, vertexBufferByteSize, out vertexBufferUploader);

            int indexBufferByteSize = Utilities.SizeOf(indexArray);
            Resource indexBufferUploader;
            Resource indexBuffer = BufferHelper.CreateDefaultBuffer(device, commandList, indexArray, indexBufferByteSize, out indexBufferUploader);

            return new Mesh
            {
                Name = name,
                VertexByteStride = Utilities.SizeOf<TVertex>(),
                VertexBufferByteSize = vertexBufferByteSize,
                VertexBufferGPU = vertexBuffer,
                VertexBufferCPU = vertexArray,
                IndexCount = indexArray.Length,
                IndexFormat = GetIndexFormat<TIndex>(),
                IndexBufferByteSize = indexBufferByteSize,
                IndexBufferGPU = indexBuffer,
                IndexBufferCPU = indexArray,
                _toDispose =
                {
                    vertexBuffer, vertexBufferUploader,
                    indexBuffer, indexBufferUploader
                }
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