using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationCollection : MonoBehaviour
{
    public static InformationCollection Instance;

    public string userName;
    public string gender;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 只保留数据
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUserInfo(string name, string genderValue)
    {
        userName = name;
        gender = genderValue;
    }
}
