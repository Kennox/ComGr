using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {
    public class Sphere {
        Vector3 centre;
        float radius;
        Vector3 colour;

        public Sphere(Vector3 center, float radius, Vector3 colour) {
            this.centre = center;
            this.radius = radius;
            this.colour = colour;
        }

        public void FindHitPoint(Sphere Sphere, Vector3 Ray) {
            var a = 1;
            var b = 2 * Vector3.Dot((Eye - Sphere.Centre), Ray);
            var c = (Eye - (Sphere.Centre)).Length() - Sphere.Radius;
            var Discriminant = b * b - 4 * a * c;
            if (Discriminant >= 0) { //at least one hit
                var Lambda1 = -b + Math.Sqrt(Discriminant) / (2 * a);
                var Lambda2 = -b - Math.Sqrt(Discriminant) / (2 * a);
                if (Lambda1 > 0 && Lambda2 > 0) {
                    H = Eye + (float)Math.Min(Lambda1, Lambda2) * Ray;
                } else if (Lambda1 > 0) {
                    H = Eye + (float)Lambda1 * Ray;
                } else if (Lambda2 > 0) {
                    H = Eye + (float)Lambda2 * Ray;
                }
            }
        }

        public Vector3 Centre { get => centre; set => centre = value; }
        public float Radius { get => radius; set => radius = value; }
        public Vector3 Colour { get => colour; set => colour = value; }
    }

}
