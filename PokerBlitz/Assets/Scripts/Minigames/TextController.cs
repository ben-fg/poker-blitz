using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{

    public TextMeshProUGUI scoreText; // The Text element to display all scores
    public TextMeshProUGUI instructionText;
    private int[] playerScores = new int[4]; // Array to store scores for 4 players

    // Start is called before the first frame update
    void Start()
    {
        // Initialize scores if needed
        UpdateScoreText();
        Color color = scoreText.color;
        color.a = 0;
        scoreText.color = color;
        instructionText.color = color;
        scoreText.text = "P1: 1        P2: 2                                                                        P3: 1        P4: 2";
        instructionText.text = "When the sun goes out.... left click to shoot your gun!";
    }

    // Method to update the score for a specific player
    public void UpdatePlayerScore(int playerIndex, int points)
    {
        // Make sure playerIndex is valid (0 to 3 for 4 players)
        if (playerIndex >= 0 && playerIndex < playerScores.Length)
        {
            playerScores[playerIndex] += points;
            UpdateScoreText();
        }
    }

    // Method to refresh the displayed text with all player scores
    private void UpdateScoreText()
    {
        scoreText.text = $"P1: {playerScores[0]}     P2: {playerScores[1]}                                                                        P3: {playerScores[2]}     P4: {playerScores[3]}";
    }

    // You can also create a reset function if necessary
    public void ResetScores()
    {
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = 0;
        }
        UpdateScoreText();
    }
}