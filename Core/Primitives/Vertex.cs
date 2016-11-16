namespace Core.Primitives
{
    using SharpDX;

    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct Vertex
    {
        public Vector3 Pos;
        public Vector4 Color;
    }
}