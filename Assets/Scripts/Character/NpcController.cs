using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] Dialogue dialogue;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;
    [SerializeField] Sprite characterPortrait;

    NpcState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        if (state == NpcState.Idle)
        {
            state = NpcState.Dialogue;
            character.LookTowards(initiator.position);

            StartCoroutine(DialogueManager.self.ShowDialogue(dialogue, characterPortrait, () => {
                idleTimer = 0f;
                state = NpcState.Idle;
            }));   
        }
    }

    private void Update()
    {
        if(state == NpcState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if(movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NpcState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if(transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NpcState.Idle;
    }

    public enum NpcState { Idle, Walking, Dialogue }
}
