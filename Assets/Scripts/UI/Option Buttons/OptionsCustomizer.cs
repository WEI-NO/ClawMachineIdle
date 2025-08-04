using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OptionsCustomizer : MonoBehaviour
{
    public List<BaseOptionButton> DefaultOptions;
    public List<BaseOptionButton> CustomizedOptions;


    public List<BaseOptionButton> GetOptions()
    {
        List<BaseOptionButton> result = DefaultOptions;
        result.AddRange(CustomizedOptions);

        return result;
    }
}
