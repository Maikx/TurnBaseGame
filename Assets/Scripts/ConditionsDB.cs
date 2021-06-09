using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Unit unit) =>
                {
                    unit.UpdateHP(unit.MaxHp / 8);
                    unit.StatusChanges.Enqueue($"{unit.Base.Name} hurt itself due to poison");
                }
            }
        },

        {ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Unit unit) =>
                {
                    unit.UpdateHP(unit.MaxHp / 16);
                    unit.StatusChanges.Enqueue($"{unit.Base.Name} hurt itself due to fire");
                }
            }
        },

        {ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Unit unit) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        unit.StatusChanges.Enqueue($"{unit.Base.Name}'s paralyzed");
                        return false;
                    }

                    return true;
                }
            }
        },

        {ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "is frozen solid",
                OnBeforeMove = (Unit unit) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        unit.CureStatus();
                        unit.StatusChanges.Enqueue($"{unit.Base.Name}'s not frozen anymore");
                        return true;
                    }

                    return false;
                }
            }
        },

        {ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Unit unit) =>
                {
                    //Sleep for 1-3 turns
                    unit.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {unit.StatusTime} moves");
                },
                OnBeforeMove = (Unit unit) =>
                {
                    if(unit.StatusTime <= 0)
                    {
                        unit.CureStatus();
                        unit.StatusChanges.Enqueue($"{unit.Base.Name} woke up!");
                        return true;
                    }

                    unit.StatusTime--;
                    unit.StatusChanges.Enqueue($"{unit.Base.Name} is sleeping");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
