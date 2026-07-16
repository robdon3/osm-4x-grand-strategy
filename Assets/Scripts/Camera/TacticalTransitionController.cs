using System.Collections;
using Osm4x.Chunks;
using Osm4x.Core;
using UnityEngine;

namespace Osm4x.CameraSystem
{
    /// <summary>
    /// Cinematic fly-in from strategic orbit to a GPS focus, then hand off to tactical.
    /// Upgrade path: Cinemachine smooth path + DOF/vignette volumes.
    /// </summary>
    public sealed class TacticalTransitionController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float globeRadius = 1000f;
        [SerializeField] private float tacticalDistance = 180f;
        [SerializeField] private float durationSeconds = 2.5f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private ChunkManager chunkManager;

        private void OnEnable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged += HandleMode;
        }

        private void OnDisable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged -= HandleMode;
        }

        private void HandleMode(GameMode prev, GameMode next)
        {
            if (next == GameMode.Transition)
                StartCoroutine(FlyIn());
        }

        private IEnumerator FlyIn()
        {
            var cam = cameraTransform != null ? cameraTransform : Camera.main?.transform;
            if (cam == null)
            {
                GameModeController.Instance?.EnterTactical();
                yield break;
            }

            var gm = GameModeController.Instance;
            Vector3 target = GeoMath.LatLonToWorld(gm.FocusLatitude, gm.FocusLongitude, globeRadius);
            Vector3 endPos = target.normalized * (globeRadius + tacticalDistance);
            Vector3 startPos = cam.position;
            Quaternion startRot = cam.rotation;
            Quaternion endRot = Quaternion.LookRotation(target - endPos, Vector3.up);

            chunkManager?.UpdateCenter(gm.FocusLatitude, gm.FocusLongitude);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, durationSeconds);
                float u = ease.Evaluate(Mathf.Clamp01(t));
                cam.position = Vector3.Lerp(startPos, endPos, u);
                cam.rotation = Quaternion.Slerp(startRot, endRot, u);
                yield return null;
            }

            gm.EnterTactical();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) && GameModeController.Instance != null)
            {
                var gm = GameModeController.Instance;
                gm.BeginBattleAt(gm.FocusLatitude, gm.FocusLongitude);
            }
            if (Input.GetKeyDown(KeyCode.N) && GameModeController.Instance != null)
                GameModeController.Instance.ReturnToStrategic();
        }
#endif
    }
}
