using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;


public class GameMaster : MonoBehaviour
{
    private bool[,] deck = new bool[13, 4];
    private List<Player> players = new List<Player>();
    private int globalMatched;
    [SerializeField] private InputField testBox;
    private int activePlayerIndex = 1;
    private bool round = false;

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

        Card.Denomination cardDenomination = (Card.Denomination)denomination;
        Card.Suit cardSuit = (Card.Suit)suit;
        Card uniqueCard = new Card(cardDenomination, cardSuit);

        Debug.Log($"Generated unique card: Denomination {cardDenomination}, Suit {cardSuit}");

        return uniqueCard;
    }

    //Start is called before the first frame update
    void Start()
    {
        Player firstPlayer = new (new(GenerateUniqueCard(), GenerateUniqueCard()), Player.Position.BTN);
        Player secondPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), Player.Position.SB);
        Player thirdPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), Player.Position.BB);
        Player fourthPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), Player.Position.UTG);

        players.Add(firstPlayer);
        players.Add(secondPlayer);
        players.Add(thirdPlayer);
        players.Add(fourthPlayer);

        Debug.Log(firstPlayer.GetPocket().ToString());
        Debug.Log(secondPlayer.GetPocket().ToString());
        Debug.Log(thirdPlayer.GetPocket().ToString());
        Debug.Log(fourthPlayer.GetPocket().ToString());

    }

    //Update is called once per frame
    void Update()
    {
        Player currentPlayer = players[activePlayerIndex];
        if (!round)
        {
            do
            {
                currentPlayer.Raise(25);
                activePlayerIndex++;
                currentPlayer = players[activePlayerIndex];
                currentPlayer.Raise(50);
                Debug.Log(" blinds played");

            }
            while (activePlayerIndex != 2);

            activePlayerIndex = 3;
            round = true;
        }
        

        if (Player.IsGlobalRaised())
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentPlayer.Raise(Int32.Parse(testBox.text));

            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                currentPlayer.Call();
                Debug.Log($"player {activePlayerIndex+1}");
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                currentPlayer.Fold();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentPlayer.Raise(Int32.Parse(testBox.text));

            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                currentPlayer.Check();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                currentPlayer.Fold();
            }
        }
        
        if (Player.GetCallCounter() == players.Count)
        {
            Player.GlobalRaised();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex+1}");
        }

    }

    private Ranking DetermineRank(Pocket pocket, Board board)
    {
        //Make sure to take into account the street of the board when retrieving the cards
        return Ranking.HighCard;
    }
}
