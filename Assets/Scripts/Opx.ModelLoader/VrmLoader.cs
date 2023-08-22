using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Opx.Utils;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using VRM;
using VRMShaders;

namespace Opx.ModelLoader
{
    public class VrmLoader : MonoBehaviour
    {
        [Header("VRM Loader 設定")] 
        [SerializeField] private string _modelPath;
        [SerializeField] private Transform _rootTransform;

        public void LoadVrmFromEvent()
            => LoadVrm();
        
        public async UniTask LoadVrm(
            Option<string> modelPathOption = default, 
            Option<Transform> rootTransformOption = default)
        {
            var modelPath = modelPathOption.ValueOr(_modelPath);
            var rootTransform = rootTransformOption.ValueOr(_rootTransform);

            var binaryData = await _LoadFileBinaryData(modelPath);
            var vrmModelGameObject = await _ConvertToVrm(binaryData);
            vrmModelGameObject.transform.SetParent(_rootTransform);
        }

        private string _ConvertPathToWebRequestPath(string modelPath)
        {
            var path = modelPath;
#if UNITY_EDITOR
#if UNITY_EDITOR_WIN
            path = "file:///" + path;
#else
            path = "file://" + path;
#endif
#elif UNITY_WEBGL
#else
            path = "file:///" + path;
#endif
            return path;
        }

        private async UniTask<byte[]> _LoadFileBinaryData(string modelPath)
        {
            var webRequestPath = _ConvertPathToWebRequestPath(modelPath);
            Debug.LogError($"Path: {webRequestPath}");
            var webRequest = UnityWebRequest.Get(_ConvertPathToWebRequestPath(modelPath));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            await webRequest.SendWebRequest().ToUniTask();
            return webRequest.downloadHandler.data;
        }

        private async UniTask<GameObject> _ConvertToVrm(byte[] binaryData)
        {
            var glbBinaryParser = new GlbBinaryParser(binaryData, "VRMModel");
            var gltfData = glbBinaryParser.Parse();
            var vrmData = new VRMData(gltfData);
            var context = new VRMImporterContext(vrmData);
            
            var instance = await context.LoadAsync(new RuntimeOnlyAwaitCaller());
            instance.ShowMeshes();
            return instance.Root;
        }
    }
}
