using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {
    public class HitPoint {
        public Ray Ray { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Colour { get; set; }

        public HitPoint(Ray Ray, Vector3 Position, Vector3 Colour) {
            this.Ray = Ray;
            this.Position = Position;
            this.Colour = Colour;
        }
    }
}
