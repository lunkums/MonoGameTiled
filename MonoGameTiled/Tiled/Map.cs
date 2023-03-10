using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Tiled.Serialization;

namespace Tiled
{
    public class Map
    {
        public static string Directory { get; private set; }

        public TileLayer[] Layers;
        public Tileset[] Tilesets;
        public int TileHeight;
        public int TileWidth;
        public int Height;
        public int Width;

        private static readonly string TileLayerType = "tilelayer";
        private static readonly string ObjectLayerType = "objectgroup";

        private int[] gidToTilesetIndices;
        private HashSet<uint> renderLayerIds = new();

        public Map(string filePath)
        {
            Directory = Path.GetDirectoryName(filePath);
            Serializer.CopyFromFilePath(filePath, this);
        }

        // TODO: Override my value
        public float LayerDepth { get; set; } = 0.5f;

        public void Initialize(Action<TileObject> objectHandler)
        {
            InitializeLayers(objectHandler);
            InitializeTilesets();
        }

        public void Render(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new(0, 0, TileWidth, TileHeight);

            foreach (uint renderLayerId in renderLayerIds)
            {
                ref TileLayer layer = ref Layers[renderLayerId];
                ref uint[] data = ref layer.Data;

                for (int i = 0; i < data.Length; i++)
                {
                    uint gid = data[i];

                    if (gid == 0) continue;

                    ref Tileset tileset = ref Tilesets[gidToTilesetIndices[gid]];
                    ref Tile tile = ref tileset.Tiles[tileset.GidToId(gid)];

                    destinationRectangle.X = (i % layer.Width) * TileWidth;
                    destinationRectangle.Y = (i / layer.Width) * TileHeight;
                    
                    spriteBatch.Draw(
                        tileset.Image,
                        destinationRectangle,
                        tile.ImageRect,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        LayerDepth);
                }
            }
        }

        private void InitializeLayers(Action<TileObject> objectHandler)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                ref TileLayer layer = ref Layers[i];

                // Initialize the tile layer
                if (string.Equals(layer.Type, TileLayerType, StringComparison.OrdinalIgnoreCase))
                {
                    renderLayerIds.Add(layer.Id - 1);
                }
                // Initialize the object layer
                else if (string.Equals(layer.Type, ObjectLayerType, StringComparison.OrdinalIgnoreCase))
                {
                    ref TileObject[] objects = ref layer.Objects;

                    for (int j = 0; j < objects.Length; j++)
                    {
                        ref TileObject tileObject = ref objects[j];

                        objectHandler(tileObject);
                    }
                }
            }
        }

        private void InitializeTilesets()
        {
            uint maxGid = 0;

            // Copy tilesets using the source
            for (int i = 0; i < Tilesets.Length; i++)
            {
                ref Tileset tileset = ref Tilesets[i];
                uint firstGid = tileset.FirstGid;

                tileset = Serializer.DeserializeFromFilePath<Tileset>(Path.Combine(Directory, tileset.Source));
                tileset.FirstGid = firstGid;

                maxGid = (uint)Math.Max(maxGid, firstGid + tileset.TileCount + 1);
            }

            // Initialize the GID to tileset index map for fast tileset access
            gidToTilesetIndices = new int[maxGid];

            for (int i = 0; i < Tilesets.Length; i++)
            {
                ref Tileset tileset = ref Tilesets[i];
                uint firstGid = tileset.FirstGid;
                uint lastGid = (uint)(tileset.FirstGid + tileset.TileCount);

                for (uint j = firstGid; j <= lastGid; j++)
                {
                    gidToTilesetIndices[j] = i;
                }
            }

            // Assign the tilesets texture using the image path handler
            for (int i = 0; i < Tilesets.Length; i++)
            {
                ref Tileset tileset = ref Tilesets[i];

                // Initialize the tile array
                Tile[] newTiles = new Tile[tileset.TileCount];
                tileset.Tiles ??= Array.Empty<Tile>();
                ref Tile[] oldTiles = ref tileset.Tiles;

                // Copy the old tiles
                for (int j = 0; j < oldTiles.Length; j++)
                {
                    newTiles[oldTiles[j].Id] = oldTiles[j];
                }

                // Calculate the new data
                for (int j = 0; j < newTiles.Length; j++)
                {
                    newTiles[j].ImageRect = tileset.GetImageRectFromId(j);
                    newTiles[j].Id = (uint)j;
                }

                tileset.Tiles = newTiles;
            }
        }
    }
}
