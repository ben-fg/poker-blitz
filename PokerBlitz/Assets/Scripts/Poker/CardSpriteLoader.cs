using UnityEngine;

/// <summary>
/// Loads card sprites from Resources/Cards/ by denomination and suit.
/// Sprite files must be named e.g. "Ace_of_Spades", "Eight_of_Diamonds" etc.
/// </summary>
public static class CardSpriteLoader
{
    // Maps Card.Denomination enum to the name used in the sprite filename
    private static readonly string[] DenominationNames =
    {
        "Two", "Three", "Four", "Five", "Six", "Seven", "Eight",
        "Nine", "Ten", "Jack", "Queen", "King", "Ace"
    };

    // Maps Card.Suit enum to the name used in the sprite filename
    private static readonly string[] SuitNames =
    {
        "Spades", "Diamonds", "Clubs", "Hearts"
    };

    /// <summary>Returns the sprite for a given card. Returns null if not found.</summary>
    public static Sprite GetSprite(Card card)
    {
        string denomination = DenominationNames[(int)card.GetDenomination()];
        string suit = SuitNames[(int)card.GetSuit()];
        string spriteName = $"{denomination}_of_{suit}";

        Debug.Log($"Trying to load: Cards/{spriteName}");

        Sprite sprite = Resources.Load<Sprite>($"Cards/{spriteName}");

        if (sprite == null)
            Debug.LogWarning($"[CardSpriteLoader] Could not find sprite: Cards/{spriteName}");

        return sprite;
    }

    /// <summary>Returns the card back sprite.</summary>
    public static Sprite GetBackSprite()
    {
        Sprite sprite = Resources.Load<Sprite>("Cards/Back_Card");

        if (sprite == null)
            Debug.LogWarning("[CardSpriteLoader] Could not find sprite: Cards/Back_Card");

        return sprite;
    }
}