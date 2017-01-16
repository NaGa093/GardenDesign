namespace Core.Meshes
{
    using Core.Meshes.Base;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;

    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;
    public class CoordinateAxis
    {
        public static List<Mesh> New(Device device, 
            GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology)
        {
            var meshes = new List<Mesh>();

            var x = new Cylinder(device, commandList, primitiveTopology, new Vector3(0, 0, 0), new Vector3(1, 0, 0), 0.1f, 0.1f, 10, 10, Color.Red);
            meshes.Add(x);
            var y = new Cylinder(device, commandList, primitiveTopology, new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0.1f, 0.1f, 10, 10, Color.Green);
            meshes.Add(y);
            var z = new Cylinder(device, commandList, primitiveTopology, new Vector3(0, 0, 0), new Vector3(0, 0, 1), 0.1f, 0.1f, 10, 10, Color.Blue);
            meshes.Add(z);

            return meshes;
        }
    }
}