using Osm4x.Core;
using UnityEngine;

namespace Osm4x.CameraSystem
{
    /// <summary>
    /// Lightweight orbit/zoom around a globe origin. Replace with Cinemachine
    /// FreeLook when available; keep altitude hooks for mode + LOD.
    /// </summary>
    public sealed class StrategicCameraController : MonoBehaviour
    {
        [SerializeField] private Transform globeCenter;
        [SerializeField] private float globeRadius = 1000f;
        [SerializeField] private float distance = 2500f;
        [SerializeField] private float minDistance = 120f;
        [SerializeField] private float maxDistance = 8000f;
        [SerializeField] private float orbitSpeed = 40f;
        [SerializeField] private float zoomSpeed = 500f;
        [SerializeField] private float pitch = 35f;
        [SerializeField] private float yaw;

        private void Update()
        {
            if (GameModeController.Instance != null &&
                GameModeController.Instance.Mode == GameMode.Transition)
                return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            yaw += h * orbitSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch - v * orbitSpeed * Time.deltaTime, 5f, 85f);

            float scroll = Input.mouseScrollDelta.y;
            distance = Mathf.Clamp(distance - scroll * zoomSpeed * Time.deltaTime * 10f, minDistance, maxDistance);

            Vector3 center = globeCenter != null ? globeCenter.position : Vector3.zero;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = center + rot * (Vector3.back * distance);
            transform.LookAt(center);

            float altitude = GeoMath.ApproximateAltitudeMeters(transform.position - center, globeRadius);
            GameModeController.Instance?.SuggestModeFromAltitude(altitude);
        }

        public void FocusLatLon(double lat, double lon)
        {
            Vector3 onSphere = GeoMath.LatLonToUnitSphere(lat, lon);
            yaw = Mathf.Atan2(onSphere.x, onSphere.z) * Mathf.Rad2Deg;
            pitch = 40f;
        }
    }
}
