using System.Reflection;
using UnityEngine;

namespace CustomLibrary.References
{
    // ----- Initializer -----
    // Description:
    //              A collection of function to help with initializing
    //                  references.
    public struct Initializer
    {
        // ----- Set Instance -----
        // Description:
        //              Makes sure only one instance of a class is present.
        //              Equivalent to:
        //                              if (!instance)
        //                                  instance = this;
        //                              else
        //                                  Destroy(this.gameObject);
        public static void SetInstance<T>(T instance, string field_name = "Instance") where T : MonoBehaviour
        {
            // Get the property named instance.
            FieldInfo field = typeof(T).GetField(field_name);
            // If property exists.
            if (field != null)
            {
                // Get the static instance of T.
                T static_inst = field.GetValue(null) as T;
                // If static instance is null.
                if (static_inst == null)
                {
                    field.SetValue(null, instance);
                    return;
                }
                // UnityEngine.Object.Destroy(instance);
                Object.Destroy(instance.gameObject);
                return;
            }
        }

    }
}
