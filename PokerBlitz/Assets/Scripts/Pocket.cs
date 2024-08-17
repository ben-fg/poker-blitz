using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Pocket
{
    private Card card1;
    private Card card2;
    private Card[] pocketCards = new Card[2];
    private bool folded;

    public Pocket(Card card1, Card card2)
    {
        this.card1 = card1;
        this.card2 = card2;
        pocketCards[0] = card1;
        pocketCards[1] = card2;
        folded = false;
    }

    public Card[] GetCards()
    {
        return pocketCards;
    }

    public void FoldCards()
    {
        folded = true;
    }

    public bool GetFolded()
    {
        return folded;
    }

    public override string ToString()
    {
        string[] denominations = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };
        string[] suits = { "Spades", "Diamonds", "Clubs", "Hearts" };

        string result = "";

        for (int i = 0; i < pocketCards.Length; i++)
        {
            int suit = pocketCards[i].GetSuit();
            int denomination = pocketCards[i].GetDenomination();

            result += $"{denominations[denomination]} of {suits[suit]}";

            if (i < pocketCards.Length - 1)
            {
                result += ", ";
            }
        }

        return result;

    }
}
