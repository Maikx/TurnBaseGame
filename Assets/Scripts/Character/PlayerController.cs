using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;
    private Character character;

    public event Action OnEncounter;
    public event Action<Collider2D> OnEnterTrainersView;

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
                StartCoroutine (character.Move(input, OnMoveOver));
            }
        }
    }

    void Interact()
    {
        if(Input.GetKeyDown(KeyCode.Z) && !character.Animator.IsMoving || Input.GetKeyDown(KeyCode.Return) && !character.Animator.IsMoving)
        {
            var facingDir = new Vector3(character.Animator.Horizontal, character.Animator.Vertical);
            var interactPos = transform.position + facingDir;

            var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.self.InterctableLayer);
            if(collider != null)
            {
                collider.GetComponent<Interactable>()?.Interact(transform);
            }
        }
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckIfInTrainersView();
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

    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.1f, GameLayers.self.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);
        }
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
