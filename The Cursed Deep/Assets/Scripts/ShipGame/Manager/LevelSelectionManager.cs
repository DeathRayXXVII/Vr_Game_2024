using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ShipGame.Manager
{
    public class LevelSelectionManager : MonoBehaviour
    {
        [SerializeField, ReadOnly] private bool _levelSelected;
        [SerializeField, ReadOnly] private bool _bossLevelSelected;
        
        [SerializeField] private BoolData levelSelectedHolder;
        [SerializeField] private BoolData bossLevelSelectedHolder;

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
        private int _selectedLevelIndex = -2;
        
        private bool ValidSelection(int index)
        {
            // -2 is the default empty value, -1 is the merchant selection, 0 and above are valid level selections
            if (index >= -1 && index < _levelOptions.Length)
            {
                return true;
            }
            
            Debug.LogError($"Invalid Level Index: {index} provided", this);
            return false;
        }

        [System.Serializable]
        private struct LevelSelectionSocket
        {
            private int _id;
            public bool isBossLevel;
            public CreepData enemyData;
            public Transform transform;
            public SocketMatchInteractor socket;
            
            public int id { get => _id; set => _id = value; }
        }
        
        [SerializeField] private LevelSelectionSocket[] _levelOptions;
        
        private void SelectionConfirmed()
        {
            if(!ValidSelection(_selectedLevelIndex)) return;
            
            Debug.Log($"Level [{_selectedLevelIndex}] Confirmed", this);
        }
        
        private void SelectionCancelled()
        {
            if(!ValidSelection(_selectedLevelIndex)) return;
            
            Debug.Log($"Level [{_selectedLevelIndex}] Cancelled", this);
            
            var selectedSocket = _selectedLevelIndex == -1 ? 
                ref _merchantOption.socket :
                ref _levelOptions[_selectedLevelIndex].socket;
            
            SetSocketGrabState(true, ref selectedSocket);
            
            var uiTargetPosition = _selectedLevelIndex == -1 ? 
                _merchantOption.transform.position :
                _levelOptions[_selectedLevelIndex].transform.position;
            
            _levelSelectUIManager.DeactivateUI(_activatedUIPosition.position, uiTargetPosition);
            
            _selectedLevelIndex = -2;
        }
        
        private void LevelSocketed()
        {
            bossLevelSelected = false;
            levelSelected = true;
        }
        
        private void BossLevelSocketed()
        {
            bossLevelSelected = true;
            levelSelected = false;
        }
        
        private void MerchantSocketed()
        {
            Debug.Log("Merchant Selected", this);
            levelSelected = bossLevelSelected = false;
            
            _selectedLevelIndex = -1;
            SetAllSocketsState(false);
            
            _levelSelectUIManager.ActivateUI("Head to Black Market?", _merchantOption.transform.position,
                _activatedUIPosition.position);
        }
        
        private void HandleRemovedFromSocket()
        {
            Debug.Log("Selection Removed", this);
            levelSelected = bossLevelSelected = false;
            
            SetAllSocketsState(true);
        }
        
        private void HandleSocketedInLevelSelection(int index)
        {
            if (index < 0 || index >= _levelOptions.Length)
            {
                Debug.LogError("Index out of range", this);
                return;
            }
            
            _selectedLevelIndex = index;
            var levelSelection = _levelOptions[index];
            Debug.Log($"Level Option[{index}] Selected", this);
            
            if (levelSelection.isBossLevel)
            {
                BossLevelSocketed();
            }
            else
            {
                LevelSocketed();
            }
            _levelSelectUIManager.ActivateUI(
                $"{(levelSelection.isBossLevel ? "Boss" : "Level")} Selection {index}",
                levelSelection.transform.position, _activatedUIPosition.position);

            SetAllSocketsState(false, index);
        }
        
        private static void SetSocketGrabState(bool state, ref SocketMatchInteractor socket)
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
            if (selectionIndex == -1)
            {
                SetSocketGrabState(state, ref _merchantOption.socket);
            }
            else
            {
                SetSocketActiveState(state, ref _merchantOption.socket);
            }
            
            foreach (var selection in _levelOptions)
            {
                var socket = selection.socket;
                
                if (selection.id == selectionIndex)
                {
                    SetSocketGrabState(state, ref socket);
                    continue;
                }
                
                if (socket == null)
                {
                    Debug.LogError("Socket is missing", this);
                    continue;
                }
            
                SetSocketActiveState(state, ref socket);
            }
            
            DebugSocketState();
        }

        [System.Serializable]
        private struct MerchantSelectionSocket
        {
            public Transform transform;
            public SocketMatchInteractor socket;
        }
        
        [SerializeField] private MerchantSelectionSocket _merchantOption;
        
        private void HandleSocketedInMerchantSelection()
        {
            if (_merchantOption.transform == null || _merchantOption.socket == null)
            {
                Debug.LogError("Merchant Socket is missing", this);
                return;
            }
            
            MerchantSocketed();
        }
        
        private void SetMerchantSocketState(bool state)
        {
            var socket = _merchantOption.socket;
                
            if (socket == null)
            {
                Debug.LogError("Socket is missing", this);
                return;
            }
            
            SetSocketActiveState(state, ref socket);
        }

        private void Awake()
        {
            bossLevelSelected = levelSelected = false;
            
            System.UInt16 errorFlags = 0; 
            errorFlags |= (ushort)(_levelOptions is { Length: > 0 } ? 0x1 : 0x0);
            errorFlags |= (ushort)(_merchantOption.transform != null && _merchantOption.socket != null ? 0x2 : 0x0);
            
            switch (errorFlags)
            {
                case 0x0:
                    Debug.LogError($"Error [{errorFlags}]: Level Selections and Merchant Selection are missing. They must be provided.", this);
                    return;
                case 0x1:
                    Debug.LogError($"Error [{errorFlags}]: Level Selections are missing. They must be provided.", this);
                    return;
                case 0x2:
                    Debug.LogError($"Error [{errorFlags}]: Merchant Selection are missing. They must be provided.", this);
                    break;
                case 0x3:
                    Debug.Log($"Debug: Flag[{errorFlags}], Level Selections and Merchant Selection are present. Initializing...", this);
                    break;
                default:
                    Debug.LogError($"Error [{errorFlags}]: Unexpected behavior occurred.", this);
                    return;
            }
            
            SetListenerStates(true);
        }
        
        private readonly Dictionary<int, UnityAction> _levelSelectionListeners = new();

        private void SetListenerStates(bool listenState)
        {
            for (var i = 0; i < _levelOptions.Length; i++)
            {
                var levelSelection = _levelOptions[i];
                levelSelection.id = i;

                if (listenState)
                {
                    UnityAction socketListener = () => HandleSocketedInLevelSelection(levelSelection.id);
                    _levelSelectionListeners[levelSelection.id] = socketListener;

                    levelSelection.socket.onObjectSocketed.AddListener(socketListener);
                    levelSelection.socket.onObjectUnsocketed.AddListener(HandleRemovedFromSocket);
                }
                else
                {
                    if (_levelSelectionListeners.TryGetValue(levelSelection.id, out var socketListener))
                    {
                        levelSelection.socket.onObjectSocketed.RemoveListener(socketListener);
                        _levelSelectionListeners.Remove(levelSelection.id);
                    }

                    levelSelection.socket.onObjectUnsocketed.RemoveListener(HandleRemovedFromSocket);
                }
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
        
        private void DebugSocketState()
        {
            Debug.Log($"Level Selected: {levelSelected}, Boss Level Selected: {bossLevelSelected}", this);
            Debug.Log($"Merchant Socket Active: {_merchantOption.socket.SocketState()}, Can be grabbed: {_merchantOption.socket.GrabState()}", this);
            foreach (var levelSelection in _levelOptions)
            {
                Debug.Log($"Level Option[{levelSelection.id}] Socket Active: {levelSelection.socket.SocketState()}, Can be grabbed: {levelSelection.socket.GrabState()}", this);
            }
        }
    }
}
