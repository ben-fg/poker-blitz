using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public enum Denomination
    {
        Two = 0,
        Three = 1,
        Four = 2,
        Five = 3,
        Six = 4,
        Seven = 5,
        Eight = 6,
        Nine = 7,
        Ten = 8,
        Jack = 9,
        Queen = 10,
        King = 11,
        Ace = 12
    }

    public enum Suit
    {
        Hearts = 0,
        Diamonds = 1,
        Clubs = 2,
        Spades = 3
    }

    private Denomination denomination;
    private Suit suit;

    public Card(Denomination denomination, Suit suit)
    {
        this.denomination = denomination;
        this.suit = suit;
    }

    public Suit GetSuit()
    {
        return suit;
    }

    public void SetSuit(Suit suit)
    {
        this.suit = suit;
    }

    public Denomination GetDenomination()
    {
        return denomination;
    }

    public void SetDenomination(Denomination denomination)
    {
        this.denomination = denomination;
    }
}
