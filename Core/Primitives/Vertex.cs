namespace Core.Primitives
{
    using SharpDX;

    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector4 Color;
    }
}