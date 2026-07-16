using UnityEngine;

namespace Osm4x.GIS
{
    /// <summary>
    /// Scene bootstrap notes for ArcGIS Maps SDK.
    /// Attach next to your ArcGIS Map component. Fill API key via SDK auth,
    /// not hardcoded here. When the package is imported, replace TODOs with
    /// real Esri types (ArcGISMap, ArcGISCameraController, etc.).
    /// </summary>
    public sealed class ArcGISMapBootstrap : MonoBehaviour
    {
        [Header("Intent (configure actual ArcGIS components in Inspector)")]
        [SerializeField] private bool preferGlobeView = true;
        [SerializeField] private bool enableOsmBasemap = true;
        [SerializeField] private bool enable3dBuildings = true;
        [SerializeField] private bool enableElevation = true;

        [Header("Default focus")]
        [SerializeField] private double startLatitude = 38.8816;
        [SerializeField] private double startLongitude = -77.0910;
        [SerializeField] private double startAltitudeMeters = 2_000_000;

        private void Start()
        {
            Debug.Log(
                "[Osm4x] ArcGIS bootstrap: " +
                $"globe={preferGlobeView}, osm={enableOsmBasemap}, " +
                $"buildings3d={enable3dBuildings}, elev={enableElevation}, " +
                $"start={startLatitude},{startLongitude} alt={startAltitudeMeters}m. " +
                "Wire ArcGIS Map component + API key in Editor.");

            // TODO (after SDK import):
            // 1. Ensure ArcGISMap is in Globe mode.
            // 2. Add basemap layer (OSM or Imagery Hybrid).
            // 3. Enable elevation source.
            // 4. Enable OSM 3D Buildings layer (SDK 2.x+).
            // 5. Position ArcGIS camera at start lat/lon/alt.
        }
    }
}
