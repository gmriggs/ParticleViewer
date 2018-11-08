using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACParticle.Model
{
    public class Setup
    {
        public SetupModel _setup;

        public List<GfxObj> Parts;

        public List<Matrix> PlacementFrames;

        public BoundingBox BoundingBox;

        public Setup(uint setupID)
        {
            _setup = DatManager.PortalDat.ReadFromDat<SetupModel>(setupID);

            Parts = new List<GfxObj>();

            foreach (var part in _setup.Parts)
                Parts.Add(GfxObjCache.Get(part));

            PlacementFrames = new List<Matrix>();

            foreach (var placementFrame in _setup.PlacementFrames[0].AnimFrame.Frames)
                PlacementFrames.Add(placementFrame.ToXna());

            var verts = GetVertices();
        }

        public List<Vector3> GetVertices()
        {
            var verts = new List<Vector3>();

            for (var i = 0; i < Parts.Count; i++)
            {
                if (_setup.Parts[i] == 0x010001EC)
                    continue;

                var part = Parts[i];
                var placementFrame = PlacementFrames[i];

                var partVerts = part.VertexArray.Select(v => v.Position).ToList();

                foreach (var partVert in partVerts)
                    verts.Add(Vector3.Transform(partVert, placementFrame));
            }
            return verts;
        }
    }
}
