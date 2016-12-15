namespace Core.Meshes
{
    using Core.Meshes.Base;
    using Core.Primitives;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;
    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;

    public class Grid : Mesh
    {
        public Grid(
           Device device,
           GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology,
           int cellsPerSide,
           float cellSize,
           Color color,
           string name = "Default")
        {
            _commandList = commandList;
            _primitiveTopology = primitiveTopology;

            this.Name = name;

            var numLines = cellsPerSide + 1;
            var lineLength = cellsPerSide * cellSize;

            var xStart = -lineLength / 2.0f;
            var yStart = -lineLength / 2.0f;

            var xCurrent = xStart;
            var yCurrent = yStart;

            var vertices = new List<Vertex>();
            var indices = new List<short>();

            short index = 0;
            for (var y = 0; y < numLines; y++)
            {
                vertices.Add(new Vertex { Pos = new Vector3(xCurrent, yStart, 0), Color = color.ToVector4() });
                indices.Add(index++);
                vertices.Add(new Vertex { Pos = new Vector3(xCurrent, yStart + lineLength, 0), Color = color.ToVector4() });
                indices.Add(index++);
                xCurrent += cellSize;
            }

            for (var x = 0; x < numLines; x++)
            {
                vertices.Add(new Vertex { Pos = new Vector3(xStart, yCurrent, 0), Color = color.ToVector4() });
                indices.Add(index++);
                vertices.Add(new Vertex { Pos = new Vector3(xStart + lineLength, yCurrent, 0), Color = color.ToVector4() });
                indices.Add(index++);
                yCurrent += cellSize;
            }

            this.Initialize(device, vertices, indices);
        }

        public new void Draw()
        {
            _commandList.SetVertexBuffer(0, VertexBufferView);
            _commandList.SetIndexBuffer(IndexBufferView);
            _commandList.PrimitiveTopology = _primitiveTopology;
            _commandList.DrawIndexedInstanced(IndexCount, 1, 0, 0, 0);
        }
    }
}