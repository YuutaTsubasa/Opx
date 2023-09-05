using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Opx.Utils;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Opx.Inputs
{
    public class InputToCharacterController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController _characterController;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private static readonly int Forward = Animator.StringToHash("Forward");
        private float _angle;

        public void SetupCharacter(
            Animator animator,
            CharacterController characterController)
        {
            _animator = animator;
            _characterController = characterController;
            _angle = 0;
        }

        public void SetForwardAngle(float angle)
        {
            _angle = angle;
        }

        private async UniTaskVoid Start()
        {
            await UniTask.WaitUntil(() => _characterController != null, cancellationToken: _cancellationTokenSource.Token);

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (!Input.anyKey)
                {
                    _animator.SetFloat(Forward, 0);
                    await UniTask.Yield();
                    continue;
                }

                float speedForward = 
                    (Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f) +
                    (Input.GetKey(KeyCode.DownArrow) ? -1.0f : 0.0f);

                
                float speedRight =
                    (Input.GetKey(KeyCode.LeftArrow) ? -1.0f : 0.0f) +
                    (Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f);

                var moveDistance = (new Vector2(speedRight, speedForward)).sqrMagnitude;

                if (moveDistance > 0)
                {
                    _characterController.transform.Rotate(
                        0,
                        _angle - Vector3.SignedAngle(Vector3.forward, _characterController.transform.forward,
                            Vector3.up),
                        0);
                }

                _animator.SetFloat(Forward, moveDistance);
                Transform characterControllerTransform = _characterController.transform;
                _characterController.SimpleMove((speedForward * characterControllerTransform.forward + speedRight * characterControllerTransform.right).normalized * 5);
                await UniTask.Yield();
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
        }
    }

}
