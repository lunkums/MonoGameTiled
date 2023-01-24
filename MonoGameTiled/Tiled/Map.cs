using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tiled
{
    public class Map
    {
        public TileLayer[] Layers;
        public Tileset[] Tilesets;
        public int TileHeight;
        public int TileWidth;
        public int Height;
        public int Width;

        private static readonly string TileLayerType = "tilelayer";
        private static readonly string ObjectLayerType = "objectlayer";

        private HashSet<uint> renderLayerIds = new();
        private Tileset Empty;
        private string directory;

        public Map(string filePath)
        {
            Serializer.CopyFromFilePath(filePath, this);
            directory = Path.GetDirectoryName(filePath);
        }

        // TODO: Override me
        public float LayerDepth { get; set; } = 0.5f;

        public void Initialize(Action<TileObject> objectHandler, Func<string, Texture2D> imagePathHandler,
            GraphicsDevice graphicsDevice)
        {
            Texture2D blank = new(graphicsDevice, 1, 1);
            blank.SetData(new Color[] { new(0, 0, 0, 0) });
            Initialize(objectHandler, imagePathHandler, blank);
        }

        public void Initialize(Action<TileObject> objectHandler, Func<string, Texture2D> imagePathHandler,
            Texture2D blank)
        {
            Empty = new()
            {
                Columns = 1, // Prevent divide by 0
                Texture = blank, // Prevent NPE
                FirstGid = 0, // Prevent invalid array access
                Tiles = new Tile[] // Prevent invalid array access
                {
                    new()
                    {
                        ImageRect = new(0, 0, 0, 0) // Prevent texture from being visible
                    }
                }
            };
            InitializeLayers(objectHandler);
            InitializeTilesets(imagePathHandler);
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
                    ref Tileset tileset = ref GetTilesetFromGid(gid);
                    ref Tile tile = ref tileset.Tiles[tileset.GidToId(gid)];

                    destinationRectangle.X = (i % layer.Width) * TileWidth;
                    destinationRectangle.Y = (i / layer.Width) * TileHeight;
                    
                    spriteBatch.Draw(
                        tileset.Texture,
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
                        string template = tileObject.Template;
                        
                        if (!string.IsNullOrEmpty(template))
                        {
                            tileObject
                                = Serializer.DeserializeFromFilePath<TileObject>(Path.Combine(directory, template));
                        }

                        objectHandler(tileObject);
                    }
                }
            }
        }

        private void InitializeTilesets(Func<string, Texture2D> imagePathHandler)
        {
            // Copy tilesets using the source
            for (int i = 0; i < Tilesets.Length; i++)
            {
                ref Tileset tileset = ref Tilesets[i];
                uint firstGid = tileset.FirstGid;

                tileset = Serializer.DeserializeFromFilePath<Tileset>(Path.Combine(directory, tileset.Source));
                tileset.FirstGid = firstGid;
            }

            // Assign the tilesets texture using the image path handler
            for (int i = 0; i < Tilesets.Length; i++)
            {
                ref Tileset tileset = ref Tilesets[i];

                tileset.Texture = imagePathHandler(tileset.Image);

                // Initialize the tile array
                Tile[] newTiles = new Tile[tileset.TileCount];
                ref Tile[] oldTiles = ref tileset.Tiles;

                // Copy the old tiles
                for (int j = 0; j < oldTiles.Length; j++)
                {
                    newTiles[oldTiles[j].Id] = oldTiles[j];
                }

                // Calculate the new data
                for (int j = 0; j < newTiles.Length; j++)
                {
                    newTiles[j].ImageRect = tileset.GetSourceRectangleFromId(j);
                    newTiles[j].Id = (uint)j;
                }

                tileset.Tiles = newTiles;
            }
        }

        private ref Tileset GetTilesetFromGid(uint id)
        {
            for (int i = 0; i < Tilesets.Length; i++)
            {
                ref Tileset tileset = ref Tilesets[i];

                if (tileset.ContainsGid(id))
                {
                    return ref tileset;
                }
            }

            return ref Empty;
        }
    }
}
