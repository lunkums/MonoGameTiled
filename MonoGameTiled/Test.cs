using Disarray.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using Tiled;
using Tiled.Serialization;

namespace MonoGameTiled
{
    public class Test : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Dictionary<string, Texture2D> textureMap = new();

        private Map tilemap;

        public Test()
        {
            graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Instantiate the tilemap by passing in the file path to a Tiled tilemap, saved in JSON format
            tilemap = new("path/to/tilemap/tilemap.json");

            // Initialize the tilemap by passing in your logic for initializing Tiled objects (objectHandler) and
            // your logic for loading textures from an image file path (imagePathHandler)
            tilemap.Initialize(HandleObject);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone);

            // Call tilemap.Render(...) to render the tilemap
            tilemap.Render(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // TODO: Implement your own object-handling logic to pass into the tilemap; I recommend an Entity Component
        // System or some type of GameObjectManager
        private void HandleObject(TileObject tileObject)
        {
            // Add the object to your ECS, instantiate a custom class, etc.
        }

        // TODO: Implement your own texture-loading logic to pass into the tilemap; I recommend a TextureCache
        // singleton or TextureFactory
        private Texture2D LoadTextureFromPath(string filePath)
        {
            if (!textureMap.TryGetValue(filePath, out Texture2D texture))
            {
                texture = Content.Load<Texture2D>(Path.GetFileNameWithoutExtension(filePath));
                textureMap[filePath] = texture;
            }
            return texture;
        }
    }
}