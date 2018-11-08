using Microsoft.Xna.Framework;

namespace ACParticle.Model
{
    public class SetupInstance
    {
        public Setup Setup;

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public SetupInstance(uint setupID)
        {
            Setup = SetupCache.Get(setupID);

            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }
}
