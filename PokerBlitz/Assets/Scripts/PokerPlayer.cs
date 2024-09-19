using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PokerPlayer
{
    private Pocket pocket;
    private int playerNum;
    private int balance;
    private int winTally;
    private bool isFolded;
    private bool isChecked;
    private int raise;
    private int amountCalled = 0;
    private int previousCall = 0;
    private static int totalRaises = 0;
    private static List<int> previousRaises = new List<int>();
    private static int callCounter = 0;
    private static int checkCounter = 0;
    private static int foldCounter = 0;
    private static bool globalRaised = false;
    private static int pot = 0;
    private static int previousRaise = 0;

    public enum Position
    {
        SB, // Small Blind
        BB, // Big Blind
        UTG, // Under The Gun
        BTN // Button
    }

    private Position position;
    public PokerPlayer(Pocket pocket, Position position, int playerNum)
    {
        this.pocket = pocket;
        this.position = position;
        this.playerNum = playerNum;
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

    public void SetNum(int playerNum)
    {
        this.playerNum = playerNum;
    }

    public int GetNum()
    {
        return playerNum;
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
        foldCounter++;
    }

    public void Unfold()
    {
        isFolded = false;
    }

    public bool IsFolded()
    {
        return isFolded;
    }

    public void Check()
    {
        isChecked = true;
        checkCounter++;
    }

    public bool IsChecked()
    {
        return isChecked;
    }

    public void Raise(int raise)
    {
        this.raise = raise;
        if (raise <= balance)
        {
            pot += (raise + GetPreviousRaises());
            balance -= (raise + GetPreviousRaises());
            globalRaised = true;
            callCounter = 0;
            previousRaises.Add(raise);
            amountCalled += (raise + GetPreviousRaises());
            
            totalRaises += raise;
            previousRaise = raise;
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

    public int GetPreviousRaise()
    {
        return previousRaise;
    }

    public int GetPreviousRaises()
    {
        return totalRaises - previousCall;
    }

    public void Call()
    {
        if (totalRaises - amountCalled <= balance)
        {
            foreach (var currentRaise in previousRaises)
            {
                pot += currentRaise;
                balance -= currentRaise;
            }
            callCounter++;
            /*to subtract from the pot and add to balance any amount he has previously called,
                * so it wouldnt be added to the pot and subtracted from the balance twice*/
            pot -= amountCalled;
            balance += amountCalled;

            previousCall = amountCalled;
            amountCalled = totalRaises;
        }
        else
        {
            Debug.Log($" You only have {balance}. Get your bread up lil bro");
        }
    }

    public static void DecreaseCallCounter()
    {
        callCounter--;
    }

    public static void IncreaseCallCounter()
    {
        callCounter++;
    }

    public static int GetCallCounter()
    {
        return callCounter;
    }

    public static int GetFoldCounter()
    {
        return foldCounter;
    }

    public static int GetCheckCounter()
    {
        return checkCounter;
    }

    public static int GetNumRaises()
    {
        return previousRaises.Count; 
    }



    public static void ResetGlobals()
    {
        globalRaised = false;
        previousRaises.Clear();
        totalRaises = 0;
        callCounter = 0;
        checkCounter = 0;
        foldCounter = 0;
    }

    public static bool IsGlobalRaised()
    {
        return globalRaised;
    }

    public static void SetPot(int pot)
    {
        PokerPlayer.pot = pot;
    }

    public static int GetPot()
    {
        return pot;
    }

    public void Reset()
    {
        isChecked = false;
        raise = 0;
        amountCalled = 0;
        previousCall = 0;
    }
}
