using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class LightingController : MonoBehaviour
{
    // Sun movement and scaling parameters
    public float speed = 0.3f;  // The speed at which the sun will rise
    public Vector3 targetPosition = new Vector3(1.0f, 2.4f, 0f);  // The final position where the sun will stop
    private Vector3 startPosition;  // Initial position of the sun
    public float scaleMultiplier = 2.7f; // How much the sun will enlargen
    private Vector3 startScale;

    // foreground light control parameters
    public Light2D foregroundLight; // Reference to the Global Light 2D object
    public float minIntensity = 0.2f; // Initial intensity (night)
    public float maxIntensity = 1.0f; // Maximum intensity (day)
    public Color nightColor = new Color(0.125f, 0.094f, 0.3f); // Dark blue
    public Color dayColor = Color.white; // white
    public float scaleFactor = 2.0f; // how quickly dark changes to light

    private float sunRiseHeight = 5f; // The height where the sun is fully risen
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI scoreboardText;
    private bool updated = false;

    void Start()
    {
        // Store the initial position and scale of the sun
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    void Update()
    {
        // Move the sun towards the target position over time
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        // Calculate the percentage of the sun's movement progress
        float progress = (transform.position.y - startPosition.y) / (targetPosition.y - startPosition.y);

        // Adjust the suns scale as it rises
        transform.localScale = startScale * (1 + progress * (scaleMultiplier - 1));

        // Control the lighting based on the sun's height
        float sunProgress = Mathf.Clamp01(transform.position.y / sunRiseHeight * scaleFactor);

        // change intensity and color based on sun progress
        foregroundLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, sunProgress);
        foregroundLight.color = Color.Lerp(nightColor, dayColor, sunProgress);

        Color color = instructionText.color;
        if (updated == false && transform.position.y > 2.2f)
        {
            //audioSource.PlayOneShot(gunshotEffect);
            color.a = 1;
            instructionText.color = color;
            scoreboardText.color = color;
            updated = true;
        }
    }
}

