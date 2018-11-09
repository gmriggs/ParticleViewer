using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;

namespace ACParticle
{
    public class Camera
    {
        public ParticleViewer Game;

        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;

        public Vector3 Position;
        public Vector3 Dir;
        public Vector3 Up;

        public WpfKeyboard KeyboardState;
        public WpfMouse MouseState;
        public MouseState PrevMouseState;
        public int PrevScrollWheelValue;

        public float Speed = 1.0f;
        public float SpeedMod = 1.5f;

        public int DrawDistance = 100000;

        public float FieldOfView = 90.0f;

        public Camera(ParticleViewer game)
        {
            Game = game;
            Init();
        }

        public void Init()
        {
            var dist = 10.0f;

            Position = new Vector3(dist, dist, 1);
            Dir = Vector3.Normalize(new Vector3(-dist, -dist, 0));

            Up = Vector3.UnitZ;

            CreateLookAt();
            CreateProjection();

            KeyboardState = new WpfKeyboard(Game);
            MouseState = new WpfMouse(Game);

            //SetMouse();
        }

        public void OnResize()
        {
            CreateProjection();
        }

        public Matrix CreateLookAt()
        {
            return ViewMatrix = Matrix.CreateLookAt(Position, Position + Dir, Up);
        }

        public Matrix CreateProjection()
        {
            return ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                 FieldOfView * 0.0174533f / 2,       // degrees to radians
                 (float)Game.GraphicsDevice.Viewport.Width /
                 Game.GraphicsDevice.Viewport.Height,
                 0.0001f,
                 DrawDistance);
        }

        public void Update(GameTime gameTime)
        {
            if (MouseState == null)
                return;

            var mouseState = MouseState.GetState();
            var keyboardState = KeyboardState.GetState();

            if (!Game.IsActive)
            {
                PrevMouseState = mouseState;
                return;
            }
            if (keyboardState.IsKeyDown(Keys.W))
                Position += Dir * Speed;
            if (keyboardState.IsKeyDown(Keys.S))
                Position -= Dir * Speed;
            if (keyboardState.IsKeyDown(Keys.A))
                Position += Vector3.Cross(Up, Dir) * Speed;
            if (keyboardState.IsKeyDown(Keys.D))
                Position -= Vector3.Cross(Up, Dir) * Speed;
            if (keyboardState.IsKeyDown(Keys.Space))
                Position += Up * Speed;

            // camera speed control
            /*if (mouseState.ScrollWheelValue != PrevScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevScrollWheelValue;
                if (diff >= 0)
                    Speed *= SpeedMod;
                else
                    Speed /= SpeedMod;

                PrevScrollWheelValue = mouseState.ScrollWheelValue;
            }

            // yaw / x-rotation
            Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Up,
                -MathHelper.PiOver4 / 160 * (mouseState.X - PrevMouseState.X)));

            // pitch / y-rotation
            Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Vector3.Cross(Up, Dir),
                MathHelper.PiOver4 / 160 * (mouseState.Y - PrevMouseState.Y)));

            Dir.Normalize();*/

            //SetMouse();

            CreateLookAt();

            //Console.WriteLine("Camera pos: " + ParticleViewer.Instance.Render.Camera.Position);
            //Console.WriteLine("Camera dir: " + ParticleViewer.Instance.Render.Camera.Dir);
        }

        public void SetMouse()
        {
            // set mouse position to center of window
            //Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            //PrevMouseState = Mouse.GetState();
        }
    }
}
