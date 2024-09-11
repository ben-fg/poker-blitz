using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;


public class GameMaster : MonoBehaviour
{
    private PokerPlayer firstPlayer;
    private PokerPlayer secondPlayer;
    private PokerPlayer thirdPlayer;
    private PokerPlayer fourthPlayer;
    private bool[,] deck = new bool[13, 4];
    private List<PokerPlayer> players = new List<PokerPlayer>();
    private List<PokerPlayer> activePlayers = new List<PokerPlayer>();
    private Card[] boardCards = new Card[5];
    private Board board;

    private int globalMatched;
    [SerializeField] private InputField testBox;
    private int activePlayerIndex = 1;
    private bool isPreFlop = true;
    private bool isFlop = false;
    private bool isTurn = false;
    private bool isRiver = false;
    private const int SmallBlind = 25;
    private const int BigBlind = 50;
    private int positionEnum = 0;
    private PokerPlayer currentPlayer;

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

        /*Debug.Log($"Generated unique card: Denomination {cardDenomination}, Suit {cardSuit}");*/

        return uniqueCard;
    }

    //Start is called before the first frame update
    void Start()
    {




    }

    //Update is called once per frame
    void Update()
    {
        // Checks whether preflop is true to start a new round
        if (isPreFlop)
        {
            // Makes all the cards available 
            deck = new bool[13, 4];

            // New cards and positions
            // The positionEnum is used so that ypu could alternate the positions every round 
            firstPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((0 + positionEnum) % 4));
            secondPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((1 + positionEnum) % 4));
            thirdPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((2 + positionEnum) % 4));
            fourthPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((3 + positionEnum) % 4));

            // Added all to a list
            players.Add(firstPlayer);
            players.Add(secondPlayer);
            players.Add(thirdPlayer);
            players.Add(fourthPlayer);

            // Shows on screen the Pocket cards for every player
            Debug.Log(firstPlayer.GetPocket().ToString());
            Debug.Log(secondPlayer.GetPocket().ToString());
            Debug.Log(thirdPlayer.GetPocket().ToString());
            Debug.Log(fourthPlayer.GetPocket().ToString());

            // Generates cards for the board
            for (int i = 0; i < 5; i++)
            {
                boardCards[i] = GenerateUniqueCard();
            }

            board = new(boardCards);

            // Add all players to an active players list 
            // Play the blinds
            if (board.GetCurrentStreet().Equals(Board.Street.Preflop))
            {
                activePlayers.AddRange(players);
                currentPlayer = activePlayers[activePlayerIndex];
                currentPlayer.Raise(SmallBlind);
                activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
                currentPlayer = activePlayers[activePlayerIndex];
                currentPlayer.Raise(BigBlind);
                activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
                currentPlayer = activePlayers[activePlayerIndex];
                Debug.Log(" blinds played");

            }
            positionEnum++;
            isPreFlop = false;
            isFlop = true;
        }



        if (board.GetCurrentStreet().Equals(Board.Street.Flop))
        {
            // It checks if it is true so it would only show the flop once
            if (isFlop)
            {
                /*if (activePlayerIndex == 1)
                {

                    currentPlayer.SetBalance(currentPlayer.GetBalance()-25);
                    Player.SetPot(Player.GetPot()+25);
                }*/
                Debug.Log("I am flopping");

                // Shows the flop cards
                Debug.Log(board.ToString());

                // Now isTurn is true but cannot access the turn cards unless the street is incremented
                isFlop = false;
                isTurn = true;
            }
        }
        else if (board.GetCurrentStreet().Equals(Board.Street.Turn))
        {
            // It checks if it is true so it would only show the turn once
            if (isTurn)
            {
                Debug.Log("I am turning");
                Debug.Log(board.ToString());
                isTurn = false;
                isRiver = true;
            }
        }
        else if (board.GetCurrentStreet().Equals(Board.Street.River))
        {
            // It checks if it is true so it would only show the river once
            if (isRiver)
            {
                Debug.Log("I am rivering");
                Debug.Log(board.ToString());
                isRiver = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            // Get the raised amount from the text box
            // Move to the next player
            currentPlayer.Raise(int.Parse(testBox.text));
            Debug.Log($" Player {activePlayerIndex + 1} raised by {currentPlayer.AmountRaised()}. Current balance: {currentPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
            activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex + 1}");
            currentPlayer = activePlayers[activePlayerIndex];
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            // If someone raised, and you pressed C, you will Call 
            if (PokerPlayer.IsGlobalRaised())
            {
                currentPlayer.Call();
                Debug.Log($"Previous raise; {PokerPlayer.GetPreviousRaise()}");
                Debug.Log($" Player {activePlayerIndex + 1} called {PokerPlayer.GetPreviousRaise()}. Current balance: {currentPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");


                currentPlayer = activePlayers[activePlayerIndex];
            }
            else
            {
                currentPlayer.Check();
                Debug.Log($" Player {activePlayerIndex + 1} checked. Current balance: {currentPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
            }

            // Move to the next player
            activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex + 1}");
            currentPlayer = activePlayers[activePlayerIndex];
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            currentPlayer.Fold();
            Debug.Log($" Player {activePlayerIndex + 1} folded. Current balance: {currentPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
            activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
            Debug.Log($"Current player is now Player {activePlayerIndex + 1}");
            currentPlayer = activePlayers[activePlayerIndex];
        }


        // If the 3 people called or 4 peole checked, then isGlobalRaised flag and the counter are reset to false and 0
        if (PokerPlayer.GetCallCounter() == (activePlayers.Count - 1) || PokerPlayer.GetCheckCounter() == activePlayers.Count)
        {
            PokerPlayer.ResetGlobals();

            // However if it is also on the showdown, that means the game has ended and a new hand is dealt
            if (board.GetCurrentStreet().Equals(Board.Street.Showdown))
            {
                isPreFlop = true;
            }
            else
            {
                board.IncrementStreet();
            }

        }
        
        if (PokerPlayer.GetFoldCounter() == (players.Count-1))
        {
            PokerPlayer.ResetGlobals();
            isPreFlop = true;
            positionEnum++;
        }
    }


    private List<List<(int,int)>> GenerateAllHands(Pocket pocket, Board board)
    {
        // although variable is called sevenCardHand it could also include 6 cards
        List<Card> tempCards = new List<Card>(pocket.GetCards());
        tempCards.AddRange(board.GetFlop());


        List<(int denomination, int suit)> cards = new List<(int, int)>{};

        foreach (Card card in tempCards)
        {
            cards.Add(((int)card.GetDenomination(), (int)card.GetSuit()));
        }


        int r = 5;
        List<List<(int,int)>> combinations = GetCombinations(cards, r);

        return combinations;

        static List<List<(int,int)>> GetCombinations(List<(int,int)> cards, int r)
        {
            List<List<(int,int)>> result = new List<List<(int,int)>>();
            List<(int,int)> combination = new List<(int,int)>();

            GenerateCombinations(cards, r, 0, combination, result);
            return result;
        }
        
        
        static void GenerateCombinations(List<(int,int)> cards, int r, int start, List<(int,int)> combination, List<List<(int,int)>> result)
        {
            
            if (combination.Count == r)
            {
                result.Add(new List<(int,int)>(combination));
                return;
            }


            for (int i = start; i < cards.Count; i++)
            {
                combination.Add(cards[i]);
                GenerateCombinations(cards, r, i + 1, combination, result);
                combination.RemoveAt(combination.Count - 1);
            }
        }
    }

private Ranking DetermineBestHand(Pocket pocket, Board board)
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

        /*if (isFlush && isStraight)                                  return Ranking.StraightFlush;              
        if (denominationList.SequenceEqual(new List<int>{4,1}))     return Ranking.FourOfAKind;
        if (denominationList.SequenceEqual(new List<int>{3,2}))     return Ranking.FullHouse;
        if (isFlush)                                                return Ranking.Flush;
        if (isStraight)                                             return Ranking.Straight;
        if (denominationList.SequenceEqual(new List<int>{3,1,1}))   return Ranking.ThreeOfAKind;
        if (denominationList.SequenceEqual(new List<int>{2,2,1}))   return Ranking.TwoPair;
        if (denominationList.SequenceEqual(new List<int>{2,1,1,1})) return Ranking.Pair;*/
        
        //return Ranking.HighCard;
        return null;
    }
}

