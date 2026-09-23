using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterControllers))]
public class CharacterCameraFollow : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);
    [SerializeField, Min(0f)] private float smoothTime = 0.15f;
    [SerializeField, Min(0f)] private float teleportDistance = 10f;

    private CharacterControllers controller;
    private Camera followedCamera;
    private Vector2 velocity;
    private Vector2 previousTargetPosition;
    private bool hasFollowed;

    private void Awake()
    {
        controller = GetComponent<CharacterControllers>();
    }

    private void OnDisable()
    {
        hasFollowed = false;
        velocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (controller == null || controller != CharacterControllers.Current ||
            !controller.isActiveAndEnabled || !controller.CanRun)
        {
            hasFollowed = false;
            velocity = Vector2.zero;
            return;
        }

        // Resolve again so the persistent character can follow with a new scene camera.
        Camera currentCamera = Camera.main;
        if (currentCamera == null)
        {
            hasFollowed = false;
            return;
        }

        Vector2 targetPosition = transform.position;
        Vector2 destination = targetPosition + offset;
        bool snap = !hasFollowed || followedCamera != currentCamera ||
            (targetPosition - previousTargetPosition).sqrMagnitude > teleportDistance * teleportDistance;

        Vector3 cameraPosition = currentCamera.transform.position;
        if (snap || smoothTime <= 0f)
        {
            velocity = Vector2.zero;
        }
        else
        {
            destination = Vector2.SmoothDamp(cameraPosition, destination, ref velocity, smoothTime);
        }

        // Keep the scene camera's depth, rotation, and zoom settings.
        currentCamera.transform.position = new Vector3(destination.x, destination.y, cameraPosition.z);
        followedCamera = currentCamera;
        previousTargetPosition = targetPosition;
        hasFollowed = true;
    }
}
