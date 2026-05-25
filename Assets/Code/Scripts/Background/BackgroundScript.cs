using UnityEngine;

public class BackgroundScript : MonoBehaviour
{
    public GameObject camara;

    void Update()
    {
        transform.position = new Vector3(
            camara.transform.position.x,
            camara.transform.position.y,
            transform.position.z
        );
    }
}