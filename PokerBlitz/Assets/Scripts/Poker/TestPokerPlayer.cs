public class TestPokerPlayer
{
    private string name;

    public int ActorNumber = -1; // -1 = offline/AI, not a real networked player
    public enum Position
    {
        SB, // Small Blind
        BB, // Big Blind
        UTG, // Under The Gun
        BTN // Button
    }

    private Position position;

    private Pocket pocket;
    private int balance;
    public bool isFolded;
    public bool hasActedThisStreet;


    public int totalContributed;   // total money put in this hand

    public bool IsAllIn => balance == 0;
    public bool IsEliminated; // out for the rest of the tournament, stays true forever


    public TestPokerPlayer(string name)
    {
        this.name = name;
    }

    public string GetName()
    {
        return name;
    }

    public void DealPocket(Pocket pocket)
    {
        this.pocket = pocket;
        isFolded = false;
    }

    public Pocket GetPocket()
    {
        return pocket;
    }

    public void SetBalance(int balance)
    {
        this.balance = balance;
    }

    public int GetBalance()
    {
        return balance;
    }

    public void Fold()
    {
        isFolded = true;
    }

    public bool IsFolded()
    {
        return isFolded;
    }

    public void ResetForNewHand()
    {
        hasActedThisStreet = false;
        isFolded = IsEliminated; // eliminated players stay folded forever
        totalContributed = 0;
    }

}