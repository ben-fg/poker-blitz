using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sits on the same GameMaster object as the GameMaster script. Wire everything up in the Inspector.
public class PokerUIManager : MonoBehaviour
{
    // ──────────────────────────────
    //  Inspector References
    // ──────────────────────────────

    [Header("Game Master")]
    public GameMaster gameMaster;

    [Header("Player Seats (0=Bottom/You, 1=Left, 2=Top, 3=Right)")]
    public PlayerSeatUI[] seats = new PlayerSeatUI[4];

    [Header("Community Cards")]
    public Image flopCard1;
    public Image flopCard2;
    public Image flopCard3;
    public Image turnCard;
    public Image riverCard;

    [Header("Community Card Win Highlights")]
    public GameObject flopCard1Highlight;
    public GameObject flopCard2Highlight;
    public GameObject flopCard3Highlight;
    public GameObject turnCardHighlight;
    public GameObject riverCardHighlight;

    [Header("Pot & Street")]
    public TextMeshProUGUI potText;
    public TextMeshProUGUI streetText;
    public TextMeshProUGUI messageText;   // general status message at top

    [Header("Action Buttons")]
    public Button foldButton;
    public Button checkButton;
    public Button callButton;
    public Button betButton;
    public Button raiseButton;

    [Header("Raise / Bet Amount")]
    public Slider raiseSlider;
    public TextMeshProUGUI raiseAmountText;

    [Header("Call Amount Label")]
    public TextMeshProUGUI callAmountText;  // shows "Call $X" on the call button

    [Header("Showdown / Result")]
    public GameObject showdownPanel;        // only active while a result is showing
    public TextMeshProUGUI resultText;      // "Moaz wins $150 with a Flush!"
    public TextMeshProUGUI statusText;      // "Waiting for host..." on non-host clients
    public Button showCardsButton;          // lets an uncontested winner reveal their hand
    public Button startNextHandButton;      // host-only; deals the next hand on click

    // ──────────────────────────────
    //  Private State
    // ──────────────────────────────

    // Seat of the local player. Not cached since it isn't known until the game
    // master builds its roster, which can happen after this component's Awake.
    private int LOCAL_PLAYER_INDEX => gameMaster.GetLocalSeatIndex();

    private int raiseAmount;
    private int cachedBetToCall;
    private const int RAISE_STEP = 5;

    // Rotates a canonical (dealer-order) player index into the seat slot it renders
    // into on this client, so the local player always lands in seats[0] (Bottom/You).
    private int ToUiSlot(int canonicalIndex)
    {
        int n = gameMaster.GetPlayerCount();
        return ((canonicalIndex - LOCAL_PLAYER_INDEX) % n + n) % n;
    }

    // ──────────────────────────────
    //  Unity
    // ──────────────────────────────

    void Awake()
    {

        if (foldButton != null) foldButton.onClick.AddListener(OnFold);
        else Debug.LogError("FoldButton is not assigned in PokerUIManager!");

        if (checkButton != null) checkButton.onClick.AddListener(OnCheck);
        else Debug.LogError("CheckButton is not assigned in PokerUIManager!");

        if (callButton != null) callButton.onClick.AddListener(OnCall);
        else Debug.LogError("CallButton is not assigned in PokerUIManager!");

        if (betButton != null) betButton.onClick.AddListener(OnBet);
        else Debug.LogError("BetButton is not assigned in PokerUIManager!");

        if (raiseButton != null) raiseButton.onClick.AddListener(OnRaise);
        else Debug.LogError("RaiseButton is not assigned in PokerUIManager!");

        if (raiseSlider != null) raiseSlider.onValueChanged.AddListener(OnSliderChanged);
        else Debug.LogError("RaiseSlider is not assigned in PokerUIManager!");

        if (showCardsButton != null) showCardsButton.onClick.AddListener(OnShowCards);

        if (startNextHandButton != null) startNextHandButton.onClick.AddListener(OnStartNextHand);
    }

    // ──────────────────────────────
    //  Public API called by GameMaster
    // ──────────────────────────────

