using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;


public class GameMaster : MonoBehaviour
{
    private PokerPlayer firstPlayer;
    private PokerPlayer secondPlayer;
    private PokerPlayer thirdPlayer;
    private PokerPlayer fourthPlayer;
    private bool[,] deck = new bool[13, 4];
    private List<PokerPlayer> players = new List<PokerPlayer>();
    private Card[] boardCards = new Card[5];
    private Board board;

    [SerializeField] private InputField testBox;
    private int activePokerPlayerIndex;
    private int sbIndex;
    private bool isPreFlop = true;
    private bool isFlop = false;
    private bool isTurn = false;
    private bool isRiver = false;
    private bool nextPokerPlayer = false;
    private bool excuteSB = true;
    private const int SmallBlind = 25;
    private const int BigBlind = 50;
    private int positionEnum = 0;
    private int shift = 0;
    private int activeplayers = 4;
    private PokerPlayer currentPokerPlayer;

    public static int gameNumber;
    public const int maxGames = 2;
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

        firstPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)0, 1);
        secondPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)1, 2);
        thirdPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)2, 3);
        fourthPlayer = new(new(GenerateUniqueCard(), GenerateUniqueCard()), (PokerPlayer.Position)3, 4);

    }

    //Update is called once per frame
    void Update()
    {
        // Checks whether preflop is true to start a new round
        if (isPreFlop)
        {
            // Makes all the cards available 
            deck = new bool[13, 4];

            players.Clear();

            activeplayers = 4;

            // New cards and positions
            // The positionEnum is used so that you could alternate the positions every round 

            shift = (4 - positionEnum) % 4;

            firstPlayer.SetPocket(new(GenerateUniqueCard(), GenerateUniqueCard()));
            secondPlayer.SetPocket(new(GenerateUniqueCard(), GenerateUniqueCard()));
            thirdPlayer.SetPocket(new(GenerateUniqueCard(), GenerateUniqueCard()));
            fourthPlayer.SetPocket(new(GenerateUniqueCard(), GenerateUniqueCard()));

            firstPlayer.SetPosition((PokerPlayer.Position)((0 + shift) % 4));
            secondPlayer.SetPosition((PokerPlayer.Position)((1 + shift) % 4));
            thirdPlayer.SetPosition((PokerPlayer.Position)((2 + shift) % 4));
            fourthPlayer.SetPosition((PokerPlayer.Position)((3 + shift) % 4));

            // Added all to a list
            players.Add(firstPlayer);
            players.Add(secondPlayer);
            players.Add(thirdPlayer);
            players.Add(fourthPlayer);

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

            // Add all players to an active players list 
            // Play the blinds

            // To change the starting PokerPlayer each round based on who is the SB
            activePokerPlayerIndex = (0 + positionEnum) % 4;
            sbIndex = activePokerPlayerIndex;
            currentPokerPlayer = players[activePokerPlayerIndex];

            // Manually processes the small blind
            currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() - 25);
            PokerPlayer.SetPot(PokerPlayer.GetPot() + 25);

            // Move to big blind
            activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
            currentPokerPlayer = players[activePokerPlayerIndex];

            // Big Blind buts in his big blind
            currentPokerPlayer.Raise(BigBlind);

            // Move to next PokerPlayer
            activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
            currentPokerPlayer = players[activePokerPlayerIndex];
            Debug.Log(" blinds played");
            PokerPlayer.DecreaseCallCounter();

            isPreFlop = false;
            isFlop = true;
            nextPokerPlayer = true;
            excuteSB = true;
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
                currentPokerPlayer = players[activePokerPlayerIndex];

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
                activePokerPlayerIndex = sbIndex;
                currentPokerPlayer = players[activePokerPlayerIndex];
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
                activePokerPlayerIndex = sbIndex;
                currentPokerPlayer = players[activePokerPlayerIndex];
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
                if (currentPokerPlayer.GetPosition().Equals(PokerPlayer.Position.SB) && board.GetCurrentStreet().Equals(Board.Street.Preflop) && excuteSB)
                {
                    currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() - 25 + 50);
                    PokerPlayer.SetPot(PokerPlayer.GetPot() + 25 - 50);
                    excuteSB = false;
                }
                // Get the raised amount from the text box
                currentPokerPlayer.Raise(int.Parse(testBox.text));

                if (PokerPlayer.GetNumRaises() > 1)
                {
                    Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} raised by {currentPokerPlayer.AmountRaised()} more. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                }
                else
                {
                    Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} raised by {currentPokerPlayer.AmountRaised()}. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                }

                // Move to the next PokerPlayer
                activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
                currentPokerPlayer = players[activePokerPlayerIndex];
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
                        if (excuteSB)
                        {
                            currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() - 25);
                            PokerPlayer.SetPot(PokerPlayer.GetPot() + 25);
                            Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} with position {currentPokerPlayer.GetPosition()} called the rest of the blind, 25. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                            excuteSB = false;
                        }
                        
                       /* I know this does not make sense but trust me, it is a very silly edge case that I have to cover*/
                        if ((currentPokerPlayer.GetPreviousRaises() - 50) != 0)
                        {
                            currentPokerPlayer.Call();
                            PokerPlayer.DecreaseCallCounter();
                            currentPokerPlayer.SetBalance(currentPokerPlayer.GetBalance() + 50);
                            PokerPlayer.SetPot(PokerPlayer.GetPot() - 50);
                            Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} called {currentPokerPlayer.GetPreviousRaises() - 50}. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                            
                        }
                        PokerPlayer.IncreaseCallCounter();
                    }
                    /*If it is the BB in the preflop, then it checks. It has to be inside the if (PokerPlayer.IsGlobalRaised())
                    because the GlobalRaised is not yet false because the call counter has not reached its goal*/
                    else if (currentPokerPlayer.GetPosition().Equals(PokerPlayer.Position.BB) && board.GetCurrentStreet().Equals(Board.Street.Preflop))
                    {
                        /* Another edge case */
                        if (PokerPlayer.GetNumRaises() > 1)
                        {
                            currentPokerPlayer.Call();
                            Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} called {currentPokerPlayer.GetPreviousRaises()}. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                        }
                        else
                        {
                            currentPokerPlayer.Check();
                            PokerPlayer.IncreaseCallCounter();
                            Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} checked. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");
                        }
                        
                    }
                    // Just a normal call
                    else
                    {
                        currentPokerPlayer.Call();
                        Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} called {currentPokerPlayer.GetPreviousRaises()}. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                    }

                    // Move to the next PokerPlayer
                    activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
                    currentPokerPlayer = players[activePokerPlayerIndex];
                    nextPokerPlayer = true;
                }

                // If no one raised, then normal check
                else
                {
                    currentPokerPlayer.Check();
                    Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} checked. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                    // Move to the next PokerPlayer
                    activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
                    currentPokerPlayer = players[activePokerPlayerIndex];
                    nextPokerPlayer = true;
                }



            }

            // Fold
            else if (Input.GetKeyDown(KeyCode.F))
            {
                currentPokerPlayer.Fold();
                Debug.Log($" PokerPlayer {currentPokerPlayer.GetNum()} folded. Current balance: {currentPokerPlayer.GetBalance()} Current pot: {PokerPlayer.GetPot()}");

                // Move to the next PokerPlayer
                activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
                currentPokerPlayer = players[activePokerPlayerIndex];
                activeplayers--;
                nextPokerPlayer = true;
            }
        }

        // If PokerPlayer is folded, then move to next PokerPlayer
        else
        {
            activePokerPlayerIndex = (activePokerPlayerIndex + 1) % players.Count;
            currentPokerPlayer = players[activePokerPlayerIndex];
            nextPokerPlayer = true;

        }

        // If the 3 people called or 4 people checked, then isGlobalRaised flag and the counter are reset to false and 0
        if (PokerPlayer.GetCallCounter() == (activeplayers - 1) || PokerPlayer.GetCheckCounter() == activeplayers)
        {
            PokerPlayer.ResetGlobals();

            // Resets the isChecked flag to false, raise to 0, and amountCalled to 0
            foreach (var pokerPlayer in players)
            {
                pokerPlayer.Reset();
            }

            // However if it is also on the showdown, that means the game has ended and a new hand is dealt
            if (board.GetCurrentStreet().Equals(Board.Street.Showdown))
            {
                foreach (var pokerPlayer in players)
                {
                    pokerPlayer.Unfold();
                }
                isPreFlop = true;
            }
            else
            {

                // Move to next street if we are not in showdown
                board.IncrementStreet();
            }

        }

        if (PokerPlayer.GetFoldCounter() == (players.Count - 1))
        {

            PokerPlayer.ResetGlobals();
            foreach (var pokerPlayer in players)
            {
                pokerPlayer.Reset();
                pokerPlayer.Unfold();
            }

            isPreFlop = true;
        }
    }



    private List<List<(int,int)>> GenerateAllHands(Pocket pocket, Board board)
    {
        /* although variable is called sevenCardHand it could also include 6 cards
            gets all cards available to the player
        */
        List<Card> tempCards = new List<Card>(pocket.GetCards());
        tempCards.AddRange(board.GetFlop());


        List<(int denomination, int suit)> cards = new List<(int, int)>{};

        
        // converts Card[denomination, suit] into List<(int,int)> format
        foreach (Card card in tempCards)
        {
            cards.Add(((int)card.GetDenomination(), (int)card.GetSuit()));
        }


        
        /* generates all possible 5-card hands from cards available to player
            and stores in combinations */
        int r = 5;
        List<List<(int,int)>> combinations = GetCombinations(cards, r);

        return combinations;

        
        /* recursive function to calculate all combinations
            calls GenerateCombination as helper function
        */
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

    private Ranking DetermineBestHand(List<List<(int,int)>> listOfHands)
        {

            static int determineHandType(List<int> denominationList, bool isFlush, bool isStraight)
                {
                    if (isFlush && isStraight)                                  return (int)Ranking.CardRank.StraightFlush;              
                    if (denominationList.SequenceEqual(new List<int>{4,1}))     return (int)Ranking.CardRank.FourOfAKind;
                    if (denominationList.SequenceEqual(new List<int>{3,2}))     return (int)Ranking.CardRank.FullHouse;
                    if (isFlush)                                                return (int)Ranking.CardRank.Flush;
                    if (isStraight)                                             return (int)Ranking.CardRank.Straight;
                    if (denominationList.SequenceEqual(new List<int>{3,1,1}))   return (int)Ranking.CardRank.ThreeOfAKind;
                    if (denominationList.SequenceEqual(new List<int>{2,2,1}))   return (int)Ranking.CardRank.TwoPair;
                    if (denominationList.SequenceEqual(new List<int>{2,1,1,1})) return (int)Ranking.CardRank.Pair;
                    return (int)Ranking.CardRank.HighCard;
                }



            /* sort cards from highest denomination to lowest denomination in each hand
                example of sortedHand = [9,4] [7,2] [7,1] [5,3] [5,2] 
            */
            List<List<(int,int)>> sortedHands = new List<List<(int, int)>>();

            foreach (var hand in listOfHands)
            {
                var sortedHand = hand.OrderByDescending(card => card.Item1).ToList();
                sortedHands.Add(sortedHand);
            }


            int strongestHandLevel = -1;
            int currentHandLevel = -1;
            List<List<(int,int)>> topRankedHands = new List<List<(int, int)>>();

            foreach (var hand in sortedHands)
            {
                // determines whether current hand is flush or straight
                bool isFlush = true;
                bool isStraight = true;

                int straightDenomination = hand[0].Item1;
                int flushSuit = hand[0].Item2;
            
                foreach (var card in hand)
                {
                    if (card.Item2 != flushSuit) isFlush = false;
                    if (card.Item1 != straightDenomination) isStraight = false;
                    straightDenomination--;
                }

                
                /* here we count the number of occurrences of each rank, which helps us determine
                whether the hand is a four of a kind, three of a kind, pair.. etc
                */
                Dictionary<int, int> denominationCounts = new Dictionary<int, int>();

                foreach (var (x, y) in hand)
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

                List<int> denominationList = denominationCounts.Values.ToList();
                denominationList = denominationList.OrderByDescending(n => n).ToList();

                currentHandLevel = determineHandType(denominationList, isFlush, isStraight);

                // keep a list of only the highest ranking hands
                if (currentHandLevel > strongestHandLevel)
                {
                    strongestHandLevel = currentHandLevel;
                    topRankedHands.Clear();
                    topRankedHands.Add(hand);
                }
                else if (currentHandLevel == strongestHandLevel)
                {
                    topRankedHands.Add(hand);
                }

            }

            
            // only 1 best hand, so no showdown needed
            if (topRankedHands.Count == 1) return null;

            
            // more than 1 hand of same rank, so showdown needed
            switch (strongestHandLevel)
            {
                case 0:
                    // high card showdown
                    

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    break;
            }

            return null;

        }
}

