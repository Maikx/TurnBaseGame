using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Sprite portrait;
    [SerializeField] bool willFight;
    [SerializeField] bool willJoinParty;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue dialogueAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    [SerializeField] NpcState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    bool battleLost = false;
    bool sawPlayer;

    Character character;
    GameController gameController;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        if (!willFight)
            fov.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (state == NpcState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }

        character.HandleUpdate();
        SetFovRotation(character.Animator.CurrentDirection);
    }

    IEnumerator Walk()
    {
        
        state = NpcState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        if (sawPlayer)
            state = NpcState.Fight;
        else
            state = NpcState.Idle;
    }

    public IEnumerator TriggerNpcBattle(PlayerController player)
    {
        sawPlayer = true;
        //Show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //Walk towards the player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Show dialogue
        StartCoroutine(DialogueManager.self.ShowDialogue(dialogue, portrait, () =>
        {
            GameController.self.StartTrainerBattle(this);
        }));
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if(dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (state == NpcState.Idle && willFight && !battleLost)
        {
            state = NpcState.Dialogue;
            character.LookTowards(initiator.position);

            StartCoroutine(DialogueManager.self.ShowDialogue(dialogue, portrait, () =>
            {
                GameController.self.StartTrainerBattle(this);
            }));
        }
        else if (state == NpcState.Idle && willFight && battleLost)
        {
            state = NpcState.Dialogue;
            character.LookTowards(initiator.position);

            StartCoroutine(DialogueManager.self.ShowDialogue(dialogueAfterBattle, portrait, () => {
                idleTimer = 0f;
                state = NpcState.Idle;
            }));
        }
        else if (state == NpcState.Idle && !willFight && !battleLost)
        {
            state = NpcState.Dialogue;
            character.LookTowards(initiator.position);

            StartCoroutine(DialogueManager.self.ShowDialogue(dialogue, portrait, () => {
                idleTimer = 0f;
                state = NpcState.Idle;
            }));
        }
    }

    public void BattleLost()
    {
        sawPlayer = false;
        battleLost = true;
        idleTimer = 0f;
        state = NpcState.Idle;
        fov.gameObject.SetActive(false);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public bool JoinsParty
    {
        get => willJoinParty;
    }
}

public enum NpcState { Idle, Walking, Dialogue, Fight }
