using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitParty : MonoBehaviour
{
    [SerializeField] List<Unit> units;

    private void Start()
    {
        foreach (var unit in units)
        {
            unit.Init();
        }
    }

    public Unit GetHealthyUnit()
    {
        return units.Where(x => x.HP > 0).FirstOrDefault();
    }

}
