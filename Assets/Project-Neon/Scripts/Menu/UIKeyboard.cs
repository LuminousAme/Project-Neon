using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

//based on this video https://youtu.be/PyKW9kecyqg
public class UIKeyboard : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject UpperCaseButtons;
    [SerializeField] GameObject LowerCaseButtons;
    private bool caps;
    public static Action<string> OnConfirmText;
    public static UIKeyboard instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            //gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        LowerCaseButtons.SetActive(true);
        UpperCaseButtons.SetActive(false);
        caps = false;
        inputField.text = "";
    }

    public void InsertChar(string c)
    {
        inputField.text += c;
    }

    public void InsertSpace()
    {
        inputField.text += " ";
    }

    public void DeleteChar()
    {
        if(inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }

    public void CapsPressed()
    {
        if(!caps)
        {
            LowerCaseButtons.SetActive(false);
            UpperCaseButtons.SetActive(true);
            caps = true;
        }
        else
        {
            LowerCaseButtons.SetActive(true);
            UpperCaseButtons.SetActive(false);
            caps = false;
        }
    }

    public void Confirm()
    {
        OnConfirmText?.Invoke(inputField.text);
        gameObject.SetActive(false);
    }

    public string ReadText()
    {
        return inputField.text;
    }
}
