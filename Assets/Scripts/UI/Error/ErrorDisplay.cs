using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    public TextMeshProUGUI errorText;

    public Animator anim;

    private float internalTimer = 0.0f;

    public void Initialize(string errorText, float duration)
    {
        this.errorText.text = errorText;
        StartCoroutine(DestroySequence(duration));
        internalTimer = 0.0f;
    }

    public void AddEntry()
    {
        string current = errorText.text;

        // Regex: find " x<number>" at end of string
        Match match = Regex.Match(current, @" x(\d+)$");

        if (match.Success)
        {
            // Already has x#, so increment
            int num = int.Parse(match.Groups[1].Value);
            num++;

            // Replace old x# with new x#
            current = Regex.Replace(current, @" x\d+$", $" x{num}");
        }
        else
        {
            // No suffix -> add x1
            current += " x1";
        }

        errorText.text = current;
        ResetTimer();
    }

    private IEnumerator DestroySequence(float duration)
    {
        while (internalTimer <= duration)
        {
            internalTimer += Time.deltaTime;
            yield return null;
        }

        anim.SetTrigger("End");
    }

    private void ResetTimer()
    {
        internalTimer = 0.0f;
    }

}
