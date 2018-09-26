using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SphereScene {
    public class Ray {
        public Vector3 Origin { get => Origin; set => Origin = value; }
        public Vector3 Direction { get => Direction; set => Direction = value; }

        public Ray(Vector3 Origin, Vector3 Direction) {
            this.Origin = Origin;
            this.Direction = Direction;
        }

    }
}
