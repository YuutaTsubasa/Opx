using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Opx.Camera;
using Opx.Inputs;
using Opx.Utils;
using UniGLTF;
using UnityEditor;
using UnityEditor.Animations;
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
        [SerializeField] private AnimatorController _characterAnimatorController;

        public void LoadVrmFromEvent()
            => LoadVrm(_modelPath, _rootTransform, _characterAnimatorController);
        
        public async UniTask LoadVrm(string modelPath, Transform rootTransform, 
            AnimatorController characterAnimatorController)
        {
            var binaryData = await _LoadFileBinaryData(modelPath);
            var vrmModelGameObject = await _ConvertToVrm(binaryData);
            vrmModelGameObject.transform.SetParent(_rootTransform);
            _SetupCharacter(vrmModelGameObject, characterAnimatorController);
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

        private void _SetupCharacter(GameObject vrmGameObject, AnimatorController characterAnimatorController)
        {
            vrmGameObject.transform.localPosition = Vector3.zero;
            var characterAnimator = vrmGameObject.GetComponent<Animator>();
            if (characterAnimator != null && characterAnimatorController != null)
            {
                characterAnimator.runtimeAnimatorController = characterAnimatorController;
            }

            var characterController = vrmGameObject.AddComponent<CharacterController>();
            characterController.center = new Vector3(0, 0.75f, 0);
            characterController.radius = 0.5f;
            characterController.height = 1.5f;

            var inputToCharacterController = vrmGameObject.AddComponent<InputToCharacterController>();
            inputToCharacterController.SetupCharacter(characterAnimator, characterController);
            var mouseAimCamera = UnityEngine.Camera.main.GetComponent<MouseAimCamera>();
            mouseAimCamera.SetupCharacter(characterAnimator, characterController, inputToCharacterController);

            // var characterRigidbody = vrmGameObject.AddComponent<Rigidbody>();
            // characterRigidbody.constraints =
            //     RigidbodyConstraints.FreezeRotationX | 
            //     RigidbodyConstraints.FreezeRotationY |
            //     RigidbodyConstraints.FreezeRotationZ;
        }
    }
}
