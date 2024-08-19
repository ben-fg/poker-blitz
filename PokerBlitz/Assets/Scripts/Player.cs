using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player 
{
    private Pocket pocket;
    private int balance;
    private int winTally;
    private bool isFolded;
    private bool isChecked;
    private bool isRaised;
    private bool isBet;
    private static bool globalRaised = false;
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

    public void RaiseBet()
    {
        isRaised = true;
    }

    public bool IsRaised()
    {
        return isRaised;
    }

    public void Bet()
    {
        isBet = true;
    }

    public bool IsBet()
    {
        return isBet;
    }

    public void GlobalRaised()
    {
        globalRaised = true;
    }

    public bool IsGlobalRaied()
    {
        return globalRaised;
    }

    public void Reset()
    {
        isChecked = false;
        isRaised = false;
        isBet = false;
        globalRaised = false;
    }
}
