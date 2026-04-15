using UnityEngine;
using UnityEngine.InputSystem;


public class FishingRing : MonoBehaviour {
    [SerializeField] private float timeDuration = 3f;
    private SpriteRenderer ringSpriteRenderer;
    private string outerRingColor = "#1c6efb";

    private float timeRemaining;

    private void Awake()
    {
        if (ringSpriteRenderer == null)
        {
            ringSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        timeRemaining = timeDuration;
    }

    private void Update()
    {
        var mouse = Mouse.current;
        
        if (Mouse.current != null && mouse.leftButton.wasPressedThisFrame) {
            if (timeRemaining <= 1f) {
                Debug.Log("Deleted!");
                Destroy(gameObject);
            } else {
                Debug.Log("Too early");
            }
        }

        if (timeRemaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        timeRemaining -= Time.deltaTime;
    }

}