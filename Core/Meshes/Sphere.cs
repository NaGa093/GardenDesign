namespace Core.Meshes
{
    using Core.Meshes.Base;
    using Core.Primitives;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;

    using System;
    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;

    public class Sphere : Mesh
    {
        public Sphere(Device device, GraphicsCommandList commandList, PrimitiveTopology primitiveTopology,
            Vector3 position, float radius, int slices, int stacks, Color color, ref int index, string name = "Default")
        {
            base.CommandList = commandList;
            base.PrimitiveTopology = primitiveTopology;
            base.Name = name;

            var vertices = new List<Vertex>();
            var indices = new List<short>();

            var numVerticesPerRow = slices + 1;
            var numVerticesPerColumn = stacks + 1;

            var verticalAngularStride = (float) Math.PI/stacks;
            var horizontalAngularStride = ((float) Math.PI*2)/slices;

            for (var verticalIt = 0; verticalIt < numVerticesPerColumn; verticalIt++)
            {
                // beginning on top of the sphere:
                var theta = ((float) Math.PI/2.0f) - verticalAngularStride*verticalIt;

                for (var horizontalIt = 0; horizontalIt < numVerticesPerRow; horizontalIt++)
                {
                    var phi = horizontalAngularStride*horizontalIt;

                    // position
                    var x = radius*(float) Math.Cos(theta)*(float) Math.Cos(phi);
                    var y = radius*(float) Math.Cos(theta)*(float) Math.Sin(phi);
                    var z = radius*(float) Math.Sin(theta);

                    vertices.Add(new Vertex
                    {
                        Position = new Vector3(position.X + x, position.Y + y, position.Z + z),
                        Color = color.ToVector4()
                    });
                }
            }

            for (var verticalIt = 0; verticalIt < stacks; verticalIt++)
            {
                for (var horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    var lt = (short) (horizontalIt + verticalIt*(numVerticesPerRow));
                    var rt = (short) ((horizontalIt + 1) + verticalIt*(numVerticesPerRow));

                    var lb = (short) (horizontalIt + (verticalIt + 1)*(numVerticesPerRow));
                    var rb = (short) ((horizontalIt + 1) + (verticalIt + 1)*(numVerticesPerRow));

                    indices.Add(lt);
                    indices.Add(rt);
                    indices.Add(lb);

                    indices.Add(rt);
                    indices.Add(rb);
                    indices.Add(lb);
                }
            }

            this.Initialize(device, ref index, vertices, indices);
        }
    }
}