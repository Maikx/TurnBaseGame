using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private Vector2 input;
    private Character character;

    public event Action OnEncounter;

    private void Awake()
    {
        Init();
    }

    public void HandleUpdate()
    {
        Inputs();
        Interact();
        character.HandleUpdate();
    }

    private void Init()
    {
        character = GetComponent<Character>();
    }

    private void Inputs()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //remove diagonal movement
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine (character.Move(input, CheckForEncounters));
            }
        }
    }

    void Interact()
    {
        if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            var facingDir = new Vector3(character.Animator.Horizontal, character.Animator.Vertical);
            var interactPos = transform.position + facingDir;

            var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.self.InterctableLayer);
            if(collider != null)
            {
                collider.GetComponent<Interactable>()?.Interact();
            }
        }
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.1f, GameLayers.self.EncounterLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                OnEncounter();
            }
        }
    }
}
