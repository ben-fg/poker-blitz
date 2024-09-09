using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player 
{
    private Pocket pocket;
    private int balance;
    private int winTally;
    private bool isFolded;
    private bool isChecked;
    private int raise;
    private int bet;
    private static int previousRaise = 0;
    private static int callCounter = 0;
    private static bool globalRaised = false;
    private static int pot = 0;
    public enum Position
    {
        BTN, // Button
        SB, // Small Blind
        BB, // Big Blind
        UTG, // Under The Gun
    }

    private Position position;
    public Player(Pocket pocket, Position position)
    {
        this.pocket = pocket;
        this.position = position;
        balance = 1000;
        winTally = 0;
    }

    public Pocket GetPocket()
    {
        return pocket;
    }

    public void SetPocket(Pocket pocket)
    {
        this.pocket = pocket;
    }

    public Position GetPosition()
    {
        return position;
    }

    public void SetPosition(Position position)
    {
        this.position = position;
    }

    public int GetBalance()
    {
        return balance;
    }

    public void SetBalance(int balance)
    {
        this.balance = balance;
    }

    public int GetWinTally()
    {
        return winTally;
    }

    public void SetWinTally(int winTally)
    {
        this.winTally = winTally;
    }

    public void Fold()
    {
        isFolded = true;
    }

    public bool IsFolded()
    {
        return isFolded;
    }

    public void Check()
    {
        isChecked = true;
    }

    public bool IsChecked()
    {
        return isChecked;
    }

    public void Blinds(int raise)
    {
        this.raise = raise;
        if (raise <= balance)
        {
            pot += raise;
            balance -= raise;
            callCounter = 0;
            previousRaise = raise;

            Debug.Log($" blind by {raise}. Current balance: {balance}Current pot: {pot}");
        }
        else
        {
            Debug.Log($" You only have {balance}. Get your bread up lil bro");
        }
    }

    public void Raise(int raise)
    {
        this.raise = raise;
        if (raise <= balance)
        {
            pot += raise;
            balance -= raise;
            globalRaised = true;
            callCounter = 0;
            previousRaise = raise;

            Debug.Log($" raised by {raise}. Current balance: {balance}Current pot: {pot}");
        }
        else
        {
            Debug.Log($" You only have {balance}. Get your bread up lil bro");
        }

    }

    public int AmountRaised()
    {
        return raise;
    }

    public static int GetPreviousRaise()
    {
        return previousRaise;
    }

    public void Call()
    {
        if (previousRaise <= balance)
        {
            pot += previousRaise;
            balance -= previousRaise;
            callCounter++;
     
            Debug.Log($" Matched {previousRaise}. Current balance: {balance}Current pot: {pot}");
        }
        else
        {
            Debug.Log($" You only have {balance}. Get your bread up lil bro");
        }
    }

    public static int GetCallCounter()
    {
        return callCounter;
    }

    public void Bet(int bet)
    {
        this.bet = bet; 
    }

    public int AmountBet()
    {
        return bet;
    }

    public static void GlobalRaised()
    {
        globalRaised = false;
    }

    public static bool IsGlobalRaised()
    {
        return globalRaised;
    }

    public static void SetPot(int pot)
    {
        Player.pot = pot;
    }

    public static int GetPot()
    {
        return pot;
    }

    public void Reset()
    {
        isChecked = false;
        raise = 0;
        bet = 0;
    }
}
