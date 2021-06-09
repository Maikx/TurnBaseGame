using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class Condition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public Action<Unit> OnStart { get; set; }
    public Func<Unit, bool> OnBeforeMove { get; set; }
    public Action<Unit> OnAfterTurn { get; set; }
}
