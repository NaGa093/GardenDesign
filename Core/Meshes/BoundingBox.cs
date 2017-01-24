namespace Core.Meshes
{
    using Core.Meshes.Base;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;

    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;

    public class BoundingBox
    {
        public static List<Mesh> New(Device device,
            GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology,
           ref int index, Vector3 start, Vector3 stop)
        {
            var meshes = new List<Mesh>();
            var radius = 2;

            var x = new Cylinder(device, commandList, primitiveTopology, start, new Vector3(stop.X,start.Y,start.Z), radius, radius, 10, 10, Color.Red, ref index);
            meshes.Add(x);
            x = new Cylinder(device, commandList, primitiveTopology, new Vector3(stop.X, start.Y, start.Z), stop, radius, radius, 10, 10, Color.Red, ref index);
            meshes.Add(x);
            x = new Cylinder(device, commandList, primitiveTopology, stop, new Vector3(start.X, stop.Y, stop.Z), radius, radius, 10, 10, Color.Red, ref index);
            meshes.Add(x);
            x = new Cylinder(device, commandList, primitiveTopology, new Vector3(start.X, stop.Y, stop.Z), start, radius, radius, 10, 10, Color.Red, ref index);
            meshes.Add(x);

            return meshes;
        }
    }
}
