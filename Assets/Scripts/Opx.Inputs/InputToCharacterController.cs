using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Opx.Inputs
{
    public class InputToCharacterController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController _characterController;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private static readonly int Forward = Animator.StringToHash("Forward");
        private static readonly int Turn = Animator.StringToHash("Turn");

        public void SetupCharacter(
            Animator animator,
            CharacterController characterController)
        {
            _animator = animator;
            _characterController = characterController;
        }

        private async UniTaskVoid Start()
        {
            await UniTask.WaitUntil(() => _characterController != null, cancellationToken: _cancellationTokenSource.Token);

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (!Input.anyKey)
                {
                    _animator.SetFloat(Turn, 0);
                    _animator.SetFloat(Forward, 0);
                    await UniTask.Yield();
                    continue;
                }

                float speed = 
                    (Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f) +
                    (Input.GetKey(KeyCode.DownArrow) ? -1.0f : 0.0f);

                float rotateAngle =
                    (Input.GetKey(KeyCode.LeftArrow) ? -1.0f : 0.0f) +
                    (Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f);
                
                _animator.SetFloat(Turn, rotateAngle * 0.5f);
                _animator.SetFloat(Forward, Mathf.Abs(speed));
                _characterController.transform.Rotate(Vector3.up, rotateAngle * 90.0f * Time.deltaTime, Space.Self);
                _characterController.SimpleMove(speed * _characterController.transform.forward);
                await UniTask.Yield();
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
        }
    }

}
