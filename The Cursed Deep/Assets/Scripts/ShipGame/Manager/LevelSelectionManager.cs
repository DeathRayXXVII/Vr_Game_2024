using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

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
        
        [SerializeField] private BoolData levelSelectedHolder;
        [SerializeField] private BoolData bossLevelSelectedHolder;
        
        private readonly WaitForFixedUpdate _waitFixed = new();

        private bool levelSelected
        {
            get => _levelSelected;
            set
            {
                _levelSelected = value;
                levelSelectedHolder.value = value;
            }
        }

        private bool bossLevelSelected
        {
            get => _bossLevelSelected;
            set
            {
                _bossLevelSelected = value;
                bossLevelSelectedHolder.value = value;
            }
        }
        
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
        
        private void SelectionConfirmed()
        {
            if(!ValidSelection(_selectedLevelIndex)) return;
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level [{_selectedLevelIndex}] Confirmed", this);
            
            // TODO: Handle selection confirmation
        }
        
        private bool _cancelingSelection;
        private void SelectionCancelled()
        {
            if(_cancelingSelection || !ValidSelection(_selectedLevelIndex)) return;
            _cancelingSelection = true;
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level [{_selectedLevelIndex}] Cancelled", this);
            
            StartCoroutine(WaitForCancel());
        }
        
        private IEnumerator WaitForCancel()
        {
            var selectedSocket = _selectedLevelIndex == -1 ? 
                ref _merchantOption.socket :
                ref _levelOptions[_selectedLevelIndex].socket;
            
            yield return StartCoroutine(_levelSelectUIManager.DeactivateUIAndWait(_activatedUIPosition.position, selectedSocket.transform.position, _animationDuration));
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Level [{_selectedLevelIndex}] Cancelled", this);
            
            SetSocketGrabState(true, ref selectedSocket);
            yield return new WaitUntil(() => selectedSocket.GrabState());
            
            _cancelingSelection = false;
            yield return _waitFixed;
        }
        
        private void HandleRemovedFromSocket()
        {
            if (_allowDebug) 
                Debug.Log("[DEBUG] Selection Removed", this);
            levelSelected = bossLevelSelected =_merchantSelected = false;
            
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
            
            _merchantSelected = false;
            bossLevelSelected = levelSelection.isBossLevel;
            levelSelected = !bossLevelSelected;
            
            _levelSelectUIManager.ActivateUI($"Level {_currentLevel ?? 0}",
                $"{(bossLevelSelected ? "BOSS" : "")} {levelSelection.enemyData.unitName}", 
                levelSelection.socket.transform.position, _activatedUIPosition.position, _animationDuration);

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

        private void SetAllSocketsState(bool state, int selectionIndex = -1)
        {
            // grab state is FALSE, only if state is FALSE and selectionIndex EQUALS the selected socket index
            // and TRUE if state is TRUE or selectionIndex DOES NOT EQUAL the selected socket index
            // active state is FALSE, only if state is FALSE and selectionIndex DOES NOT EQUAL the selected socket index
            // and TRUE if state is TRUE or selectionIndex EQUALS the selected socket index
            
            // Working on the merchant socket
            var merchantSelected = selectionIndex == -1;
            
            SetSocketGrabState(state || !merchantSelected, ref _merchantOption.socket);
            SetSocketActiveState(state || merchantSelected, ref _merchantOption.socket);
            
            // Working on the level sockets
            foreach (var selection in _levelOptions)
            {
                var socket = selection.socket;
                
                if (socket == null)
                {
                    Debug.LogError("[ERROR] Socket is missing", this);
                    continue;
                }
                
                SetSocketGrabState(state || selection.id != selectionIndex, ref socket);
                SetSocketActiveState(state || selection.id == selectionIndex, ref socket);
            }
            
#if UNITY_EDITOR
            DebugSocketState();
#endif
        }

        [System.Serializable]
        private struct MerchantSelection
        {
            public Transform transform;
            public SocketMatchInteractor socket;
        }
        
        [SerializeField] private MerchantSelection _merchantOption;
        
        private void HandleSocketedInMerchantSelection()
        {
            _merchantSelected = true;
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Merchant Selected: {_merchantSelected}", this);
            levelSelected = bossLevelSelected = false;
            
            _selectedLevelIndex = -1;
            SetAllSocketsState(false, _selectedLevelIndex);
            
            _levelSelectUIManager.ActivateUI("Head to", "Black Market?", _merchantOption.socket.transform.position,
                _activatedUIPosition.position, _animationDuration);
        }

        private void Awake()
        {
            bossLevelSelected = levelSelected = _merchantSelected = false;
            
            var debugMessage = string.Empty;
            var errorMessage = string.Empty;
            
            bool hasLevels = _levelOptions is { Length: > 0 };
            bool hasMerchant = _merchantOption.transform != null && _merchantOption.socket != null;
            bool hasUIManager = _levelSelectUIManager != null;
            bool hasCurrentLevel = _currentLevel != null;
            bool hasCountToBoss = _countToBoss != null;
            bool hasLevelSelectedHolder = levelSelectedHolder != null;
            bool hasBossLevelSelectedHolder = bossLevelSelectedHolder != null;
            
            bool hasAllRequired = hasLevels && hasMerchant && hasUIManager && hasCurrentLevel && hasLevelSelectedHolder &&
                                  hasBossLevelSelectedHolder;

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
            
            if (!hasLevelSelectedHolder)
                errorMessage += "\t- Level Selected Holder is missing\n";
            
            if (!hasBossLevelSelectedHolder)
                errorMessage += "\t- Boss Level Selected Holder is missing\n";
            
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
            uint lockedLevels = 0;
            uint bossLevels = 0;
            uint normalLevels = 0;

            var debugLocked = string.Empty;
            var debugBoss = string.Empty;
            var debugNormal = string.Empty;
            
            // Wait for all initializations to complete
            foreach (var task in RetrieveTasks(opt => opt.LoadCoroutine()))
            {
                yield return StartCoroutine(task);
            }

            // Process all levels after initialization
            foreach (var option in _levelOptions)
            {
                if (option == null || !option.isLoaded) continue;

                // Update locked, boss, and normal levels
                if (option.isLocked)
                {
                    lockedLevels |= 1u << option.id;
                }
                if (option.isBossLevel)
                {
                    bossLevels |= 1u << option.id;
                }
                else
                {
                    normalLevels |= 1u << option.id;
                }
                debugLocked += option.isLocked ? "1" : "0";
                debugBoss += option.isBossLevel ? "1" : "0";
                debugNormal += !option.isBossLevel ? "1" : "0";
            }

            // Calculate expected unlocked count
            uint expectedUnlockedCount = 0;
            var debugExpected = string.Empty;
            for (var i = 0; i < _countToBoss.value; i++)
            {
                expectedUnlockedCount |= 1u << i;
                debugExpected += "1";
            }

            uint lockedNormalLevels = lockedLevels & normalLevels;
            uint lockedBossLevels = lockedLevels & bossLevels;

            // Handle mismatch between locked levels and expected count
            int lockedDifference = (int)~lockedNormalLevels - (int)expectedUnlockedCount;
            
            Debug.Log($"[DEBUG] Locked Levels: {lockedLevels}[{debugLocked}], Boss Levels: {bossLevels}[{debugBoss}], " +
                      $"Normal Levels: {normalLevels}[{debugNormal}], Expected Count: {expectedUnlockedCount}[{debugExpected}]" +
                      $"Locked Normal Levels: {lockedNormalLevels}, Locked Boss Levels: {lockedBossLevels}, " +
                      $"Locked Difference: {lockedDifference}", this);

            if (lockedDifference > 0)
            {
                // Not enough locked levels: Lock randomly up to the expected count
                UpdateLevels(_levelOptions.ToList(), lockedDifference, true, opt => !opt.isBossLevel && !opt.isLocked);
            }
            else if (lockedDifference < 0)
            {
                // Too many locked levels: Unlock randomly down to the expected count
                UpdateLevels(_levelOptions.ToList(), -lockedDifference, false, opt => !opt.isBossLevel && opt.isLocked);
            }
            else if (expectedUnlockedCount <= 0)
            {
                Debug.LogError("[ERROR] Expected Unlocked Count is 0", this);
            }

            // Unlock all boss levels if _countToBoss.value is 0
            if (_countToBoss.value == 0 && lockedBossLevels != 0)
            {
                UpdateLevels(_levelOptions.ToList(), _levelOptions.Length, false, opt => opt.isBossLevel && opt.isLocked);
            }

            foreach (var task in RetrieveTasks(opt => opt.Initialize()))
            {
                yield return StartCoroutine(task);
            }

            _initCoroutine = null;
        }
        
        private List<IEnumerator> RetrieveTasks(System.Func<LevelSelection, IEnumerator> taskSelector)
        {
            return (from option in _levelOptions where option != null select taskSelector(option)).ToList();
        }
        
        private void UpdateLevels(
            List<LevelSelection> levels,
            int count,
            bool lockState,
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
                Debug.Log($"{(lockState ? "Locked" : "Unlocked")} Level: {level.id}", this);
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

                _levelSelectUIManager.confirmedSelection.AddListener(SelectionConfirmed);
                _levelSelectUIManager.cancelledSelection.AddListener(SelectionCancelled);
            }
            else
            {
                _merchantOption.socket.onObjectSocketed.RemoveListener(HandleSocketedInMerchantSelection);
                _merchantOption.socket.onObjectUnsocketed.RemoveListener(HandleRemovedFromSocket);

                _levelSelectUIManager.confirmedSelection.RemoveListener(SelectionConfirmed);
                _levelSelectUIManager.cancelledSelection.RemoveListener(SelectionCancelled);
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
                $"[DEBUG] Selection -> Normal Level: {levelSelected}, Boss Level: {bossLevelSelected}, Merchant: {_merchantSelected}\n";
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
