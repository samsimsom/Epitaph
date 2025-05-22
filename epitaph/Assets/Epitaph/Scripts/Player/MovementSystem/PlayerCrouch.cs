using System;
using Epitaph.Scripts.Player.ScriptableObjects;
using PrimeTween;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerCrouch : PlayerBehaviour
    {
        // Static Events
        public static event Action<bool> OnCrouchStateChanged;

        // Private Instance Fields
        private PlayerData _playerData;
        private CharacterController _characterController;
        private PlayerMove _playerMove;
        private Camera _playerCamera;
        private float _initialCameraYLocalPosition;
        private bool _wasGroundedBeforeTransition; // Kaymayı önlemek için geçiş öncesi durumu sakla

        private Tween _characterControllerTween;
        private Tween _playerCameraTween;

        // Constructor
        public PlayerCrouch(PlayerController playerController,
            PlayerData playerData,
            CharacterController characterController,
            PlayerMove playerMove,
            Camera playerCamera) : base(playerController)
        {
            _playerData = playerData;
            _characterController = characterController;
            _playerMove = playerMove;
            _playerCamera = playerCamera;

            Initialize();
        }

        // Public Methods (Unity Lifecycle & Public API)
        public override void Update()
        {
            // Eğer crouch durumunda ve isFalling true olduysa, otomatik Stand'a dönüş
            if (_playerData.isCrouching && _playerData.isFalling)
            {
                // Stand() metodunu doğrudan çağırmak yerine SetCrouchState(false) kullanacağız.
                if (CanStandUp())
                {
                    SetCrouchState(false); // Ayağa kalkma durumunu ayarla ve geçişi başlat
                }
            }
        }

        public void ToggleCrouch()
        {
            if (_playerData.isCrouching) // Şu anda eğilmiş durumda, ayağa kalkmayı dene
            {
                if (CanStandUp())
                {
                    SetCrouchState(false); // Ayağa kalk
                }
            }
            else // Şu anda ayakta, eğilmeyi dene
            {
                if (_playerData.isGrounded) // Yalnızca yerdeyse eğil
                {
                    SetCrouchState(true); // Eğil
                }
            }
        }
        
        // Durumu ayarlayan ve geçişi başlatan merkezi bir metod
        private void SetCrouchState(bool newCrouchState)
        {
            // Eğer durum zaten istenen durumdaysa bir şey yapma (opsiyonel bir kontrol)
            if (_playerData.isCrouching == newCrouchState) return;

            _playerData.isCrouching = newCrouchState;
            OnCrouchStateChanged?.Invoke(_playerData.isCrouching);

            if (newCrouchState)
            {
                _playerMove.SetCrouchingSpeed();
            }
            else
            {
                _playerMove.SetWalkingSpeed();
            }

            _wasGroundedBeforeTransition = _characterController.isGrounded; // Mevcut yer durumu sakla
            SmoothCrouchTransition(); // Animasyonlu geçişi başlat
        }

        // Private Methods
        private void Initialize()
        {
            _initialCameraYLocalPosition = _playerCamera != null ?
                _playerCamera.transform.localPosition.y : 0f;

            _playerData.standingHeight = _characterController.height;
        }

        private bool CanStandUp()
        {
            if (_characterController == null) return false;

            ComputeCeilingRayOrigin(out var radius, out var rayDistance,
                out var originTip, out var originRoot);

            var raycast = !Physics.Raycast(originRoot, Vector3.up, rayDistance, _playerData.ceilingLayers);
            var raySphere = !Physics.CheckSphere(originTip, radius, _playerData.ceilingLayers);

            return raycast && raySphere;
        }

        private void ComputeCeilingRayOrigin(out float radius,
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = _characterController.radius;
            rayDistance = _playerData.ceilingCheckDistance;
            originTip = _characterController.transform.position
                        + _characterController.center
                        + Vector3.up * (_characterController.height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = _characterController.transform.position
                         + _characterController.center
                         + Vector3.up * (_characterController.height / 2f);
        }

        private void SmoothCrouchTransition()
        {
            // Aynı nesneler üzerindeki mevcut tween'leri durdur (hızlı geçişlerde çakışmayı önler)
            Tween.StopAll(onTarget: _characterControllerTween);
            if (_playerCamera != null)
            {
                Tween.StopAll(onTarget: _playerCameraTween);
            }

            var startHeight = _characterController.height;
            var endHeight = _playerData.isCrouching ? _playerData.crouchHeight : _playerData.standingHeight;
            // Ayağa kalkma süresini eğilme süresinin yarısı yapmak mantıklı, orijinaldeki gibi.
            var duration = _playerData.isCrouching ? _playerData.crouchTransitionTime : _playerData.crouchTransitionTime / 2f;

            var startCenterY = _characterController.center.y;
            // Eğilirken merkez Y'si crouchHeight / 2f, ayaktayken 0f (kullanıcının mevcut mantığına göre).
            var endCenterY = _playerData.isCrouching ? _playerData.crouchHeight / 2f : 0f;

            // Yükseklik ve merkezi tek bir tween ile güncellemek, senkronizasyonu ve Move çağrısını basitleştirir.
            // t, 0'dan 1'e interpolasyon faktörü olacaktır.
            _characterControllerTween = Tween.Custom(0f, 1f, duration,
                onValueChange: t =>
                {
                    // Bu tween adımındaki yükseklik/merkez güncellemelerinden ÖNCEKİ
                    // CharacterController'ın mevcut yüksekliğine ve merkezine göre
                    // kapsülün alt kısmının transform'un y pozisyonuna göre olan ofsetini al.
                    var bottomOffsetBeforeUpdate = _characterController.center.y - _characterController.height / 2f;

                    var newHeight = Mathf.Lerp(startHeight, endHeight, t);
                    var newCenterY = Mathf.Lerp(startCenterY, endCenterY, t); // Bu, bu kare için hedeflenen merkezdir

                    _characterController.height = newHeight;
                    var centerVec = _characterController.center; // center bir struct olduğu için al, y'yi değiştir, geri ata
                    centerVec.y = newCenterY;
                    _characterController.center = centerVec;

                    // Yükseklik/merkez güncellemelerinden SONRA kapsülün alt kısmının ofsetini hesapla
                    var bottomOffsetAfterUpdate = newCenterY - newHeight / 2f;

                    // Bu adımda yükseklik/merkez değişiklikleri nedeniyle
                    // kapsülün alt kısmının transform'un pozisyonuna GÖRE ne kadar aşağı doğru yer değiştirdiği.
                    var relativeBottomDisplacement =
                        bottomOffsetAfterUpdate - bottomOffsetBeforeUpdate;

                    // Transform'u bu miktar kadar YUKARI hareket ettirmek istiyoruz ki
                    // kapsül altının göreceli batmasını telafi edelim.
                    // Yani, relativeBottomDisplacement negatifse (aşağı batmışsa), transform'u YUKARI hareket ettiririz.
                    var compensationMove = Vector3.up * -relativeBottomDisplacement;

                    // Şimdi topraklama itmesini de uygula
                    var groundingMove = Vector3.zero;
                    if (_wasGroundedBeforeTransition || _characterController.isGrounded)
                    {
                        groundingMove = Vector3.down * (_characterController.skinWidth + 0.01f);
                    }

                    // Hareketleri birleştir ve uygula
                    _characterController.Move(compensationMove + groundingMove);
                }, ease: Ease.OutQuad);

            // Kamera pozisyonunu yumuşak bir şekilde ayarla
            if (_playerCamera != null)
            {
                var startCameraY = _playerCamera.transform.localPosition.y;
                var endCameraY = _initialCameraYLocalPosition +
                                 (_playerData.isCrouching ? _playerData.crouchCameraYOffset :
                                     _playerData.standingCameraYOffset);

                _playerCameraTween = Tween.Custom(startCameraY, endCameraY, duration,
                    onValueChange: newCameraY =>
                    {
                        var camPos = _playerCamera.transform.localPosition;
                        camPos.y = newCameraY;
                        _playerCamera.transform.localPosition = camPos;
                    }, ease: Ease.OutQuad);
            }
        }

#if UNITY_EDITOR
        // Unity Editor Specific Methods
        public override void OnDrawGizmos()
        {
            if (_characterController == null) return;

            ComputeCeilingRayOrigin(out var radius, out var rayDistance,
                out var originTip, out var originRoot);

            var color = CanStandUp() ? Color.green : Color.red;
            Gizmos.color = color;
            Gizmos.DrawLine(originRoot, originRoot + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(originTip, radius);
        }
#endif
    }
}