using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Unit> units;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Unit> units)
    {
        this.units = units;

        for (int i=0; i < memberSlots.Length; i++)
        {
            if (i < units.Count)
                memberSlots[i].SetData(units[i]);
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Choose who you want to switch.";
    }

    public void UpdateMemberSlection(int selectedMember)
    {
        for (int i=0; i<units.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
