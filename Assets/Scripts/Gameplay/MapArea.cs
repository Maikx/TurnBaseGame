using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Unit> wildUnits;

    public Unit GetRandomWildUnit()
    {
        var wildUnit = wildUnits[Random.Range(0, wildUnits.Count)];
        wildUnit.Init();
        return wildUnit;
    }
}
