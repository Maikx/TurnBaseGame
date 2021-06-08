using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpBar;
    [SerializeField] Color highlightedColor;

    Unit _unit;

    public void SetData(Unit unit)
    {
        _unit = unit;

        nameText.text = unit.Base.Name;
        levelText.text = "Lv " + unit.Level;
        hpBar.SetHP((float)unit.HP / unit.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }    
}
