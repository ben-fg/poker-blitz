using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board
{
    //All 5 cards to be displayed
    private int[] cards = new int[5];

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
    public Board(int[] cards)
    {
        this.cards = cards;
        currentStreet = Street.Preflop;
    }

    //Getters
    public int[] GetFlop()
    {
        int[] flop = new int[3];
        for (int i = 0; i < 3; i++)
        {
            flop[i] = cards[i];
        }
        return flop;
    }

    public int GetTurn()
    {
        return cards[3];
    }

    public int GetRiver()
    {
        return cards[4];
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