    public void RefreshUI(
        List<PokerPlayer> players,
        Card[] boardCards,
        List<Pot> pots,
        int currentBetToCall,
        int currentPlayerId,
        int dealerId,
        int sbId,
        int bbId,
        string streetName)
    {
        UpdateStreetLabel(streetName);
        UpdatePot(pots);
        UpdatePlayerSeats(players, currentPlayerId, dealerId, sbId, bbId, streetName);
        UpdateBoardCards(boardCards, streetName);
        UpdateActionButtons(players, currentBetToCall, currentPlayerId, streetName);
        UpdateRaiseSlider(players, currentBetToCall);
    }

    public void RevealFlop(Card c1, Card c2, Card c3)
    {
        SetCommunityCard(flopCard1, c1);
        SetCommunityCard(flopCard2, c2);
        SetCommunityCard(flopCard3, c3);
        StartCoroutine(FadeIn(flopCard1));
        StartCoroutine(FadeIn(flopCard2));
        StartCoroutine(FadeIn(flopCard3));
    }

    public void RevealTurn(Card card)
    {
        SetCommunityCard(turnCard, card);
        StartCoroutine(FadeIn(turnCard));
    }

    public void RevealRiver(Card card)
    {
        SetCommunityCard(riverCard, card);
        StartCoroutine(FadeIn(riverCard));
    }

    public void ResetBoard()
    {
        HideCommunityCard(flopCard1);
        HideCommunityCard(flopCard2);
        HideCommunityCard(flopCard3);
        HideCommunityCard(turnCard);
        HideCommunityCard(riverCard);

        ClearHandResult();
        ClearWinningHighlights();
        SetStatusText("");
        SetShowCardsButtonVisible(false);
        SetStartNextHandButtonVisible(false);

        foreach (var seat in seats)
            seat.ResetForNewHand();
    }

    public void DealPocketCards(List<PokerPlayer> players)
    {
        for (int i = 0; i < players.Count && i < seats.Length; i++)
        {
            var player = players[i];
            var seat = seats[ToUiSlot(i)];

            if (i == LOCAL_PLAYER_INDEX)
            {
                var cards = player.GetPocket().GetCards();
                seat.ShowCards(cards[0], cards[1]);
            }
            else
            {
                seat.ShowCardBacks();
            }
        }
    }

    public void ShowMessage(string msg, float duration = 2f)
    {
        StopAllCoroutines();
        messageText.text = msg;
        StartCoroutine(ClearMessageAfter(duration));
    }

    public void ShowShowdown(List<PokerPlayer> players)
    {
        for (int i = 0; i < players.Count && i < seats.Length; i++)
        {
            if (!players[i].IsFolded())
            {
                var cards = players[i].GetPocket().GetCards();
                seats[ToUiSlot(i)].ShowCards(cards[0], cards[1]);
            }
        }
    }

