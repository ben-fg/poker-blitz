using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to each PlayerSeat prefab. Holds all the UI refs for one seat.
public class PlayerSeatUI : MonoBehaviour
{
    [Header("Player Info")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;         // shows current street bet
    public GameObject dealerBadge;     // "D" chip
    public GameObject sbBadge;         // "SB" chip
    public GameObject bbBadge;         // "BB" chip

    [Header("Cards")]
    public Image card1Image;
    public Image card2Image;
    public GameObject card1WinHighlight;   // glow shown when this card is part of the winning hand
    public GameObject card2WinHighlight;

    [Header("Status")]
    public GameObject foldedOverlay;        // greyed-out overlay when folded
    public GameObject allInBadge;           // "ALL IN" badge
    public GameObject activeTurnHighlight;  // glowing border when it's this player's turn

    // ──────────────────────────────
    //  Runtime
    // ──────────────────────────────

    public void ShowCards(Card c1, Card c2)
    {
        card1Image.sprite = CardSpriteLoader.GetSprite(c1);
        card2Image.sprite = CardSpriteLoader.GetSprite(c2);
        card1Image.enabled = true;
        card2Image.enabled = true;
    }

    public void ShowCardBacks()
    {
        Sprite back = CardSpriteLoader.GetBackSprite();
        card1Image.sprite = back;
        card2Image.sprite = back;
        card1Image.enabled = true;
        card2Image.enabled = true;
    }

    public void HideCards()
    {
        card1Image.enabled = false;
        card2Image.enabled = false;
    }

    public void SetName(string playerName) => nameText.text = playerName;
    public void SetBalance(int balance) => balanceText.text = $"${balance}";
    public void SetBet(int bet) => betText.text = bet > 0 ? $"${bet}" : "";
    public void SetFolded(bool folded) => foldedOverlay.SetActive(folded);
    public void SetAllIn(bool allIn) => allInBadge.SetActive(allIn);
    public void SetActiveTurn(bool active) => activeTurnHighlight.SetActive(active);
    public void SetDealer(bool isDealer) => dealerBadge.SetActive(isDealer);
    public void SetSB(bool isSB) => sbBadge.SetActive(isSB);
    public void SetBB(bool isBB) => bbBadge.SetActive(isBB);

    public void SetCard1Highlighted(bool on) { if (card1WinHighlight != null) card1WinHighlight.SetActive(on); }
    public void SetCard2Highlighted(bool on) { if (card2WinHighlight != null) card2WinHighlight.SetActive(on); }

    public void ResetForNewHand()
    {
        SetFolded(false);
        SetAllIn(false);
        SetActiveTurn(false);
        SetDealer(false);
        SetSB(false);
        SetBB(false);
        SetCard1Highlighted(false);
        SetCard2Highlighted(false);
        SetBet(0);
        HideCards();
    }
}