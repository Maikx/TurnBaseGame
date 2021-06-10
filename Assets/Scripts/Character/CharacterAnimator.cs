using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> idleDownSprites;
    [SerializeField] List<Sprite> idleUpSprites;
    [SerializeField] List<Sprite> idleRightSprites;
    [SerializeField] List<Sprite> idleLeftSprites;
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;

    //Parameters
    public float Horizontal { get; set; }
    public float Vertical { get; set; }
    public bool IsMoving { get; set; }

    //States
        //Idle
        SpriteAnimator idleDownAnim;
        SpriteAnimator idleUpAnim;
        SpriteAnimator idleRightAnim;
        SpriteAnimator idleLeftAnim;
        
        //Walk
        SpriteAnimator walkDownAnim;
        SpriteAnimator walkUpAnim;
        SpriteAnimator walkRightAnim;
        SpriteAnimator walkLeftAnim;

    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;

    //References
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        idleDownAnim = new SpriteAnimator(idleDownSprites, spriteRenderer);
        idleUpAnim = new SpriteAnimator(idleUpSprites, spriteRenderer);
        idleRightAnim = new SpriteAnimator(idleRightSprites, spriteRenderer);
        idleLeftAnim = new SpriteAnimator(idleLeftSprites, spriteRenderer);
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        currentAnim = idleDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (IsMoving)
        {
            if (Horizontal == 1)
                currentAnim = walkRightAnim;
            else if (Horizontal == -1)
                currentAnim = walkLeftAnim;
            else if (Vertical == 1)
                currentAnim = walkUpAnim;
            else if (Vertical == -1)
                currentAnim = walkDownAnim;
        }
        else
        {
            if (Horizontal == 1)
                currentAnim = idleRightAnim;
            else if (Horizontal == -1)
                currentAnim = idleLeftAnim;
            else if (Vertical == 1)
                currentAnim = idleUpAnim;
            else if (Vertical == -1)
                currentAnim = idleDownAnim;
        }

        if (currentAnim != prevAnim || IsMoving != wasPreviouslyMoving)
            currentAnim.Start();

            currentAnim.HandleUpdate();

        wasPreviouslyMoving = IsMoving;
    }
}
