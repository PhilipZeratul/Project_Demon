using UnityEngine;
using Zenject;


public class CameraFollow : MonoBehaviour
{
    [Inject (Id = Constants.InjectID.Player)]
    private GameObject player;


    private void LateUpdate()
    {
        if (player.activeSelf)
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }
}
