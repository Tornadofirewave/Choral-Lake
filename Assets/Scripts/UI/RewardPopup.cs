using System.Collections;
using UnityEngine;
using TMPro;

namespace ChoralLake.UI
{
    public class RewardPopup : MonoBehaviour
    {
        public static RewardPopup Instance { get; private set; }

        [SerializeField] private SpriteRenderer rewardIcon;
        [SerializeField] private TextMeshPro rewardLabel;
        [SerializeField] private SpriteRenderer[] fadedRenderers;
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.25f;

        private Coroutine _showCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            SetAlpha(0f);
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
                SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            foreach (var sr in fadedRenderers)
            {
                if (sr == null) continue;
                var c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, alpha);
            }
            if (rewardLabel != null) rewardLabel.alpha = alpha;
        }
    }
}
