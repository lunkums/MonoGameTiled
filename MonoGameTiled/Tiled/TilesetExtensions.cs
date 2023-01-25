using Microsoft.Xna.Framework;

namespace Tiled
{
    public static class TilesetExtensions
    {
        public static int GidToId(this ref Tileset tileset, uint gid)
        {
            return (int)(gid - tileset.FirstGid);
        }

        public static Rectangle GetImageRectFromId(this ref Tileset tileset, int id)
        {
            return new((id % tileset.Columns) * tileset.TileWidth, (id / tileset.Columns) * tileset.TileHeight,
                tileset.TileWidth, tileset.TileHeight);
        }
    }
}
