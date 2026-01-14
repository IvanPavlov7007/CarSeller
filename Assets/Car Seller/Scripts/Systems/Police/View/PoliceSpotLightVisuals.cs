using System;
using UnityEngine;

public class PoliceSpotLightVisuals : MonoBehaviour
{
    PoliceUnit policeUnit;
    SpotlightColors spotlightColors => PoliceManager.Instance.SpotlightColors;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh coneMesh;

    public void Intialize(PoliceUnit policeUnit)
    {
        this.policeUnit = policeUnit;

        // Ensure we have required components
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        createVisualSlice();
    }

    /// <summary>
    /// Creates a 2D cone mesh (fan) in local X‑Y plane, pointing along +Y.
    /// Radius and angle are taken from policeUnit.ConeVision.
    /// </summary>
    private void createVisualSlice()
    {
        if (policeUnit == null || policeUnit.ConeVision == null) return;

        float radius = policeUnit.ConeVision.Radius;
        float angleDeg = policeUnit.ConeVision.Angle;
        int segments = Mathf.Max(8, Mathf.RoundToInt(angleDeg / 5f)); // ~5° per segment

        coneMesh = new Mesh();
        coneMesh.name = "PoliceConeMesh";

        // Vertices: center + arc points
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // center

        float halfAngleRad = 0.5f * angleDeg * Mathf.Deg2Rad;
        float startAngle = -halfAngleRad;
        float endAngle = halfAngleRad;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float ang = Mathf.Lerp(startAngle, endAngle, t);

            // Cone is in local space, pointing along +Y (forward in this local 2D plane)
            float x = Mathf.Sin(ang) * radius;
            float y = Mathf.Cos(ang) * radius;

            vertices[i + 1] = new Vector3(x, y, 0);
        }

        // Triangles: fan from center
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 3;
            triangles[baseIndex] = 0;
            triangles[baseIndex + 1] = i + 1;
            triangles[baseIndex + 2] = i + 2;
        }

        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();

        meshFilter.sharedMesh = coneMesh;
    }

    private void LateUpdate()
    {
        if (policeUnit == null || policeUnit.GraphMovement == null) return;

        // Position this visual at the police unit's world position
        // Assuming world is 2D in X‑Y and Z is constant.
        var worldPos2D = policeUnit.CityPosition.WorldPosition;
        transform.position = new Vector3(worldPos2D.x, worldPos2D.y, transform.position.z);

        // Face it forward: GraphMovement.Up gives a 2D forward direction in world space.
        Vector2 forward2D = policeUnit.GraphMovement.Up;
        if (forward2D.sqrMagnitude > Mathf.Epsilon)
        {
            // Convert forward vector to angle around Z
            float angleDeg = Mathf.Atan2(forward2D.x, forward2D.y) * Mathf.Rad2Deg;
            // Our mesh is built pointing along +Y, so rotate around Z to match
            transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);
        }

        // Optional: change color by police state
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            Color c = spotlightColors.idle;
            switch (policeUnit.State)
            {
                case PoliceUnitState.Chase:
                    c = spotlightColors.chase;
                    break;
                case PoliceUnitState.Suspect:
                case PoliceUnitState.Backup:
                    c = spotlightColors.alert;
                    break;
            }

            meshRenderer.sharedMaterial.color = c;
        }
    }
}

[Serializable]
public class SpotlightColors
{
    public Color chase = Color.red;
    public Color alert = Color.yellow;
    public Color idle = Color.blue;
}