using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {
    public class Sphere {
        public Vector3 Centre { get => Centre; set => Centre = value; }
        public float Radius { get => Radius; set => Radius = value; }
        public Vector3 Colour { get => Colour; set => Colour = value; }

        public Sphere(Vector3 Center, float Radius, Vector3 Colour) {
            this.Centre = Center;
            this.Radius = Radius;
            this.Colour = Colour;
        }

        public float FindHitPoint(Sphere Sphere, Ray Ray) {
            var a = 1;
            var b = 2 * Vector3.Dot((Ray.Origin - Sphere.Centre), Ray.Direction);
            var c = (Ray.Origin - (Sphere.Centre)).Length() - Sphere.Radius;
            var Discriminant = b * b - 4 * a * c;
            if (Discriminant >= 0) { //at least one hit
                var Lambda1 = -b + Math.Sqrt(Discriminant) / (2 * a);
                var Lambda2 = -b - Math.Sqrt(Discriminant) / (2 * a);
                if (Lambda1 > 0 && Lambda2 > 0) {
                    return (float) Math.Min(Lambda1, Lambda2);
                    //H = Ray.Origin + (float)Math.Min(Lambda1, Lambda2) * Ray.Direction;
                } else if (Lambda1 > 0) {
                    return (float) Lambda1;
                    //H = Ray.Origin + (float)Lambda1 * Ray.Direction;
                } else if (Lambda2 > 0) {
                    return (float) Lambda2;
                    //H = Ray.Origin + (float)Lambda2 * Ray.Direction;
                }
                
            }
            return 0;
        }
    }

}
