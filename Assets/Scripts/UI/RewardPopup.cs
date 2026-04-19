using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChoralLake.Player;

namespace ChoralLake.UI
{
    /// <summary>
    /// Zelda-style item-get popup. World-space canvas that follows the player.
    /// Call Show() after a ticket reward is granted; it fades in, holds, then fades out.
    /// </summary>
    public class RewardPopup : MonoBehaviour
    {
        public static RewardPopup Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private Vector3 playerOffset = new Vector3(0f, 1.8f, 0f);
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.25f;

        private Coroutine _showCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            var player = PlayerRoot.Instance;
            if (player != null)
                transform.position = player.transform.position + playerOffset;
        }

        public void Show(Sprite sprite, string text)
        {
            if (rewardIcon != null) rewardIcon.sprite = sprite;
            if (rewardLabel != null) rewardLabel.text = text;

            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);
            _showCoroutine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = to;
        }
    }
}
