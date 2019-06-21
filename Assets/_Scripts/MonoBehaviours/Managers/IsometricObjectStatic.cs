using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public class IsometricObjectStatic : MonoBehaviour
{
    public float gizmoLength = 1.0f;

    [Tooltip("Will use this object to compute z-order")]
    public Transform target;

    [Tooltip("Use this to offset the object slightly in front or behind the Target object")]
    public float targetOffset = 0;

    private SortingGroup sortingGroup;
    private SpriteRenderer spriteRenderer;


    private void Start()
    {
        if (target == null)
            target = transform;

        sortingGroup = GetComponent<SortingGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (!sortingGroup && !spriteRenderer)
            Debug.LogErrorFormat("IsometricObjectStatic: {0} does not have a SortingGroup nor SpriteRenderer", gameObject.name);

        float order = (-target.position.y + targetOffset) * Constants.MapInfo.PixelPerUnit;

        if (sortingGroup)
        {
            sortingGroup.sortingOrder = (int)order;
        }
        else
            spriteRenderer.sortingOrder = (int)order;
    }

#if UNITY_EDITOR
    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            float order = (-target.position.y + targetOffset) * Constants.MapInfo.PixelPerUnit;

            if (sortingGroup)
            {
                sortingGroup.sortingOrder = (int)order;
            }
            else
                spriteRenderer.sortingOrder = (int)order;
        }
    }
#endif

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 pivotLineStart = new Vector3(transform.position.x - gizmoLength, transform.position.y - targetOffset, transform.position.z);
        Vector3 pivotLineEnd = new Vector3(transform.position.x + gizmoLength, transform.position.y - targetOffset, transform.position.z);

        Gizmos.DrawLine(pivotLineStart, pivotLineEnd);
    }
}