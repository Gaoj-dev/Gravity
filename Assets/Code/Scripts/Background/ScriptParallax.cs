using UnityEngine;

public class ScriptParallax : MonoBehaviour
{
    private float startPos;
    private float length;
    public GameObject camara;
    public float efectoParallax;

    private void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void LateUpdate()
    {
        float distancia = camara.transform.position.x * efectoParallax;
        float movement = camara.transform.position.x * (1 - efectoParallax);

        transform.position = new Vector3(startPos + distancia, transform.position.y, transform.position.z);

        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }
    }
}
