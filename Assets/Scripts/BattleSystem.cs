using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver }
public enum BattleAction { Move, SwitchUnit, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    UnitParty playerParty;
    UnitParty trainerParty;
    Unit wildUnit;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    public void StartBattle(UnitParty playerParty, Unit wildUnit)
    {
        this.playerParty = playerParty;
        this.wildUnit = wildUnit;
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(UnitParty playerParty, UnitParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            //Wild Unit Battle
            playerUnit.Setup(playerParty.GetHealthyUnit());
            enemyUnit.Setup(wildUnit);

            dialogueBox.SetMoveNames(playerUnit.Unit.Moves);

            yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Unit.Base.Name} has appeared.");
        }
        else
        {
            //Trainer Battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogueBox.TypeDialogue($"{trainer.Name} is forcing you to battle");

            //Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var unitEnemy = trainerParty.GetHealthyUnit();
            enemyUnit.Setup(unitEnemy);
            yield return dialogueBox.TypeDialogue($"{trainer.Name} send out {unitEnemy.Base.Name}");


            //Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var unitPlayer = playerParty.GetHealthyUnit();
            playerUnit.Setup(unitPlayer);
            yield return dialogueBox.TypeDialogue($"Go {unitPlayer.Base.Name}!");
            dialogueBox.SetMoveNames(playerUnit.Unit.Moves);

        }

        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Units.ForEach(u => u.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogueBox.SetDialogue("What should i do?");
        dialogueBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Units);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnabledMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Unit.CurrentMove = playerUnit.Unit.Moves[currentMove];
            enemyUnit.Unit.CurrentMove = enemyUnit.Unit.GetRandomMove();

            int playerMovePriority = playerUnit.Unit.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Unit.CurrentMove.Base.Priority;

            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Unit.Speed >= enemyUnit.Unit.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondunit = secondUnit.Unit;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Unit.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondunit.HP > 0)
            {
                //Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Unit.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }

            if(state != BattleState.BattleOver)
            {
                ActionSelection();
            }
        }
        else
        {
            if(playerAction == BattleAction.SwitchUnit)
            {
                var selectedUnit = playerParty.Units[currentMember];
                state = BattleState.Busy;
                yield return SwitchUnit(selectedUnit);
            }

            //Enemy Turn
            var enemyMove = enemyUnit.Unit.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Unit.OnBeforeMove();
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Unit);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Unit);

        move.PP--;
        yield return dialogueBox.TypeDialogue($"{sourceUnit.Unit.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Unit, targetUnit.Unit))
        {

            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Unit, targetUnit.Unit, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Unit.TakeDamage(move, sourceUnit.Unit);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Unit.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Unit, targetUnit.Unit, secondary.Target);
                }
            }

            if (targetUnit.Unit.HP <= 0)
            {
                yield return dialogueBox.TypeDialogue($"{targetUnit.Unit.Base.Name} Fainted");
                targetUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);

            }

        }
        else
        {
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Unit.Base.Name}'s attack missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Unit source, Unit target, MoveTarget moveTarget)
    {
        //Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //Status Condition
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Statuses like burn or psn will hurt the pokemon after the turn
        sourceUnit.Unit.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Unit);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Unit.HP <= 0)
        {
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Unit.Base.Name} Fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    bool CheckIfMoveHits(Move move, Unit source, Unit target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];



        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerable ShowStatusChanges(Unit unit)
    {
        while (unit.StatusChanges.Count > 0)
        {
            var message = unit.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextUnit = playerParty.GetHealthyUnit();
            if (nextUnit != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {   if (!isTrainerBattle)
                BattleOver(true);
            else
            {
                var _nextUnit = trainerParty.GetHealthyUnit();
                if(_nextUnit != null)
                {
                    StartCoroutine(SendNextTrainerUnit(_nextUnit));
                }
                else
                {
                    BattleOver(true);
                }
            }
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
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
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                ++currentAction;
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                --currentAction;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                currentAction += 2;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                currentAction -= 2;

            currentAction = Mathf.Clamp(currentAction, 0, 3);

            dialogueBox.UpdateActionSelection(currentAction);

            if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
                if(currentAction == 0)
                {
                    //Fight
                    MoveSelection();
                }
                else if (currentAction == 1)
                {
                    //Party
                    prevState = state;
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
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Unit.Moves.Count - 1);

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Unit.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            var move = playerUnit.Unit.Moves[currentMove];
            if (move.PP == 0) return;

            dialogueBox.EnabledMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if(Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogueBox.EnabledMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Units.Count - 1);

        partyScreen.UpdateMemberSlection(currentMember);

        if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
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

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchUnit));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchUnit(selectedMember));
            }

            state = BattleState.Busy;
            StartCoroutine(SwitchUnit(selectedMember));
        }
        else if(Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
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
        dialogueBox.SetMoveNames(newUnit.Moves);
        yield return dialogueBox.TypeDialogue($"You are all of us {newUnit.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerUnit(Unit _nextUnit)
    {
        state = BattleState.Busy;

        enemyUnit.Setup(_nextUnit);
        yield return dialogueBox.TypeDialogue($"{trainer.Name} send out {_nextUnit.Base.Name}!");

        state = BattleState.RunningTurn;      
    }
}



