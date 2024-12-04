using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.Manager
{
    public class LevelSelectionManager : MonoBehaviour, INeedButton
    {
        // level selected bool
        // boss level selected bool
        // level selection UI manager
        // Array of level selections
        //     - Boss level toggle
        //     - Transform of level selection
        //     - Socket for level selection
        
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

        [System.Serializable]
        private struct LevelSelection
        {
            private int _id;
            public bool isBossLevel;
            public Transform transform;
            public SocketMatchInteractor socket;
            
            public int id { get => _id; set => _id = value; }
        }
        
        [SerializeField] private LevelSelection[] _levelOptions;
        
        private void LevelSelected()
        {
            bossLevelSelected = false;
            levelSelected = true;
        }
        
        private void BossLevelSelected()
        {
            bossLevelSelected = true;
            levelSelected = false;
        }
        
        private void MerchantSelected()
        {
            Debug.Log("Merchant Selected", this);
            levelSelected = bossLevelSelected = false;
            
            SetLevelSockets(false);
        }
        
        private void HandleSelectionRemoved()
        {
            Debug.Log("Selection Removed", this);
            levelSelected = bossLevelSelected = false;
            
            SetLevelSockets(true);
            SetMerchantSocket(true);
        }
        
        private void HandleLevelSelection(int index)
        {
            if (index < 0 || index >= _levelOptions.Length)
            {
                Debug.LogError("Index out of range", this);
                return;
            }
            Debug.Log($"Level Option[{index}] Selected", this);
            
            if (_levelOptions[index].isBossLevel)
            {
                BossLevelSelected();
            }
            else
            {
                LevelSelected();
            }

            SetLevelSockets(false, index);
        }
        
        private static void SetSocketState(bool state, ref SocketMatchInteractor socket)
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
        
        private void SetLevelSockets(bool state, int excludeIndex = -1)
        {
            foreach (var selection in _levelOptions)
            {
                if (selection.id == excludeIndex)
                {
                    continue;
                }

                var socket = selection.socket;
                
                if (socket == null)
                {
                    Debug.LogError("Socket is missing", this);
                    continue;
                }
            
                SetSocketState(state, ref socket);
            }
        }

        [System.Serializable]
        private struct MerchantSelection
        {
            public Transform transform;
            public SocketMatchInteractor socket;
        }
        
        [SerializeField] private MerchantSelection _merchantOption;
        
        private void HandleMerchantSelection()
        {
            if (_merchantOption.transform == null || _merchantOption.socket == null)
            {
                return;
            }
            
            MerchantSelected();
        }
        
        private void SetMerchantSocket(bool state)
        {
            var socket = _merchantOption.socket;
                
            if (socket == null)
            {
                Debug.LogError("Socket is missing", this);
                return;
            }
            
            SetSocketState(state, ref socket);
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
                    Debug.LogError($"Error: {errorFlags}, Level Selections and Merchant Selection are missing. They must be provided.", this);
                    return;
                case 0x1:
                    Debug.LogError($"Error: {errorFlags}, Level Selections are missing. They must be provided.", this);
                    return;
                case 0x2:
                    Debug.LogError($"Error: {errorFlags}, Merchant Selection are missing. They must be provided.", this);
                    break;
                case 0x3:
                    Debug.Log($"Debug: {errorFlags}, Level Selections and Merchant Selection are present. Initializing...", this);
                    break;
                default:
                    Debug.LogError($"Error: {errorFlags}, Unexpected behavior occurred.", this);
                    return;
            }
            
            for (var i = 0; i < _levelOptions.Length; i++)
            {
                var levelSelection = _levelOptions[i];
                levelSelection.id = i;
                _levelOptions[i].socket.onObjectSocketed.AddListener(() => HandleLevelSelection(levelSelection.id));
                _levelOptions[i].socket.onObjectUnsocketed.AddListener(HandleSelectionRemoved);
            }
            
            _merchantOption.socket.onObjectSocketed.AddListener(HandleMerchantSelection);
            _merchantOption.socket.onObjectUnsocketed.AddListener(HandleSelectionRemoved);
        }

        public System.Collections.Generic.List<(System.Action, string)> GetButtonActions()
        {
            return new System.Collections.Generic.List<(System.Action, string)>
            {
                
                (
                () =>
                {
                    if (!Application.isPlaying) return;
                    SetLevelSockets(true); 
                    SetSocketState(true, ref _merchantOption.socket);
                }, "Unlock Sockets")
            };
        }
    }
}
