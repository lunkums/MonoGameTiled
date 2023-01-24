using Microsoft.Xna.Framework.Graphics;

namespace Tiled
{
    public struct Tileset
    {
        public Tile[] Tiles;
        public Texture2D Texture;
        public string Image;
        public string Source;
        public uint FirstGid;
        public int Columns;
        public int ImageHeight;
        public int ImageWidth;
        public int TileCount;
        public int TileHeight;
        public int TileWidth;
    }
}