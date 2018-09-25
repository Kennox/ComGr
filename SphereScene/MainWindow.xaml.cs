using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public WriteableBitmap WriteableBitmap { get; set; }

        public MainWindow() {
            WriteableBitmap = new WriteableBitmap(666, 666, 96, 96, PixelFormats.Bgr32, null);
            InitializeComponent();
        }
        
        public Vector3 Eye = new Vector3(0, 0, -4);
        Vector3 LookAt = new Vector3(0, 0, 6);
        Vector3 Up = new Vector3(0, 1, 0);
        float FOV = 36;


        public Sphere[] Scene = { new Sphere(new Vector3(-1001, 0, 0), 1000, new Vector3(0, 0, 1)),
                            new Sphere(new Vector3(1001, 0, 0), 1000, new Vector3(1, 0, 0)),
                            new Sphere(new Vector3(0, 0, 1001), 1000, new Vector3(0, 1, 0)),
                            new Sphere(new Vector3(0, -1001, 0), 1000, new Vector3(1, 0, 1)),
                            new Sphere(new Vector3(0, 1001, 0), 1000, new Vector3(1, 0, 1)),
                            new Sphere(new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, new Vector3(0, 1, 1)),
                            new Sphere(new Vector3(0.3f, 0.4f, 0.3f), 0.6f, new Vector3(1, 1, 0))};



        public Vector3 CreateEyeRay(Vector3 Eye, Vector3 LookAt, float FOV, Vector2 Pixel) {
            Vector3 f = LookAt - Eye;
            Vector3 r = Vector3.Cross(f, Up);
            Vector3 u = Vector3.Cross(r, f);

            Vector3 d = Vector3.Normalize(f) + Pixel.X * r * (float)Math.Tan(FOV / 2) + Pixel.Y * u * (float) Math.Tan(FOV / 2);

            return Vector3.Normalize(d);
        }

        public Vector3 FindClosestHitPoint(Sphere[] Scene, Vector3 Ray) {

            Vector3 H = new Vector3(0, 0, 0);

            for (int i = 0; i < Scene.Length; i++) {
                var a = 1;
                var b = 2 * Vector3.Dot((Eye - Scene[i].Centre), Ray);
                var c = (Eye - (Scene[i].Centre)).Length() - Scene[i].Radius;
                var Discriminant = b * b - 4 * a * c;
                if (Discriminant >= 0) { //at least one hit
                    var Lambda1 = -b + Math.Sqrt(Discriminant) / (2 * a);
                    var Lambda2 = -b - Math.Sqrt(Discriminant) / (2 * a);
                    if (Lambda1 > 0 && Lambda2 > 0) {
                        H = Eye + (float) Math.Min(Lambda1, Lambda2) * Ray;
                    } else if (Lambda1 > 0) {
                        H = Eye + (float) Lambda1 * Ray;
                    } else if (Lambda2 > 0) {
                        H = Eye + (float) Lambda2 * Ray;
                    }
                }
            }
            return H;
        }

        public Vector3 CalcColour(Sphere[] Scene, Vector3 Ray) {
            var H = FindClosestHitPoint(Scene, Ray);

            return null;
        }
    }
}
