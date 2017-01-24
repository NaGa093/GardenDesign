namespace Core.Meshes
{
    using Core.Meshes.Base;
    using Core.Primitives;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;

    using Device = SharpDX.Direct3D12.Device;

    public class Triangle : Mesh
    {
        public Triangle(Device device, GraphicsCommandList commandList, PrimitiveTopology primitiveTopology, Vector3 a,
            Vector3 b, Vector3 c, Color color, ref int index, string name = "Default")
        {
            base.CommandList = commandList;
            base.PrimitiveTopology = primitiveTopology;
            base.Name = name;

            Vertex[] vertices =
            {
                new Vertex {Position = a, Color = color.ToVector4()},
                new Vertex {Position = b, Color = color.ToVector4()},
                new Vertex {Position = c, Color = color.ToVector4()}
            };

            short[] indices =
            {
                0, 1, 2
            };

            this.Initialize(device, ref index, vertices, indices);
        }
    }
}