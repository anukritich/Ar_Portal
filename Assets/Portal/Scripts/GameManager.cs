using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("AR References")]
    public GameObject portal;
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;

    [Header("Game Components")]
    public PatternManager patternManager;
    public PlayerMovementChecker playerMovementChecker;

    [Header("UI Elements")]
    public Button startButton;
    public GameObject startButtonObject; // Drag the StartButton GameObject here
    public TMP_Text levelText;
    public GameObject levelTransitionPanel;
    public float levelTransitionTime = 2f;
    public AudioSource levelUpSound;

    [HideInInspector]
    public int currentLevel = 1;
    private bool gameStarted = false;
    private bool isTransitioning = false;

    // Pattern length settings - explicitly set to 3
    public int startingPatternLength = 3;  // Level 1 will have 3 tiles
    public int maxPatternLength = 10;      // Maximum pattern length

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        startButton.onClick.AddListener(StartGame);

        // Make sure UI elements are hidden initially
        if (levelText)
            levelText.gameObject.SetActive(false);

        if (levelTransitionPanel)
            levelTransitionPanel.SetActive(false);

        // Make sure we disable pattern manager at start
        if (patternManager)
            patternManager.enabled = false;
    }

    void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        currentLevel = 1;  // Ensure we start at level 1
        startButton.gameObject.SetActive(false);
        startButtonObject.gameObject.SetActive(false);
        DisableARFeatures();
        LockPortalPosition();

        if (patternManager)
        {
            patternManager.enabled = true;
            patternManager.patternLength = startingPatternLength; // Explicitly set starting pattern to 3

            // Clear any existing pattern data
            patternManager.ResetPattern();
        }

        if (playerMovementChecker)
            playerMovementChecker.enabled = true;

        StartNewLevel();
    }

    public void StartNewLevel()
    {
        // Prevent multiple transitions from happening simultaneously
        if (isTransitioning) return;

        // Reset pattern manager's UI elements
        if (patternManager && patternManager.patternCountText)
            patternManager.patternCountText.gameObject.SetActive(false);

        StartCoroutine(LevelTransition());
    }

    IEnumerator LevelTransition()
    {
        isTransitioning = true;

        // Ensure UI elements are active
        if (levelTransitionPanel)
            levelTransitionPanel.SetActive(true);

        if (levelText)
            levelText.gameObject.SetActive(true);

        // Hide pattern text temporarily
        if (patternManager && patternManager.patternCountText)
            patternManager.patternCountText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f); // Ensure UI resets properly

        // Set the new pattern length for this level
        if (patternManager)
        {
            patternManager.patternLength = Mathf.Min(startingPatternLength + (currentLevel - 1), maxPatternLength);
            Debug.Log($"Level {currentLevel} - Setting pattern length to {patternManager.patternLength}");
        }

        // **Update Level Text (both in panel and separate UI text)**
        UpdateLevelTextOnPanel();
        if (levelText)
        {
            levelText.text = $"Level {currentLevel}";
            Debug.Log("Displaying Level Text: " + levelText.text);
        }

        // Play level-up sound (if not level 1)
        if (levelUpSound && currentLevel > 1)
            levelUpSound.Play();

        yield return new WaitForSeconds(levelTransitionTime); // Keep both visible

        // Hide UI after transition
        if (levelTransitionPanel)
            levelTransitionPanel.SetActive(false);

        if (levelText)
            levelText.gameObject.SetActive(false);

        // Short delay before showing pattern
        yield return new WaitForSeconds(0.5f);

        // Start pattern sequence
        if (patternManager)
        {
            patternManager.StartPatternSequence();
        }

        isTransitioning = false;
    }


    private void UpdateLevelTextOnPanel()
    {
        if (!levelTransitionPanel) return;

        bool textUpdated = false;

        // Update `TextMeshPro` and `Text` elements inside the panel
        TMP_Text[] tmpTexts = levelTransitionPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in tmpTexts)
        {
            if (t.name.Contains("Level"))
            {
                t.text = "Level " + currentLevel;
                textUpdated = true;
            }
        }

        Text[] texts = levelTransitionPanel.GetComponentsInChildren<Text>(true);
        foreach (Text t in texts)
        {
            if (t.name.Contains("Level"))
            {
                t.text = "Level " + currentLevel;
                textUpdated = true;
            }
        }

        if (textUpdated)
            Debug.Log($"Updated Level Text on Panel: Level {currentLevel}");
        else
            Debug.LogWarning("No Level text was found in the panel!");
    }

    public void OnPatternCompleted()
    {
        // Increase level number
        currentLevel++;

        // Start next level after a short delay
        StartCoroutine(DelayedLevelStart());
    }

    IEnumerator DelayedLevelStart()
    {
        yield return new WaitForSeconds(1.5f); // Short celebration delay
        StartNewLevel();
    }

    void DisableARFeatures()
    {
        if (planeManager)
        {
            planeManager.enabled = false;
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }

        if (raycastManager)
            raycastManager.enabled = false;
    }

    void LockPortalPosition()
    {
        if (portal)
        {
            Rigidbody rb = portal.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true;
            }

            // Get reference to the PatternManager from the portal if needed
            PatternManager portalPatternManager = portal.GetComponent<PatternManager>();
            if (portalPatternManager != null && patternManager == null)
            {
                patternManager = portalPatternManager;
            }

            // Disable other scripts on the portal except PatternManager
            MonoBehaviour[] scripts = portal.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (!(script is PatternManager))
                {
                    script.enabled = false;
                }
            }
        }
    }
}