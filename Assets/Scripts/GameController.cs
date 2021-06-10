using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialogue}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Awake()
    {
        ConditionsDB.Init();
    }

    private void Start()
    {
        playerController.OnEncounter += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        DialogueManager.self.OnShowDialogue += () =>
        {
            state = GameState.Dialogue;
        };

        DialogueManager.self.OnCloseDialogue += () =>
        {
            if(state == GameState.Dialogue)
                state = GameState.FreeRoam;
        };
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<UnitParty>();
        var wildUnit = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildUnit();

        battleSystem.StartBattle(playerParty, wildUnit);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if(state == GameState.Dialogue)
        {
            DialogueManager.self.HandleUpdate();
        }
    }
}
