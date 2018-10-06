using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        public Vector3 Eye = new Vector3(0, 0, -4);
        Vector3 LookAt = new Vector3(0, 0, 6);
        Vector3 Up = new Vector3(0, 1, 0);
        float FOV = (float)(36 * Math.PI / 180);
        Light Light = new Light(new Vector3(0, -0.9f, 0), new Vector3(1.0f, 1, 1.0f));

        static Light[] Lights = new Light[1];
        Light LightRed = new Light(new Vector3(0, -0.9f, -0.5f), new Vector3(1, 0.0f, 0.0f));
        Light LightBlue = new Light(new Vector3(0, -0.9f, 0.5f), new Vector3(0.0f, 0.0f, 1));

        int RecursionDepth = 2;

        static Bitmap Earth = LoadImage("Earth2.png");
        static Bitmap Sun = LoadImage("sun.jpg");


        public Sphere[] Scene = { new PlainSphere(new Vector3(-1001, 0, 0), 1000, new Vector3(1, 0, 0), 0, 0),
                            new PlainSphere(new Vector3(1001, 0, 0), 1000, new Vector3(0, 0, 1), 0, 0),
                            new PlainSphere(new Vector3(0, 0, 1001), 1000, new Vector3(1, 1, 1), 0, 0),
                            new PlainSphere(new Vector3(0, -1001, 0), 1000, new Vector3(1, 1, 1), 0, 0),
                            new PlainSphere(new Vector3(0, 1001, 0), 1000, new Vector3(1, 1, 1), 0, 0),
                            new TextureSphere(new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Sun, 0.1f, 0),
                            new TextureSphere(new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Earth, 0.1f, 0f)};

        public static BVHSphere BVHScene;

        public MainWindow() {
            WriteableBitmap = new WriteableBitmap(666, 666, 96, 96, PixelFormats.Bgr32, null);
            Lights[0] = Light;
            //Lights[1] = LightBlue;
            //Lights[2] = LightRed;
            BVHScene = CreateBVHSpheres(Scene);
            InitializeComponent();
            Paint();
        }

        public Ray CreateEyeRay(Vector3 Eye, Vector3 LookAt, float FOV, Vector2 Pixel) {

            Vector3 f = Vector3.Normalize(LookAt - Eye);
            Vector3 r = Vector3.Normalize(Vector3.Cross(Up, f));
            Vector3 u = Vector3.Normalize(Vector3.Cross(f, r));
            Vector3 d = f + Pixel.X * r * (float)Math.Tan(FOV / 2) + Pixel.Y * u * (float)Math.Tan(FOV / 2);

            return new Ray(Eye, Vector3.Normalize(d));
        }

        public HitPoint FindClosestHitPoint(Sphere[] Scene, Ray Ray) {

            Sphere HitSphere = null;
            Vector3 H = new Vector3(0, 0, 0);
            Vector3 Colour = new Vector3(0, 0, 0);
            Vector3 Normal = new Vector3(0, 0, 0);
            float SmallestLambda = float.PositiveInfinity;

            for (int i = 0; i < Scene.Length; i++) {

                float Lambda = Scene[i].FindHitPoint(Scene[i], Ray);

                if (Lambda > 0 && Lambda < SmallestLambda) {

                    H = Ray.Origin + Lambda * Ray.Direction;
                    //Colour = Scene[i].GetColour();
                    Normal = Vector3.Normalize(H - Scene[i].Centre);
                    SmallestLambda = Lambda;
                    HitSphere = Scene[i];
                }
            }
            HitPoint Hitpoint = new HitPoint(Ray, H, Normal, HitSphere);
            return Hitpoint;
        }

        public Vector3 CalcColour(Sphere[] Scene, Ray Ray, int Recursion) {

            HitPoint Hitpoint = FindClosestHitPoint(Scene, Ray);
            if (Hitpoint.Sphere == null) {
                return Vector3.Zero;
            }
            if (Hitpoint.Sphere.Luminous > 0) {
                return Hitpoint.Sphere.GetColour(Hitpoint.Normal);
            }

            Vector3 Diffuse = new Vector3(0, 0, 0);
            Vector3 Specular = new Vector3(0, 0, 0);
            Vector3 Shadow = Vector3.Zero;
            Vector3 Reflection = Vector3.Zero;

            for (int l = 0; l < Lights.Length; l++) {

                Shadow += Lights[l].Colour;
                Vector3 L = Vector3.Normalize(Lights[l].Position - Hitpoint.Position);
                float nL = Vector3.Dot(Hitpoint.Normal, L);

                if (nL >= 0) {

                    Diffuse += Vector3.Multiply(Lights[l].Colour, Hitpoint.Sphere.GetColour(Hitpoint.Normal)) * nL;
                    Vector3 s = L - Vector3.Dot(L, Hitpoint.Normal) * Hitpoint.Normal;
                    Vector3 r = L - 2 * s;
                    Specular += Lights[l].Colour * (float)Math.Pow(Vector3.Dot(Vector3.Normalize(r), Vector3.Normalize(Hitpoint.Position - Eye)), 10);

                }

                Ray LightRay = new Ray(Hitpoint.Position, Vector3.Normalize(Lights[l].Position - Hitpoint.Position));
                LightRay.Origin += LightRay.Direction * 0.001f + Hitpoint.Normal * 0.001f;

                for (int i = 0; i < Scene.Length; i++) {
                    float Lambda = Scene[i].FindHitPoint(Scene[i], LightRay);
                    if (Lambda != 0 && Lambda < (Lights[l].Position - Hitpoint.Position).Length()) {
                        Shadow -= Lights[l].Colour * 0.99f;
                        break;
                    }
                }
            }

            if (Recursion > 0) {
                Ray Reflect = new Ray(Hitpoint.Position, Vector3.Reflect(Ray.Direction, Hitpoint.Normal));
                Reflect.Origin += Reflect.Direction * 0.01f;
                Reflection = CalcColour(Scene, Reflect, Recursion - 1) * Hitpoint.Sphere.Reflection;

            }

            return (Diffuse + Specular) * Shadow + Reflection;
        }

        public void Paint() {

            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;
            int cc = 4; //Colour Channels
            byte[] ColourData = new byte[width * height * cc];

            for (int i = 0; i < width; i++) {

                for (int j = 0; j < height; j++) {

                    Vector3 Colour = CalcColour(Scene, CreateEyeRay(Eye, LookAt, FOV, new Vector2((float)2.0 / width * i - 1, (float)2.0 / height * j - 1)), RecursionDepth);
                    System.Windows.Media.Color c = System.Windows.Media.Color.FromScRgb(1, Colour.X, Colour.Y, Colour.Z);
                    ColourData[(i * 4 + j * width * cc)] = c.B;         //Convert.ToByte(Math.Min(Colour.Z * 255, 255));
                    ColourData[(i * 4 + j * width * cc + 1)] = c.G;     //Convert.ToByte(Math.Min(Colour.Y * 255, 255));
                    ColourData[(i * 4 + j * width * cc + 2)] = c.R;     //Convert.ToByte(Math.Min(Colour.X * 255, 255));
                }
            }
            WriteableBitmap.Lock();
            WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), ColourData, width * cc, 0);
            WriteableBitmap.Unlock();
        }

        public static Bitmap LoadImage(String Filename) {
            var Path = @"Resources\" + Filename;
            var Image = new Bitmap(Path);
            return Image;
        }

        public BVHSphere CreateBVHSpheres(Sphere[] Scene) {
            float ShortestDistance = float.MaxValue;
            Sphere S1 = null;
            Sphere S2 = null;
            BVHSphere End = null;
            
            for(int i = 0; i < Scene.Length; i++) {
                int j = i + 1;
                for (; j < Scene.Length; j++) {

                    float Dist = (Scene[i].Centre - Scene[j].Centre).Length();
                    if (Dist < ShortestDistance) {
                        ShortestDistance = Dist;
                        S1 = Scene[i];
                        S2 = Scene[j];
                    }
                }
                j++;
            }

            if (Scene.Length -1 > 1) {
                Sphere[] NextScene = new Sphere[Scene.Length - 1];
                int index = 0;
                for (int i = 0; i < Scene.Length; i++) {
                    if(Scene[i].Centre != S1.Centre && Scene[i].Centre != S2.Centre) {
                        NextScene[index] = Scene[i];
                        index++;
                    }
                }
                float Radius = ((S1.Centre - S2.Centre).Length() + (S1.Radius - S2.Radius)) / 2f;
                Vector3 Center = S1.Centre + Vector3.Normalize(S2.Centre - S1.Centre) * (Radius - S1.Radius);
                NextScene[Scene.Length - 2] = new BVHSphere(Center, Radius, 0, 0, S1, S2);
            End = CreateBVHSpheres(NextScene);
            }
            return End;
        }
    }
}
