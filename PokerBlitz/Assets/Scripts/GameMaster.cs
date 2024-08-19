using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Card;
using static Player;

public class GameMaster : MonoBehaviour
{
    private bool[,] deck = new bool[13, 4];
    private List<Player> players = new List<Player>();
    private enum Ranking
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
    /*
    For card denominations:
    (I'm sorry in advance but there's no way around this)
    0 = 2
    1 = 3
    2 = 4
    3 = 5
    4 = 6
    5 = 7
    6 = 8
    7 = 9
    8 = 10
    9 = Jack
    10 = Queen
    11 = King
    12 = Ace

    For card suits:
    0 = Spades
    1 = Diamonds
    2 = Clubs
    3 = Hearts

    E.g. King of hearts == [11,3]
    */

    // Generates a random card denomination (0-12) and suit (0-3)
    /* private void GenerateRandomCard(out int denomination, out int suit)
     {
         denomination = UnityEngine.Random.Range(0, 13);
         suit = UnityEngine.Random.Range(0, 4);
     }*/

    // Generates a unique card
    private Card GenerateUniqueCard()
    {
        int denomination;
        int suit;
        

        // Keep generating random cards until a unique one is found or the max attempts is reached
        do
        {
            denomination = UnityEngine.Random.Range(0, 13);
            suit = UnityEngine.Random.Range(0, 4);

        }
        while (deck[denomination, suit]);

        // Mark the card as used in the deck
        deck[denomination, suit] = true;

        Denomination cardDenomination = (Denomination)denomination;
        Suit cardSuit = (Suit)suit;
        Card uniqueCard = new Card(cardDenomination, cardSuit);

        Debug.Log($"Generated unique card: Denomination {cardDenomination}, Suit {cardSuit}");

        return uniqueCard;
    }

    //Start is called before the first frame update
    void Start()
    {
        Player firstPlayer = new (new(GenerateUniqueCard(), GenerateUniqueCard()), Position.BTN);
        Player secondPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), Position.SB);

        players.Add(firstPlayer);
        players.Add(secondPlayer);

        Debug.Log(firstPlayer.GetPocket().ToString());
        Debug.Log(secondPlayer.GetPocket().ToString());

    }
    //Update is called once per frame
    void Update()
    {
       
    }

    private Ranking DetermineRank(Pocket pocket, Board board)
    {
        //Make sure to take into account the street of the board when retrieving the cards
        return Ranking.HighCard;
    }
}
