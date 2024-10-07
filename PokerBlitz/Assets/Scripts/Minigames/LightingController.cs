using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingController : MonoBehaviour
{
    // Sun movement and scaling parameters
    public float speed = 0.3f;  // The speed at which the sun will rise
    public Vector3 targetPosition = new Vector3(1.0f, 2.4f, 0f);  // The final position where the sun will stop
    private Vector3 startPosition;  // Initial position of the sun
    public float scaleMultiplier = 2.7f; // How much the sun will enlarge
    private Vector3 startScale;

    // foreground light control parameters
    public Light2D foregroundLight; // Reference to the Global Light 2D object
    public float minIntensity = 1f; // Initial intensity (night)
    public float maxIntensity = 1.9f; // Maximum intensity (day)
    public Color nightColor = new Color(0.125f, 0.094f, 0.3f); // Dark blue
    public Color dayColor = Color.white; // white
    public float scaleFactor = 2.0f; // how quickly dark changes to light

    // Spot light radius parameters
    public float minInnerAngle = 133f; // Minimum inner radius
    public float maxInnerAngle = 360f; // Maximum inner radius
    public float minOuterAngle = 303f; // Minimum outer radius
    public float maxOuterAngle = 360f; // Maximum outer radius

    private float sunRiseHeight = 5f; // The height where the sun is fully risen

    private Light2D light2D; // Reference to the Light2D component

    void Start()
    {
        // Store the initial position and scale of the sun
        startPosition = transform.position;
        startScale = transform.localScale;

        // Get the Light2D component attached to this GameObject
        light2D = GetComponent<Light2D>();
        
        // Initialize light settings
        if (light2D != null)
        {
            light2D.pointLightInnerAngle = minInnerAngle; // Set initial inner radius
            light2D.pointLightOuterAngle = minOuterAngle; // Set initial outer radius
        }
    }

    void Update()
    {
        // Move the sun towards the target position over time
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        // Calculate the percentage of the sun's movement progress
        float progress = (transform.position.y - startPosition.y) / (targetPosition.y - startPosition.y);
        
        // Adjust the sun's scale as it rises
        transform.localScale = startScale * (1 + progress * (scaleMultiplier - 1));

        // Control the light properties based on the sun's height
        float sunProgress = Mathf.Clamp01(transform.position.y / sunRiseHeight);

        // change intensity and color based on sun progress
        foregroundLight.color = Color.Lerp(nightColor, dayColor, sunProgress);

        // Directly modify the Light2D properties
        if (light2D != null)
        {
            light2D.pointLightInnerAngle = Mathf.Lerp(minInnerAngle, maxInnerAngle, sunProgress * 8f);
            light2D.pointLightOuterAngle = Mathf.Lerp(minOuterAngle, maxOuterAngle, sunProgress * 8f);
        }
    }
}
