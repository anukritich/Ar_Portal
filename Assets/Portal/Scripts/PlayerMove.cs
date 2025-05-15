using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementChecker : MonoBehaviour
{
    public Camera arCamera;  // AR Camera to track movement
    public float detectionRadius = 0.3f;  // How close the player needs to be to a tile
    public LayerMask tileLayer;  // Layer for tiles
    public Color correctColor = Color.green;  // Color when stepping correctly
    public Color wrongColor = Color.red;  // Color when stepping wrong
    public Color defaultColor = Color.white;  // Reset color after mistake

    // UI Reference
    public GameObject levelCompletedUI;  // Reference to Level Completed UI object with text
    public float levelDisplayTime = 2f;  // How long to show the level text

    private List<GameObject> patternTiles = new List<GameObject>();  // Correct tile sequence
    private int currentStep = 0;  // Track which tile player should step on next
    private bool gameOver = false;  // Prevent multiple detections after failure
    private bool patternCompleted = false;  // Flag to track pattern completion
    private GameObject lastDetectedTile;  // Track the last tile that was detected
    private bool processingTile = false;  // Flag to prevent multiple detections while processing

    public void SetPattern(List<GameObject> pattern)
    {
        patternTiles = pattern;
        currentStep = 0;
        gameOver = false;
        patternCompleted = false;
        lastDetectedTile = null;
        processingTile = false;
    }

    void Update()
    {
        if (!gameOver && patternTiles.Count > 0 && !processingTile)
        {
            CheckPlayerPosition();
        }
    }

    void CheckPlayerPosition()
    {
        Collider[] hitColliders = Physics.OverlapSphere(arCamera.transform.position, detectionRadius, tileLayer);
        if (hitColliders.Length == 0)
        {
            // Reset the last detected tile when player moves away from all tiles
            lastDetectedTile = null;
            return;
        }

        // Find the closest tile
        GameObject closestTile = null;
        float closestDistance = Mathf.Infinity;
        foreach (Collider hit in hitColliders)
        {
            float distance = Vector3.Distance(arCamera.transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = hit.gameObject;
            }
        }

        if (closestTile == null) return; // No valid tile detected

        // Only process the tile if it's not the same as the last detected tile
        if (closestTile != lastDetectedTile)
        {
            lastDetectedTile = closestTile;
            processingTile = true;

            if (closestTile == patternTiles[currentStep])
            {
                StartCoroutine(ChangeTileColor(closestTile, correctColor));
                currentStep++;
                if (currentStep >= patternTiles.Count)
                {
                    Debug.Log("✅ Pattern Complete!");
                    patternCompleted = true;
                    gameOver = true;
                    // Notify GameManager that pattern is complete
                    GameManager.Instance.OnPatternCompleted();
                }
            }
            else
            {
                StartCoroutine(ChangeTileColor(closestTile, wrongColor));
                Debug.Log("❌ Wrong Tile! Restarting...");
                StartCoroutine(RestartGame());
            }
        }
    }

    // Method to check if pattern is completed - needed for PatternManager
    public bool HasCompletedPattern()
    {
        return patternCompleted;
    }

    IEnumerator ChangeTileColor(GameObject tile, Color targetColor)
    {
        Renderer rend = tile.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = targetColor;
            yield return new WaitForSeconds(0.5f);
            if (targetColor == wrongColor)
            {
                rend.material.color = defaultColor;
            }
            processingTile = false;
        }
    }

    IEnumerator RestartGame()
    {
        gameOver = true;
        yield return new WaitForSeconds(1f);
        SetPattern(patternTiles);  // Restart with the same pattern
    }
}