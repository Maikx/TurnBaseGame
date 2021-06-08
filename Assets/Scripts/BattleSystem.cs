using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

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

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Unit.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Unit.Base.Name} has appeared.");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogueBox.SetDialogue("What should i do?");
        dialogueBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Units);
        partyScreen.gameObject.SetActive(true);
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

            var nextUnit = playerParty.GetHealthyUnit();
            if (nextUnit != null)
            {
                OpenPartyScreen();
            }
            else
            {
                OnBattleOver(true);
            }
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
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }


        void HandleActionSelection()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                ++currentAction;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                --currentAction;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                currentAction += 2;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                currentAction -= 2;

            currentAction = Mathf.Clamp(currentAction, 0, 3);

            dialogueBox.UpdateActionSelection(currentAction);

            if(Input.GetKeyDown(KeyCode.Z))
            {
                if(currentAction == 0)
                {
                    //Fight
                    PlayerMove();
                }
                else if (currentAction == 1)
                {
                    //Party
                    OpenPartyScreen();
                }
                else if(currentAction == 2)
                {
                    //Bag
                }
                else if (currentAction == 3)
                {
                    //Run
                }
            }
        }
    

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Unit.Moves.Count - 1);

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Unit.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnabledMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogueBox.EnabledMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            PlayerAction();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Units.Count - 1);

        partyScreen.UpdateMemberSlection(currentMember);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Units[currentMember];
            if(selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("It cannot fight right now");
                return;
            }
            if(selectedMember == playerUnit.Unit)
            {
                partyScreen.SetMessageText("I'm already fighting dumbass!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchUnit(selectedMember));
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    IEnumerator SwitchUnit(Unit newUnit)
    {
        if (playerUnit.Unit.HP > 0)
        {
            yield return dialogueBox.TypeDialogue($"Alright, take a short rest {playerUnit.Unit.Base.Name}");
            playerUnit.PlaySwitchAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newUnit);
        playerHud.SetData(newUnit);
        dialogueBox.SetMoveNames(newUnit.Moves);
        yield return dialogueBox.TypeDialogue($"You are all of us {newUnit.Base.Name}!");

        StartCoroutine(EnemyMove());
    }
}



