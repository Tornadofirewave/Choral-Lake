using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Tickets
{
    /// <summary>
    /// Placed once in Town.unity. Listens for a pending ticket and spawns the ship prefab
    /// both on scene ready and on each return to the Town scene.
    /// </summary>
    public class TicketShipSpawner : MonoBehaviour
    {
        [SerializeField] private Transform shipSpawnPoint;
        [SerializeField] private Transform shipDockPoint;
        [SerializeField] private Transform attendantExitPoint;
        [SerializeField] private GameObject shipPrefab;

        private TicketShip _liveShip;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnSceneLoadComplete += TrySpawnShip;
                gm.OnPendingTicketChanged += TrySpawnShip;
            }
            TrySpawnShip();
        }

        private void OnDisable()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnSceneLoadComplete -= TrySpawnShip;
                gm.OnPendingTicketChanged -= TrySpawnShip;
            }
        }

        private void TrySpawnShip()
        {
            if (_liveShip != null) return;
            var gm = GameManager.Instance;
            if (gm == null || !gm.HasPendingTicket) return;
            if (shipPrefab == null || shipSpawnPoint == null || shipDockPoint == null || attendantExitPoint == null)
            {
                Debug.LogError("[TicketShipSpawner] Missing required references.");
                return;
            }

            var go = Instantiate(shipPrefab, shipSpawnPoint.position, Quaternion.identity);
            _liveShip = go.GetComponent<TicketShip>();
            if (_liveShip == null)
            {
                Debug.LogError("[TicketShipSpawner] Ship prefab has no TicketShip component.");
                Destroy(go);
                return;
            }

            _liveShip.Initialize(shipDockPoint.position, attendantExitPoint, OnShipDone);
        }

        private void OnShipDone() => _liveShip = null;
    }
}
