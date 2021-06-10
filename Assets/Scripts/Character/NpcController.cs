using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] Dialogue dialogue;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NpcState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact()
    {
        if(state == NpcState.Idle)
            StartCoroutine(DialogueManager.self.ShowDialogue(dialogue));
    }

    private void Update()
    {
        if (DialogueManager.self.IsShowing) return;

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

        yield return character.Move(movementPattern[currentPattern]);
        currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NpcState.Idle;
    }

    public enum NpcState { Idle, Walking }
}
