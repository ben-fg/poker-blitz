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
    private int firstPlayerBalance = 1000;
    private int secondPlayerBalance = 1000;
    private int thirdPlayerBalance = 1000;
    private int fourthPlayerBalance = 1000;
    private bool[,] deck = new bool[13, 4];
    private List<PokerPlayer> PokerPlayers = new List<PokerPlayer>();
    private Card[] boardCards = new Card[5];
    private Board board;

    private int globalMatched;
    [SerializeField] private InputField testBox;
    private int activePokerPlayerIndex;
    private int sbIndex;
    private bool isPreFlop = true;
    private bool isFlop = false;
    private bool isTurn = false;
    private bool isRiver = false;
    private bool nextPokerPlayer = false;
    private const int SmallBlind = 25;
    private const int BigBlind = 50;
    private int positionEnum = 0;
    private int shift = 0;
    private int activePokerPlayers = 4;
    private PokerPlayer currentPokerPlayer;

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

            PokerPlayers.Clear();

            // New cards and positions
            // The positionEnum is used so that you could alternate the positions every round 

            shift = (4 - positionEnum) % 4;

            firstPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((0 + shift) % 4), 1);
            secondPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((1 + shift) % 4), 2);
            thirdPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((2 + shift) % 4), 3);
            fourthPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)((3 + shift) % 4), 4);

            // Added all to a list
            PokerPlayers.Add(firstPlayer);
            PokerPlayers.Add(secondPlayer);
            PokerPlayers.Add(thirdPlayer);
            PokerPlayers.Add(fourthPlayer);

            firstPlayer.SetBalance(firstPlayerBalance);
            secondPlayer.SetBalance(secondPlayerBalance);
            thirdPlayer.SetBalance(thirdPlayerBalance);
            fourthPlayer.SetBalance(fourthPlayerBalance);

            // Shows on screen the Pocket cards for every PokerPlayer
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



            // Add all PokerPlayers to an active PokerPlayers list 
            // Play the blinds

            // To change the starting PokerPlayer each round based on who is the SB
            activePokerPlayerIndex = (0 + positionEnum) % 4;
            sbIndex = activePokerPlayerIndex;
            currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];

            // Manually processes the small blind
            currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() - 25);
            PokerPlayer.SetPot(PokerPlayer.GetPot() + 25);

            // Move to big blind
            activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
            currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];

            // Big Blind buts in his big blind
            currentPokerPlayer.Raise(BigBlind);

            // Move to next PokerPlayer
            activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
            currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];
            Debug.Log(" blinds played");
            PokerPlayer.DecreaseCallCounter();

            isPreFlop = false;
            isFlop = true;
            nextPokerPlayer = true;
            positionEnum++;
        }



        if (board.GetCurrentStreet().Equals(Board.Street.Flop))
        {

            // It checks if it is true so it would only show the flop once
            if (isFlop)
            {
                Debug.Log("I am flopping");

                // Shows the flop cards
                Debug.Log(board.ToString());

                // To start the flop with the SB
                activePokerPlayerIndex = sbIndex;
                currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];

                /* Now isTurn is true but cannot access the turn cards unless the street is incremented
                   If raised, it will not be ablt to show the turn cards 
                 */
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

        // Checks if the PokerPlayer is not folded
        if (!currentPokerPlayer.IsFolded())
        {

            // Prints on screen the PokerPlayer that is in turn to play once 
            if (nextPokerPlayer)
            {
                Debug.Log($"Current PokerPlayer is now PokerPlayer {currentPokerPlayer.GetNum()}");
                nextPokerPlayer = false;
            }

            // Checking for input
            if (Input.GetKeyDown(KeyCode.R))
            {
                // If the SB raises in the preflop, the the rest of the small blind is taken
                if (currentPokerPlayer.GetPosition().Equals(PokerPlayer.Position.SB) && board.GetCurrentStreet().Equals(Board.Street.Preflop))
                {
                    currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() - 25);
                    PokerPlayer.SetPot(PokerPlayer.GetPot() + 25);
                }
                // Get the raised amount from the text box
                currentPokerPlayer.Raise(int.Parse(testBox.text));
                Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} raised by {currentPokerPlayer.AmountRaised()}. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                // Move to the next PokerPlayer
                activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
                currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];
                nextPokerPlayer = true;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {

                // If someone raised, and you pressed C, you will Call 
                if (PokerPlayer.IsGlobalRaised())
                {

                    // Manually detucts the 25 from the SB if he calls
                    if (currentPokerPlayer.GetPosition().Equals(PokerPlayer.Position.SB) && board.GetCurrentStreet().Equals(Board.Street.Preflop))
                    {
                        currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() - 25);
                        PokerPlayer.SetPot(PokerPlayer.GetPot() + 25);
                        PokerPlayer.IncreaseCallCounter();
                        Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} with position {currentPokerPlayer.GetPosition()} called the rest of the blind, 25. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                    }
                    /*If it is the BB in the preflop, then it checks. It has to be inside the if (PokerPlayer.IsGlobalRaised())
                    because the GlobalRaised is not yet false because the call counter has not reached its goal*/
                    else if (currentPokerPlayer.GetPosition().Equals(PokerPlayer.Position.BB) && board.GetCurrentStreet().Equals(Board.Street.Preflop))
                    {
                        currentPokerPlayer.Check();
                        PokerPlayer.IncreaseCallCounter();
                        Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} checked. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                    }
                    // Just a normal call
                    else
                    {
                        currentPokerPlayer.Call();
                        Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} called {currentPokerPlayer.GetPreviousRaises()}. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                    }

                    // Move to the next PokerPlayer
                    activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
                    currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];
                    nextPokerPlayer = true;
                }

                // If no one raised, then normal check
                else
                {
                    currentPokerPlayer.Check();
                    Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} checked. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                    // Move to the next PokerPlayer
                    activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
                    currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];
                    nextPokerPlayer = true;
                }



            }

            // Fold
            else if (Input.GetKeyDown(KeyCode.F))
            {
                currentPokerPlayer.Fold();
                Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} folded. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                // Move to the next PokerPlayer
                activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
                currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];
                activePokerPlayers--;
                nextPokerPlayer = true;
            }
        }

        // If PokerPlayer is folded, then move to next PokerPlayer
        else
        {
            activePokerPlayerIndex = (activePokerPlayerIndex + 1) % PokerPlayers.Count;
            currentPokerPlayer = PokerPlayers[activePokerPlayerIndex];
            nextPokerPlayer = true;

        }

        // If the 3 people called or 4 people checked, then isGlobalRaised flag and the counter are reset to false and 0
        if (PokerPlayer.GetCallCounter() == (activePokerPlayers - 1) || PokerPlayer.GetCheckCounter() == activePokerPlayers)
        {
            PokerPlayer.ResetGlobals();

            // Resets the isChecked flag to false, raise to 0, and amountCalled to 0
            foreach (var PokerPlayer in PokerPlayers)
            {
                PokerPlayer.Reset();
            }

            // However if it is also on the showdown, that means the game has ended and a new hand is dealt
            if (board.GetCurrentStreet().Equals(Board.Street.Showdown))
            {

                firstPlayerBalance = PokerPlayers[0].GetBalance();
                secondPlayerBalance = PokerPlayers[1].GetBalance();
                thirdPlayerBalance = PokerPlayers[2].GetBalance();
                fourthPlayerBalance = PokerPlayers[3].GetBalance();

                activePokerPlayers = 4;
                isPreFlop = true;
            }
            else
            {

                // Move to next street if we are not in showdown
                board.IncrementStreet();
            }

        }

        if (PokerPlayer.GetFoldCounter() == (PokerPlayers.Count - 1))
        {

            PokerPlayer.ResetGlobals();
            firstPlayerBalance = PokerPlayers[0].GetBalance();
            secondPlayerBalance = PokerPlayers[1].GetBalance();
            thirdPlayerBalance = PokerPlayers[2].GetBalance();
            fourthPlayerBalance = PokerPlayers[3].GetBalance();
            activePokerPlayers = 4;

            isPreFlop = true;
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
        
        
        // first combine pocket cards and board cards to get the PokerPlayer hand
        List<Card> tempPokerPlayerHand = new List<Card>(pocket.GetCards());
        tempPokerPlayerHand.AddRange(board.GetFlop());
        
        
        /* cards are stored as words, so this stores the card in their number counterpart
        eg Two of Spades = [2,1]
           King of Hearts = [13,4] 
        */
        List<(int denomination, int suit)> PokerPlayerHand = new List<(int, int)>{};

        foreach (Card card in tempPokerPlayerHand)
        {
            PokerPlayerHand.Add(((int)card.GetDenomination(), (int)card.GetSuit()));
        }


        /* sort cards from highest rank to lowest rank
            example of sortedHand = [9,4] [7,2] [7,1] [5,3] [5,2] 
        */
        var sortedHand = PokerPlayerHand.OrderByDescending(card => card.suit).ToList();

        
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
        
        return Ranking.HighCard;
        //return null;

    }
}

