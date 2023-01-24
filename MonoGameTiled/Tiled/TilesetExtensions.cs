using Microsoft.Xna.Framework;

namespace Tiled
{
    public static class TilesetExtensions
    {
        public static int GidToId(this ref Tileset tileset, uint gid)
        {
            return (int)(gid - tileset.FirstGid);
        }

        public static bool ContainsGid(this ref Tileset tileset, uint gid)
        {
            return gid >= tileset.FirstGid && gid <= tileset.FirstGid + tileset.TileCount;
        }

        public static Rectangle GetDestinationRectangleFromGid(this ref Tileset tileset, uint gid)
        {
            int id = tileset.GidToId(gid);
            return new((id % tileset.Columns) * tileset.TileWidth, (id / tileset.Columns) * tileset.TileHeight,
                tileset.TileWidth, tileset.TileHeight);
        }

        public static Rectangle GetSourceRectangleFromId(this ref Tileset tileset, int id)
        {
            return new((id % tileset.Columns) * tileset.TileWidth, (id / tileset.Columns) * tileset.TileHeight,
                tileset.TileWidth, tileset.TileHeight);
        }
    }
}
