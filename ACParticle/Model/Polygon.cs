using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace ACParticle.Model
{
    public class Polygon
    {
        public GfxObj GfxObj;
        public ACE.DatLoader.Entity.Polygon _polygon;

        public Texture2D Texture;

        public List<ushort> IndexArray;
        public IndexBuffer IndexBuffer;

        public Polygon(GfxObj gfxObj, ACE.DatLoader.Entity.Polygon polygon)
        {
            GfxObj = gfxObj;
            _polygon = polygon;

            BuildIndices();

            Texture = gfxObj.Textures[polygon.PosSurface];
        }

        public void BuildIndices()
        {
            IndexArray = new List<ushort>();

            ushort firstIdx = 0;
            ushort lastIdx = 0;

            for (var i = 0; i < _polygon.VertexIds.Count; i++)
            {
                var vertID = _polygon.VertexIds[i];
                ushort uvIdx = 0;
                if (_polygon.PosUVIndices != null && i < _polygon.PosUVIndices.Count)
                {
                    uvIdx = _polygon.PosUVIndices[i];
                }
                var key = new Tuple<ushort, ushort>((ushort)vertID, uvIdx);
                if (!GfxObj.UVLookup.TryGetValue(key, out var idx))
                {
                    var debug = true;
                }
                if (i == 0)
                    firstIdx = idx;
                if (i > 2)
                {
                    // make triangle fan
                    IndexArray.Add(firstIdx);
                    IndexArray.Add(lastIdx);
                }
                lastIdx = idx;
                IndexArray.Add(idx);
            }

            //Console.WriteLine($"Poly verts: {_polygon.VertexIds.Count} ({IndexArray.Count})");

            IndexBuffer = new IndexBuffer(GfxObj.GraphicsDevice, typeof(short), IndexArray.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(IndexArray.ToArray());
        }

    }
}
