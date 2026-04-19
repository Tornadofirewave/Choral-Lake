using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "Attendant_", menuName = "Choral Lake/NPC Attendant")]
    public class NpcAttendantSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Dialogue")]
        [SerializeField] private string dialogueConversationId;

        [Header("Presentation")]
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite walkSprite;
        [SerializeField] private RuntimeAnimatorController animatorController;

        public string Id => id;
        public string DisplayName => displayName;
        public string DialogueConversationId => dialogueConversationId;
        public Sprite Portrait => portrait;
        public Sprite WalkSprite => walkSprite;
        public RuntimeAnimatorController AnimatorController => animatorController;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                Debug.LogWarning($"[NpcAttendantSO] '{name}' has no ID set.", this);
        }
    }
}
