using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask encounterObjectsLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask levelChangeLayer;
    public static GameLayers self { get; set; }
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        self = this;
    }

    public LayerMask SolidLayer
    {
        get => solidObjectsLayer;
    }

    public LayerMask InterctableLayer
    {
        get => interactableLayer;
    }

    public LayerMask EncounterLayer
    {
        get => encounterObjectsLayer;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    public LayerMask FovLayer
    {
        get => fovLayer;
    }

    public LayerMask LevelChange
    {
        get => levelChangeLayer;
    }

    public LayerMask TriggerableLayers
    {
        get => encounterObjectsLayer | levelChangeLayer;
        /*fovLayer*/
    }
}
