using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    private bool isMoving;
    private Vector2 input;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public LayerMask encounterObjectsLayer;
    Animator animator;

    public event Action OnEncounter;

    private void Awake()
    {
        Init();
    }

    public void HandleUpdate()
    {
        Inputs();
        Animations();
        Interact();
    }

    private void Init()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    private void Inputs()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("Horizontal", input.x);
                animator.SetFloat("Vertical", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if(isWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
    }

    private void Animations()
    {
        animator.SetBool("isMoving", isMoving);
    }

    void Interact()
    {
        if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            var facingDir = new Vector3(animator.GetFloat("Horizontal"), animator.GetFloat("Vertical"));
            var interactPos = transform.position + facingDir;

            var collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
            if(collider != null)
            {
                collider.GetComponent<Interactable>()?.Interact();
            }
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        CheckForEncounters();
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }

        return true;
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.1f, encounterObjectsLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncounter();
            }
        }
    }
}
