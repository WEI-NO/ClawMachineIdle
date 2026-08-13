using CustomLibrary.References;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class TopHUDAlphaController : MonoBehaviour
{
    public static TopHUDAlphaController Instance;

    [SerializeField] Material appliedMaterial;

    private List<Image> childImage = new List<Image>();
    private List<CanvasGroup> childCanvasGroups = new List<CanvasGroup>();


    Coroutine enableCoroutine = null;
    Coroutine disableCoroutine = null;

    [SerializeField] bool debug_enable_alpha = false;
    [SerializeField] bool debug_disable_alpha = false;

    [SerializeField] float lerpSpeed = 1.0f;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        FindAndSetAllMaterials();
        FindAllText();
        ResetAlpha();
    }

    private void Update()
    {
        if (debug_enable_alpha)
        {
            EnableAlpha();
            debug_enable_alpha = false;
        }

        if (debug_disable_alpha)
        {
            DisableAlpha();
            debug_disable_alpha = false;
        }
    }

    private void FindAndSetAllMaterials()
    {
        if (appliedMaterial == null) return;

        Image[] child_imgs = GetComponentsInChildren<Image>();

        foreach (var img in child_imgs)
        {
            img.material = appliedMaterial;
            childImage.Add(img);
        }
    }

    private void FindAllText()
    {
        TextMeshProUGUI[] child_text = GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var text in child_text)
        {
            var canvasGroup = text.AddComponent<CanvasGroup>();
            childCanvasGroups.Add(canvasGroup);
        }
    }

    private void StopAlphaCoroutine()
    {
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
        }

        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
        }
    }

    public void EnableAlpha()
    {
        StopAlphaCoroutine();
        enableCoroutine = StartCoroutine(EnableAlphaCoroutine());
    }

    public void DisableAlpha()
    {
        StopAlphaCoroutine();
        disableCoroutine = StartCoroutine(DisableAlphaCoroutine());
    }

    IEnumerator EnableAlphaCoroutine()
    {
        while (true)
        {
            bool allFinished = true;
            if (appliedMaterial != null)
            {
                var alpha = appliedMaterial.GetFloat("_Alpha");

                foreach (var img in childImage)
                {
                    if (alpha < 1.0f)
                    {
                        allFinished = false;
                    }
                    var newAlpha = Mathf.Lerp(alpha, 1.0f, Time.deltaTime * lerpSpeed);
                    img.material.SetFloat("_Alpha", newAlpha);
                }
            }

            foreach (var cg in childCanvasGroups)
            {
                var alpha = cg.alpha;
                if (alpha < 1.0f)
                {
                    allFinished = false;
                }
                var newAlpha = Mathf.Lerp(alpha, 1.0f, Time.deltaTime * lerpSpeed);
                cg.alpha = newAlpha;
            }
            yield return null;
            if (allFinished)
            {
                yield break;
            }
        }

    }

    IEnumerator DisableAlphaCoroutine()
    {
        while (true)
        {
            bool allFinished = true;
            if (appliedMaterial != null)
            {
                var alpha = appliedMaterial.GetFloat("_Alpha");
                
                foreach (var img in childImage)
                {
                    if (alpha > 0.0f)
                    {
                        allFinished = false;
                    }
                    var newAlpha = Mathf.Lerp(alpha, 0.0f, Time.deltaTime * lerpSpeed);
                    img.material.SetFloat("_Alpha", newAlpha);
                }
            }

            foreach (var cg in childCanvasGroups)
            {
                var alpha = cg.alpha;
                if (alpha > 0.0f)
                {
                    allFinished = false;
                }
                var newAlpha = Mathf.Lerp(alpha, 0.0f, Time.deltaTime * lerpSpeed);
                cg.alpha = newAlpha;
            }

            yield return null;
            if (allFinished)
            {
                yield break;
            }
        }
    }

    private void ResetAlpha()
    {
        if (appliedMaterial != null)
        {
            appliedMaterial.SetFloat("_Alpha", 1.0f);
        }

        foreach (var cg in childCanvasGroups)
        {
            cg.alpha = 1.0f;
        }
    }
}
