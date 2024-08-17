using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board
{
    //All 5 cards to be displayed
    private Card[] boardCards = new Card[5];

    //(Research poker terminology if you don't recognise these words)
    public enum Street
    {
        Preflop,
        Flop,
        Turn,
        River,
        Showdown,
    }
    private Street currentStreet;

    //Constructor
    public Board(Card[] boardCards)
    {
        this.boardCards = boardCards;
        currentStreet = Street.Preflop;
    }

    //Getters
    public Card[] GetFlop()
    {
        Card[] flop = new Card[3];
        for (int i = 0; i < 3; i++)
        {
            flop[i] = boardCards[i];
        }
        return flop;
    }

    public Card GetTurn()
    {
        return boardCards[3];
    }

    public Card GetRiver()
    {
        return boardCards[4];
    }

    public Street GetCurrentStreet()
    {
        return currentStreet;
    }

    //Moves to the next street
    //Make sure to create validation so this method is never run during showdown
    public void IncrementStreet()
    {
        int nextStreetIndex = Array.IndexOf(Enum.GetValues(typeof(Street)), currentStreet) + 1;
        currentStreet = (Street)Enum.GetValues(typeof(Street)).GetValue(nextStreetIndex);
    }
}
