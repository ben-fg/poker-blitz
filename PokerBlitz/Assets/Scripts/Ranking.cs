using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranking
{
    private Card[] fiveCardHand = new Card[5];
    public enum CardRank
    {
        HighCard,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush,
    }
    private CardRank currentRank;
    
    public Ranking(CardRank currentRank, Card[] fiveCardHand)
    {
        this.currentRank = currentRank;
        this.fiveCardHand = fiveCardHand;
    }
    //GETTERS AND SETTERS ETC.....
}
