using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Tactical
{
    public sealed class TacticalBattleBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private int placeholderUnitCount = 6;
        [SerializeField] private float spawnRadius = 20f;

        private void OnEnable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged += OnMode;
        }

        private void OnDisable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged -= OnMode;
        }

        private void OnMode(GameMode prev, GameMode next)
        {
            if (next == GameMode.Tactical)
                SpawnPlaceholders();
        }

        private void SpawnPlaceholders()
        {
            Vector3 center = GameModeController.Instance != null
                ? GameModeController.Instance.FocusWorld
                : transform.position;

            for (int i = 0; i < placeholderUnitCount; i++)
            {
                Vector2 disk = Random.insideUnitCircle * spawnRadius;
                Vector3 pos = new Vector3(center.x + disk.x, center.y + 2f, center.z + disk.y);
                if (unitPrefab != null)
                    Instantiate(unitPrefab, pos, Quaternion.identity, transform);
                else
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.name = $"Unit_{i}";
                    go.transform.SetParent(transform, false);
                    go.transform.position = pos;
                }
            }
            Debug.Log("[Proc4x] Tactical units on generated terrain.");
        }
    }
}
