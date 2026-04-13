namespace ChoralLake.Dialogue
{
    public class DialogueLine
    {
        public string SpeakerName { get; }
        public string Text { get; }

        public DialogueLine(string speakerName, string text)
        {
            SpeakerName = speakerName ?? string.Empty;
            Text = text ?? string.Empty;
        }
    }
}
