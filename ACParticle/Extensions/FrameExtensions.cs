using Microsoft.Xna.Framework;
using ACE.DatLoader.Entity;

namespace ACParticle
{
    public static class FrameExtensions
    {
        public static Matrix ToXna(this Frame frame)
        {
            var translate = Matrix.CreateTranslation(frame.Origin.ToXna());
            var rotate = Matrix.CreateFromQuaternion(frame.Orientation.ToXna());

            return rotate * translate;
        }
    }
}
