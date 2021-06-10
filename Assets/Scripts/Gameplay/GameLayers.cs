using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask encounterObjectsLayer;

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
}
