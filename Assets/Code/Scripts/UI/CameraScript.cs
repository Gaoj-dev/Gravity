using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followTime = 0.2f;
    [SerializeField] private Vector2 followOffset = new Vector2(0f, 3f);

    private void LateUpdate()
    {
        if (!TryResolvePlayer())
        {
            return;
        }

        Vector3 current = transform.position;
        Vector3 target = new Vector3(
            player.position.x + followOffset.x,
            player.position.y + followOffset.y,
            current.z
        );

        if (followTime <= 0f)
        {
            transform.position = target;
            return;
        }

        float speed = Vector3.Distance(current, target) / followTime;
        transform.position = Vector3.MoveTowards(current, target, speed * Time.deltaTime);
    }

    // Recupera automaticamente el Player persistente si la referencia del inspector no es la valida.
    private bool TryResolvePlayer()
    {
        if (player != null && player.gameObject.scene.IsValid())
        {
            return true;
        }

        PlayerModeHandler playerModeHandler = FindFirstObjectByType<PlayerModeHandler>();
        if (playerModeHandler != null)
        {
            player = playerModeHandler.transform;
            return true;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            player = taggedPlayer.transform;
            return true;
        }

        return false;
    }
}
