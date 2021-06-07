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
    public LayerMask encounterObjectsLayer;
    Animator Anim;

    public event Action OnEncounter;

    private void Awake()
    {
        Init();
    }

    public void HandleUpdate()
    {
        Inputs();
        Animations();
    }

    private void Init()
    {
        Anim = gameObject.GetComponentInChildren<Animator>();
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
                Anim.SetFloat("Horizontal", input.x);
                Anim.SetFloat("Vertical", input.y);

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
        Anim.SetBool("isMoving", isMoving);
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
        if (Physics2D.OverlapCircle(targetPos, 0.1f, solidObjectsLayer) != null)
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
                Anim.SetBool("isMoving", false);
                OnEncounter();
            }
        }
    }
}
