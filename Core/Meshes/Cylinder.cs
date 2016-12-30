namespace Core.Meshes
{
    using Core.Meshes.Base;
    using Core.Primitives;
    using Core.Helpers;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;

    using System;
    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;

    public class Cylinder : Mesh
    {
        public Cylinder(Device device,
           GraphicsCommandList commandList,
           PrimitiveTopology primitiveTopology,
           Vector3 startPoint,
           Vector3 endPoint,
           float radiusBottom,
           float radiusTop,
           int slices,
           Color color,
           string name = "Default")
        {
            this._commandList = commandList;
            this._primitiveTopology = primitiveTopology;

            this.Name = name;

            var vertices = new List<Vertex>();
            var indices = new List<short>();

            var length = MathHelper.DistanceBetweenVector(startPoint, endPoint);

            int numVerticesPerRow = slices + 1;

            var theta = 0.0f;
            var horizontalAngularStride = ((float)Math.PI * 2) / slices;

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
                        z = length / 2 + 1;
                    }
                    else
                    {
                        // lower circle
                        x = radiusBottom * (float)Math.Cos(theta);
                        y = radiusBottom * (float)Math.Sin(theta);
                        z = -length / 2 + 1;
                    }

                    vertices.Add(new Vertex { Pos = new Vector3(x, z, y), Color = color.ToVector4() });
                }
            }

            vertices.Add(new Vertex { Pos = new Vector3(0, length / 2 + 1, 0), Color = color.ToVector4() });
            vertices.Add(new Vertex { Pos = new Vector3(0, -length / 2 + 1, 0), Color = color.ToVector4() });

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

            this.Initialize(device, vertices, indices);

            Vector3 vz = new Vector3(0, 1, 0);

            Vector3 p = startPoint - endPoint;
            p.Normalize();
            Vector3 t = Vector3.Cross(vz, p);

            double angle = 180 / Math.PI * Math.Acos((Vector3.Dot(vz, p) / p.Length()));
            Vector3 middle = MathHelper.Middle2Vector(startPoint, endPoint);
            Transform = Matrix.RotationAxis(new Vector3(t.X, t.Y, t.Z), MathHelper.DegreeToRadian((float)angle)) *
                Matrix.Translation(middle);
        }

        public new void Draw()
        {
            _commandList.SetVertexBuffer(0, VertexBufferView);
            _commandList.SetIndexBuffer(IndexBufferView);
            _commandList.PrimitiveTopology = _primitiveTopology;
            _commandList.SetGraphicsRootConstantBufferView(0, this.VertexBufferGPU.GPUVirtualAddress + ObjCBIndex * BufferHelper.CalcConstantBufferByteSize<ObjectConstants>());
            _commandList.DrawIndexedInstanced(IndexCount, 1, 0, 0, 0);
        }
    }
}