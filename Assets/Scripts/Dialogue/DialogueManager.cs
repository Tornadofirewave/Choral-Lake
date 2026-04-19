using System;
using System.Collections;
using UnityEngine;
using ChoralLake.UI;
using UnityEngine.InputSystem;
using TMPro;
using ChoralLake.Core;
using ChoralLake.Player;

namespace ChoralLake.Dialogue
{
    /// <summary>
    /// Modal dialogue controller. Handles typewriter effect, input-driven advancement,
    /// action-map swapping, and player movement lock.
    /// Place one instance in the Boot scene; it persists via DontDestroyOnLoad.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyText;

        [Header("Typewriter")]
        [SerializeField, Min(0.001f)] private float charactersPerSecond = 45f;

        [Header("Audio")]
        [SerializeField] private AudioSource typewriterAudio;
        [SerializeField, Range(0.5f, 2f)] private float pitchMin = 0.9f;
        [SerializeField, Range(0.5f, 2f)] private float pitchMax = 1.1f;
        [Tooltip("Play a tick every N characters revealed.")]
        [SerializeField, Min(1)] private int tickEveryNChars = 2;

        public bool IsOpen { get; private set; }

        /// <summary>Fires when a conversation closes naturally. Passes the conversation id.</summary>
        public event Action<string> OnConversationEnded;

        private DialogueConversation _activeConversation;
        private int _currentLineIndex;
        private Coroutine _typewriterCoroutine;
        private bool _isRevealing;
        private string _fullLineText;

        private PlayerControls _controls;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _controls = new PlayerControls();

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        private void OnEnable()
        {
            _controls.UI.Advance.performed += OnAdvancePressed;
        }

        private void OnDisable()
        {
            _controls.UI.Advance.performed -= OnAdvancePressed;
            _controls.UI.Disable();
        }

        /// <summary>
        /// Looks up the conversation by ID and starts it. No-op if dialogue is already open
        /// or the ID is unknown.
        /// </summary>
        public void StartConversation(string conversationId)
        {
            if (IsOpen)
            {
                Debug.LogWarning("[DialogueManager] StartConversation called while another dialogue is open. Ignoring.");
                return;
            }
            if (GameManager.Instance?.DialogueDatabase == null)
            {
                Debug.LogError("[DialogueManager] Dialogue database not available on GameManager.");
                return;
            }

            var convo = GameManager.Instance.DialogueDatabase.GetById(conversationId);
            if (convo == null || convo.Lines.Count == 0)
            {
                Debug.LogWarning($"[DialogueManager] Conversation '{conversationId}' not found or empty.");
                return;
            }

            _activeConversation = convo;
            _currentLineIndex = 0;
            IsOpen = true;
            ModalStack.Push();

            _controls.Player.Disable();
            _controls.UI.Enable();

            PlayerRoot.Instance?.Movement?.LockMovement();

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            var line = _activeConversation.Lines[_currentLineIndex];
            if (speakerLabel != null)
            {
                speakerLabel.text = line.SpeakerName;
                speakerLabel.gameObject.SetActive(!string.IsNullOrEmpty(line.SpeakerName));
            }

            _fullLineText = line.Text;
            if (bodyText != null) bodyText.text = string.Empty;

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }

        private IEnumerator TypewriterRoutine()
        {
            _isRevealing = true;
            float secondsPerChar = 1f / charactersPerSecond;
            var sb = new System.Text.StringBuilder();
            int charCount = 0;

            foreach (char c in _fullLineText)
            {
                sb.Append(c);
                if (bodyText != null) bodyText.text = sb.ToString();

                if (!char.IsWhiteSpace(c) && typewriterAudio != null && charCount % tickEveryNChars == 0)
                {
                    typewriterAudio.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
                    typewriterAudio.PlayOneShot(typewriterAudio.clip);
                }
                charCount++;

                yield return new WaitForSecondsRealtime(secondsPerChar);
            }

            _isRevealing = false;
            _typewriterCoroutine = null;
        }

        private void OnAdvancePressed(InputAction.CallbackContext ctx)
        {
            if (!IsOpen) return;

            if (_isRevealing)
            {
                // Complete current line instantly.
                if (_typewriterCoroutine != null)
                {
                    StopCoroutine(_typewriterCoroutine);
                    _typewriterCoroutine = null;
                }
                if (bodyText != null) bodyText.text = _fullLineText;
                _isRevealing = false;
                return;
            }

            _currentLineIndex++;
            if (_currentLineIndex >= _activeConversation.Lines.Count)
                CloseDialogue();
            else
                ShowCurrentLine();
        }

        private void CloseDialogue()
        {
            string endedId = _activeConversation?.Id;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            IsOpen = false;
            ModalStack.Pop();
            _activeConversation = null;

            _controls.UI.Disable();
            _controls.Player.Enable();

            PlayerRoot.Instance?.Movement?.UnlockMovement();
            OnConversationEnded?.Invoke(endedId);
        }
    }
}
