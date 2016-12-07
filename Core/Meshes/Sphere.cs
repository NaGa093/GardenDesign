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
        public static Mesh Create(
           Device device,
           GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology,
           float radius,
           int slices,
           int stacks,
           Color color,
           string name = "Default")
        {
            var vertices = new List<Vertex>();
            var indices = new List<int>();

            int numVerticesPerRow = slices + 1;
            int numVerticesPerColumn = stacks + 1;

            float theta = 0.0f;
            float phi = 0.0f;

            float verticalAngularStride = (float)Math.PI / stacks;
            float horizontalAngularStride = ((float)Math.PI * 2) / slices;

            for (int verticalIt = 0; verticalIt < numVerticesPerColumn; verticalIt++)
            {
                // beginning on top of the sphere:
                theta = ((float)Math.PI / 2.0f) - verticalAngularStride * verticalIt;

                for (int horizontalIt = 0; horizontalIt < numVerticesPerRow; horizontalIt++)
                {
                    phi = horizontalAngularStride * horizontalIt;

                    // position
                    float x = radius * (float)Math.Cos(theta) * (float)Math.Cos(phi);
                    float y = radius * (float)Math.Cos(theta) * (float)Math.Sin(phi);
                    float z = radius * (float)Math.Sin(theta);

                    vertices.Add(new Vertex { Pos = new Vector3(x, y, z), Color = color.ToVector4() });
                }
            }

            for (int verticalIt = 0; verticalIt < stacks; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    short lt = (short)(horizontalIt + verticalIt * (numVerticesPerRow));
                    short rt = (short)((horizontalIt + 1) + verticalIt * (numVerticesPerRow));

                    short lb = (short)(horizontalIt + (verticalIt + 1) * (numVerticesPerRow));
                    short rb = (short)((horizontalIt + 1) + (verticalIt + 1) * (numVerticesPerRow));

                    indices.Add(lt);
                    indices.Add(rt);
                    indices.Add(lb);

                    indices.Add(rt);
                    indices.Add(rb);
                    indices.Add(lb);
                }
            }

            return Create(device, commandList, primitiveTopology, vertices, indices);
        }
    }
}