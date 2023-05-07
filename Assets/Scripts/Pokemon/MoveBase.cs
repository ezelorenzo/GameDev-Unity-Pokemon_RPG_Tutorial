using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public PokemonType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int PP
    {
        get { return pp; }
    }

    //public bool IsSpecial //this was replaced by the use of category serialized field.
    //{
    //    get 
    //    {   if
    //         (type == PokemonType.Fire || 
    //         type == PokemonType.Water || 
    //         type == PokemonType.Grass || 
    //         type == PokemonType.Electric ||
    //         type == PokemonType.Ice ||
    //         type == PokemonType.Dragon)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }            
    //    }
    //}

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects
    {
        get { return effects; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class MoveEffects
{
    //[SerializeField] Dictionary <Stat, int> // unity cant serialize a Dictionary so we do the next
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}


public enum MoveCategory
{
    Physical, Special, Status
}

public enum MoveTarget
{
    Foe, Self
}