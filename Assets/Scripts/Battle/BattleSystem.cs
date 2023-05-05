using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//I need a variable to store the state of that dialog box (this was the step when I created the dialog box action selector to ppText.
public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}


public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    //[SerializeField] BattleHud playerHud;//moved to battleunit
    [SerializeField] BattleUnit enemyUnit;
    //[SerializeField] BattleHud enemyHud; //moved to battleunit
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;


    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    public event Action<bool> OnBattleOver;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.SetUp(playerParty.GetHealthyPokemon());
        enemyUnit.SetUp(wildPokemon);        

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return (dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name } appeared"));
        //after showing this text I will wait one second and then I will go to the player action state 
        //this wait was moved to the TypeDialog Method.
        

        ActionSelection();
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialogText("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        OnBattleOver(won);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        //I need to change the state so the player cant make another move.
        state = BattleState.PerformMove;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);


        //move.PP--;
        //yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}"); 

        //playerUnit.PlayAttackAnimation();
        //yield return new WaitForSeconds(1f);
        //enemyUnit.PlayHitAnimation();


        //var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);

        //Debug.Log("player damage = " + damageDetails.ToString());

        //yield return enemyHud.UpdateHP();
        //yield return ShowDamageDetails(damageDetails);
        //if (damageDetails.Fainted)
        //{
        //    yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted");
        //    enemyUnit.PlayFaintAnimation();

        //    yield return new WaitForSeconds(2f);
        //    OnBattleOver(true);
        //}        

        //if the battle state was not changed by RunMove, then go to next step.
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        var move = enemyUnit.Pokemon.GetRandomMove();

        yield return RunMove(enemyUnit, playerUnit, move);
        //move.PP--;
        //yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        //enemyUnit.PlayAttackAnimation();
        //yield return new WaitForSeconds(1f);
        //playerUnit.PlayHitAnimation();

        //var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);

        //Debug.Log("enemy damage = " + damageDetails.ToString());
        //yield return playerHud.UpdateHP();//this update in the UI the hp
        //yield return ShowDamageDetails(damageDetails);
        //if (damageDetails.Fainted)
        //{
        //    yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted");
        //    playerUnit.PlayFaintAnimation();
        //    yield return new WaitForSeconds(2f);

        //    var nextPokemon = playerParty.GetHealthyPokemon();

        //    if (nextPokemon != null)
        //    {
        //        OpenPartyScreen();
        //    }
        //    else
        //    {
        //        OnBattleOver(false);
        //    }            
        //}

        //if the battle state was not changed by RunMove, then go to next step.
        if(state == BattleState.PerformMove)
        {
            ActionSelection();
        }            
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();


        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);

        Debug.Log("player damage = " + damageDetails.ToString());

        yield return targetUnit.Hud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();

            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1)
        {
            yield return dialogBox.TypeDialog($"A Critical hit");
        }
        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog($"It's super effective");
        }
        else if(damageDetails.TypeEffectiveness < 1)
        {
            yield return dialogBox.TypeDialog($"It's not very effective");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
            
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }  
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Math.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if(currentAction == 1)
            {
                //Bag
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Math.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }

        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMove = Math.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }
            if(selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }

        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if(playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }               

        playerUnit.SetUp(newPokemon);        
        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return (dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!"));

        StartCoroutine(EnemyMove());
    }
}
