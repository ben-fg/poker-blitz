using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountDetails : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameBox;
    [SerializeField] private Text usernameText;

    void Start()
    {
        usernameBox.text = PlayerPrefs.GetString("Username");
    }

    void Update()
    {
        
    }

    public void CheckUsername()
    {
        string username = usernameBox.text;
        if (username == "Emerald")
        {
            usernameText.text = "You cannot use that username.";
        }
        else if (username.Length > 10)
        {
            usernameText.text = "Cannot be more than 10 characters.";
        }
        else
        {
            PlayerPrefs.SetString("Username", username);
            usernameText.text = "Successfully set username!";
        }
    }

    public void ResetConfirmText()
    {
        usernameText.text = "";
        usernameBox.text = PlayerPrefs.GetString("Username");
    }
}
