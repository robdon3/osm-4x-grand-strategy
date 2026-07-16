using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Tactical
{
    /// <summary>
    /// Spawns placeholder units and notes NavMesh bake hooks when entering tactical mode.
    /// </summary>
    public sealed class TacticalBattleBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private int placeholderUnitCount = 6;
        [SerializeField] private float spawnRadius = 40f;

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
            for (int i = 0; i < placeholderUnitCount; i++)
            {
                Vector2 disk = Random.insideUnitCircle * spawnRadius;
                Vector3 pos = transform.position + new Vector3(disk.x, 0f, disk.y);
                if (unitPrefab != null)
                    Instantiate(unitPrefab, pos, Quaternion.identity, transform);
                else
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.name = $"Unit_{i}";
                    go.transform.SetParent(transform, false);
                    go.transform.position = pos;
                    go.transform.localScale = new Vector3(2f, 3f, 2f);
                }
            }

            Debug.Log("[Osm4x] Tactical placeholders spawned. Bake NavMesh per chunk next.");
        }
    }
}
