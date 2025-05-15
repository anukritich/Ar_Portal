using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatternManager : MonoBehaviour
{
    [Header("Pattern Settings")]
    public Color highlightColor = Color.yellow;
    public Color defaultColor = Color.white;
    public int patternLength = 5;
    public float transitionTime = 0.5f;

    [Header("UI Elements")]
    public Button startButton;
    public TMP_Text patternCountText;
    public AudioSource playSound;

    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> currentPattern = new List<GameObject>();
    private bool patternSequenceActive = false;
    private PlayerMovementChecker playerChecker; // Reference to PlayerMovementChecker

    void Start()
    {
        startButton.onClick.AddListener(StartPatternSequence);
        startButton.gameObject.SetActive(true);
        patternCountText.gameObject.SetActive(false);

        // Find all tiles and set them inactive
        GameObject[] tileObjects = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in tileObjects)
        {
            tile.SetActive(false);
            tiles.Add(tile);
        }

        // Get reference to PlayerMovementChecker
        playerChecker = FindObjectOfType<PlayerMovementChecker>();
    }

    public void SetPortal(GameObject portal)
    {
        // Assuming the portal has child objects tagged as "Tile"
        StartCoroutine(SetupTiles(portal));
        startButton.gameObject.SetActive(true);
    }

    private IEnumerator SetupTiles(GameObject portal)
    {
        yield return null; // Wait for the next frame

        if (portal == null)
        {
            Debug.LogError("PatternManager: No portal assigned!");
            yield break;
        }

        // Clear the existing tiles list
        tiles.Clear();

        // Find all child objects of the portal that are tagged as "Tile"
        foreach (Transform child in portal.transform)
        {
            if (child.CompareTag("Tile"))
            {
                tiles.Add(child.gameObject);
                child.gameObject.SetActive(false); // Ensure they are inactive by default
            }
        }

        Debug.Log("Total Tiles Loaded: " + tiles.Count);
    }

    public void StartPatternSequence()
    {
        if (patternSequenceActive) return;

        patternSequenceActive = true;
        SelectActiveTiles();  // Randomly activate tiles
        GeneratePattern();    // Generate pattern from those active tiles

        if (patternCountText)
        {
            patternCountText.text = "Remember " + currentPattern.Count + " tiles";
            patternCountText.gameObject.SetActive(true);
        }

        // Set the pattern in PlayerMovementChecker
        if (playerChecker != null)
        {
            playerChecker.SetPattern(currentPattern);
        }

        StartCoroutine(StartPatternWithDelay());
    }

    public void ResetPattern()
    {
        currentPattern.Clear();
        patternCountText.gameObject.SetActive(false);
        patternSequenceActive = false;

        // Deactivate all tiles
        foreach (GameObject tile in tiles)
        {
            tile.SetActive(false);
        }
    }

    IEnumerator StartPatternWithDelay()
    {
        yield return new WaitForSeconds(2f);
        if (patternCountText)
            patternCountText.gameObject.SetActive(false);

        yield return StartCoroutine(DisplayPattern());
        patternSequenceActive = false;
    }

    void SelectActiveTiles()
    {
        // Clear any previously active tiles
        foreach (GameObject tile in tiles)
        {
            tile.SetActive(false);
        }

        // Randomly activate a subset of tiles
        int numActiveTiles = Random.Range(4, 10);
        List<GameObject> shuffledTiles = new List<GameObject>(tiles);
        ShuffleList(shuffledTiles);

        for (int i = 0; i < Mathf.Min(numActiveTiles, shuffledTiles.Count); i++)
        {
            shuffledTiles[i].SetActive(true);
        }
    }

    void GeneratePattern()
    {
        currentPattern.Clear();
        List<GameObject> activeTiles = new List<GameObject>();

        // Collect currently active tiles
        foreach (GameObject tile in tiles)
        {
            if (tile.activeSelf)
            {
                activeTiles.Add(tile);
            }
        }

        // Randomly select tiles for the pattern
        List<GameObject> shuffledActiveTiles = new List<GameObject>(activeTiles);
        ShuffleList(shuffledActiveTiles);
        int actualPatternLength = Mathf.Min(patternLength, shuffledActiveTiles.Count);

        for (int i = 0; i < actualPatternLength; i++)
        {
            currentPattern.Add(shuffledActiveTiles[i]);
        }
    }

    IEnumerator DisplayPattern()
    {
        foreach (GameObject tile in currentPattern)
        {
            if (playSound != null)
                playSound.Play();

            yield return StartCoroutine(ChangeTileColor(tile, highlightColor, transitionTime));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ChangeTileColor(tile, defaultColor, transitionTime));
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator ChangeTileColor(GameObject tile, Color targetColor, float duration)
    {
        Renderer rend = tile.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError($"Renderer missing on {tile.name}");
            yield break;
        }

        Color startColor = rend.material.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            rend.material.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            yield return null;
        }

        rend.material.color = targetColor;
    }

    void ShuffleList(List<GameObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}