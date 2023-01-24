namespace Tiled
{
    public struct TileLayer
    {
        public TileObject[] Objects;
        public uint[] Data;
        public string Name;
        public string Type;
        public uint Id;
        public int Height;
        public int Width;
        public float Opacity;
    }
}