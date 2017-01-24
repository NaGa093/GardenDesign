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
        public Grid(Device device, GraphicsCommandList commandList, PrimitiveTopology primitiveTopology,
            int cellsPerSideH, int cellsPerSideV, float cellSize, Color color, ref int index, string name = "Default")
        {
            base.CommandList = commandList;
            base.PrimitiveTopology = primitiveTopology;
            base.Name = name;

            var lineLengthH = cellsPerSideH*cellSize;
            var lineLengthV = cellsPerSideV*cellSize;

            var xStart = 0.0f;
            var yStart = -lineLengthH;

            var xCurrent = xStart;
            var yCurrent = yStart;

            var vertices = new List<Vertex>();
            var indices = new List<short>();

            short gridIndex = 0;
            for (var y = 0; y <= cellsPerSideV; y++)
            {
                vertices.Add(new Vertex {Position = new Vector3(xCurrent, yStart, 0), Color = color.ToVector4()});
                indices.Add(gridIndex++);
                vertices.Add(new Vertex
                {
                    Position = new Vector3(xCurrent, yStart + lineLengthH, 0),
                    Color = color.ToVector4()
                });
                indices.Add(gridIndex++);
                xCurrent += cellSize;
            }

            for (var x = 0; x <= cellsPerSideH; x++)
            {
                vertices.Add(new Vertex {Position = new Vector3(xStart, yCurrent, 0), Color = color.ToVector4()});
                indices.Add(gridIndex++);
                vertices.Add(new Vertex
                {
                    Position = new Vector3(xStart + lineLengthV, yCurrent, 0),
                    Color = color.ToVector4()
                });
                indices.Add(gridIndex++);
                yCurrent += cellSize;
            }

            this.Initialize(device, ref index, vertices, indices);
        }
    }
}