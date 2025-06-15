using CustomLibrary.References;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchControllerRevised : MonoBehaviour
{
    public static TouchController Instance;


    [Header("Targets")]
    public IsometricBuilding selectedBuilding;

    [Header("States")]
    public bool editMode = false;
    public bool fingerHolding;
    public bool triggeredHold;
    public int currentPointerID;

    [Header("Position Properties")]
    private Vector2Int dragOffset;
    private Vector3 dragOrigin;
    private Vector2Int startOrigin;
    private Vector2 lastTouchPosition;

    void Awake()
    {
        Initializer.SetInstance(this);
    }
}
