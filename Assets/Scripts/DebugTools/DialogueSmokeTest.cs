using System.Collections;
using UnityEngine;
using ChoralLake.Core;
using ChoralLake.Dialogue;

namespace ChoralLake.DebugTools
{
    /// <summary>
    /// TEMPORARY: verifies dialogue wiring. Logs database stats and auto-triggers a test
    /// conversation after a short delay. Delete after acceptance criteria pass.
    /// </summary>
    public class DialogueSmokeTest : MonoBehaviour
    {
        [SerializeField] private string testConversationId = "smoke_test_convo";
        [SerializeField] private float autoTriggerDelay = 2f;

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[DialogueSmokeTest] GameManager.Instance is null.");
                return;
            }
            if (gm.DialogueDatabase == null)
            {
                Debug.LogError("[DialogueSmokeTest] DialogueDatabase is null.");
                return;
            }

            Debug.Log($"[DialogueSmokeTest] Conversation count: {gm.DialogueDatabase.ConversationCount}");
            Debug.Log($"[DialogueSmokeTest] Contains '{testConversationId}': {gm.DialogueDatabase.Contains(testConversationId)}");

            StartCoroutine(AutoTrigger());
        }

        private IEnumerator AutoTrigger()
        {
            yield return new WaitForSeconds(autoTriggerDelay);
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueSmokeTest] DialogueManager.Instance is null.");
                yield break;
            }
            Debug.Log($"[DialogueSmokeTest] Auto-triggering '{testConversationId}'");
            DialogueManager.Instance.StartConversation(testConversationId);
        }
    }
}
