using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public enum Denomination
    {
        Two,
        Three,
        Four,
        Five,
        Six ,
        Seve,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public enum Suit
    {
        Spades,
        Diamonds,
        Clubs,
        Hearts
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
