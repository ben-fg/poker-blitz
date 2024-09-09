using System;
using System.Collections;
using System.Linq;
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
    private const int SmallBlind = 25;
    private const int BigBlind = 50;
    private Player currentPlayer;

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

        currentPlayer = players[activePlayerIndex];
        currentPlayer.Raise(SmallBlind);
        activePlayerIndex++;
        currentPlayer = players[activePlayerIndex];
        currentPlayer.Raise(BigBlind);
        Debug.Log(" blinds played");
    }

    //Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentPlayer.Raise(int.Parse(testBox.text));
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex + 1}");
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (Player.IsGlobalRaised())
            {
                currentPlayer.Check();
            }
            else
            {
                currentPlayer.Call();
                Debug.Log($"player {activePlayerIndex + 1}");
            }
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex + 1}");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            currentPlayer.Fold();
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex + 1}");
        }
        
        if (Player.GetCallCounter() == players.Count)
        {
            Player.GlobalRaised();
        }
    }

    private Ranking DetermineRank(Pocket pocket, Board board)
    {
        // make sure to take into account the street of the board when retrieving the cards
        // if more than 5 cards, find best hand then find the rank
        // assuming 5 cards total for now
        
        
        // first combine pocket cards and board cards to get the player hand
        List<Card> tempPlayerHand = new List<Card>(pocket.GetCards());
        tempPlayerHand.AddRange(board.GetFlop());
        
        
        /* cards are stored as words, so this stores the card in their number counterpart
        eg Two of Spades = [2,1]
           King of Hearts = [13,4] 
        */
        List<(int denomination, int suit)> playerHand = new List<(int, int)>{};

        foreach (Card card in tempPlayerHand)
        {
            playerHand.Add(((int)card.GetDenomination(), (int)card.GetSuit()));
        }


        /* sort cards from highest rank to lowest rank
            example of sortedHand = [9,4] [7,2] [7,1] [5,3] [5,2] 
        */
        var sortedHand = playerHand.OrderByDescending(card => card.suit).ToList();

        
        // here we check if the hand is a flush or a straight
        bool isFlush = true;
        bool isStraight = true;

        int flushSuit = sortedHand[0].Item2;
        int straightDenomination = sortedHand[0].Item1;

        foreach (var card in sortedHand)
        {
            if (card.Item2 != flushSuit) isFlush = false;
            if (card.Item1 != straightDenomination) isStraight = false;
            straightDenomination--;
        }



        /* here we count the number of occurrences of each rank, which helps us determine
            whether the hand is a four of a kind, three of a kind, pair.. etc
        */
        Dictionary<int, int> denominationCounts = new Dictionary<int, int>();

        foreach (var (x, y) in sortedHand)
        {
            if (denominationCounts.ContainsKey(x))
            {
                denominationCounts[x]++;
            }
            else
            {
                denominationCounts[x] = 1;
            }
        }

        /* from previous example dictionary will be 9:1, 7:2, 5:2
            converts occurrences values into list [1,2,2]
            sorts highest to lowest  [2,2,1]
            possibilites: 
            [4,1] (four of a kind)
            [3,2] (full house)
            [3,1,1] (three of a kind)
            [2,2,1] (two pair)
            [2,1,1,1] (pair) 
            [1,1,1,1,1] (high card)
            NOTE that any of these possibilites could be a flush and [1,1,1,1,1]
            could also be a straight so we must return the rankings in order from
            strongest to weakest
        */

        List<int> denominationList = denominationCounts.Values.ToList();
        denominationList = denominationList.OrderByDescending(n => n).ToList();

        if (isFlush && isStraight)                                  return Ranking.StraightFlush;
        if (denominationList.SequenceEqual(new List<int>{4,1}))     return Ranking.FourOfAKind;
        if (denominationList.SequenceEqual(new List<int>{3,2}))     return Ranking.FullHouse;
        if (isFlush)                                                return Ranking.Flush;
        if (isStraight)                                             return Ranking.Straight;
        if (denominationList.SequenceEqual(new List<int>{3,1,1}))   return Ranking.ThreeOfAKind;
        if (denominationList.SequenceEqual(new List<int>{2,2,1}))   return Ranking.TwoPair;
        if (denominationList.SequenceEqual(new List<int>{2,1,1,1})) return Ranking.Pair;
        
        return Ranking.HighCard;
    }
}
