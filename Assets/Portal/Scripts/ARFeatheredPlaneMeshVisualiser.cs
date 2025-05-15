using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlaneMeshVisualizer), typeof(MeshRenderer), typeof(ARPlane))]
public class ARFeatheredPlaneMeshVisualizer : MonoBehaviour
{
    [Tooltip("The width of the texture feathering (in world units).")]
    [SerializeField]
    float m_FeatheringWidth = 0.2f;

    public float featheringWidth
    {
        get { return m_FeatheringWidth; }
        set { m_FeatheringWidth = value; }
    }

    static List<Vector3> s_FeatheringUVs = new List<Vector3>();
    static List<Vector3> s_Vertices = new List<Vector3>();

    ARPlaneMeshVisualizer m_PlaneMeshVisualizer;
    ARPlane m_Plane;
    Material m_FeatheredPlaneMaterial;

    void Awake()
    {
        m_PlaneMeshVisualizer = GetComponent<ARPlaneMeshVisualizer>();
        m_Plane = GetComponent<ARPlane>();
        m_FeatheredPlaneMaterial = GetComponent<MeshRenderer>().material;
    }

    void OnEnable()
    {
        m_Plane.boundaryChanged += ARPlane_boundaryUpdated;
    }

    void OnDisable()
    {
        m_Plane.boundaryChanged -= ARPlane_boundaryUpdated;
    }

    void ARPlane_boundaryUpdated(ARPlaneBoundaryChangedEventArgs eventArgs)
    {
        GenerateBoundaryUVs(m_PlaneMeshVisualizer.mesh);
    }

    void GenerateBoundaryUVs(Mesh mesh)
    {
        if (mesh == null)
            return;

        int vertexCount = mesh.vertexCount;
        s_FeatheringUVs.Clear();

        if (s_FeatheringUVs.Capacity < vertexCount)
            s_FeatheringUVs.Capacity = vertexCount;

        mesh.GetVertices(s_Vertices);

        // Use the ARPlane's defined center instead of assuming the last vertex is center
        Vector3 center = m_Plane.center;
        float maxDistance = 0f;

        // Compute max distance from center (to normalize UV values)
        for (int i = 0; i < vertexCount; i++)
        {
            float distance = Vector3.Distance(s_Vertices[i], center);
            if (distance > maxDistance)
                maxDistance = distance;
        }

        // Assign UV2 values (feathering values)
        for (int i = 0; i < vertexCount; i++)
        {
            float distance = Vector3.Distance(s_Vertices[i], center);
            float featheringUV = Mathf.Clamp01(1 - (distance / Mathf.Max(maxDistance, 0.001f))); // Inverted for correct feathering effect
            s_FeatheringUVs.Add(new Vector3(featheringUV, 0, 0));
        }

        // Apply UVs to UV2 channel
        mesh.SetUVs(1, s_FeatheringUVs);
        mesh.UploadMeshData(false);
    }
}
