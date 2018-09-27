using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {

    class Light {

        public Vector3 Position { get; set; }
        public Vector3 Colour { get; set; }

        public Light(Vector3 Position, Vector3 Colour) {

            this.Position = Position;
            this.Colour = Colour;
        }
    }
}
