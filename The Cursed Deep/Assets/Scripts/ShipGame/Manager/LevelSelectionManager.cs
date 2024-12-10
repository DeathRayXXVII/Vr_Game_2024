using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ShipGame.Manager
{
    public class LevelSelectionManager : MonoBehaviour
    {
        [SerializeField] private bool _allowDebug;
        
        [SerializeField] private IntData _currentLevel;
        [SerializeField] private IntData _countToBoss;
        
        [SerializeField, ReadOnly] private bool _levelSelected;
        [SerializeField, ReadOnly] private bool _bossLevelSelected;
        [SerializeField, ReadOnly] private bool _merchantSelected;
        
        [SerializeField] private GameAction _unlockDoorToLevelAction;
        
        [SerializeField] private BoolData toNormalLevelBool;
        [SerializeField] private BoolData toBossLevelBool;
        
        [SerializeField] private GameAction _unlockDoorToMerchantAction;
        [SerializeField] private BoolData toMerchantBool;

        private List<LevelSelection> _bossLevelsList; 
        private List<LevelSelection> bossLevelsList => 
            _bossLevelsList ??= _levelOptions.Where(opt => opt != null && opt.isBossLevel).ToList();

        private List<LevelSelection> _normalLevelsList; 
        private List<LevelSelection> normalLevelsList => 
            _normalLevelsList ??= _levelOptions.Where(opt => opt != null && !opt.isBossLevel).ToList();
        
        private readonly WaitForFixedUpdate _waitFixed = new();
        
        [SerializeField] private LevelSelectUIManager _levelSelectUIManager;
        [SerializeField] private Transform _activatedUIPosition;
        [SerializeField, SteppedRange(0.1f, 5, 0.1f)] private float _animationDuration = 1f;
        private int _selectedLevelIndex = -2;
        
        private bool ValidSelection(int index)
        {
            // -2 is the default empty value, -1 is the merchant selection, 0 and above are valid level selections
            if (index >= -1 && index < _levelOptions.Length)
            {
                return true;
            }
            
            Debug.LogError($"[ERROR] Invalid Level Index: {index} provided", this);
            return false;
        }
        
        [SerializeField] private LevelSelection[] _levelOptions;
        private bool _needsBossConfirmation;
        private Coroutine _bossConfirmationCoroutine;
        
        private void ConfirmButtonPressed() => HandleButtonPressed(true);
        private void DeclineButtonPressed() => HandleButtonPressed(false);
        
        private void HandleButtonPressed(bool confirmed)
        {
            if (_needsBossConfirmation && _bossConfirmationCoroutine == null)
            {
                _bossConfirmationCoroutine = StartCoroutine(HandleBossConfirmation(confirmed));
                return;
            }

            StartCoroutine(confirmed ? SelectionConfirmed() : SelectionCancelled());
        }
        
        private IEnumerator SelectionConfirmed()
        {
            if(!ValidSelection(_selectedLevelIndex)) yield break;
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level [{_selectedLevelIndex}] Confirmed", this);
            
            GameAction leaveAction = _merchantSelected ? _unlockDoorToMerchantAction : _unlockDoorToLevelAction;
            leaveAction.RaiseAction();
        }
        
        private bool _cancelingSelection;
        
        private IEnumerator WaitForUIDeactivation(Transform disappearTransform)
        {
            yield return StartCoroutine(_levelSelectUIManager.DeactivateUI(
                _activatedUIPosition.position, disappearTransform.position, _animationDuration));
        }
        
        private IEnumerator SelectionCancelled()
        {
            if(_cancelingSelection || !ValidSelection(_selectedLevelIndex)) yield break;
            _cancelingSelection = true;
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level [{_selectedLevelIndex}] Cancelled", this);
            
            var selectedSocket = _selectedLevelIndex == -1 ? 
                ref _merchantOption.socket :
                ref _levelOptions[_selectedLevelIndex].socket;
            
            yield return StartCoroutine(WaitForUIDeactivation(selectedSocket.transform));
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level [{_selectedLevelIndex}] Cancelled", this);
            
            SetSocketGrabState(true, ref selectedSocket);
            yield return new WaitUntil(() => selectedSocket.GrabState());
            
            _cancelingSelection = false;
            yield return _waitFixed;
        }
        
        private IEnumerator HandleBossConfirmation(bool confirmed)
        {
            if (!_needsBossConfirmation)
            {
                Debug.LogError("[ERROR] Boss Confirmation called incorrectly.", this);
                yield break;
            }
            
            _merchantOption.lockedIndicator.SetActive(false);
            if (confirmed)
            {
                UpdateLevels(bossLevelsList, bossLevelsList.Count, false, opt => opt.isLocked);
            }
            else
            {
                _countToBoss.value = normalLevelsList.Count;
                UpdateLevels(normalLevelsList, normalLevelsList.Count, false, opt => opt.isLocked);
            }
            
            yield return _waitFixed;
            SetAllSocketsState(true, _selectedLevelIndex);
            
            foreach (var task in RetrieveLevelOptionTasks(opt => opt.Initialize(confirmed)))
            {
                StartCoroutine(task);
            }
            
            yield return StartCoroutine(WaitForUIDeactivation(_activatedUIPosition));
            
            _needsBossConfirmation = false;
        }
        
        private void HandleRemovedFromSocket()
        {
            if (_allowDebug) 
                Debug.Log("[DEBUG] Selection Removed", this);
            _levelSelected = _bossLevelSelected = _merchantSelected = false;
            toNormalLevelBool.value = toBossLevelBool.value = toMerchantBool.value = false;
            
            StartCoroutine(SetSocketStateAfterCancel());
        }
        
        private IEnumerator SetSocketStateAfterCancel()
        {
            if (_cancelingSelection)
            {
                yield return new WaitUntil(() => !_cancelingSelection);
            }
            
            _selectedLevelIndex = -2;
            yield return _waitFixed;
            
            SetAllSocketsState(true, _selectedLevelIndex);
        }
        
        private void HandleSocketedInLevelSelection(int index)
        {
            if (index < 0 || index >= _levelOptions.Length)
            {
                Debug.LogError("[ERROR] Index out of range", this);
                return;
            }
            
            _selectedLevelIndex = index;
            var levelSelection = _levelOptions[index];
            
            if (levelSelection.socket == null)
            {
                Debug.LogError("[ERROR] Socket is missing", this);
                return;
            }
            
            if (levelSelection.enemyData == null)
            {
                Debug.LogError("[ERROR] Enemy Data is missing", this);
                return;
            }
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level Option[{index}] Selected, with name {levelSelection.enemyData.unitName}", this);
            
            _merchantSelected = toMerchantBool.value = false;
            _bossLevelSelected = toBossLevelBool.value = levelSelection.isBossLevel;
            _levelSelected = toNormalLevelBool.value = !_bossLevelSelected;
            
            StartCoroutine(_levelSelectUIManager.ActivateUI($"Level {_currentLevel ?? 0}",
                $"{(_bossLevelSelected ? "BOSS" : "")} {levelSelection.enemyData.unitName}", "Confirm", "CANcEL",
                levelSelection.socket.transform.position, _activatedUIPosition.position, _animationDuration));

            SetAllSocketsState(false, index);
        }
        
        private void SetSocketGrabState(bool state, ref SocketMatchInteractor socket)
        {
            socket.AllowGrabInteraction(state);
        }
        
        private static void SetSocketActiveState(bool state, ref SocketMatchInteractor socket)
        {
            if (state)
            {
                socket.EnableSocket();
            }
            else
            {
                socket.DisableSocket();
            }
        }

        private void SetAllSocketsState(bool state, int selectionIndex = -1, bool excludeMerchant = false)
        {
            // grab state is FALSE, only if state is FALSE and selectionIndex EQUALS the selected socket index
            // and TRUE if state is TRUE or selectionIndex DOES NOT EQUAL the selected socket index
            // active state is FALSE, only if state is FALSE and selectionIndex DOES NOT EQUAL the selected socket index
            // and TRUE if state is TRUE or selectionIndex EQUALS the selected socket index
            
            // Working on the merchant socket
            if (!excludeMerchant)
            {
                var merchantSelected = selectionIndex == -1;
            
                SetSocketGrabState(state || !merchantSelected, ref _merchantOption.socket);
                SetSocketActiveState(state || merchantSelected, ref _merchantOption.socket);
            }
            
            // Working on the level sockets
            foreach (var selection in _levelOptions)
            {
                var socket = selection.socket;
                
                if (socket == null)
                {
                    Debug.LogError("[ERROR] Socket is missing", this);
                    continue;
                }
                
                SetSocketGrabState((state || selection.id != selectionIndex) && !selection.isLocked, ref socket);
                SetSocketActiveState((state || selection.id == selectionIndex) && !selection.isLocked, ref socket);
            }
            
#if UNITY_EDITOR
            if (_allowDebug) DebugSocketState();
#endif
        }

        [System.Serializable]
        private struct MerchantSelection
        {
            public Transform transform;
            public SocketMatchInteractor socket;
            public GameObject lockedIndicator;
        }
        
        [SerializeField] private MerchantSelection _merchantOption;
        
        private void HandleSocketedInMerchantSelection()
        {
            _merchantSelected = toMerchantBool.value = true;
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Merchant Selected: {_merchantSelected}", this);
            
            _levelSelected = toNormalLevelBool.value = _bossLevelSelected = toBossLevelBool.value = false;
            
            _selectedLevelIndex = -1;
            SetAllSocketsState(false, _selectedLevelIndex);
            
            StartCoroutine(_levelSelectUIManager.ActivateUI("Head to", "Black Market?",
                "Confirm", "CANcEL", _merchantOption.socket.transform.position, _activatedUIPosition.position,
                _animationDuration));
        }

        private void Awake()
        {
            _bossLevelSelected = _levelSelected = _merchantSelected = false;
            
            var debugMessage = string.Empty;
            var errorMessage = string.Empty;
            
            bool hasLevels = _levelOptions is { Length: > 0 };
            bool hasMerchant = _merchantOption.transform != null && _merchantOption.socket != null;
            bool hasUIManager = _levelSelectUIManager != null;
            bool hasCurrentLevel = _currentLevel != null;
            bool hasCountToBoss = _countToBoss != null;
            bool hasDoorToMerchant = _unlockDoorToMerchantAction != null;
            bool hasDoorToLevel = _unlockDoorToLevelAction != null;
            bool hasToNormalLevelBool = toNormalLevelBool != null;
            bool hasToBossLevelBool = toBossLevelBool != null;
            bool hasToMerchantBool = toMerchantBool != null;
            
            bool hasAllRequired = hasLevels && hasMerchant && hasUIManager && hasCurrentLevel && hasToNormalLevelBool &&
                                  hasToBossLevelBool && hasToMerchantBool;

            if (!hasLevels)
                errorMessage += "\t- Level Selections are missing\n";
            
            for (var i = 0; i < _levelOptions.Length; i++)
            {
                var option = _levelOptions[i];
                if (option == null)
                {
                    errorMessage += $"\t- Level Option {i} is missing\n";
                    continue;
                }
                option.id = i;
            }
            
            if (!hasMerchant)
                errorMessage += "\t- Merchant Selection is missing\n";
            
            if (!hasUIManager)
                errorMessage += "\t- Level Selection UI Manager is missing\n";
            
            if (!hasCurrentLevel)
                errorMessage += "\t- Current Level Data is missing\n";
            
            if (!hasCountToBoss)
                errorMessage += "\t- Count to Boss Data is missing\n";
            
            if (!hasDoorToMerchant)
                errorMessage += "\t- Door to Merchant Action is missing\n";
            
            if (!hasDoorToLevel)
                errorMessage += "\t- Door to Level Action is missing\n";    
            
            if (!hasToNormalLevelBool)
                errorMessage += "\t- Level Selected Holder is missing\n";
            
            if (!hasToBossLevelBool)
                errorMessage += "\t- Boss Level Selected Holder is missing\n";
            
            if (!hasToMerchantBool)
                errorMessage += "\t- Merchant Selected Holder is missing\n";
            
            if (_allowDebug && hasAllRequired)
                debugMessage +=
                    "[DEBUG] All required components are present and accounted for, initializing...\n";
            
            if (_allowDebug && !string.IsNullOrEmpty(debugMessage))
                Debug.Log(debugMessage, this); 
            
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError($"[ERROR] The following is missing and must be provided:\n{errorMessage}", this);
                return;
            }
            
            SetListenerStates(true);
        }
        
        private Coroutine _initCoroutine;
        public void Start()
        {
            _initCoroutine ??= StartCoroutine(Initialize());
        }

        public IEnumerator Initialize()
        {
            var unlockedNormalLevels = normalLevelsList.Count(option => !option.isLocked);
            
            // Wait for all initializations to complete
            foreach (var task in RetrieveLevelOptionTasks(opt => opt.LoadCoroutine()))
            {
                yield return StartCoroutine(task);
            }
            
            // Get Locked Normal Levels Count
            unlockedNormalLevels = normalLevelsList.Where(option => 
                option.isLoaded).Count(option => !option.isLocked);
            
            var lockedDifference = _countToBoss.value - unlockedNormalLevels;
            switch (lockedDifference)
            {
                case > 0:
                    // Too many locked levels: Unlock randomly down to the expected count
                    if (_allowDebug)
                        Debug.Log($"[DEBUG] Locked Normal Level Difference is {lockedDifference}, unlocking down to the expected count.", this);
                    UpdateLevels(normalLevelsList, lockedDifference, false, opt => opt.isLocked);
                    break;
                case < 0:
                    // Not enough locked levels: Lock randomly up to the expected count
                    if (_allowDebug)
                        Debug.Log($"[DEBUG] Locked Normal Level Difference is {lockedDifference}, locking up to the expected count.", this);
                    UpdateLevels(normalLevelsList, -lockedDifference, true, opt => !opt.isLocked);
                    break;
                case 0 when _countToBoss.value == 0:
                    // Lock all normal levels if the expected count is 0
                    if (_allowDebug)
                        Debug.Log($"[DEBUG] Locked Normal Level Difference is {lockedDifference}, locking all normal levels.", this);
                    UpdateLevels(normalLevelsList, normalLevelsList.Count, true, opt => !opt.isLocked);
                    break;
            }
            // Always lock the boss levels
            UpdateLevels(bossLevelsList, bossLevelsList.Count, true, opt => !opt.isLocked);
            
            var excludeMerchantSocket = false;
            // Activate the boss level confirmation if the count to boss is 0
            if (_countToBoss.value == 0 && !toMerchantBool.value)
            {
                _needsBossConfirmation = true;
                excludeMerchantSocket = true;
                _merchantOption.lockedIndicator.SetActive(true);
                yield return StartCoroutine(_levelSelectUIManager.ActivateUI("WARNING", "Are ye ready to face the boss?",
                    "Yes", "No", _activatedUIPosition.position, _activatedUIPosition.position, _animationDuration));
            }
            
            SetAllSocketsState(true, _selectedLevelIndex, excludeMerchantSocket);
            SetSocketActiveState(false, ref _merchantOption.socket);
            
            foreach (var task in RetrieveLevelOptionTasks(opt => opt.Initialize(_needsBossConfirmation)))
            {
                yield return StartCoroutine(task);
            }
            
            toBossLevelBool.value = false;
            toNormalLevelBool.value = false;
            toMerchantBool.value = false;
            _initCoroutine = null;
        }
        
        private List<IEnumerator> RetrieveLevelOptionTasks(System.Func<LevelSelection, IEnumerator> taskSelector)
        {
            return (from option in _levelOptions where option != null select taskSelector(option)).ToList();
        }
        
        private void UpdateLevels(List<LevelSelection> levels, int count, bool lockState,
            System.Func<LevelSelection, bool> filterCondition = null
        )
        {
            // Filter levels if a condition is provided
            var filteredLevels = filterCondition != null
                ? levels.Where(filterCondition).ToList()
                : levels;

            // Randomly select the required number of levels
            var random = new System.Random();
            var levelsToUpdate = filteredLevels.OrderBy(x => random.Next()).Take(count).ToList();

            // Update the lock state
            foreach (var level in levelsToUpdate)
            {
                level.SetLockState(lockState);
                if (_allowDebug) Debug.Log($"{(lockState ? "Locked" : "Unlocked")} Level: {level.id}", this);
            }
        }
        
        private readonly Dictionary<int, UnityAction> _levelSelectionListeners = new();
        private void SetListenerStates(bool listenState)
        {
            HandleMerchantSelectListenerStates(listenState);
            
            foreach (var levelSelection in _levelOptions)
            {
                HandleLevelSelectListenerState(listenState, levelSelection);
            }
        }
        
        private void HandleLevelSelectListenerState(bool listenState, LevelSelection levelSelection)
        {
            if (levelSelection == null)
            {
                Debug.LogError("[ERROR] Level Selection is missing", this);
                return;
            }
            
            var id = levelSelection.id;
            
            if (listenState)
            {
                UnityAction socketListener = () => HandleSocketedInLevelSelection(id);
                _levelSelectionListeners[id] = socketListener;

                levelSelection.socket.onObjectSocketed.AddListener(socketListener);
                levelSelection.socket.onObjectUnsocketed.AddListener(HandleRemovedFromSocket);
            }
            else
            {
                if (_levelSelectionListeners.TryGetValue(id, out var socketListener))
                {
                    levelSelection.socket.onObjectSocketed.RemoveListener(socketListener);
                    _levelSelectionListeners.Remove(id);
                }

                levelSelection.socket.onObjectUnsocketed.RemoveListener(HandleRemovedFromSocket);
            }
        }
        private void HandleMerchantSelectListenerStates(bool listenState)
        {
            if (_merchantOption.socket == null)
            {
                Debug.LogError("[ERROR] Merchant Socket is missing", this);
                return;
            }
            
            if (listenState)
            {
                _merchantOption.socket.onObjectSocketed.AddListener(HandleSocketedInMerchantSelection);
                _merchantOption.socket.onObjectUnsocketed.AddListener(HandleRemovedFromSocket);

                _levelSelectUIManager.confirmedSelection.AddListener(ConfirmButtonPressed);
                _levelSelectUIManager.cancelledSelection.AddListener(DeclineButtonPressed);
            }
            else
            {
                _merchantOption.socket.onObjectSocketed.RemoveListener(HandleSocketedInMerchantSelection);
                _merchantOption.socket.onObjectUnsocketed.RemoveListener(HandleRemovedFromSocket);

                _levelSelectUIManager.confirmedSelection.RemoveListener(ConfirmButtonPressed);
                _levelSelectUIManager.cancelledSelection.RemoveListener(DeclineButtonPressed);
            }
        }
        
        private void OnDisable()
        {
            SetListenerStates(false);
        }

#if UNITY_EDITOR
        private string FillDebugLine(int lineLength, string message, char filler = '-')
        {
            var fillerLength = lineLength - message.Length;
            
            if (fillerLength < 0)
            {
                return message;
            }

            return new string(filler, fillerLength) + message;
        }
        
        private void DebugSocketState()
        {
            if (!_allowDebug) return;
            
            const int lineLength = 80;
            var message =
                $"[DEBUG] Selection -> Normal Level: {_levelSelected}, Boss Level: {_bossLevelSelected}, Merchant: {_merchantSelected}\n";
            message += FillDebugLine(lineLength,$"Merchant Socket -> Active: {_merchantOption.socket.SocketState()}, Grab State: {_merchantOption.socket.GrabState()}\n", ' ');
            
            foreach (var levelSelection in _levelOptions)
            {
                message += FillDebugLine(
                    lineLength, $"Level Option[{levelSelection.id}] Socket -> Active: {levelSelection.socket.SocketState()}, " +
                                $"Grab State: {levelSelection.socket.GrabState()}\n", ' ');
            }
            
            Debug.Log(message, this);
        }
#endif
    }
}
