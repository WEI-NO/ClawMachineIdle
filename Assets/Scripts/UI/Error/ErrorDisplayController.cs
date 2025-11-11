using CustomLibrary.References;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class ErrorDisplayController : MonoBehaviour
{
    public static ErrorDisplayController Instance;

    [Header("Components")]
    public ErrorDisplay displayPrefab;
    public Transform content;
    public float errorDuration = 5.0f;
    private static Dictionary<string, ErrorDisplay> MessagesToDisplay = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
        content = transform;
    }


    public static void AddMessage(string message, float duration = 0)
    {
        if (duration == 0) duration = Instance.errorDuration;

        if (MessagesToDisplay.TryGetValue(message, out var dis))
        {
            if (dis == null)
            {
                MessagesToDisplay.Remove(message);
            } else
            {
                dis.AddEntry();
                return;
            }
        }
        var display = Instantiate(Instance.displayPrefab, Instance.content);
        display.Initialize(message, duration);
        MessagesToDisplay.Add(message, display);
    }
    
}
