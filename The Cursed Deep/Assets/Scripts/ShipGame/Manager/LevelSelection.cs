using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.Manager
{
    [System.Serializable]
    public class LevelSelection : MonoBehaviour, ISaveSystem
    {
        private int _id;
        public int id { get => _id; set => _id = value; }
        
        [SerializeField] private bool _isBossLevel;
        public bool isBossLevel { get => _isBossLevel; private set => _isBossLevel = value; }
        
        [SerializeField, ReadOnly] private bool _isLocked;

        public bool isLocked
        {
            get => _isLocked;
            private set
            {
                _isLocked = value;
                Save();
            }
        }
        public void SetLockState(bool state)
        {
            isLocked = state;
        }
        
        [SerializeField] private GameObject _lockedIndicator;
        [SerializeField] private MeshRenderer[] _levelMeshGameObjects;
        private Material[] _levelMaterials;
        [SerializeField] private Material _lockedMaterial;
        
        private bool _materialsUpdated;
        private void UpdateMaterials()
        {
            if (_levelMeshGameObjects == null || _levelMeshGameObjects.Length == 0)
            {
                Debug.LogError("Level Mesh Game Objects are null or empty, cannot update materials", this);
                _materialsUpdated = true;
                return;
            }
            
            var meshCount = _levelMeshGameObjects?.Length;
            if (_levelMeshGameObjects == null || meshCount == 0)
            {
                _materialsUpdated = true;
                return;
            }
            
            var materialCount = _levelMaterials?.Length;
            if (_levelMaterials == null || materialCount == 0)
            {
                System.Diagnostics.Debug.Assert(meshCount != null, $"{nameof(meshCount)} != null");
                _levelMaterials = new Material[(int)meshCount];
                for (var i = 0; i < meshCount; i++)
                {
                    _levelMaterials[i] = _levelMeshGameObjects[i].material;
                }
            }
            
            for (var i = 0; i < meshCount; i++)
            {
                _levelMeshGameObjects[i].material = isLocked ? _lockedMaterial : _levelMaterials[i];
            }
            
            _materialsUpdated = true;
        }
        
        public CreepData enemyData;
        public SocketMatchInteractor socket;

        private string _filePath;
        public string filePath => _filePath ??= $"{Application.persistentDataPath}/LevelSelectData/Level_Selection_{id}";
        public bool savePathExists => File.Exists(filePath);
        
        [System.Serializable]
        private struct SaveData
        {
            public int id;
            public bool isLocked;
        }
        
        public bool isLoaded;
        public void Save()
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            var saveData = new SaveData
            {
                id = id,
                isLocked = isLocked
            };

            var json = JsonUtility.ToJson(saveData);
            File.WriteAllText(filePath, json);
        }
        
        public void Load()
        {
            if (!savePathExists)
            {
                Save();
                isLoaded = true;
                return;
            }

            var json = File.ReadAllText(filePath);

            var saveData = JsonUtility.FromJson<SaveData>(json);

            id = saveData.id;
            isLocked = saveData.isLocked;

            isLoaded = true;
        }

        public IEnumerator LoadCoroutine()
        {
            isLoaded = false;
            Load();
            yield return new WaitUntil(() => isLoaded);
        }


        public void DeleteSavedData()
        {
            if (savePathExists)
            {
                File.Delete(filePath);
            }
        }
        
        private Coroutine _initializeCoroutine;
        public IEnumerator Initialize()
        {
            _initializeCoroutine ??= StartCoroutine(PerformInitialization());
            yield return new WaitUntil(() => _initializeCoroutine == null);
        }
        
        private IEnumerator PerformInitialization()
        {
            if (!isLoaded)
            {
                yield return LoadCoroutine();
            }
            
            if (_lockedIndicator)
            {
                _lockedIndicator.SetActive(_isLocked);
            }
            yield return null;
            
            UpdateMaterials();
            yield return new WaitUntil(() => _materialsUpdated);
            
            _initializeCoroutine = null;
        }
    }
}
