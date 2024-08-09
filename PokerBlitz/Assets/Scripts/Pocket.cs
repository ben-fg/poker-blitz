using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pocket
{
    private int[] cards = new int[2];
    private bool folded;

    public Pocket(int[] cards)
    {
        this.cards = cards;
        folded = false;
    }

    public int[] GetCards()
    {
        return cards;
    }

    public void FoldCards()
    {
        folded = true;
    }

    public bool GetFolded()
    {
        return folded;
    }
}
