// Tracks which minigame is currently selected. Used by WaitingLobby, PreGame,
// PowerUps, PlayerMovement, and PlayerPowerUps. Used to live as static fields on
// GameMaster, moved here once GameMaster got renamed and taken over by poker.
public static class MinigameSelection
{
    public static int gameNumber;
    public const int maxGames = 4;
}
