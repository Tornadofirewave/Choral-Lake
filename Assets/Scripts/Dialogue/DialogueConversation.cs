using System.Collections.Generic;

namespace ChoralLake.Dialogue
{
    public class DialogueConversation
    {
        public string Id { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }

        public DialogueConversation(string id, List<DialogueLine> lines)
        {
            Id = id;
            Lines = lines ?? new List<DialogueLine>();
        }
    }
}
