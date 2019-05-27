using UnityEngine;
using Zenject;


public class CameraFollow : MonoBehaviour
{
    public float offset = 2f;

    [Inject (Id = Constants.InjectID.Player)]
    private GameObject player;


    private void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x + offset, transform.position.y, transform.position.z);
    }
}
