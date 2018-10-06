using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {

    public abstract class Sphere {

        public Vector3 Centre { get; set; }
        public float Radius { get; set; }
        //public Vector3 Colour { get; set; }
        public float Reflection { get; set; }
        public float Luminous { get; set; }

        public Sphere(Vector3 Center, float Radius, float Reflection, float Luminous) {

            this.Centre = Center;
            this.Radius = Radius;
            //this.Colour = Colour;
            this.Reflection = Reflection;
            this.Luminous = Luminous;
        }

        public float FindHitPoint(Sphere Sphere, Ray Ray) {

            var a = 1;
            var CE = Ray.Origin - Sphere.Centre;
            var b = 2 * Vector3.Dot(CE, Ray.Direction);
            var c = CE.Length() * CE.Length() - Sphere.Radius * Sphere.Radius;
            var Discriminant = b * b - 4 * a * c;

            if (Discriminant >= 0) { //at least one hit

                var Lambda1 = (-b + Math.Sqrt(Discriminant)) / (2 * a);
                var Lambda2 = (-b - Math.Sqrt(Discriminant)) / (2 * a);

                if (Lambda1 > 0 && Lambda2 > 0) {

                    return (float) Math.Min(Lambda1, Lambda2);

                } else if (Lambda1 > 0) {

                    return (float) Lambda1;

                } else if (Lambda2 > 0) {

                    return (float) Lambda2;
                }  
            }
            return 0;
        }

        public abstract Vector3 GetColour(Vector3 Pixel);
    }

    public class PlainSphere : Sphere {

        public Vector3 Colour { get; set; }

        public PlainSphere(Vector3 Center, float Radius, Vector3 Colour, float Reflection, float Luminous):base(Center, Radius, Reflection, Luminous) {
            
            this.Colour = Colour;
        }

        public override Vector3 GetColour(Vector3 Pixel) {
            return Colour;
        }
    }

    public class TextureSphere : Sphere {

        public Bitmap Bitmap { get; set; }

        public TextureSphere(Vector3 Center, float Radius, Bitmap Bitmap, float Reflection, float Luminous):base(Center, Radius, Reflection, Luminous) {

            this.Bitmap = Bitmap;
        }

        public override Vector3 GetColour(Vector3 Point) {
            var s = ((Math.Atan2(Point.X, Point.Z) / Math.PI + 1) * Bitmap.Width / 2.0 + +1300) % Bitmap.Width;
            var t = (Math.Acos(Point.Y) * Bitmap.Height / Math.PI * -1 + Bitmap.Height - 1);
            var c = Bitmap.GetPixel((int)s, (int)t);
            return new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
        }
    }

    public class BVHSphere : Sphere {

        public Sphere S1 { get; set; }
        public Sphere S2 { get; set; }

        public BVHSphere(Vector3 Center, float Radius, float Reflection, float Luminous, Sphere S1, Sphere S2):base(Center, Radius, Reflection, Luminous) {
            this.S1 = S1;
            this.S2 = S2;
        }

        public override Vector3 GetColour(Vector3 Pixel) {
            return new Vector3(0, 0, 0);
        }

    }

}

