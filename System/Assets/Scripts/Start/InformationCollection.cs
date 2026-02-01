using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationCollection : MonoBehaviour
{

    public TMP_InputField inputField;
    public TMP_Dropdown genderDropdown;

    string userName;
    string gender;

    public void Submit()
    {
        userName = inputField.text;
        gender = genderDropdown.options[genderDropdown.value].text;

        //Debug.Log($"ÐÕÃû£º{userName}£¬ÐÔ±ð£º{gender}");
    }
}
