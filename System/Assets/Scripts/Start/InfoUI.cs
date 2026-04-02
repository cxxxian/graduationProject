using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Dropdown genderDropdown;

    public void OnSubmit()
    {
        string name = inputField.text;
        string gender = genderDropdown.options[genderDropdown.value].text;

        InformationCollection.Instance.SetUserInfo(name, gender);
    }
}
