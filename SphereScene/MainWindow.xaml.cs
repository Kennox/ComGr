using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SphereScene {

    public partial class MainWindow : Window {

        public WriteableBitmap WriteableBitmap { get; set; }

        public MainWindow() {

            WriteableBitmap = new WriteableBitmap(666, 666, 96, 96, PixelFormats.Bgr32, null);
            InitializeComponent();
            Paint();
        }
        
        public Vector3 Eye = new Vector3(0, 0, -4);
        Vector3 LookAt = new Vector3(0, 0, 6);
        Vector3 Up = new Vector3(0, 1, 0);
        float FOV = (float) (36 * Math.PI / 180);
        Light Light = new Light(new Vector3(0, -0.9f, 0), new Vector3(0.8f, 1, 0.8f));


        public Sphere[] Scene = { new Sphere(new Vector3(-1001, 0, 0), 1000, new Vector3(1, 0, 0)),
                            new Sphere(new Vector3(1001, 0, 0), 1000, new Vector3(0, 0, 1)),
                            new Sphere(new Vector3(0, 0, 1001), 1000, new Vector3(1, 1, 1)),
                            new Sphere(new Vector3(0, -1001, 0), 1000, new Vector3(1, 1, 1)),
                            new Sphere(new Vector3(0, 1001, 0), 1000, new Vector3(1, 1, 1)),
                            new Sphere(new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, new Vector3(1, 1, 0)),
                            new Sphere(new Vector3(0.3f, 0.4f, 0.3f), 0.6f, new Vector3(1, 0, 1))};



        public Ray CreateEyeRay(Vector3 Eye, Vector3 LookAt, float FOV, Vector2 Pixel) {

            Vector3 f = Vector3.Normalize(LookAt - Eye);
            Vector3 r = Vector3.Normalize(Vector3.Cross(Up, f));
            Vector3 u = Vector3.Normalize(Vector3.Cross(f, r));
            Vector3 d = f + Pixel.X * r * (float)Math.Tan(FOV / 2) + Pixel.Y * u * (float) Math.Tan(FOV / 2);

            return new Ray(Eye, Vector3.Normalize(d));
        }

        public HitPoint FindClosestHitPoint(Sphere[] Scene, Ray Ray) {

            Vector3 H = new Vector3(0, 0, 0);
            Vector3 Colour = new Vector3(0, 0, 0);
            Vector3 Normal = new Vector3(0, 0, 0);
            float SmallestLambda = float.PositiveInfinity;
            
            for (int i = 0; i < Scene.Length; i++) {

                float Lambda = Scene[i].FindHitPoint(Scene[i], Ray);

                if (Lambda > 0 && Lambda < SmallestLambda) {

                    H = Ray.Origin + Lambda * Ray.Direction;
                    Colour = Scene[i].Colour;
                    Normal = Vector3.Normalize(H - Scene[i].Centre);
                    SmallestLambda = Lambda;
                }
            }
            HitPoint Hitpoint = new HitPoint(Ray, H, Colour, Normal);
            return Hitpoint;
        }

        public Vector3 CalcColour(Sphere[] Scene, Ray Ray) {

            HitPoint Hitpoint = FindClosestHitPoint(Scene, Ray);
            Vector3 Diffuse = new Vector3(0, 0, 0);
            Vector3 Specular = new Vector3(0, 0, 0);
            Vector3 Reflection;

            Vector3 L = Vector3.Normalize(Light.Position - Hitpoint.Position);
            float nL = Vector3.Dot(Hitpoint.Normal, L);

            if (nL >= 0) {

                Diffuse = Vector3.Multiply(Light.Colour, Hitpoint.Colour) * nL;
                Vector3 s = L - Vector3.Dot(L, Hitpoint.Normal) * Hitpoint.Normal;
                Vector3 r = L - 2 * s;
                Specular = Light.Colour * (float) Math.Pow(Vector3.Dot(Vector3.Normalize(r), Vector3.Normalize(Hitpoint.Position - Eye)), 10);
            }

            return Diffuse + Specular;
        }

        public void Paint() {

            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;
            int cc = 4; //Colour Channels
            byte[] ColourData = new byte[width * height * cc];

            for (int i = 0; i < width; i++) {

                for (int j = 0; j < height; j++) {

                    Vector3 Colour = CalcColour(Scene, CreateEyeRay(Eye, LookAt, FOV, new Vector2((float) 2.0 / width * i - 1, (float) 2.0 / height * j - 1)));
                    ColourData[(i * 4 + j * width * cc)] = Convert.ToByte(Colour.Z * 255);
                    ColourData[(i * 4 + j * width * cc + 1)] = Convert.ToByte(Colour.Y * 255);
                    ColourData[(i * 4 + j * width * cc + 2)] = Convert.ToByte(Colour.X * 255);
                }
            }
            WriteableBitmap.Lock();
            WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), ColourData, width * cc, 0);
            WriteableBitmap.Unlock();
        }
    }
}
