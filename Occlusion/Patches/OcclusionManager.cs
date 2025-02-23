using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Occlusion.Patches
{
    public class OcclusionManager : MonoBehaviour
    {
        private static Dictionary<Piece, MeshRenderer[]> _meshCache;
        private const float CHECK_INTERVAL = 0.5f;
        private int _currentIndex = 0;
        private List<Piece> _pieces;
        private LineRenderer _lineRenderer;

        private void Start()
        {
            Debug.Log("Occlusion Manager has started!");
            _meshCache = PiecePatches.MeshCache;
            _pieces = new List<Piece>(_meshCache.Keys);

            // ✅ Create a LineRenderer dynamically
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = 0.02f;
            _lineRenderer.endWidth = 0.02f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Default simple shader
            _lineRenderer.positionCount = 2;
            _lineRenderer.startColor = Color.green;
            _lineRenderer.endColor = Color.red;

            StartCoroutine(PerformOcclusionCulling());
        }

        private Vector3[] GetBoundingBoxCorners(Bounds bounds)
{
    Vector3 min = bounds.min;
    Vector3 max = bounds.max;

    return new Vector3[]
    {
        new Vector3(min.x, min.y, min.z), // Bottom-left-front
        new Vector3(max.x, min.y, min.z), // Bottom-right-front
        new Vector3(min.x, max.y, min.z), // Top-left-front
        new Vector3(max.x, max.y, min.z), // Top-right-front
        new Vector3(min.x, min.y, max.z), // Bottom-left-back
        new Vector3(max.x, min.y, max.z), // Bottom-right-back
        new Vector3(min.x, max.y, max.z), // Top-left-back
        new Vector3(max.x, max.y, max.z)  // Top-right-back
    };
}

private IEnumerator PerformOcclusionCulling()
{
    while (true)
    {
        _pieces.Clear();
        _pieces.AddRange(_meshCache.Keys);

        if (_pieces.Count == 0)
        {
            Debug.Log("No Occlusion Pieces found!");
            yield return new WaitForSeconds(CHECK_INTERVAL);
            continue;
        }

        if (_meshCache == null || _meshCache.Count == 0)
        {
            yield return new WaitForSeconds(CHECK_INTERVAL);
            continue;
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("OcclusionManager: Camera.main is null!");
            yield return new WaitForSeconds(CHECK_INTERVAL);
            continue;
        }

        Vector3 cameraPos = Camera.main.transform.position;

        int batchSize = Mathf.Max(1, _pieces.Count / 10);
        int startIndex = _currentIndex;
        int endIndex = Mathf.Min(startIndex + batchSize, _pieces.Count);

        Dictionary<MeshRenderer, bool> visibilityMap = new Dictionary<MeshRenderer, bool>();

        for (int i = startIndex; i < endIndex; i++)
        {
            if (!_meshCache.TryGetValue(_pieces[i], out MeshRenderer[] renderers) || renderers == null)
                continue;

            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                    continue;

                bool isVisible = false; // Reset per renderer

                try
                {
                    Bounds bounds = renderer.bounds;
                    Vector3[] checkPoints = GetBoundingBoxCorners(bounds);
                    LayerMask occlusionMask = LayerMask.GetMask("piece");

                    foreach (Vector3 point in checkPoints)
                    {
                        Vector3 direction = (point - cameraPos).normalized;
                        float distance = Vector3.Distance(cameraPos, point);

                        // Cast ray and check what it hits
                        if (Physics.Raycast(cameraPos, direction, out RaycastHit hit, distance, occlusionMask))
                        {
                            Collider expectedCollider = renderer.GetComponentInParent<Collider>(); // Get the correct collider
                            if (expectedCollider == null)
                            {
                                expectedCollider = renderer.GetComponentInChildren<Collider>(); // Try children if needed
                            }

                            if (expectedCollider != null && hit.collider == expectedCollider)
                            {
                                isVisible = true;
                                DrawRay(cameraPos, point, true);
                                break;
                            }
                            else
                            {
                                Debug.Log($"Ray hit {hit.collider.gameObject.name}, expected {expectedCollider?.gameObject.name}");
                            }
                        }
                    

                        


                    }

                    visibilityMap[renderer] = isVisible;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"OcclusionManager: Error accessing bounds for {renderer?.gameObject?.name} - {ex.Message}");
                }
            }
        }

        // ✅ Update only if the state actually changes
        foreach (var kvp in visibilityMap)
        {
            MeshRenderer renderer = kvp.Key;
            bool isVisible = kvp.Value;

            if (renderer != null)
            {
                if (renderer.enabled != isVisible)
                {
                    renderer.enabled = isVisible;
                }

                if (renderer.gameObject.activeSelf != isVisible)
                {
                    renderer.gameObject.SetActive(isVisible);
                }
            }
        }

        _currentIndex += batchSize;
        if (_currentIndex >= _pieces.Count) _currentIndex = 0;

        yield return new WaitForSeconds(CHECK_INTERVAL / 10);
    }
}

        

        // ✅ Method to draw the ray in-game using LineRenderer
        private void DrawRay(Vector3 start, Vector3 end, bool hit)
        {
            GameObject lineObj = new GameObject("DebugRay");
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;

            lineRenderer.startColor = hit ? Color.green : Color.red;
            lineRenderer.endColor = hit ? Color.green : Color.red;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // Destroy the LineRenderer after 2 seconds
            GameObject.Destroy(lineObj, 2f);
        }

    }
}
