using UnityEngine;

public class RMVisualiserUnity : MonoBehaviour
{
    float forwardOffsetZ = 0.8f;

    /// <summary>
    /// Use it to 'retrieve' and orient any GameObject attached to this script to face the user
    /// </summary>
    public void ResetToFaceUser()
    {
        transform.localScale = Vector3.one;
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * forwardOffsetZ;
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }
}
