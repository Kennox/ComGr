using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ComGr {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {

        public WriteableBitmap WriteableBitmap { get; set; }

        public MainWindow() {
            WriteableBitmap = new WriteableBitmap(666, 666, 96, 96, PixelFormats.Bgr32, null);
            InitializeComponent();

            CompositionTarget.Rendering += DrawThis;

        }

        private int Cycle = 0;
        private int cc = 4;     //ColourChannels
        private byte[] ColourData;

        private void DrawThis(object sender, EventArgs args) {

            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;

            Rainbow(Cycle);

            //TODO: RGB into sRGB?
            for (int i = 0; i < height; i++) {

                WriteableBitmap.Lock();
                WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, 1), ColourData, width * cc, 0, i);
                WriteableBitmap.Unlock();
            }
            Cycle += cc;
            
        }

        private byte[] Rainbow(int Cycle) {
            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;
            int cwidth = width / 6;
            int ccwidth = cwidth * 4;
            ColourData = new byte[width * cc];

            for (int i = 0; i < width * cc; i += cc) {
                int column = (i % (width * cc)) / cwidth;
                if (column < cc) {
                    ColourData[(i + Cycle) % (width * cc)] = 0;          // B
                    ColourData[(i + 1 + Cycle) % (width * cc)] = (byte)(255.0 / ccwidth * (i % ccwidth));      // G
                    ColourData[(i + 2 + Cycle) % (width * cc)] = 255;    // R

                } else if (column < 2 * cc) {
                    ColourData[(i + Cycle) % (width * cc)] = 0;          // B
                    ColourData[(i + 1 + Cycle) % (width * cc)] = 255;      // G
                    ColourData[(i + 2 + Cycle) % (width * cc)] = (byte)(255 - (255.0 / ccwidth * (i % ccwidth)));    // R

                } else if (column < 3 * cc) {
                    ColourData[(i + Cycle) % (width * cc)] = (byte)(255.0 / ccwidth * (i % ccwidth));          // B
                    ColourData[(i + 1 + Cycle) % (width * cc)] = 255;      // G
                    ColourData[(i + 2 + Cycle) % (width * cc)] = 0;    // R

                } else if (column < 4 * cc) {
                    ColourData[(i + Cycle) % (width * cc)] = 255;          // B
                    ColourData[(i + 1 + Cycle) % (width * cc)] = (byte)(255 - (255.0 / ccwidth * (i % ccwidth)));      // G
                    ColourData[(i + 2 + Cycle) % (width * cc)] = 0;    // R

                } else if (column < 5 * cc) {
                    ColourData[(i + Cycle) % (width * cc)] = 255;          // B
                    ColourData[(i + 1 + Cycle) % (width * cc)] = 0;      // G
                    ColourData[(i + 2 + Cycle) % (width * cc)] = (byte)(255.0 / ccwidth * (i % ccwidth));    // R

                } else if (column < 6 * cc) {
                    ColourData[(i + Cycle) % (width * cc)] = (byte)(255 - (255.0 / ccwidth * (i % ccwidth)));        // B
                    ColourData[(i + 1 + Cycle) % (width * cc)] = 0;    // G
                    ColourData[(i + 2 + Cycle) % (width * cc)] = 255;    // R
                }
            }
            return ColourData;
        }
    }
}
