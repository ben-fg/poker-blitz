using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player 
{
    private Pocket pocket;
    private int balance = 800;
    private int winTally = 0;

    public Player(Pocket pocket)
    {
        this.pocket = pocket;
    }

    public Pocket GetPocket()
    {
        return pocket;
    }

    public void SetPocket(Pocket pocket)
    {
        this.pocket = pocket;
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
}
