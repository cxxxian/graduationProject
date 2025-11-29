using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowInformation : MonoBehaviour
{
    public GameObject priceCanvas;
    public void showPanel()
    {
        priceCanvas.SetActive(true);
    }
    public void disShowPanel()
    {
        priceCanvas.SetActive(false);
    }
}
