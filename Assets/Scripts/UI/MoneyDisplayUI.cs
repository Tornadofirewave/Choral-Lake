using System.Collections;
using TMPro;
using UnityEngine;
using ChoralLake.Audio;
using ChoralLake.Core;

namespace ChoralLake.UI
{
    public class MoneyDisplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private string format = "{0}";

        [Header("Tally Animation")]
        [SerializeField] private float tallyDuration = 1.5f;
        [SerializeField] private string tallySfxId = "sfx_receipt_tick";
        [SerializeField] private float tickInterval = 0.05f;

        private int _displayedValue;
        private Coroutine _tallyCoroutine;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnMoneyChanged += Refresh;
            SnapTo(gm.SaveData.money);
        }

        private void OnDisable()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnMoneyChanged -= Refresh;
            if (_tallyCoroutine != null) StopCoroutine(_tallyCoroutine);
        }

        private void Refresh()
        {
            int target = GameManager.Instance.SaveData.money;
            if (target > _displayedValue)
            {
                if (_tallyCoroutine != null) StopCoroutine(_tallyCoroutine);
                _tallyCoroutine = StartCoroutine(TallyCoroutine(target));
            }
            else
            {
                SnapTo(target);
            }
        }

        private void SnapTo(int value)
        {
            if (_tallyCoroutine != null) { StopCoroutine(_tallyCoroutine); _tallyCoroutine = null; }
            _displayedValue = value;
            moneyText.text = string.Format(format, value);
        }

        private IEnumerator TallyCoroutine(int target)
        {
            float rate = (target - _displayedValue) / tallyDuration;
            float current = _displayedValue;
            float tickTimer = 0f;

            while (current < target)
            {
                current = Mathf.Min(current + rate * Time.deltaTime, target);
                _displayedValue = Mathf.RoundToInt(current);
                moneyText.text = string.Format(format, _displayedValue);

                tickTimer -= Time.deltaTime;
                if (tickTimer <= 0f)
                {
                    AudioManager.Instance?.PlaySfx(tallySfxId);
                    tickTimer = tickInterval;
                }

                yield return null;
            }

            _displayedValue = target;
            moneyText.text = string.Format(format, target);
            _tallyCoroutine = null;
        }
    }
}
