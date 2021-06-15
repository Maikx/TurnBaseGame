using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterObjects : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            GameController.self.StartBattle();
        }
    }
}
