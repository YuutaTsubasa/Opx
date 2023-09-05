using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Opx.Inputs;
using UnityEngine;

namespace Opx.Camera
{
    public class MouseAimCamera : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private float _rotateSpeed = 5;
        [SerializeField] private Vector3 _offset;

        private CancellationTokenSource _cancellationTokenSource;
        
        
        private Animator _targetCharacterAnimator;
        private CharacterController _targetCharacterController;
        private InputToCharacterController _inputToCharacterController;
        private static readonly int Turn = Animator.StringToHash("Turn");

        public void SetupCharacter(
            Animator animator,
            CharacterController characterController,
            InputToCharacterController inputToCharacterController)
        {
            _targetCharacterAnimator = animator;
            _targetCharacterController = characterController;
            _inputToCharacterController = inputToCharacterController;
        }
        
        private async UniTaskVoid OnEnable()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            float previousDiff = 0;
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_targetCharacterAnimator == null || 
                    _targetCharacterController == null)
                {
                    _targetCharacterAnimator?.SetFloat(Turn, 0);
                    await UniTask.Yield();
                    continue;
                }

                float diff = previousDiff + (Input.GetMouseButton(0) ? Input.GetAxis("Mouse X") : 0);
                float horizontal = diff * _rotateSpeed;
                Quaternion rotation = Quaternion.Euler(0, horizontal, 0);
                _camera.transform.position = _targetCharacterController.transform.position - (rotation * _offset);
                _camera.transform.LookAt(_targetCharacterController.gameObject.transform.position + Vector3.up * 2);
                _inputToCharacterController.SetForwardAngle(horizontal);
                
                previousDiff = diff;
                await UniTask.Yield();
            }

        }

        private void OnDisable()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}