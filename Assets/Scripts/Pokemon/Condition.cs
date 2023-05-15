using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition 
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public Action<Pokemon> OnStart { get; set; }
    public Func<Pokemon, bool> OnBeforeMove { get; set; }//used Func because Action only applies for non returning value methods.
    public Action<Pokemon> OnAfterTurn { get; set; }    
}
