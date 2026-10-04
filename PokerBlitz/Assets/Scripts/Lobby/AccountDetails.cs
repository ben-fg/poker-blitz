using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountDetails : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameBox;
    [SerializeField] private Text usernameText;
    [SerializeField] private TMP_Dropdown dropdown;
    public static string cardPreference;

    void Start()
    {
        usernameBox.text = PlayerPrefs.GetString("Username");
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
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

    void OnDropdownChanged(int value)
    {
        if (value == 0)
        {
            Debug.Log("Jumbo selected");
            cardPreference = "Jumbo";

        }
        else if (value == 1)
        {
            Debug.Log("Realistic selected");
            cardPreference = "Realistic";
        }
    }
}
