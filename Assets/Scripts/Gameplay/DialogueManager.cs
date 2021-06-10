using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] Text dialogueText;
    [SerializeField] Image dialogueImage;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    public static DialogueManager self { get; private set; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        self = this;
    }

    Dialogue dialogue;
    Sprite characterImageDialogue;
    int currentLine = 0;
    bool isTyping;

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogue(Dialogue dialogue, Sprite characterImageDialogue)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();

        IsShowing = true;
        this.dialogue = dialogue;
        dialogueBox.SetActive(true);
        dialogueImage.sprite = characterImageDialogue;
        StartCoroutine(TypeDialogue(dialogue.Lines[0]));
    }

    public void HandleUpdate()
    {
        if( Input.GetKeyDown(KeyCode.Z) && !isTyping || Input.GetKeyDown(KeyCode.Return) && !isTyping)
        {
            ++currentLine;
            if(currentLine < dialogue.Lines.Count)
            {
                StartCoroutine(TypeDialogue(dialogue.Lines[currentLine]));
            }
            else
            {
                currentLine = 0;
                IsShowing = false;
                dialogueBox.SetActive(false);
                OnCloseDialogue?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }
}
