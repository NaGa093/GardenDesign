namespace Core.Meshes
{
    using Core.Meshes.Base;
    using Core.Primitives;

    using SharpDX;
    using SharpDX.Direct3D12;
    using System;
    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;

    public class Cylinder : Mesh
    {
        public static Mesh Create(
           Device device,
           GraphicsCommandList commandList,
           float height, float radiusBottom,
           float radiusTop,
           int slices,
           Color color,
           string name = "Default")
        {
            var vertices = new List<Vertex>();
            var indices = new List<int>();

            int numVerticesPerRow = slices + 1;

            float theta = 0.0f;
            float horizontalAngularStride = ((float)Math.PI * 2) / slices;

            for (int verticalIt = 0; verticalIt < 2; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < numVerticesPerRow; horizontalIt++)
                {
                    float x;
                    float y;
                    float z;

                    theta = (horizontalAngularStride * horizontalIt);

                    if (verticalIt == 0)
                    {
                        // upper circle
                        x = radiusTop * (float)Math.Cos(theta);
                        y = radiusTop * (float)Math.Sin(theta);
                        z = height;
                    }
                    else
                    {
                        // lower circle
                        x = radiusBottom * (float)Math.Cos(theta);
                        y = radiusBottom * (float)Math.Sin(theta);
                        z = 0;
                    }

                    vertices.Add(new Vertex { Pos = new Vector3(x, z, y), Color = color.ToVector4() });
                }
            }

            vertices.Add(new Vertex { Pos = new Vector3(0, height, 0), Color = color.ToVector4() });
            vertices.Add(new Vertex { Pos = new Vector3(0, 0, 0), Color = color.ToVector4() });

            for (int verticalIt = 0; verticalIt < 1; verticalIt++)
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

            for (int verticalIt = 0; verticalIt < 1; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    short lt = (short)(horizontalIt + verticalIt * (numVerticesPerRow));
                    short rt = (short)((horizontalIt + 1) + verticalIt * (numVerticesPerRow));

                    short patchIndexTop = (short)(numVerticesPerRow * 2);

                    indices.Add(lt);
                    indices.Add(patchIndexTop);
                    indices.Add(rt);
                }
            }

            for (int verticalIt = 0; verticalIt < 1; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    short lb = (short)(horizontalIt + (verticalIt + 1) * (numVerticesPerRow));
                    short rb = (short)((horizontalIt + 1) + (verticalIt + 1) * (numVerticesPerRow));

                    short patchIndexBottom = (short)(numVerticesPerRow * 2 + 1);
                    indices.Add(lb);
                    indices.Add(rb);
                    indices.Add(patchIndexBottom);
                }
            }

            return Create(device, commandList, vertices, indices);
        }
    }
}