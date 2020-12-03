using System.Numerics;

namespace BetterSDR.Common {
    public static class Extensions {
        public static Complex Conjugate(this Complex meh) {
            return new Complex(meh.Real, -meh.Imaginary);
        }
    }
}
