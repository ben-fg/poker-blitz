using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using Photon.Pun;

public class Showdown : MonoBehaviour
{
    public AudioClip showdownGunSounds;
    public AudioClip gunShotSound;
    public AudioSource audioSource;
    public TextMeshProUGUI instructionText;
    public Light2D globalLight;
    public Light2D sunlight;
    public GameObject[] gunshots; // Array to hold the 4 gunshot GameObjects
    public float[] delayTimes;    // Array of delay times for each gunshot
    
    private Color color;
    
    private PhotonView photonView;


    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        color = instructionText.color;
        color.a = 0;
        instructionText.color = color;
        // Ensure each gunshot is initially hidden
        foreach (GameObject gunshot in gunshots)
        {
            gunshot.SetActive(false);
        }
        Invoke("IntroEffects", 16f);
        Invoke("StartShowdown", 22f);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            HandleMouseInput();
        }
    }

    public void IntroEffects()
    {
        Color color = instructionText.color;
        color.a = 1;
        instructionText.color = color;
        audioSource.PlayOneShot(gunShotSound);
    }

    public void StartShowdown()
    {
        Debug.Log("Showdown Started!");
        instructionText.text = "stay focused, the sun will go out anytime in the next 3 minutes...";
        int randomTime = Random.Range(20,200);
        Invoke("DimSun",5);
    }

    private void DimSun()
    {
        globalLight.intensity = 0.1f;
        sunlight.intensity = 0.01f;
        color.a = 0;
        instructionText.color = color;
        audioSource.Stop();
        audioSource.PlayOneShot(showdownGunSounds);
        // Loop through each gunshot and schedule its appearance
        for (int i = 0; i < gunshots.Length; i++)
        {
            Invoke("ShowGunshot", delayTimes[i]);
        }

        StartCoroutine(UnDim());
    }

    // Function to show and hide the gunshot after 0.2 seconds
    void ShowGunshot()
    {
        for (int i = 0; i < gunshots.Length; i++)
        {
            if (!gunshots[i].activeSelf)
            {
                gunshots[i].SetActive(true); // Show the gunshot
                StartCoroutine(HideAfterDelay(gunshots[i], 0.3f)); // Hide after 0.2 seconds
                break; // Ensure only one gunshot is activated at a time
            }
        }
    }

    // Coroutine to hide the gunshot after a specified delay
    private IEnumerator HideAfterDelay(GameObject gunshot, float delay)
    {
        yield return new WaitForSeconds(delay);
        gunshot.SetActive(false); // Hide the gunshot
    }

    IEnumerator UnDim()
    {
        // Delay the start of the fading effect by 4 seconds
        Debug.Log("PreWaitroutine");
        yield return new WaitForSeconds(4.0f);
        Debug.Log("PostRoutine");

        float timeElapsed = 0f;

        // Gradually increase intensity over the given duration
        while (timeElapsed < 4.0f)
        {
            timeElapsed += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(0.1f, 1.8f, timeElapsed / 4.0f);
            sunlight.intensity = Mathf.Lerp(0.01f, 1.0f, timeElapsed / 4.0f);

            // Wait until the next frame before continuing the loop
            yield return null;
        }

        // Ensure the intensity is exactly the target at the end
        globalLight.intensity = 1.0f;
    }

    void HandleMouseInput()
    {
        // Detect left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " clicked the mouse!");
        }
    }
}
