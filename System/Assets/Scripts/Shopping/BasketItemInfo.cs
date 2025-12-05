using UnityEngine;

[System.Serializable]
public class BasketItemInfo
{
    public GameObject obj;
    public Vector3 localPos;
    public Quaternion localRot;

    public BasketItemInfo(GameObject obj, Transform cart)
    {
        this.obj = obj;
        localPos = cart.InverseTransformPoint(obj.transform.position);
        localRot = Quaternion.Inverse(cart.rotation) * obj.transform.rotation;
    }
}
