using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] Dialogue dialogue;
    public void Interact()
    {
       StartCoroutine(DialogueManager.self.ShowDialogue(dialogue));
    }
}
