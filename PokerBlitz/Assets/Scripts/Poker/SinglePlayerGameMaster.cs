using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class SinglePlayerGameMaster : MonoBehaviourPun
{
    // ────────────────────────────────────────────────────────────
    //  Enums
    // ────────────────────────────────────────────────────────────

    private enum Street { Preflop, Flop, Turn, River, Showdown }

    private enum Ranking
    {
        HighCard, Pair, TwoPair, ThreeOfAKind,
        Straight, Flush, FullHouse, FourOfAKind,
        StraightFlush, RoyalFlush
    }

    private static readonly string[] RankingNames =
    {
        "High Card", "Pair", "Two Pair", "Three of a Kind",
        "Straight", "Flush", "Full House", "Four of a Kind",
        "Straight Flush", "Royal Flush"
    };

    private struct HandResult
    {
        public int[] Score;
        public List<Card> BestFive;
    }

    // Records when and with how much a player busted, for tournament ranking.
    private class EliminationRecord
    {
        public TestPokerPlayer Player;
        public int HandEliminated;
        public int StackBeforeThatHand;
    }

    // ────────────────────────────────────────────────────────────
    //  Constants
    // ────────────────────────────────────────────────────────────

    private const int SMALL_BLIND = 5;
    private const int BIG_BLIND = 10;
    private const int STARTING_BALANCE = 1000;
    private const int MAX_HANDS = 10;

    // ────────────────────────────────────────────────────────────
    //  Game State
    // ────────────────────────────────────────────────────────────

    private List<TestPokerPlayer> pokerPlayers = new List<TestPokerPlayer>();

    private bool[,] deck;        // [denomination 0-12, suit 0-3]
    private Card[] boardCards;  // up to 5 community cards

    private Street currentStreet;

    private int dealerId = -1;
    private int sbId;
    private int bbId;
    private int playerId;        // whose turn it is

    public List<Pot> pots = new List<Pot>();

    private int currentBetToCall; // highest total contribution required this street
    private int lastRaiseSize;    // used to enforce min-raise

    private int handsPlayed;
    private readonly List<EliminationRecord> eliminationLog = new List<EliminationRecord>();
    private readonly Dictionary<TestPokerPlayer, int> stackAtHandStart = new Dictionary<TestPokerPlayer, int>();

    [Header("UI")]
    public PokerUIManager uiManager;

    private TestPokerPlayer CurrentPlayer => pokerPlayers[playerId];
    public TestPokerPlayer GetCurrentPlayer() => CurrentPlayer;
    public TestPokerPlayer GetPlayer(int index) => pokerPlayers[index];
    public int GetPlayerCount() => pokerPlayers.Count;
    public int GetLastRaiseSize() => lastRaiseSize;

    // ────────────────────────────────────────────────────────────
    //  Unity Entry Point
    // ────────────────────────────────────────────────────────────

    void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.PlayerList.Length > 0)
        {
            // Sort by actor number so every client builds the same seat order.
            var orderedPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber);

            foreach (var photonPlayer in orderedPlayers)
            {
                string displayName = string.IsNullOrEmpty(photonPlayer.NickName)
                    ? $"Player {photonPlayer.ActorNumber}"
                    : photonPlayer.NickName;

                var p = new TestPokerPlayer(displayName);
                p.ActorNumber = photonPlayer.ActorNumber;
                p.SetBalance(STARTING_BALANCE);
                pokerPlayers.Add(p);
            }

            // Only the master deals; everyone else waits for its broadcasts.
            if (PhotonNetwork.IsMasterClient)
                StartNewHand();
        }
        else
        {
            // Offline (e.g. Play in Editor with no room) - fall back to local AI names.
            string[] names = { "Moaz", "Tom", "Issa", "Charan" };
            foreach (string n in names)
            {
                var p = new TestPokerPlayer(n);
                p.SetBalance(STARTING_BALANCE);
                pokerPlayers.Add(p);
            }

            StartNewHand();
        }
    }

    public int GetLocalSeatIndex()
    {
        if (PhotonNetwork.LocalPlayer == null) return 0;

        for (int i = 0; i < pokerPlayers.Count; i++)
            if (pokerPlayers[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                return i;

        return 0;
    }

    private TestPokerPlayer FindPlayerByActor(int actorNumber) =>
        pokerPlayers.FirstOrDefault(p => p.ActorNumber == actorNumber);

    // ────────────────────────────────────────────────────────────
    //  Hand Lifecycle
    // ────────────────────────────────────────────────────────────

    private void StartNewHand()
    {
        // ── Eliminate broke players ───────────────────────────────
        // Marked, never removed - pokerPlayers must stay a fixed 4 slots so every
        // client's array indices keep lining up (see BroadcastPublicState). An
        // eliminated player just stays permanently folded from here on.
        var newlyEliminated = pokerPlayers.Where(p => p.GetBalance() == 0 && !p.IsEliminated).ToList();
        foreach (var e in newlyEliminated)
        {
            e.IsEliminated = true;
            int stackBefore = stackAtHandStart.TryGetValue(e, out var s) ? s : 0;
            eliminationLog.Add(new EliminationRecord
            {
                Player = e,
                HandEliminated = handsPlayed,
                StackBeforeThatHand = stackBefore
            });

            Debug.Log($"{e.GetName()} has been eliminated!");
        }

        // ── Tournament end check: 10 hands played, or down to one player ──
        int stillIn = pokerPlayers.Count(p => !p.IsEliminated);
        if (handsPlayed >= MAX_HANDS || stillIn < 2)
        {
            EndTournament();
            return;
        }

        handsPlayed++;

        // ── Reset hand state ──────────────────────────────────────
        pots.Clear();
        deck = new bool[13, 4];
        boardCards = new Card[5];

        if (uiManager != null) uiManager.ResetBoard();

        foreach (var p in pokerPlayers)
            p.ResetForNewHand();

        ResetBettingState();

        stackAtHandStart.Clear();
        foreach (var p in pokerPlayers)
            stackAtHandStart[p] = p.GetBalance();

        // ── Advance dealer button ─────────────────────────────────
        // Clamp dealerId in case a player was removed and list shrank
        dealerId = dealerId % pokerPlayers.Count;
        do { dealerId = (dealerId + 1) % pokerPlayers.Count; }
        while (pokerPlayers[dealerId].GetBalance() == 0);

        // ── Blinds ────────────────────────────────────────────────
        sbId = GetNextActiveIndex(dealerId);
        bbId = GetNextActiveIndex(sbId);

        // ── Deal pocket cards (starting from SB, going around) ────
        int idx = sbId;
        do
        {
            if (pokerPlayers[idx].GetBalance() > 0)
                pokerPlayers[idx].DealPocket(
                    new Pocket(GenerateUniqueCard(), GenerateUniqueCard()));

            idx = (idx + 1) % pokerPlayers.Count;
        }
        while (idx != sbId);

        foreach (var p in pokerPlayers)
            if (p.GetBalance() > 0)
                Debug.Log($"{p.GetName()} dealt: {p.GetPocket()}");

        if (uiManager != null) uiManager.DealPocketCards(pokerPlayers);

        currentStreet = Street.Preflop;

        PostBlinds();

        // First to act preflop = player after BB
        playerId = GetNextActiveIndex(bbId);
        Debug.Log($"--- Preflop begins. First to act: {CurrentPlayer.GetName()} ---");

        // State first, then hole cards, so the board is already reset when cards land.
        RefreshUI(isNewHand: true);
        DealPocketCardsOverNetwork();
    }

    // ────────────────────────────────────────────────────────────
    //  Blinds  (bypass normal bet validation)
    // ────────────────────────────────────────────────────────────

    private void PostBlinds()
    {
        PostBlind(pokerPlayers[sbId], SMALL_BLIND);
        PostBlind(pokerPlayers[bbId], BIG_BLIND);

        currentBetToCall = BIG_BLIND;
        lastRaiseSize = BIG_BLIND;

        Debug.Log($"{pokerPlayers[sbId].GetName()} posts SB ({SMALL_BLIND})");
        Debug.Log($"{pokerPlayers[bbId].GetName()} posts BB ({BIG_BLIND})");
    }

    private void PostBlind(TestPokerPlayer player, int amount)
    {
        int actual = Mathf.Min(amount, player.GetBalance());
        player.SetBalance(player.GetBalance() - actual);
        player.totalContributed = actual;
        AddToPot(player, actual);
    }

    // ────────────────────────────────────────────────────────────
    //  Public Actions
    // ────────────────────────────────────────────────────────────

    public bool RequestFold(TestPokerPlayer player)
    {
        if (!IsPlayerTurn(player)) { Debug.Log("Not your turn."); return false; }
        if (player.IsFolded()) { Debug.Log("Already folded."); return false; }

        player.Fold();
        player.hasActedThisStreet = true;

        foreach (var pot in pots)
            pot.eligiblePlayers.Remove(player);

        Debug.Log($"{player.GetName()} folds.");

        var active = pokerPlayers.Where(p => !p.IsFolded()).ToList();
        if (active.Count == 1)
        {
            var winner = active[0];
            int total = pots.Sum(pot => pot.amount);
            bool isLocalWinner = winner == GetPlayer(GetLocalSeatIndex());

            if (uiManager != null)
            {
                uiManager.ShowHandResult($"{winner.GetName()} wins ${total} (everyone else folded)");
                if (isLocalWinner) uiManager.SetShowCardsButtonVisible(true);
                uiManager.DisableActionButtons();
            }

            // Mark the hand over so UpdateActionButtons stops trusting whose turn it
            // technically still is - this fold never advances the turn, so without
            // this, a refresh could re-enable buttons based on stale turn state.
            currentStreet = Street.Showdown;

            AwardPotToLastPlayer(winner);
            BroadcastUncontestedWin(winner, total);
            RefreshUI();
            PrepareNextHand();
            return true;
        }

        AdvanceTurn();
        if (IsBettingRoundComplete()) EndBettingRound();
        RefreshUI();
        return true;
    }

    public bool RequestCheck(TestPokerPlayer player)
    {
        if (!IsPlayerTurn(player)) { Debug.Log("Not your turn."); return false; }
        if (player.IsFolded()) { Debug.Log("Already folded."); return false; }
        if (player.IsAllIn) { Debug.Log("You are all-in."); return false; }

        if (currentBetToCall != player.totalContributed)
        {
            Debug.Log($"Cannot check – must call {currentBetToCall - player.totalContributed}.");
            return false;
        }

        player.hasActedThisStreet = true;
        Debug.Log($"{player.GetName()} checks.");

        AdvanceTurn();
        if (IsBettingRoundComplete()) EndBettingRound();
        RefreshUI();
        return true;
    }

    public bool RequestCall(TestPokerPlayer player)
    {
        if (!IsPlayerTurn(player)) { Debug.Log("Not your turn."); return false; }
        if (player.IsFolded()) { Debug.Log("Already folded."); return false; }
        if (player.IsAllIn) { Debug.Log("You are all-in."); return false; }

        int toCall = currentBetToCall - player.totalContributed;
        if (toCall <= 0) { Debug.Log("Nothing to call – use Check."); return false; }

        int actual = Mathf.Min(toCall, player.GetBalance());
        player.SetBalance(player.GetBalance() - actual);
        player.totalContributed += actual;
        AddToPot(player, actual);

        player.hasActedThisStreet = true;
        Debug.Log($"{player.GetName()} calls {actual}.");

        AdvanceTurn();
        if (IsBettingRoundComplete()) EndBettingRound();
        RefreshUI();
        return true;
    }

    public bool RequestBet(TestPokerPlayer player, int amount)
    {
        if (!IsPlayerTurn(player)) { Debug.Log("Not your turn."); return false; }
        if (player.IsFolded()) { Debug.Log("Already folded."); return false; }
        if (player.IsAllIn) { Debug.Log("You are all-in."); return false; }

        if (currentBetToCall != 0)
        {
            Debug.Log("There is already a bet – use Raise instead.");
            return false;
        }

        if (amount < BIG_BLIND)
        {
            Debug.Log($"Minimum bet is {BIG_BLIND}.");
            return false;
        }

        int actual = Mathf.Min(amount, player.GetBalance());
        player.SetBalance(player.GetBalance() - actual);
        player.totalContributed += actual;
        AddToPot(player, actual);

        lastRaiseSize = actual;
        currentBetToCall = actual;

        player.hasActedThisStreet = true;
        ResetHasActedExcept(player);

        Debug.Log($"{player.GetName()} bets {actual}.");

        AdvanceTurn();
        if (IsBettingRoundComplete()) EndBettingRound();
        RefreshUI();
        return true;
    }

    public bool RequestRaise(TestPokerPlayer player, int raiseToAmount)
    {
        if (!IsPlayerTurn(player)) { Debug.Log("Not your turn."); return false; }
        if (player.IsFolded()) { Debug.Log("Already folded."); return false; }
        if (player.IsAllIn) { Debug.Log("You are all-in."); return false; }

        if (raiseToAmount <= currentBetToCall)
        {
            Debug.Log($"Raise must be above current bet of {currentBetToCall}.");
            return false;
        }

        int raiseBy = raiseToAmount - currentBetToCall;
        int maxPossible = player.totalContributed + player.GetBalance();

        if (raiseBy < lastRaiseSize && raiseToAmount != maxPossible)
        {
            Debug.Log($"Min raise is {lastRaiseSize}. Raise to at least {currentBetToCall + lastRaiseSize}.");
            return false;
        }

        int chipsNeeded = raiseToAmount - player.totalContributed;
        if (chipsNeeded > player.GetBalance()) { Debug.Log("Not enough chips."); return false; }

        int actual = Mathf.Min(chipsNeeded, player.GetBalance());
        player.SetBalance(player.GetBalance() - actual);
        player.totalContributed += actual;
        AddToPot(player, actual);

        lastRaiseSize = raiseBy;
        currentBetToCall = raiseToAmount;

        player.hasActedThisStreet = true;
        ResetHasActedExcept(player);

        Debug.Log($"{player.GetName()} raises to {raiseToAmount}.");

        AdvanceTurn();
        if (IsBettingRoundComplete()) EndBettingRound();
        RefreshUI();
        return true;
    }

    // ────────────────────────────────────────────────────────────
    //  Networked Action Requests (UI calls these, not Request* directly)
    // ────────────────────────────────────────────────────────────

    public void SendFoldRequest()
    {
        if (PhotonNetwork.InRoom) photonView.RPC(nameof(RPC_Fold), RpcTarget.MasterClient);
        else RequestFold(GetPlayer(GetLocalSeatIndex()));
    }

    public void SendCheckRequest()
    {
        if (PhotonNetwork.InRoom) photonView.RPC(nameof(RPC_Check), RpcTarget.MasterClient);
        else RequestCheck(GetPlayer(GetLocalSeatIndex()));
    }

    public void SendCallRequest()
    {
        if (PhotonNetwork.InRoom) photonView.RPC(nameof(RPC_Call), RpcTarget.MasterClient);
        else RequestCall(GetPlayer(GetLocalSeatIndex()));
    }

    public void SendBetRequest(int amount)
    {
        if (PhotonNetwork.InRoom) photonView.RPC(nameof(RPC_Bet), RpcTarget.MasterClient, amount);
        else RequestBet(GetPlayer(GetLocalSeatIndex()), amount);
    }

    public void SendRaiseRequest(int amount)
    {
        if (PhotonNetwork.InRoom) photonView.RPC(nameof(RPC_Raise), RpcTarget.MasterClient, amount);
        else RequestRaise(GetPlayer(GetLocalSeatIndex()), amount);
    }

    [PunRPC]
    private void RPC_Fold(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var player = FindPlayerByActor(info.Sender.ActorNumber);
        if (player != null) RequestFold(player);
    }

    [PunRPC]
    private void RPC_Check(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var player = FindPlayerByActor(info.Sender.ActorNumber);
        if (player != null) RequestCheck(player);
    }

    [PunRPC]
    private void RPC_Call(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var player = FindPlayerByActor(info.Sender.ActorNumber);
        if (player != null) RequestCall(player);
    }

    [PunRPC]
    private void RPC_Bet(int amount, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var player = FindPlayerByActor(info.Sender.ActorNumber);
        if (player != null) RequestBet(player, amount);
    }

    [PunRPC]
    private void RPC_Raise(int raiseToAmount, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var player = FindPlayerByActor(info.Sender.ActorNumber);
        if (player != null) RequestRaise(player, raiseToAmount);
    }

    // ────────────────────────────────────────────────────────────
    //  Betting Helpers
    // ────────────────────────────────────────────────────────────

    private bool IsPlayerTurn(TestPokerPlayer player) => player == CurrentPlayer;

    private void RefreshUI(bool isNewHand = false)
    {
        if (uiManager != null)
            uiManager.RefreshUI(pokerPlayers, boardCards, pots, currentBetToCall,
                playerId, dealerId, sbId, bbId, currentStreet.ToString());

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            BroadcastPublicState(isNewHand);
    }

    // ────────────────────────────────────────────────────────────
    //  Networking (Photon) — master-authoritative state sync
    // ────────────────────────────────────────────────────────────

    // Everything needed to render the table, minus hole cards.
    private void BroadcastPublicState(bool isNewHand)
    {
        if (!PhotonNetwork.InRoom) return;

        int n = pokerPlayers.Count;
        int[] balances = new int[n];
        int[] contributed = new int[n];
        bool[] folded = new bool[n];

        for (int i = 0; i < n; i++)
        {
            balances[i] = pokerPlayers[i].GetBalance();
            contributed[i] = pokerPlayers[i].totalContributed;
            folded[i] = pokerPlayers[i].IsFolded();
        }

        int potTotal = pots.Sum(p => p.amount);

        photonView.RPC(nameof(RPC_SyncPublicState), RpcTarget.Others,
            balances, contributed, folded, potTotal, currentBetToCall, lastRaiseSize,
            playerId, dealerId, sbId, bbId, (int)currentStreet, isNewHand);
    }

    [PunRPC]
    private void RPC_SyncPublicState(int[] balances, int[] contributed, bool[] folded, int potTotal,
        int betToCall, int lastRaise, int turnIndex, int dealer, int sb, int bb, int streetIndex, bool isNewHand)
    {
        if (PhotonNetwork.IsMasterClient) return;

        if (isNewHand)
        {
            if (uiManager != null) uiManager.ResetBoard();
            boardCards = new Card[5];
            foreach (var p in pokerPlayers) p.ResetForNewHand();
        }

        for (int i = 0; i < pokerPlayers.Count && i < balances.Length; i++)
        {
            pokerPlayers[i].SetBalance(balances[i]);
            pokerPlayers[i].totalContributed = contributed[i];
            if (folded[i]) pokerPlayers[i].Fold();
        }

        currentBetToCall = betToCall;
        lastRaiseSize = lastRaise;
        playerId = turnIndex;
        dealerId = dealer;
        sbId = sb;
        bbId = bb;
        currentStreet = (Street)streetIndex;

        pots.Clear();
        var syncedPot = new Pot();
        syncedPot.amount = potTotal;
        pots.Add(syncedPot);

        if (uiManager != null)
            uiManager.RefreshUI(pokerPlayers, boardCards, pots, currentBetToCall,
                playerId, dealerId, sbId, bbId, currentStreet.ToString());
    }

    // Each player's hole cards go out as a targeted RPC, never broadcast.
    private void DealPocketCardsOverNetwork()
    {
        if (!PhotonNetwork.InRoom) return;

        foreach (var p in pokerPlayers)
        {
            if (p.ActorNumber < 0 || p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) continue;

            var targetPlayer = PhotonNetwork.CurrentRoom.Players.Values
                .FirstOrDefault(pl => pl.ActorNumber == p.ActorNumber);
            if (targetPlayer == null) continue;

            var cards = p.GetPocket().GetCards();
            photonView.RPC(nameof(RPC_DealPocket), targetPlayer,
                (int)cards[0].GetDenomination(), (int)cards[0].GetSuit(),
                (int)cards[1].GetDenomination(), (int)cards[1].GetSuit());
        }
    }

    [PunRPC]
    private void RPC_DealPocket(int d1, int s1, int d2, int s2)
    {
        var card1 = new Card((Card.Denomination)d1, (Card.Suit)s1);
        var card2 = new Card((Card.Denomination)d2, (Card.Suit)s2);
        pokerPlayers[GetLocalSeatIndex()].DealPocket(new Pocket(card1, card2));

        if (uiManager != null) uiManager.DealPocketCards(pokerPlayers);
    }

    // Voluntary reveal (e.g. after winning uncontested) - broadcast to everyone
    // else, since showing your own cards only matters if other people see it.
    public void SendShowCardsRequest()
    {
        if (!PhotonNetwork.InRoom) return;

        var cards = GetPlayer(GetLocalSeatIndex()).GetPocket().GetCards();
        photonView.RPC(nameof(RPC_RevealOwnCards), RpcTarget.Others,
            (int)cards[0].GetDenomination(), (int)cards[0].GetSuit(),
            (int)cards[1].GetDenomination(), (int)cards[1].GetSuit());
    }

    [PunRPC]
    private void RPC_RevealOwnCards(int d1, int s1, int d2, int s2, PhotonMessageInfo info)
    {
        var player = FindPlayerByActor(info.Sender.ActorNumber);
        if (player == null) return;

        var card1 = new Card((Card.Denomination)d1, (Card.Suit)s1);
        var card2 = new Card((Card.Denomination)d2, (Card.Suit)s2);
        player.DealPocket(new Pocket(card1, card2));

        if (uiManager != null)
            uiManager.RevealSeatCards(pokerPlayers.IndexOf(player), card1, card2);
    }

    private void BroadcastUncontestedWin(TestPokerPlayer winner, int total)
    {
        if (!PhotonNetwork.InRoom) return;

        int n = pokerPlayers.Count;
        int[] balances = new int[n];
        for (int i = 0; i < n; i++)
            balances[i] = pokerPlayers[i].GetBalance();

        photonView.RPC(nameof(RPC_UncontestedWin), RpcTarget.Others, winner.ActorNumber, total, balances);
    }

    [PunRPC]
    private void RPC_UncontestedWin(int winnerActorNumber, int total, int[] balances)
    {
        if (PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < pokerPlayers.Count && i < balances.Length; i++)
            pokerPlayers[i].SetBalance(balances[i]);

        var winner = FindPlayerByActor(winnerActorNumber);
        if (uiManager != null && winner != null)
        {
            uiManager.ShowHandResult($"{winner.GetName()} wins ${total} (everyone else folded)");
            if (winnerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                uiManager.SetShowCardsButtonVisible(true);
            uiManager.DisableActionButtons();
        }

        pots.Clear();
    }

    private bool IsBettingRoundComplete()
    {
        var active = pokerPlayers.Where(p => !p.IsFolded() && !p.IsAllIn).ToList();

        if (active.Count <= 1)
            return true;

        if (active.Any(p => !p.hasActedThisStreet))
            return false;

        if (active.Any(p => p.totalContributed != currentBetToCall))
            return false;

        return true;
    }

    private void ResetHasActedExcept(TestPokerPlayer actor)
    {
        foreach (var p in pokerPlayers)
            if (!p.IsFolded() && !p.IsAllIn)
                p.hasActedThisStreet = false;

        actor.hasActedThisStreet = true;
    }

    private void ResetBettingState()
    {
        currentBetToCall = 0;
        lastRaiseSize = BIG_BLIND;

        foreach (var p in pokerPlayers)
        {
            p.totalContributed = 0;
            p.hasActedThisStreet = false;
        }
    }

    private void AdvanceTurn()
    {
        playerId = GetNextActiveIndex(playerId);
        Debug.Log($"Turn: {CurrentPlayer.GetName()}");
    }

    // Next player with chips who hasn't folded/isn't all-in. Safety counter guards
    // against an infinite loop if literally everyone left is folded/all-in/broke.
    private int GetNextActiveIndex(int fromIndex)
    {
        int idx = fromIndex;
        int safety = pokerPlayers.Count;

        do
        {
            idx = (idx + 1) % pokerPlayers.Count;
            safety--;
        }
        while (
            (pokerPlayers[idx].GetBalance() == 0 ||
             pokerPlayers[idx].IsFolded() ||
             pokerPlayers[idx].IsAllIn)
            && safety > 0
        );

        return idx;
    }

    private void SetFirstPlayerToActPostFlop()
    {
        // First active player to the left of the dealer
        playerId = GetNextActiveIndex(dealerId);
        Debug.Log($"First to act: {CurrentPlayer.GetName()}");
    }

    // ────────────────────────────────────────────────────────────
    //  Street Progression
    // ────────────────────────────────────────────────────────────

    private void EndBettingRound()
    {
        Debug.Log($"--- Betting round over ({currentStreet}) ---");
        LogPots();
        ResetBettingState();

        switch (currentStreet)
        {
            case Street.Preflop:
                currentStreet = Street.Flop;
                RevealFlop();
                SetFirstPlayerToActPostFlop();
                break;

            case Street.Flop:
                currentStreet = Street.Turn;
                RevealTurn();
                SetFirstPlayerToActPostFlop();
                break;

            case Street.Turn:
                currentStreet = Street.River;
                RevealRiver();
                SetFirstPlayerToActPostFlop();
                break;

            case Street.River:
                currentStreet = Street.Showdown;
                ResolveShowdown();
                break;
        }
    }

    private void RevealFlop()
    {
        boardCards[0] = GenerateUniqueCard();
        boardCards[1] = GenerateUniqueCard();
        boardCards[2] = GenerateUniqueCard();
        Debug.Log($"=== FLOP: {boardCards[0]}  {boardCards[1]}  {boardCards[2]} ===");
        if (uiManager != null) uiManager.RevealFlop(boardCards[0], boardCards[1], boardCards[2]);

        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RPC_RevealFlop), RpcTarget.Others,
                (int)boardCards[0].GetDenomination(), (int)boardCards[0].GetSuit(),
                (int)boardCards[1].GetDenomination(), (int)boardCards[1].GetSuit(),
                (int)boardCards[2].GetDenomination(), (int)boardCards[2].GetSuit());
    }

    private void RevealTurn()
    {
        boardCards[3] = GenerateUniqueCard();
        Debug.Log($"=== TURN: {boardCards[3]} ===");
        if (uiManager != null) uiManager.RevealTurn(boardCards[3]);

        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RPC_RevealTurn), RpcTarget.Others,
                (int)boardCards[3].GetDenomination(), (int)boardCards[3].GetSuit());
    }

    private void RevealRiver()
    {
        boardCards[4] = GenerateUniqueCard();
        Debug.Log($"=== RIVER: {boardCards[4]} ===");
        if (uiManager != null) uiManager.RevealRiver(boardCards[4]);

        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RPC_RevealRiver), RpcTarget.Others,
                (int)boardCards[4].GetDenomination(), (int)boardCards[4].GetSuit());
    }

    [PunRPC]
    private void RPC_RevealFlop(int d1, int s1, int d2, int s2, int d3, int s3)
    {
        if (PhotonNetwork.IsMasterClient) return;
        boardCards[0] = new Card((Card.Denomination)d1, (Card.Suit)s1);
        boardCards[1] = new Card((Card.Denomination)d2, (Card.Suit)s2);
        boardCards[2] = new Card((Card.Denomination)d3, (Card.Suit)s3);
        if (uiManager != null) uiManager.RevealFlop(boardCards[0], boardCards[1], boardCards[2]);
    }

    [PunRPC]
    private void RPC_RevealTurn(int d, int s)
    {
        if (PhotonNetwork.IsMasterClient) return;
        boardCards[3] = new Card((Card.Denomination)d, (Card.Suit)s);
        if (uiManager != null) uiManager.RevealTurn(boardCards[3]);
    }

    [PunRPC]
    private void RPC_RevealRiver(int d, int s)
    {
        if (PhotonNetwork.IsMasterClient) return;
        boardCards[4] = new Card((Card.Denomination)d, (Card.Suit)s);
        if (uiManager != null) uiManager.RevealRiver(boardCards[4]);
    }

    // ────────────────────────────────────────────────────────────
    //  Pot Management
    // ────────────────────────────────────────────────────────────

    private void AddToPot(TestPokerPlayer player, int amount)
    {
        int remaining = amount;

        if (pots.Count == 0)
        {
            var mainPot = new Pot();
            mainPot.amount = remaining;
            foreach (var p in pokerPlayers)
                if (!p.IsFolded())
                    mainPot.eligiblePlayers.Add(p);
            pots.Add(mainPot);
            return;
        }

        foreach (var pot in pots)
        {
            if (remaining <= 0) break;

            int cap = pot.eligiblePlayers
                .Where(p => p.IsAllIn)
                .Select(p => p.totalContributed)
                .DefaultIfEmpty(int.MaxValue)
                .Min();

            if (cap == int.MaxValue)
            {
                // No all-in cap – everything goes here
                pot.amount += remaining;
                remaining = 0;
            }
            else
            {
                int canAdd = Mathf.Max(0, cap - (player.totalContributed - amount));
                int toAdd = Mathf.Min(remaining, canAdd);
                pot.amount += toAdd;
                remaining -= toAdd;
            }
        }

        // Any overflow creates a new side pot
        if (remaining > 0)
        {
            var sidePot = new Pot();
            sidePot.amount = remaining;
            foreach (var p in pokerPlayers)
                if (!p.IsFolded() && p.totalContributed >= player.totalContributed)
                    sidePot.eligiblePlayers.Add(p);
            pots.Add(sidePot);
        }
    }

    private void AwardPotToLastPlayer(TestPokerPlayer winner)
    {
        int total = pots.Sum(pot => pot.amount);
        winner.SetBalance(winner.GetBalance() + total);
        Debug.Log($"{winner.GetName()} wins {total} (everyone else folded).");
        pots.Clear();
    }

    private void LogPots()
    {
        for (int i = 0; i < pots.Count; i++)
            Debug.Log($"Pot {i}: {pots[i].amount} chips " +
                      $"({pots[i].eligiblePlayers.Count} eligible players)");
    }

    // ────────────────────────────────────────────────────────────
    //  Showdown  (with split pot support)
    // ────────────────────────────────────────────────────────────

    private void ResolveShowdown()
    {
        Debug.Log("=== SHOWDOWN ===");

        foreach (var p in pokerPlayers.Where(p => !p.IsFolded()))
            Debug.Log($"{p.GetName()}: {p.GetPocket()}");

        if (uiManager != null)
        {
            uiManager.ShowShowdown(pokerPlayers);
            uiManager.DisableActionButtons();
        }

        string resultMessage = null;
        TestPokerPlayer displayWinner = null;
        List<Card> winningFive = null;

        foreach (var pot in pots)
        {
            var contenders = pot.eligiblePlayers.Where(p => !p.IsFolded()).ToList();
            if (contenders.Count == 0) continue;

            var results = contenders.ToDictionary(p => p, p => GetPlayerHandResult(p));

            // ── Find the best score ───────────────────────────────
            int[] bestScore = null;
            foreach (var r in results.Values)
                if (bestScore == null || CompareScores(r.Score, bestScore) > 0)
                    bestScore = r.Score;

            // ── Collect everyone who ties for best ────────────────
            var winners = contenders
                .Where(p => CompareScores(results[p].Score, bestScore) == 0)
                .ToList();

            // ── Split pot (odd chip goes to first winner left of dealer) ──
            int share = pot.amount / winners.Count;
            int remainder = pot.amount % winners.Count;

            for (int i = 0; i < winners.Count; i++)
            {
                int award = share + (i == 0 ? remainder : 0);
                winners[i].SetBalance(winners[i].GetBalance() + award);
            }

            string rankName = RankingNames[bestScore[0]];
            string potMessage = winners.Count == 1
                ? $"{winners[0].GetName()} wins ${pot.amount} with {rankName}!"
                : $"Split pot of ${pot.amount} ({rankName}) between: " +
                  string.Join(", ", winners.Select(w => w.GetName()));

            Debug.Log(potMessage);

            if (resultMessage == null)
            {
                resultMessage = potMessage;
                displayWinner = winners[0];
                winningFive = results[winners[0]].BestFive;
            }
        }

        if (uiManager != null && resultMessage != null)
        {
            uiManager.ShowHandResult(resultMessage);
            uiManager.HighlightWinningHand(pokerPlayers.IndexOf(displayWinner), displayWinner, boardCards, winningFive);
        }

        if (resultMessage != null) BroadcastShowdown(resultMessage, displayWinner, winningFive);

        pots.Clear();
        LogBalances();
        PrepareNextHand();
    }

    // Hand's over, so hole card privacy no longer applies - safe to reveal to everyone.
    private void BroadcastShowdown(string resultMessage, TestPokerPlayer displayWinner, List<Card> winningFive)
    {
        if (!PhotonNetwork.InRoom) return;

        var revealed = pokerPlayers.Where(p => !p.IsFolded()).ToList();
        int[] actorNumbers = revealed.Select(p => p.ActorNumber).ToArray();
        int[] cardInts = new int[revealed.Count * 4];

        for (int i = 0; i < revealed.Count; i++)
        {
            var cards = revealed[i].GetPocket().GetCards();
            cardInts[i * 4 + 0] = (int)cards[0].GetDenomination();
            cardInts[i * 4 + 1] = (int)cards[0].GetSuit();
            cardInts[i * 4 + 2] = (int)cards[1].GetDenomination();
            cardInts[i * 4 + 3] = (int)cards[1].GetSuit();
        }

        int winnerActorNumber = displayWinner != null ? displayWinner.ActorNumber : -1;
        int[] winningFiveInts = null;
        if (winningFive != null)
        {
            winningFiveInts = new int[winningFive.Count * 2];
            for (int i = 0; i < winningFive.Count; i++)
            {
                winningFiveInts[i * 2] = (int)winningFive[i].GetDenomination();
                winningFiveInts[i * 2 + 1] = (int)winningFive[i].GetSuit();
            }
        }

        photonView.RPC(nameof(RPC_Showdown), RpcTarget.Others,
            actorNumbers, cardInts, resultMessage, winnerActorNumber, winningFiveInts);
    }

    [PunRPC]
    private void RPC_Showdown(int[] revealActorNumbers, int[] revealCardInts, string resultMessage,
        int winnerActorNumber, int[] winningFiveInts)
    {
        if (PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < revealActorNumbers.Length; i++)
        {
            var player = FindPlayerByActor(revealActorNumbers[i]);
            if (player == null) continue;

            int baseIdx = i * 4;
            var c1 = new Card((Card.Denomination)revealCardInts[baseIdx], (Card.Suit)revealCardInts[baseIdx + 1]);
            var c2 = new Card((Card.Denomination)revealCardInts[baseIdx + 2], (Card.Suit)revealCardInts[baseIdx + 3]);
            player.DealPocket(new Pocket(c1, c2));
        }

        if (uiManager != null)
        {
            uiManager.ShowShowdown(pokerPlayers);
            uiManager.DisableActionButtons();
        }

        if (uiManager != null && resultMessage != null)
        {
            uiManager.ShowHandResult(resultMessage);

            var winner = FindPlayerByActor(winnerActorNumber);
            if (winner != null && winningFiveInts != null)
            {
                var winningFive = new List<Card>();
                for (int i = 0; i < winningFiveInts.Length; i += 2)
                    winningFive.Add(new Card((Card.Denomination)winningFiveInts[i], (Card.Suit)winningFiveInts[i + 1]));

                uiManager.HighlightWinningHand(pokerPlayers.IndexOf(winner), winner, boardCards, winningFive);
            }
        }
    }

    // Hand's over - wait for the host to press Start Next Hand instead of an auto-timer.
    private void PrepareNextHand()
    {
        bool isHost = !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;

        if (uiManager != null)
        {
            uiManager.SetStatusText(isHost ? "" : "Waiting for host to start the next hand...");
            uiManager.SetStartNextHandButtonVisible(isHost);
        }
    }

    // Only the host's button ever calls this - everyone else's copy is hidden, but
    // guard anyway in case a networked non-host click gets through some other way.
    public void RequestStartNextHand()
    {
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;

        if (uiManager != null)
        {
            uiManager.SetStartNextHandButtonVisible(false);
            uiManager.SetStatusText("");
        }

        StartNewHand();
    }

    // Builds 7-card pool for a player and returns their best 5-card hand
    private HandResult GetPlayerHandResult(TestPokerPlayer player)
    {
        var seven = new List<Card>();
        seven.AddRange(player.GetPocket().GetCards());
        foreach (var bc in boardCards)
            if (bc != null) seven.Add(bc);
        return EvaluateHand(seven);
    }

    // ────────────────────────────────────────────────────────────
    //  Hand Evaluation
    // ────────────────────────────────────────────────────────────

    // Tries all C(7,5)=21 combinations, keeps the best score and the 5 cards behind it.
    private HandResult EvaluateHand(List<Card> sevenCards)
    {
        HandResult best = default;
        bool found = false;

        for (int a = 0; a < sevenCards.Count - 4; a++)
            for (int b = a + 1; b < sevenCards.Count - 3; b++)
                for (int c = b + 1; c < sevenCards.Count - 2; c++)
                    for (int d = c + 1; d < sevenCards.Count - 1; d++)
                        for (int e = d + 1; e < sevenCards.Count; e++)
                        {
                            var five = new List<Card>
                { sevenCards[a], sevenCards[b], sevenCards[c], sevenCards[d], sevenCards[e] };
                            var score = ScoreFiveCards(five);
                            if (!found || CompareScores(score, best.Score) > 0)
                            {
                                best = new HandResult { Score = score, BestFive = five };
                                found = true;
                            }
                        }

        return best;
    }

    // Returns int[0]=rank, then tiebreaker card values in priority order. Higher array = better hand.
    private int[] ScoreFiveCards(List<Card> cards)
    {
        var vals = cards
            .Select(c => (int)c.GetDenomination()) // Two=0 … Ace=12
            .OrderByDescending(v => v)
            .ToList();

        var suits = cards.Select(c => (int)c.GetSuit()).ToList();
        bool flush = suits.Distinct().Count() == 1;

        bool straight = false;
        int straightHigh = 0;

        // Normal straight
        if (vals[0] - vals[4] == 4 && vals.Distinct().Count() == 5)
        {
            straight = true;
            straightHigh = vals[0];
        }

        // Wheel: A-2-3-4-5
        if (!straight && vals.SequenceEqual(new[] { 12, 3, 2, 1, 0 }))
        {
            straight = true;
            straightHigh = 3;
        }

        var groups = vals
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        int[] counts = groups.Select(g => g.Count()).ToArray();
        int[] keys = groups.Select(g => g.Key).ToArray();

        if (flush && straight && straightHigh == 12)
            return new[] { (int)Ranking.RoyalFlush };

        if (flush && straight)
            return new[] { (int)Ranking.StraightFlush, straightHigh };

        if (counts[0] == 4)
            return new[] { (int)Ranking.FourOfAKind, keys[0], keys[1] };

        if (counts[0] == 3 && counts[1] == 2)
            return new[] { (int)Ranking.FullHouse, keys[0], keys[1] };

        if (flush)
            return new[] { (int)Ranking.Flush }.Concat(vals).ToArray();

        if (straight)
            return new[] { (int)Ranking.Straight, straightHigh };

        if (counts[0] == 3)
            return new[] { (int)Ranking.ThreeOfAKind, keys[0], keys[1], keys[2] };

        if (counts[0] == 2 && counts[1] == 2)
            return new[] { (int)Ranking.TwoPair, keys[0], keys[1], keys[2] };

        if (counts[0] == 2)
            return new[] { (int)Ranking.Pair, keys[0], keys[1], keys[2], keys[3] };

        return new[] { (int)Ranking.HighCard }.Concat(vals).ToArray();
    }

    // +1 if a wins, -1 if b wins, 0 if tied.
    private int CompareScores(int[] a, int[] b)
    {
        int len = Mathf.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            if (a[i] > b[i]) return 1;
            if (a[i] < b[i]) return -1;
        }
        return 0;
    }

    // ────────────────────────────────────────────────────────────
    //  Card Generation
    // ────────────────────────────────────────────────────────────

    private Card GenerateUniqueCard()
    {
        int d, s;
        do
        {
            d = Random.Range(0, 13);
            s = Random.Range(0, 4);
        }
        while (deck[d, s]);

        deck[d, s] = true;
        return new Card((Card.Denomination)d, (Card.Suit)s);
    }

    // ────────────────────────────────────────────────────────────
    //  Logging
    // ────────────────────────────────────────────────────────────

    private void LogBalances()
    {
        Debug.Log("── Balances ──");
        foreach (var p in pokerPlayers)
            Debug.Log($"  {p.GetName()}: {p.GetBalance()}");
    }

    // ────────────────────────────────────────────────────────────
    //  Tournament End / Final Ranking
    //  Entry point for the minigame handoff: call GetFinalRanking()
    //  once the tournament is over to get the 4 players best-to-worst.
    // ────────────────────────────────────────────────────────────

    // The result of GetFinalRanking(), null until EndTournament() has run - either
    // computed locally (master) or received via RPC_FinalRanking (everyone else).
    // Non-master clients never track eliminations locally, so GetFinalRanking()
    // must hand back this cached, broadcast value rather than recomputing anything.
    private List<TestPokerPlayer> cachedFinalRanking;

    private void EndTournament()
    {
        Debug.Log($"=== TOURNAMENT OVER (after {handsPlayed} hands) ===");

        cachedFinalRanking = ComputeFinalRanking();
        for (int i = 0; i < cachedFinalRanking.Count; i++)
            Debug.Log($"  {i + 1}. {cachedFinalRanking[i].GetName()} - ${cachedFinalRanking[i].GetBalance()}");

        if (uiManager != null)
            uiManager.ShowMessage($"{cachedFinalRanking[0].GetName()} finishes 1st!", 10f);

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            BroadcastFinalRanking(cachedFinalRanking);
    }

    // Still-standing players first by current balance, then eliminated players by
    // who survived longer, same-hand eliminations broken by chips going into it.
    // Only ever accurate when called on the master - use GetFinalRanking() elsewhere.
    private List<TestPokerPlayer> ComputeFinalRanking()
    {
        var ranking = new List<TestPokerPlayer>();

        ranking.AddRange(pokerPlayers.Where(p => !p.IsEliminated).OrderByDescending(p => p.GetBalance()));

        ranking.AddRange(eliminationLog
            .OrderByDescending(e => e.HandEliminated)
            .ThenByDescending(e => e.StackBeforeThatHand)
            .Select(e => e.Player));

        return ranking;
    }

    // The 4 players ordered best to worst. Safe to call from any client once the
    // tournament has ended - null before that. Handoff point for the minigame:
    // higher position = better powerup.
    public List<TestPokerPlayer> GetFinalRanking() => cachedFinalRanking;

    private void BroadcastFinalRanking(List<TestPokerPlayer> ranking)
    {
        if (!PhotonNetwork.InRoom) return;
        photonView.RPC(nameof(RPC_FinalRanking), RpcTarget.Others, ranking.Select(p => p.ActorNumber).ToArray());
    }

    [PunRPC]
    private void RPC_FinalRanking(int[] actorNumbersInOrder)
    {
        if (PhotonNetwork.IsMasterClient) return;

        cachedFinalRanking = actorNumbersInOrder.Select(FindPlayerByActor).Where(p => p != null).ToList();
        if (uiManager != null && cachedFinalRanking.Count > 0)
            uiManager.ShowMessage($"{cachedFinalRanking[0].GetName()} finishes 1st!", 10f);
    }

    // ────────────────────────────────────────────────────────────
    //  Testing
    // ────────────────────────────────────────────────────────────

    [ContextMenu("Run Test Hand")]
    public void RunTestHand()
    {
        Debug.Log("=== RunTestHand: everyone calls/checks to showdown ===");
        int safety = 200;
        while (currentStreet != Street.Showdown && safety-- > 0)
        {
            var p = CurrentPlayer;
            if (currentBetToCall == p.totalContributed)
                RequestCheck(p);
            else
                RequestCall(p);
        }
    }

    void Update()
    {
        // Offline debug shortcuts only - would desync a networked table.
        if (PhotonNetwork.InRoom) return;
        if (currentStreet == Street.Showdown) return;
        var p = CurrentPlayer;

        if (Input.GetKeyDown(KeyCode.F)) RequestFold(p);
        if (Input.GetKeyDown(KeyCode.C)) RequestCheck(p);
        if (Input.GetKeyDown(KeyCode.Space)) RequestCall(p);
        if (Input.GetKeyDown(KeyCode.R)) RequestRaise(p, currentBetToCall + 20);
        if (Input.GetKeyDown(KeyCode.B)) RequestBet(p, 20);
    }
}