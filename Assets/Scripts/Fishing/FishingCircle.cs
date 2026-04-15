using UnityEngine;
using UnityEngine.InputSystem;

public class FishingCircle : MonoBehaviour {
    [SerializeField] private float timeDuration = 3f;
    private float timeRemaining;
    private float elapsedTime;
    private float perfectWindow = 0.25f;
    [SerializeField] private bool debugGraceWindowLogs = true;

    private Transform ringSprite;
    private Transform circleSprite;
    private Vector3 ringStartScale;
    private Vector3 ringTargetScale;
    private bool hasLoggedGraceWindowStart;

    private void Awake()
    {
        ringSprite = transform.GetChild(0);
        circleSprite = transform.GetChild(1);
    }

    private void Start()
    {
        timeDuration = Mathf.Max(0f, timeDuration);
        perfectWindow = Mathf.Max(0f, perfectWindow);
        elapsedTime = 0f;
        timeRemaining = timeDuration;
        ringStartScale = ringSprite.localScale;
        ringTargetScale = circleSprite.localScale;
        hasLoggedGraceWindowStart = false;

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
        
        if (Mouse.current != null && mouse.leftButton.wasPressedThisFrame) {
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

}