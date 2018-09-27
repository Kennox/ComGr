using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {

    public class Ray {

        public Vector3 Origin { get; set; }
        public Vector3 Direction { get; set; }

        public Ray(Vector3 Origin, Vector3 Direction) {

            this.Origin = Origin;
            this.Direction = Direction;
        }

    }
}
