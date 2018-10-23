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
        //public Vector3 Colour { get; set; }
        public Vector3 Normal { get; set; }
        public Sphere Sphere { get; set; }
        public float Lambda { get; set; }

        public HitPoint(Ray Ray, Vector3 Position, Vector3 Normal, Sphere Sphere, float Lambda) {

            this.Ray = Ray;
            this.Position = Position;
            //this.Colour = Colour;
            this.Normal = Normal;
            this.Sphere = Sphere;
            this.Lambda = Lambda;
        }
    }
}
