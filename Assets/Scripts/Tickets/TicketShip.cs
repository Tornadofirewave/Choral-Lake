using System;
using UnityEngine;
using ChoralLake.Core;
using ChoralLake.Audio;

namespace ChoralLake.Tickets
{
    /// <summary>
    /// Drives the ship state machine: Approaching → Docked → AwaitingDialogue →
    /// AttendantReturning → Departing → destroyed off-screen.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TicketShip : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
        [SerializeField, Min(0.01f)] private float arrivalEpsilon = 0.1f;

        [Header("References")]
        [SerializeField] private Transform deckPoint;
        [SerializeField] private GameObject attendantPrefab;
        [SerializeField] private Rigidbody2D rb;

        private TicketShipPhase _phase;
        private Vector2 _dockPos;
        private Transform _attendantExitPoint;
        private Action _onDone;
        private ShipAttendant _attendant;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 dockPos, Transform attendantExitPoint, Action onDone)
        {
            _dockPos = dockPos;
            _attendantExitPoint = attendantExitPoint;
            _onDone = onDone;
            SetPhase(TicketShipPhase.Approaching);
        }

        private void FixedUpdate()
        {
            switch (_phase)
            {
                case TicketShipPhase.Approaching:
                    MoveToward(_dockPos);
                    if (Vector2.Distance(rb.position, _dockPos) < arrivalEpsilon)
                        OnDocked();
                    break;

                case TicketShipPhase.Departing:
                    rb.linearVelocity = Vector2.left * moveSpeed;
                    var vp = Camera.main != null
                        ? Camera.main.WorldToViewportPoint(transform.position)
                        : Vector3.zero;
                    if (vp.x < -0.2f) OnOffscreen();
                    break;
            }
        }

        private void MoveToward(Vector2 target)
        {
            var dir = (target - rb.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }

        private void OnDocked()
        {
            rb.linearVelocity = Vector2.zero;
            SetPhase(TicketShipPhase.Docked);
            AudioManager.Instance?.PlaySfx("sfx_ship_dock");
            SpawnAttendant();
        }

        private void SpawnAttendant()
        {
            if (attendantPrefab == null)
            {
                Debug.LogError("[TicketShip] attendantPrefab not assigned.");
                return;
            }

            var spawnPos = deckPoint != null ? deckPoint.position : transform.position;
            var go = Instantiate(attendantPrefab, spawnPos, Quaternion.identity);
            _attendant = go.GetComponent<ShipAttendant>();
            if (_attendant == null)
            {
                Debug.LogError("[TicketShip] Attendant prefab missing ShipAttendant component.");
                Destroy(go);
                return;
            }

            SetPhase(TicketShipPhase.AwaitingDialogue);
            _attendant.Initialize(_attendantExitPoint, (Vector2)spawnPos, OnAttendantDone);
        }

        private void OnAttendantDone()
        {
            _attendant = null;
            SetPhase(TicketShipPhase.Departing);
            AudioManager.Instance?.PlaySfx("sfx_ship_depart");
        }

        private void OnOffscreen()
        {
            rb.linearVelocity = Vector2.zero;
            GameManager.Instance?.ClearPendingTicket();
            _onDone?.Invoke();
            Destroy(gameObject);
        }

        private void SetPhase(TicketShipPhase phase)
        {
            _phase = phase;
            if (GameManager.Instance != null)
                GameManager.Instance.SaveData.pendingShipPhase = phase;
        }
    }
}
