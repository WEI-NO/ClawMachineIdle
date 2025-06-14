using CustomLibrary.References;
using System.Collections.Generic;
using UnityEngine;

public class MachineWallpaper : MonoBehaviour
{
    [Header("Wall Paper Settings")]
    [SerializeField] private List<Sprite> wallpapers;

    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private void Start()
    {
        ClawMachineThemeController.Instance.OnThemeChange += OnThemeChange;
    }

    private void OnThemeChange(CM_Theme theme)
    {
        int index = theme.ToInt();
        if (wallpapers == null || index >= wallpapers.Count) return;

        foreach (var i in spriteRenderers)
        {
            i.sprite = wallpapers[index];
        }
    }
}
