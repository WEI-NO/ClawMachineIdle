using UnityEngine;

namespace CustomLibrary.Math.Vector
{
    public static class Vector3Extension
    {
        // ----- Value Swap -----
        // Description:
        //              Swaps one value of a Vector3 and returns the result
        public static Vector3 ValueSwap(Vector3 original, iVector3 index, float value)
        {
            int i = (int)index;
            original[i] = value;
            return original;
        }
    }

}
