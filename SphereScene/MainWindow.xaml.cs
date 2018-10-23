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
        Light Light = new Light(new Vector3(0, -0.8f, 0), new Vector3(1.0f, 1, 1.0f));

        static Light[] Lights = new Light[1];
        Light LightRed = new Light(new Vector3(0.5f, -0.9f, -0.5f), new Vector3(0.8f, 0.1f, 0.1f));
        Light LightGreen = new Light(new Vector3(-0.5f, -0.9f, 0), new Vector3(0.1f, 0.8f, 0.1f));
        Light LightBlue = new Light(new Vector3(0.5f, -0.9f, 0.5f), new Vector3(0.1f, 0.1f, 0.8f));

        int RecursionDepth = 2;

        static Bitmap Earth = LoadImage("Earth2.png");
        static Bitmap Sun = LoadImage("sun.jpg");


        public Sphere[] Scene = { new PlainSphere(new Vector3(-1001, 0, 0), 1000, new Vector3(1, 0, 0), 0, 0),
                            new PlainSphere(new Vector3(1001, 0, 0), 1000, new Vector3(0, 0, 1), 0, 0),
                            new PlainSphere(new Vector3(0, 0, 1001), 1000, new Vector3(1, 1, 1), 0, 0),
                            new PlainSphere(new Vector3(0, -1001, 0), 1000, new Vector3(1, 1, 1), 0, 0),
                            new PlainSphere(new Vector3(0, 1001, 0), 1000, new Vector3(1, 1, 1), 0, 0),
                            //new TextureSphere(new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Earth, 0f, 0.1f),
                            //new TextureSphere(new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Sun, 0f, 0.1f)};
                            new PlainSphere(new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, new Vector3(1, 1, 0), 0.1f, 0),
                            new PlainSphere(new Vector3(0.3f, 0.4f, 0.3f), 0.6f, new Vector3(0.8f, 1, 0.8f), 0.1f, 0)};


        public static BVHSphere BVHScene;

        public MainWindow() {
            WriteableBitmap = new WriteableBitmap(666, 666, 96, 96, PixelFormats.Bgr32, null);
            Lights[0] = Light; //single light
            //Lights[0] = LightGreen;     //multiple coloured lights
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

        //find hitpoint in a scene of many shperes
        //isn't used anymore!!!!
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
            HitPoint Hitpoint = new HitPoint(Ray, H, Normal, HitSphere, SmallestLambda);
            return Hitpoint;
        }

        //find hitpoint in a bounding shpere
        public HitPoint FindClosestHitPoint(Sphere Scene, Ray Ray) {

            Sphere HitSphere = null;
            Vector3 H = new Vector3(0, 0, 0);
            Vector3 Normal = new Vector3(0, 0, 0);

            float Lambda = Scene.FindHitPoint(Scene, Ray); //returns 0 if no hit was found
            if (Lambda > 0) {
                if (Scene is BVHSphere BVH) {
                    HitPoint HpS1 = FindClosestHitPoint(BVH.S1, Ray);
                    HitPoint HpS2 = FindClosestHitPoint(BVH.S2, Ray);

                    if (HpS1.Lambda > 0 && HpS2.Lambda > 0) {

                        return HpS1.Lambda < HpS2.Lambda ? HpS1 : HpS2;

                    } else if (HpS1.Lambda > 0) {

                        return HpS1;

                    } else if (HpS2.Lambda > 0) {

                        return HpS2;
                    }
                    Lambda = 0;
                } else {
                    H = Ray.Origin + Lambda * Ray.Direction;
                    Normal = Vector3.Normalize(H - Scene.Centre);
                    HitSphere = Scene;
                }
            }

            HitPoint Hitpoint = new HitPoint(Ray, H, Normal, HitSphere, Lambda);
            return Hitpoint;
        }

        //everything colours
        public Vector3 CalcColour(Ray Ray, int Recursion) {

            HitPoint Hitpoint = FindClosestHitPoint(BVHScene, Ray);
            if (Hitpoint.Sphere == null) {
                return Vector3.Zero;
            }
            if (Hitpoint.Sphere.Luminous > 0) {
                return Hitpoint.Sphere.GetColour(Hitpoint.Normal);
            }

            Vector3 Diffuse = Vector3.Zero;
            Vector3 Specular = Vector3.Zero;
            Vector3 Shadow = Vector3.Zero;
            Vector3 Reflection = Vector3.Zero;

            for (int l = 0; l < Lights.Length; l++) {

                Vector3 L = Vector3.Normalize(Lights[l].Position - Hitpoint.Position);
                float nL = Vector3.Dot(Hitpoint.Normal, L);

                if (nL >= 0) {

                    Diffuse += Vector3.Multiply(Lights[l].Colour, Hitpoint.Sphere.GetColour(Hitpoint.Normal)) * nL;
                    Vector3 s = L - Vector3.Dot(L, Hitpoint.Normal) * Hitpoint.Normal;
                    Vector3 r = L - 2 * s;
                    Specular += Lights[l].Colour * (float)Math.Pow(Vector3.Dot(Vector3.Normalize(r), Vector3.Normalize(Hitpoint.Position - Eye)), 10);

                }

                //Shadow
                Random random = new Random();
                Vector3 LH = Vector3.Normalize(Hitpoint.Position - Lights[l].Position);
                int ShadowCounter = 0;

                int z = 0;
                while (z < 8) { //amount of shadow-feelers
                    float lightRadius = 0.19f;
                    float radius = (float)(random.NextDouble());
                    float theta = (float)(random.NextDouble() * 2 * Math.PI);
                    float x = (float)(Math.Sqrt(radius) * Math.Sin(theta));
                    float y = (float)(Math.Sqrt(radius) * Math.Cos(theta));

                    Vector3 Nx = Vector3.Normalize(Vector3.Cross(LH, Up));
                    Vector3 Ny = Vector3.Normalize(Vector3.Cross(LH, Nx));

                    Vector3 Point = Lights[l].Position + lightRadius * Nx * x + lightRadius * Ny * y;//point on disk

                    Ray LightRay = new Ray(Hitpoint.Position, Vector3.Normalize(Point - Hitpoint.Position)); //used for soft shadows
                    //Ray LightRay = new Ray(Hitpoint.Position, Vector3.Normalize(Lights[l].Position - Hitpoint.Position)); //used for basic shadow
                    LightRay.Origin += LightRay.Direction * 0.001f + Hitpoint.Normal * 0.001f;

                    HitPoint ShadowHp = FindClosestHitPoint(BVHScene, LightRay);
                    if (ShadowHp.Sphere != null) {
                        float Lambda = ShadowHp.Lambda;
                        if (Lambda != 0 && Lambda < (Point - Hitpoint.Position).Length()) { //TODO float ungenauigkeit beheben?
                            ShadowCounter++;
                        }
                    }
                    z++;
                }
                Shadow += Lights[l].Colour * (float) ((z - ShadowCounter) / (float) z);
                Shadow += Lights[l].Colour * 0.01f;
            }

            //Reflection

            if (Recursion > 0) {
                Ray Reflect = new Ray(Hitpoint.Position, Vector3.Reflect(Ray.Direction, Hitpoint.Normal));
                Reflect.Origin += Reflect.Direction * 0.01f;
                Reflection = CalcColour(Reflect, Recursion - 1) * Hitpoint.Sphere.Reflection;

            }

            return (Diffuse + Specular) * Shadow + Reflection;
        }

        public void Paint() {

            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;
            int cc = 4; //Colour Channels
            byte[] ColourData = new byte[width * height * cc];


            Parallel.For(0, width, i => {

                Random r = new Random();

                for (int j = 0; j < height; j++) {

                    Vector3 Colour = new Vector3(0, 0, 0);
                    int z = 0;
                    //AA
                    while (z < 64) {

                        double X = 2.0 / width * i - 1;
                        double Y = 2.0 / height * j - 1;
                        Vector2 Pixel = new Vector2((float)(r.NextGaussian(X, 0.4 * 2 / width)), (float)(r.NextGaussian(Y, 0.4 * 2 / height)));
                        Colour += CalcColour(CreateEyeRay(Eye, LookAt, FOV, Pixel), RecursionDepth);
                        z++;
                    }
                    Colour /= z;

                    System.Windows.Media.Color c = System.Windows.Media.Color.FromScRgb(1, Colour.X, Colour.Y, Colour.Z);
                    ColourData[(i * 4 + j * width * cc)] = c.B;
                    ColourData[(i * 4 + j * width * cc + 1)] = c.G;
                    ColourData[(i * 4 + j * width * cc + 2)] = c.R;
                }
            });
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
            float SmallestRadius = float.MaxValue;
            Sphere S1 = null;
            Sphere S2 = null;
            BVHSphere End = null;
            
            for(int i = 0; i < Scene.Length; i++) {
                for (int j = i + 1; j < Scene.Length; j++) {

                    float Rad = ((Scene[i].Centre - Scene[j].Centre).Length() + (Scene[i].Radius + Scene[j].Radius)) / 2f;
                    //float Dist = (Scene[i].Centre - Scene[j].Centre).Length();
                    if (Rad < SmallestRadius) {
                        SmallestRadius = Rad;
                        S1 = Scene[i];
                        S2 = Scene[j];
                    }
                }
            }

            if (Scene.Length > 1) {
                Sphere[] NextScene = new Sphere[Scene.Length - 1];
                int index = 0;
                for (int i = 0; i < Scene.Length; i++) {
                    if(Scene[i].Centre != S1.Centre && Scene[i].Centre != S2.Centre) {
                        NextScene[index] = Scene[i];
                        index++;
                    }
                }
                float Radius = ((S1.Centre - S2.Centre).Length() + (S1.Radius + S2.Radius)) / 2f;
                Vector3 Center = S1.Centre + Vector3.Normalize(S2.Centre - S1.Centre) * (Radius - S1.Radius);
                NextScene[Scene.Length - 2] = new BVHSphere(Center, Radius, 0, 0, S1, S2);

                End = CreateBVHSpheres(NextScene);
            } else { return (BVHSphere) Scene[0]; }
            return End;
        }
    }
}
