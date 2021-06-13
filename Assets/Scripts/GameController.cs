using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialogue, Cutscene }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    NpcController npc;

    GameState state;

    public static GameController self { get; private set; }

    private void Awake()
    {
        self = this;
        ConditionsDB.Init();
    }

    private void Start()
    {
        playerController.OnEncounter += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        playerController.OnEnterTrainersView += (Collider2D npcCollider) =>
        {
            var npc = npcCollider.GetComponentInParent<NpcController>();
            if(npc != null)
            {
                if (npc.canAttackPlayer)
                {
                    state = GameState.Cutscene;
                    StartCoroutine(npc.TriggerNpcBattle(playerController));
                }
            }
        };

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

   

    public void StartTrainerBattle(NpcController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        this.npc = trainer;

        var playerParty = playerController.GetComponent<UnitParty>();
        var trainerParty = trainer.GetComponent<UnitParty>();

        battleSystem.StartNpcBattle(playerParty, trainerParty);
    }

    void EndBattle(bool won)
    {
        if(npc != null && won == true)
        {
            npc.BattleLost();
            npc = null;
        }

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
