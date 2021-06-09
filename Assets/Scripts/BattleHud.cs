using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpBar;

    Unit _unit;

    public void SetData(Unit unit)
    {
        _unit = unit;

        nameText.text = unit.Base.Name;
        levelText.text = "Lv " + unit.Level;
        hpBar.SetHP((float)unit.HP / unit.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        if(_unit.HpChanged)
        yield return hpBar.SetHPSmooth((float)_unit.HP / _unit.MaxHp);
        _unit.HpChanged = false;
    }
}
