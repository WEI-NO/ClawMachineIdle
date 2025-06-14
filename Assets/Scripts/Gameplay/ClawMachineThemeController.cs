using CustomLibrary.References;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public enum CM_Theme
{
    Forest,
    City,
    Count
}

public class ClawMachineThemeController : MonoBehaviour
{
    public static ClawMachineThemeController Instance;

    [Header("Theme Properties")]
    [SerializeField] private CM_Theme _currentTheme;
    public CM_Theme CurrentTheme { get { return _currentTheme; } private set { } }
    public Action<CM_Theme> OnThemeChange;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        Initialize();
    }

    #region Theme   

    private void Initialize()
    {
        // Defaults to Forest
        _currentTheme = CM_Theme.Forest;
        OnThemeChange?.Invoke(_currentTheme);
    }

    public void ChangeTheme(CM_Theme theme)
    {
        if (_currentTheme == theme) { Debug.LogWarning("Already on theme: " + _currentTheme);  return; }
        _currentTheme = theme;
        OnThemeChange?.Invoke(_currentTheme);
    }

    #endregion theme

}
