using System.Collections;
using System.Collections.Generic;

public class Pocket
{
    private Card card1;
    private Card card2;
    private Card[] pocketCards = new Card[2];
    

    public Pocket(Card card1, Card card2)
    {
        this.card1 = card1;
        this.card2 = card2;
        pocketCards[0] = card1;
        pocketCards[1] = card2;
    }

    public Card[] GetCards()
    {
        return pocketCards;
    }


    public override string ToString()
    {
        string result = "";

        for (int i = 0; i < pocketCards.Length; i++)
        {
            Card.Suit suit = pocketCards[i].GetSuit();
            Card.Denomination denomination = pocketCards[i].GetDenomination();

            result += $"{denomination} of {suit}";

            if (i < pocketCards.Length - 1)
            {
                result += ", ";
            }
        }

        return result;

    }
}
