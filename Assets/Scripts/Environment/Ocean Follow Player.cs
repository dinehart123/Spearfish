using UnityEngine;

public class OceanFollowPlayer : MonoBehaviour
{
    public Transform player;

    private void LateUpdate()
    {
        if (player == null)
            return;

        transform.position = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );
    }
}