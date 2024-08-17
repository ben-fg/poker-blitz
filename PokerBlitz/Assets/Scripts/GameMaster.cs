using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    private bool[,] deck = new bool[13, 4];
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
    private void GenerateRandomCard(out int denomination, out int suit)
    {
        denomination = UnityEngine.Random.Range(0, 13);
        suit = UnityEngine.Random.Range(0, 4);
    }

    // Generates a unique card
    private Card GenerateUniqueCard()
    {
        int denomination;
        int suit;
        int attempts = 0;
        const int maxAttempts = 52;

        // Keep generating random cards until a unique one is found or the max attempts is reached
        do
        {
            GenerateRandomCard(out denomination, out suit);
            attempts++;
        }
        while (deck[denomination, suit] && attempts < maxAttempts);

        // If maxAttempts is reached, log a warning indicating that all cards have been generated
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("All cards have been generated.");
            
        }

        // Mark the card as used in the deck
        deck[denomination, suit] = true;

        Card uniqueCard = new Card(denomination, suit);

        Debug.Log($"Generated unique card: Denomination {denomination}, Suit {suit}");

        return uniqueCard;
    }
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

    //Start is called before the first frame update
    void Start()
    {
        //For now, simply deal to one player (pocket) and the board
        Pocket firstPocket = new Pocket (GenerateUniqueCard(), GenerateUniqueCard());
       
        Debug.Log("Code is running!");
        Debug.Log(firstPocket.ToString());
        
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
