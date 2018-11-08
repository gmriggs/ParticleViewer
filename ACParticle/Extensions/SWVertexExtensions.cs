using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader.Entity;

namespace ACParticle
{
    public static class SWVertexExtensions
    {
        public static List<VertexPositionNormalTexture> ToXna(this SWVertex swv)
        {
            var verts = new List<VertexPositionNormalTexture>();

            if (swv.UVs == null || swv.UVs.Count == 0)
            {
                var v = new VertexPositionNormalTexture();
                v.Position = swv.Origin.ToXna();
                v.TextureCoordinate = new Vector2(0, 0);
                v.Normal = swv.Normal.ToXna();
                //v.Color = Color.White;
                verts.Add(v);
                return verts;
            }

            for (var idx = 0; idx < swv.UVs.Count; idx++)
            {
                var v = new VertexPositionNormalTexture();
                v.Position = swv.Origin.ToXna();
                v.TextureCoordinate = new Vector2(swv.UVs[idx].U, swv.UVs[idx].V);
                v.Normal = swv.Normal.ToXna();
                //v.Color = Color.White;
                verts.Add(v);
            }
            return verts;
        }
    }
}