    public void ShowHandResult(string message)
    {
        if (showdownPanel != null) showdownPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = message;
            resultText.gameObject.SetActive(true);
        }
    }

    public void ClearHandResult()
    {
        if (showdownPanel != null) showdownPanel.SetActive(false);

        if (resultText != null)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }
    }

    public void HighlightWinningHand(int winnerSeatIndex, PokerPlayer winner, Card[] boardCards, List<Card> winningFive)
    {
        ClearWinningHighlights();
        if (winningFive == null) return;

        if (winnerSeatIndex >= 0 && winnerSeatIndex < seats.Length)
        {
            var pocket = winner.GetPocket().GetCards();
            var seat = seats[ToUiSlot(winnerSeatIndex)];
            seat.SetCard1Highlighted(winningFive.Contains(pocket[0]));
            seat.SetCard2Highlighted(winningFive.Contains(pocket[1]));
        }

        SetHighlight(flopCard1Highlight, boardCards[0] != null && winningFive.Contains(boardCards[0]));
        SetHighlight(flopCard2Highlight, boardCards[1] != null && winningFive.Contains(boardCards[1]));
        SetHighlight(flopCard3Highlight, boardCards[2] != null && winningFive.Contains(boardCards[2]));
        SetHighlight(turnCardHighlight, boardCards[3] != null && winningFive.Contains(boardCards[3]));
        SetHighlight(riverCardHighlight, boardCards[4] != null && winningFive.Contains(boardCards[4]));
    }

    public void ClearWinningHighlights()
    {
        foreach (var seat in seats)
        {
            seat.SetCard1Highlighted(false);
            seat.SetCard2Highlighted(false);
        }

        SetHighlight(flopCard1Highlight, false);
        SetHighlight(flopCard2Highlight, false);
        SetHighlight(flopCard3Highlight, false);
        SetHighlight(turnCardHighlight, false);
        SetHighlight(riverCardHighlight, false);
    }

    private void SetHighlight(GameObject highlight, bool on)
    {
        if (highlight != null) highlight.SetActive(on);
    }

    public void SetStatusText(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    public void SetShowCardsButtonVisible(bool visible)
    {
        if (showCardsButton != null) showCardsButton.gameObject.SetActive(visible);
    }

    public void SetStartNextHandButtonVisible(bool visible)
    {
        if (startNextHandButton != null) startNextHandButton.gameObject.SetActive(visible);
    }

    // Reveals THIS client's own cards on THIS client's screen only. Broadcasting
    // the reveal to everyone else is gameMaster's job since it owns the network layer.
    public void RevealSeatCards(int canonicalIndex, Card c1, Card c2)
    {
        seats[ToUiSlot(canonicalIndex)].ShowCards(c1, c2);
    }

    private void OnShowCards()
    {
        var player = gameMaster.GetPlayer(LOCAL_PLAYER_INDEX);
        var cards = player.GetPocket().GetCards();
        RevealSeatCards(LOCAL_PLAYER_INDEX, cards[0], cards[1]);
        SetShowCardsButtonVisible(false);
        gameMaster.SendShowCardsRequest();
    }

    private void OnStartNextHand() => gameMaster.RequestStartNextHand();

    // ──────────────────────────────
    //  Button Callbacks
    // ──────────────────────────────

    private void OnFold() => gameMaster.SendFoldRequest();
    private void OnCheck() => gameMaster.SendCheckRequest();
    private void OnCall() => gameMaster.SendCallRequest();
    private void OnBet() => gameMaster.SendBetRequest(raiseAmount);
    private void OnRaise() => gameMaster.SendRaiseRequest(raiseAmount);

    private void OnSliderChanged(float value)
    {
        int snapped = RoundToStep(Mathf.RoundToInt(value), RAISE_STEP);
        if (!Mathf.Approximately(raiseSlider.value, snapped))
            raiseSlider.value = snapped; // snaps the handle itself to the nearest 5

        raiseAmount = snapped;
        UpdateRaiseAmountLabel();
    }

    private void UpdateRaiseAmountLabel()
    {
        raiseAmountText.text = cachedBetToCall > 0 ? $"Raise to ${raiseAmount}" : $"Bet ${raiseAmount}";
    }

    private static int RoundToStep(int value, int step) => Mathf.RoundToInt((float)value / step) * step;
    private static int CeilToStep(int value, int step) => Mathf.CeilToInt((float)value / step) * step;

    // ──────────────────────────────
    //  Private UI Updaters
    // ──────────────────────────────

    private void UpdateStreetLabel(string streetName)
    {
        streetText.text = streetName.ToUpper();
    }

    private void UpdatePot(List<Pot> pots)
    {
        int total = 0;
        foreach (var pot in pots) total += pot.amount;
        potText.text = $"POT  ${total}";
    }

    private void UpdatePlayerSeats(
        List<PokerPlayer> players,
        int currentPlayerId,
        int dealerId, int sbId, int bbId,
        string streetName)
    {
        // Same reasoning as UpdateActionButtons. Once the hand's over, currentPlayerId
        // is stale (nobody's turn advances on the fold that ends a hand), so nobody
        // should light up as "active turn" no matter what it still says.
        bool handOver = streetName == "Showdown";

        for (int i = 0; i < players.Count && i < seats.Length; i++)
        {
            var p = players[i];
            var seat = seats[ToUiSlot(i)];

            seat.SetName(p.GetName());
            seat.SetBalance(p.GetBalance());
            seat.SetBet(p.totalContributed);
            seat.SetFolded(p.IsFolded());
            seat.SetAllIn(p.IsAllIn);
            seat.SetActiveTurn(!handOver && i == currentPlayerId);
            seat.SetDealer(i == dealerId);
            seat.SetSB(i == sbId);
            seat.SetBB(i == bbId);
        }
    }

    private void UpdateBoardCards(Card[] boardCards, string streetName)
    {
        // Board cards are revealed incrementally via RevealFlop/Turn/River
        // This just ensures hidden cards stay hidden
        if (streetName == "Preflop")
        {
            HideCommunityCard(flopCard1);
            HideCommunityCard(flopCard2);
            HideCommunityCard(flopCard3);
            HideCommunityCard(turnCard);
            HideCommunityCard(riverCard);
        }
    }

    private void UpdateActionButtons(
        List<PokerPlayer> players,
        int currentBetToCall,
        int currentPlayerId,
        string streetName)
    {
        // Once the hand's over, nobody's turn means anything anymore, no matter what
        // currentPlayerId still happens to say. Never trust it past Showdown.
        bool isLocalTurn = streetName != "Showdown" && currentPlayerId == LOCAL_PLAYER_INDEX;
        var localPlayer = players[LOCAL_PLAYER_INDEX];

        // Disable all if not local player's turn
        foldButton.interactable = isLocalTurn && !localPlayer.IsFolded();
        checkButton.interactable = isLocalTurn && currentBetToCall == localPlayer.totalContributed;
        callButton.interactable = isLocalTurn && currentBetToCall > localPlayer.totalContributed;
        betButton.interactable = isLocalTurn && currentBetToCall == 0;
        raiseButton.interactable = isLocalTurn && currentBetToCall > 0;
        raiseSlider.interactable = isLocalTurn;

        // Update call button label
        int toCall = currentBetToCall - localPlayer.totalContributed;
        callAmountText.text = toCall > 0 ? $"Call  ${Mathf.Min(toCall, localPlayer.GetBalance())}" : "Call";
    }

    // Hand's over, so grey out every action button regardless of whose turn it
    // technically still is. Nothing should look clickable while waiting for the next hand.
    public void DisableActionButtons()
    {
        if (foldButton != null) foldButton.interactable = false;
        if (checkButton != null) checkButton.interactable = false;
        if (callButton != null) callButton.interactable = false;
        if (betButton != null) betButton.interactable = false;
        if (raiseButton != null) raiseButton.interactable = false;
        if (raiseSlider != null) raiseSlider.interactable = false;
    }

    private void UpdateRaiseSlider(List<PokerPlayer> players, int currentBetToCall)
    {
        cachedBetToCall = currentBetToCall;
        var local = players[LOCAL_PLAYER_INDEX];

        // Min raise-to = currentBetToCall + lastRaiseSize, rounded up so we never quote a value below the true minimum.
        int trueMinRaise = currentBetToCall > 0 ? currentBetToCall + gameMaster.GetLastRaiseSize() : 10;
        int minRaise = CeilToStep(trueMinRaise, RAISE_STEP);
        int maxRaise = local.totalContributed + local.GetBalance();

        raiseSlider.minValue = minRaise;
        raiseSlider.maxValue = Mathf.Max(minRaise, maxRaise);

        // Only reset slider to min if it's currently out of range
        if (raiseSlider.value < minRaise)
            raiseSlider.value = minRaise;

        raiseAmount = RoundToStep(Mathf.RoundToInt(raiseSlider.value), RAISE_STEP);
        UpdateRaiseAmountLabel();
    }

    // ──────────────────────────────
    //  Community Card Helpers
    // ──────────────────────────────

    private void SetCommunityCard(Image img, Card card)
    {
        img.sprite = CardSpriteLoader.GetSprite(card);
        img.enabled = true;
        img.color = new Color(1, 1, 1, 0); // start transparent for fade-in
    }

    private void HideCommunityCard(Image img)
    {
        img.enabled = false;
        img.color = Color.white;
    }

    private IEnumerator FadeIn(Image img)
    {
        img.enabled = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; // fade speed
            img.color = new Color(1, 1, 1, Mathf.Clamp01(t));
            yield return null;
        }
        img.color = Color.white;
    }

    private IEnumerator ClearMessageAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        messageText.text = "";
    }
}