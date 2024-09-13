using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountDetails : MonoBehaviour
{
    [SerializeField] private InputField usernameBox;
    [SerializeField] private Text usernameText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckUsername()
    {
        string username = usernameBox.text;
        if (username != "Emerald")
        {
            PlayerPrefs.SetString("Username", username);
            usernameText.text = "Successfully set username!";
        }
        else
        {
            usernameText.text = "Username taken.";
        }
    }
}
