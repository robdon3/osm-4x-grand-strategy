using Osm4x.Core;
using UnityEngine;

namespace Osm4x.CameraSystem
{
    /// <summary>
    /// Free RTS-style camera over the procedural map.
    /// </summary>
    public sealed class StrategicCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 40f;
        [SerializeField] private float fastMultiplier = 3f;
        [SerializeField] private float zoomSpeed = 80f;
        [SerializeField] private float minHeight = 8f;
        [SerializeField] private float maxHeight = 400f;
        [SerializeField] private float pitch = 50f;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
            if (transform.position.y < minHeight)
                transform.position = new Vector3(transform.position.x, 80f, transform.position.z);
        }

        private void Update()
        {
            if (GameModeController.Instance != null &&
                GameModeController.Instance.Mode == GameMode.Transition)
                return;

            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 delta = Vector3.zero;
            delta += forward * Input.GetAxisRaw("Vertical");
            delta += right * Input.GetAxisRaw("Horizontal");
            if (delta.sqrMagnitude > 1f) delta.Normalize();

            transform.position += delta * speed * Time.deltaTime;

            float scroll = Input.mouseScrollDelta.y;
            float y = Mathf.Clamp(
                transform.position.y - scroll * zoomSpeed * Time.deltaTime * 10f,
                minHeight,
                maxHeight);
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
        }
    }
}
