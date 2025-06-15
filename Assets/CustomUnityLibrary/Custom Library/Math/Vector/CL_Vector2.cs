using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomLibrary.Math.Vector
{
    public static class Vector2Extension
    {
        // ----- Value Swap -----
        // Description:
        //              Swaps one value of a Vector2 and returns the result
        public static Vector2 ValueSwap(Vector2 original, iVector2 index, float value)
        {
            int i = (int)index;
            original[i] = value;
            return original;
        }

        public static float GetRandom(this Vector2 main)
        {
            return Random.Range(main.x, main.y);
        }
    }

}
