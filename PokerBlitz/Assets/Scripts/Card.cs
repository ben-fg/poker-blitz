using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    private int denomination;
    private int suit;

    public Card(int denomination, int suit)
    {
        this.denomination = denomination;
        this.suit = suit;
    }

    public int GetSuit()
    {
        return suit;
    }

    public void SetSuit(int suit)
    {
        this.suit = suit;
    }

    public int GetDenomination()
    {
        return denomination;
    }

    public void SetDenomination(int denomination)
    {
        this.denomination = denomination;
    }
}
