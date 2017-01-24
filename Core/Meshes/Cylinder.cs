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
           ref int index,
           string name = "Default")
        {
            base.CommandList = commandList;
            base.PrimitiveTopology = primitiveTopology;
            base.Name = name;

            var vertices = new List<Vertex>();
            var indices = new List<short>();

            var height = MathHelper.DistanceBetweenVector(startPoint, endPoint);
            var stackHeight = height / stackCount;
            var radiusStep = (topRadius - bottomRadius) / stackCount;
            var ringCount = stackCount + 1;

            for (int i = 0; i < ringCount; i++)
            {
                var y = -0.5f * height + i * stackHeight;
                var r = bottomRadius + i * radiusStep;
                var dTheta = 2.0f * MathUtil.Pi / sliceCount;

                for (var j = 0; j <= sliceCount; j++)
                {
                    var c = MathHelper.Cosf(j * dTheta);
                    var s = MathHelper.Sinf(j * dTheta);

                    var pos = new Vector3(r * c, y, r * s);
                    var uv = new Vector2((float)j / sliceCount, 1f - (float)i / stackCount);
                    var tangent = new Vector3(-s, 0.0f, c);

                    var dr = bottomRadius - topRadius;
                    var bitangent = new Vector3(dr * c, -height, dr * s);

                    var normal = Vector3.Cross(tangent, bitangent);
                    normal.Normalize();
                    vertices.Add(new Vertex { Position = pos, Color = color.ToColor4() });
                }
            }

            var ringVertexCount = sliceCount + 1;

            for (var i = 0; i < stackCount; i++)
            {
                for (var j = 0; j < sliceCount; j++)
                {
                   indices.Add((short)(i * ringVertexCount + j));
                   indices.Add((short)((i + 1) * ringVertexCount + j));
                   indices.Add((short)((i + 1) * ringVertexCount + j + 1));

                   indices.Add((short)(i * ringVertexCount + j));
                   indices.Add((short)((i + 1) * ringVertexCount + j + 1));
                   indices.Add((short)(i * ringVertexCount + j + 1));
                }
            }
            
            var baseIndex = vertices.Count;
            var dtheta = 2.0f * MathUtil.Pi / sliceCount;

            for (var i = 0; i <= sliceCount; i++)
            {
               var x = topRadius * MathHelper.Cosf(i * dtheta);
               var z = topRadius * MathHelper.Sinf(i * dtheta);
               var u = x / height + 0.5f;
               var v = z / height + 0.5f;

                vertices.Add(new Vertex { Position = new Vector3(x, 0.5f * height, z),  Color = color.ToColor4() });
            }

            vertices.Add(new Vertex { Position = new Vector3(0, 0.5f * height, 0),  Color = color.ToColor4() });

            var centerIndex = vertices.Count - 1;

            for (int i = 0; i < sliceCount; i++)
            {
                indices.Add((short)centerIndex);
                indices.Add((short)(baseIndex + i + 1));
                indices.Add((short)(baseIndex + i));
            }

            baseIndex = vertices.Count;
            for (var i = 0; i <= sliceCount; i++)
            {
                var x = bottomRadius * MathHelper.Cosf(i * dtheta);
                var z = bottomRadius * MathHelper.Sinf(i * dtheta);
                var u = x / height + 0.5f;
                var v = z / height + 0.5f;

                vertices.Add(new Vertex { Position = new Vector3(x, -0.5f * height, z), Color = color.ToColor4() });
            }


            vertices.Add(new Vertex { Position = new Vector3(0, -0.5f * height, 0), Color = color.ToColor4() });
            
            centerIndex = vertices.Count - 1;

            for (var i = 0; i < sliceCount; i++)
            {
                indices.Add((short)centerIndex);
                indices.Add((short)(baseIndex + i + 1));
                indices.Add((short)(baseIndex + i));
            }

            var vz = new Vector3(0, 1, 0);

            var p = startPoint - endPoint;
            p.Normalize();
            var t = Vector3.Cross(vz, p);

            var angle = 180 / Math.PI * Math.Acos(Vector3.Dot(vz, p) / p.Length());
            var middle = MathHelper.Middle2Vector(startPoint, endPoint);
            World = Matrix.RotationAxis(new Vector3(t.X, t.Y, t.Z), MathHelper.DegreeToRadian((float)angle)) *
                Matrix.Translation(middle);
               
            this.Initialize(device, ref index, vertices, indices);
        }
    }
}