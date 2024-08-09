using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    private int[,] deck = new int[13, 4];
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
