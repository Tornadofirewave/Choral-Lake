using UnityEngine;
using UnityEngine.InputSystem;

public class FishingCircle : MonoBehaviour {
    [SerializeField] private float timeDuration = 3f;
    private float timeRemaining;
    private float elapsedTime;
    [SerializeField] private float perfectWindow = 0.25f;
    [SerializeField] private bool debugGraceWindowLogs = true;

    private Transform ringSprite;
    private Transform circleSprite;
    private Collider2D circleCollider;
    private Vector3 ringStartScale;
    private Vector3 ringTargetScale;
    private bool hasLoggedGraceWindowStart;

    private void Awake()
    {
        ringSprite = transform.GetChild(0);
        circleSprite = transform.GetChild(1);
        circleCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        ringStartScale = ringSprite.localScale;
        ringTargetScale = circleSprite.localScale;
        ResetState();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Outer Ring Closes in
        float shrinkElapsedTime = Mathf.Min(elapsedTime, timeDuration);
        timeRemaining = Mathf.Max(0f, timeDuration - shrinkElapsedTime);
        float progress = timeDuration > 0f ? shrinkElapsedTime / timeDuration : 1f;
        ringSprite.localScale = Vector3.Lerp(ringStartScale, ringTargetScale, progress);

        if (debugGraceWindowLogs && !hasLoggedGraceWindowStart && elapsedTime >= timeDuration)
        {
            hasLoggedGraceWindowStart = true;
            Debug.Log($"Fishing ring grace window started ({perfectWindow:F2}s)");
        }

        // Detect on-time Mouse Clicks
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && IsMouseOverThisCircle(mouse)) {
            if (timeRemaining <= perfectWindow) {
                Debug.Log("Deleted!");
                Destroy(gameObject);
            } else {
                Debug.Log("Too early");
            }
        }

        // Delete object once the post-close grace window has passed.
        if (elapsedTime >= timeDuration + perfectWindow)
        {
            if (debugGraceWindowLogs)
            {
                Debug.Log("Fishing ring grace window ended (auto delete)");
            }

            ringSprite.localScale = ringTargetScale;
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize(float newTimeDuration, float newPerfectWindow, bool enableDebugLogs)
    {
        timeDuration = Mathf.Max(0f, newTimeDuration);
        perfectWindow = Mathf.Max(0f, newPerfectWindow);
        debugGraceWindowLogs = enableDebugLogs;
        ResetState();
    }

    private void ResetState()
    {
        timeDuration = Mathf.Max(0f, timeDuration);
        perfectWindow = Mathf.Max(0f, perfectWindow);
        elapsedTime = 0f;
        timeRemaining = timeDuration;
        hasLoggedGraceWindowStart = false;
    }

    private bool IsMouseOverThisCircle(Mouse mouse)
    {
        if (circleCollider == null || Camera.main == null)
        {
            return false;
        }

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        return circleCollider.OverlapPoint(mouseWorldPos);
    }

}