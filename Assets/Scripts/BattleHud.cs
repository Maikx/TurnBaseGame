using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HpBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Unit _unit;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Unit unit)
    {
        _unit = unit;

        nameText.text = unit.Base.Name;
        SetLevel();
        hpBar.SetHP((float)unit.HP / unit.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.psn, psnColor},
            { ConditionID.brn, brnColor},
            { ConditionID.slp, slpColor},
            { ConditionID.par, parColor},
            { ConditionID.frz, frzColor},
        };

        SetStatusText();
        _unit.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_unit.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _unit.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_unit.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lv " + _unit.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _unit.Base.GetExpForLevel(_unit.Level);
        int nextLevelExp = _unit.Base.GetExpForLevel(_unit.Level + 1);

        float normalizedExp = (float) (_unit.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public IEnumerator UpdateHP()
    {
        if(_unit.HpChanged)
        yield return hpBar.SetHPSmooth((float)_unit.HP / _unit.MaxHp);
        _unit.HpChanged = false;
    }
}
