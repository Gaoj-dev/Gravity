using UnityEngine;

public class ScriptParallax : MonoBehaviour
{
    private float startPos;
    private float length;
    public GameObject camara;
    public float efectoParallax; // 0 = follow camara, 1 = quieto, 0,5 mitad velocidad camara


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    
    void LateUpdate()
    {
        float distancia = camara.transform.position.x * efectoParallax;
        float movement = camara.transform.position.x * (1 - efectoParallax);
        transform.position = new UnityEngine.Vector3(startPos + distancia, transform.position.y, transform.position.z);
    
        if (movement > startPos + length)
        {
            startPos += length;
        } else if (movement < startPos - length)
        {
            startPos -= length;
        }
    }
}
