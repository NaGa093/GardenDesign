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
           float bottomRadius,
           float topRadius,
           int sliceCount,
           int stackCount,
           Color color,
           string name = "Default")
        {
            this.CommandList = commandList;
            this.PrimitiveTopology = primitiveTopology;

            this.Name = name;

            var vertices = new List<Vertex>();
            var indices = new List<short>();

            var height = MathHelper.DistanceBetweenVector(startPoint, endPoint);
            float stackHeight = height / stackCount;
            float radiusStep = (topRadius - bottomRadius) / stackCount;
            int ringCount = stackCount + 1;

            for (int i = 0; i < ringCount; i++)
            {
                float y = -0.5f * height + i * stackHeight;
                float r = bottomRadius + i * radiusStep;

                float dTheta = 2.0f * MathUtil.Pi / sliceCount;
                for (int j = 0; j <= sliceCount; j++)
                {
                    float c = MathHelper.Cosf(j * dTheta);
                    float s = MathHelper.Sinf(j * dTheta);

                    var pos = new Vector3(r * c, y, r * s);
                    var uv = new Vector2((float)j / sliceCount, 1f - (float)i / stackCount);
                    var tangent = new Vector3(-s, 0.0f, c);

                    float dr = bottomRadius - topRadius;
                    var bitangent = new Vector3(dr * c, -height, dr * s);

                    var normal = Vector3.Cross(tangent, bitangent);
                    normal.Normalize();
                    vertices.Add(new Vertex { Pos = pos, Color = color.ToColor4() });
                }
            }

            int ringVertexCount = sliceCount + 1;

            for (int i = 0; i < stackCount; i++)
            {
                for (int j = 0; j < sliceCount; j++)
                {
                   indices.Add((short)(i * ringVertexCount + j));
                   indices.Add((short)((i + 1) * ringVertexCount + j));
                   indices.Add((short)((i + 1) * ringVertexCount + j + 1));

                   indices.Add((short)(i * ringVertexCount + j));
                   indices.Add((short)((i + 1) * ringVertexCount + j + 1));
                   indices.Add((short)(i * ringVertexCount + j + 1));
                }
            }
            
            //Top
            int baseIndex = vertices.Count;
            float dtheta = 2.0f * MathUtil.Pi / sliceCount;

            for (int i = 0; i <= sliceCount; i++)
            {
                float x = topRadius * MathHelper.Cosf(i * dtheta);
                float z = topRadius * MathHelper.Sinf(i * dtheta);
                float u = x / height + 0.5f;
                float v = z / height + 0.5f;

                vertices.Add(new Vertex { Pos = new Vector3(x, 0.5f * height, z),  Color = color.ToColor4() });
            }

            vertices.Add(new Vertex { Pos = new Vector3(0, 0.5f * height, 0),  Color = color.ToColor4() });

            int centerIndex = vertices.Count - 1;

            for (int i = 0; i < sliceCount; i++)
            {
                indices.Add((short)centerIndex);
                indices.Add((short)(baseIndex + i + 1));
                indices.Add((short)(baseIndex + i));
            }

            //Bottom
            baseIndex = vertices.Count;
            for (int i = 0; i <= sliceCount; i++)
            {
                float x = bottomRadius * MathHelper.Cosf(i * dtheta);
                float z = bottomRadius * MathHelper.Sinf(i * dtheta);

                // Scale down by the height to try and make top cap texture coord area
                // proportional to base.
                float u = x / height + 0.5f;
                float v = z / height + 0.5f;

                vertices.Add(new Vertex { Pos = new Vector3(x, -0.5f * height, z), Color = color.ToColor4() });
            }

            // Cap center vertex.
            vertices.Add(new Vertex { Pos = new Vector3(0, -0.5f * height, 0), Color = color.ToColor4() });

            // Cache the index of center vertex.
            centerIndex = vertices.Count - 1;

            for (int i = 0; i < sliceCount; i++)
            {
                indices.Add((short)centerIndex);
                indices.Add((short)(baseIndex + i + 1));
                indices.Add((short)(baseIndex + i));
            }

            this.Initialize(device, vertices, indices);

            var vz = new Vector3(0, 1, 0);

            var p = startPoint - endPoint;
            p.Normalize();
            var t = Vector3.Cross(vz, p);

            var angle = 180 / Math.PI * Math.Acos(Vector3.Dot(vz, p) / p.Length());
            var middle = MathHelper.Middle2Vector(startPoint, endPoint);
            World = Matrix.RotationAxis(new Vector3(t.X, t.Y, t.Z), (float)angle) *
                Matrix.Translation(middle);
        }

        public new void Draw()
        {
            CommandList.SetVertexBuffer(0, VertexBufferView);
            CommandList.SetIndexBuffer(IndexBufferView);
            CommandList.PrimitiveTopology = PrimitiveTopology;
           // _commandList.SetGraphicsRootConstantBufferView(0, this.VertexBufferGPU.GPUVirtualAddress + ObjCBIndex * BufferHelper.CalcConstantBufferByteSize<ObjectConstants>());
            CommandList.DrawIndexedInstanced(IndexCount, 1, 0, 0, 0);
        }
    }
}