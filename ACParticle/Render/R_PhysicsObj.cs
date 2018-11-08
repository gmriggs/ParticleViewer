using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Server.Physics;

namespace ACParticle.Render
{
    public class R_PhysicsObj
    {
        public PhysicsObj PhysicsObj;
        public R_PartArray PartArray;

        public R_PhysicsObj(PhysicsObj obj)
        {
            PhysicsObj = obj;
            PartArray = new R_PartArray(obj.PartArray);
        }
    }
}
