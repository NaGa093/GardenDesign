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
        public Triangle(
         Device device,
         GraphicsCommandList commandList,
         PrimitiveTopology primitiveTopology,
         Vector3 vA,
         Vector3 vB,
         Vector3 vC,
         Color color,
         string name = "Default")
        {
            Vertex[] vertices =
            {
                new Vertex { Pos = vA, Color = color.ToVector4() },
                new Vertex { Pos = vB, Color = color.ToVector4() },
                new Vertex { Pos = vC, Color = color.ToVector4() }
            };

            short[] indices =
            {
                0, 1, 2
            };

            this.Initialize(device, vertices, indices);
        }
    }
}