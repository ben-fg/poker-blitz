using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public enum Denomination
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public enum Suit
    {
        Spades = 1,
        Diamonds = 2,
        Clubs = 3,
        Hearts = 4
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
