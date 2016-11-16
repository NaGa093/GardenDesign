namespace Core.Geometries
{
    using SharpDX;

    public class SubMesh
    {
        public int IndexCount { get; set; }
        public int StartIndexLocation { get; set; }
        public int BaseVertexLocation { get; set; }

        public BoundingBox Bounds { get; set; }
    }
}