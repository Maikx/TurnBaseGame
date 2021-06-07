using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogueBox dialogueBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    UnitParty playerParty;
    Unit wildUnit;

    public void StartBattle(UnitParty playerParty, Unit wildUnit)
    {
        this.playerParty = playerParty;
        this.wildUnit = wildUnit;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyUnit());
        enemyUnit.Setup(wildUnit);
        playerHud.SetData(playerUnit.Unit);
        enemyHud.SetData(enemyUnit.Unit);

        dialogueBox.SetMoveNames(playerUnit.Unit.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Unit.Base.Name} has appeared.");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogueBox.TypeDialogue("What should i do?"));
        dialogueBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnabledMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Unit.Moves[currentMove];
        move.PP--;
        yield return dialogueBox.TypeDialogue($"{playerUnit.Unit.Base.Name} used {move.Base.Name}");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        enemyUnit.PlayHitAnimation();
        
        var damageDetails = enemyUnit.Unit.TakeDamage(move, playerUnit.Unit);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Unit.Base.Name} Fainted");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Unit.GetRandomMove();
        move.PP--;
        yield return dialogueBox.TypeDialogue($"{enemyUnit.Unit.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Unit.TakeDamage(move, enemyUnit.Unit);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogueBox.TypeDialogue($"{playerUnit.Unit.Base.Name} Fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogueBox.TypeDialogue("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogueBox.TypeDialogue("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogueBox.TypeDialogue("It's not very effective!");
    }

    public void HandleUpdate()
    {
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }

        void HandleActionSelection()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (currentAction < 1)
                    ++currentAction;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (currentAction > 0)
                    --currentAction;
            }

            dialogueBox.UpdateActionSelection(currentAction);

            if(Input.GetKeyDown(KeyCode.Z))
            {
                if(currentAction == 0)
                {
                    //Fight
                    PlayerMove();
                }
                else if(currentAction == 1)
                {
                    //Run
                }
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Unit.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }

        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Unit.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Unit.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnabledMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
