using System.Collections;
using Osm4x.Chunks;
using Osm4x.Core;
using UnityEngine;

namespace Osm4x.CameraSystem
{
    public sealed class TacticalTransitionController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float tacticalHeight = 25f;
        [SerializeField] private float durationSeconds = 1.8f;
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
            var gm = GameModeController.Instance;
            if (cam == null || gm == null)
            {
                gm?.EnterTactical();
                yield break;
            }

            Vector3 end = gm.FocusWorld;
            end.y = tacticalHeight;
            Vector3 start = cam.position;
            Quaternion startRot = cam.rotation;
            Quaternion endRot = Quaternion.Euler(55f, cam.eulerAngles.y, 0f);

            chunkManager?.UpdateCenter(end, force: true);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, durationSeconds);
                float u = ease.Evaluate(Mathf.Clamp01(t));
                cam.position = Vector3.Lerp(start, end, u);
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
                var cam = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                GameModeController.Instance.BeginBattleAt(cam);
            }
            if (Input.GetKeyDown(KeyCode.N))
                GameModeController.Instance?.ReturnToStrategic();
        }
#endif
    }
}
