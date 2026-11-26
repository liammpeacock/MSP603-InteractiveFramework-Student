using System.Collections.Generic;
using UnityEngine;

namespace MSP603.TowerDefense
{
    public sealed class TowerPresentation : MonoBehaviour
    {
        private const float IdleFrameDuration = 0.45f;

        private SpriteRenderer _spriteRenderer;
        private Transform _visual;
        private float _groundRadius;
        private TowerType _type;
        private Sprite[] _idleFrames;
        private Sprite[] _attackFrames;
        private Sprite[] _destroyFrames;
        private Sprite[] _activeFrames;
        private int _frameIndex;
        private int _idleIndex;
        private float _frameTimer;
        private float _frameDuration;
        private float _idleTimer;
        private bool _holdFinalFrame;
        private bool _destroyed;

        public bool HasSpriteArtwork => _idleFrames != null && _idleFrames.Length > 0;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        public void Configure(TowerType type)
        {
            _type = type;
            GameObject visual = new GameObject("AnimatedTowerVisual");
            visual.transform.SetParent(transform, false);
            // Tower sprites use a common import convention: their pivot is the
            // logical centre of the visible base. Keep that anchor on the gameplay
            // root instead of applying a second, presentation-only lift.
            visual.transform.localPosition = Vector3.zero;
            _visual = visual.transform;
            Transform towerBase = transform.Find("TowerBase");
            Renderer baseRenderer = towerBase != null ? towerBase.GetComponent<Renderer>() : null;
            if (baseRenderer != null)
            {
                Vector3 extents = baseRenderer.bounds.extents;
                _groundRadius = Mathf.Max(extents.x, extents.z);
            }
            _spriteRenderer = visual.AddComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 10;

            if (type == TowerType.Cannon)
            {
                _idleFrames = Load(
                    "Towers/Cannon/Idle/Cannon_Idle_1",
                    "Towers/Cannon/Idle/Cannon_Idle_2");
                _attackFrames = Load(
                    "Towers/Cannon/Aim/Cannon_Aim_1",
                    "Towers/Cannon/Fire/Cannon_Fire_1",
                    "Towers/Cannon/Fire/Cannon_Fire_2",
                    "Towers/Cannon/Recoil/Cannon_Recoil_1",
                    "Towers/Cannon/Reload/Cannon_Reload_1",
                    "Towers/Cannon/Reload/Cannon_Reload_2",
                    "Towers/Cannon/Idle/Cannon_ReturnToIdle_1");
                _destroyFrames = Load(
                    "Towers/Cannon/Destroy/Cannon_Destroy_1",
                    "Towers/Cannon/Destroy/Cannon_Destroy_2",
                    "Towers/Cannon/Destroy/Cannon_Destroy_3");
            }
            else if (type == TowerType.Archer)
            {
                _idleFrames = Load(
                    "Towers/Archer/Idle/Archer_Idle_01",
                    "Towers/Archer/Idle/Archer_Idle_02",
                    "Towers/Archer/Idle/Archer_Idle_03");
                _attackFrames = Load(
                    "Towers/Archer/Aim/Archer_Aim_01",
                    "Towers/Archer/Aim/Archer_Draw_01",
                    "Towers/Archer/Aim/Archer_Draw_02",
                    "Towers/Archer/Fire/Archer_Fire_01",
                    "Towers/Archer/Recoil/Archer_Recovery_01");
                _destroyFrames = Load(
                    "Towers/Archer/Destroy/Archer_Destroy_01",
                    "Towers/Archer/Destroy/Archer_Destroy_02",
                    "Towers/Archer/Destroy/Archer_Destroyed");
            }
            else if (type == TowerType.Mage)
            {
                _idleFrames = Load("Towers/Mage/Idle/Mage_ReturnToIdle");
                _attackFrames = _idleFrames;
                _destroyFrames = _idleFrames;
            }
            else if (type == TowerType.Inferno)
            {
                _idleFrames = Load("Towers/Inferno/Master/Inferno_Master");
                _attackFrames = Load(
                    "Towers/Inferno/Fire/Inferno_WindUp",
                    "Towers/Inferno/Fire/Inferno_Fire",
                    "Towers/Inferno/Fire/Inferno_Fire",
                    "Towers/Inferno/Fire/Inferno_Fire",
                    "Towers/Inferno/Recoil/Inferno_Recovery");
                _destroyFrames = Load(
                    "Towers/Inferno/Destroy/Inferno_Destroyed",
                    "Towers/Inferno/Destroy/Inferno_Ruined");
            }
            else
            {
                _idleFrames = System.Array.Empty<Sprite>();
                _attackFrames = System.Array.Empty<Sprite>();
                _destroyFrames = System.Array.Empty<Sprite>();
            }

            ShowIdleFrame();
        }

        public void SetAimDirection(Vector3 direction)
        {
            if (_spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
            {
                _spriteRenderer.flipX = direction.x < 0f;
            }
        }

        public void PlayFire(float cooldown)
        {
            if (_destroyed || _attackFrames == null || _attackFrames.Length == 0)
            {
                return;
            }

            StartSequence(_attackFrames, Mathf.Clamp(cooldown / _attackFrames.Length, 0.06f, 0.18f), false);
        }

        public void PlayDestroyed(bool immediate = false)
        {
            if (_destroyed || _destroyFrames == null || _destroyFrames.Length == 0)
            {
                if (!_destroyed)
                {
                    _destroyed = true;
                    transform.localRotation = Quaternion.Euler(18f, 0f, 28f);
                    foreach (Renderer towerRenderer in GetComponentsInChildren<Renderer>())
                        towerRenderer.material.color *= 0.35f;
                }
                return;
            }

            _destroyed = true;
            if (immediate)
            {
                _activeFrames = null;
                _spriteRenderer.sprite = _destroyFrames[_destroyFrames.Length - 1];
                return;
            }
            StartSequence(_destroyFrames, 0.14f, true);
        }

        public void SetLevel(int level)
        {
            if (_spriteRenderer != null)
            {
                float levelScale = 1f + 0.08f * (level - 1);
                float towerTypeScale = TowerVisualCalibration.GetPermanentScale(_type);
                _spriteRenderer.transform.localScale = Vector3.one * (levelScale * towerTypeScale);
            }
        }

        private void Update()
        {
            if (_activeFrames != null)
            {
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _frameDuration)
                {
                    _frameTimer = 0f;
                    _frameIndex++;
                    if (_frameIndex >= _activeFrames.Length)
                    {
                        Sprite finalFrame = _activeFrames[_activeFrames.Length - 1];
                        _activeFrames = null;
                        if (_holdFinalFrame)
                        {
                            _spriteRenderer.sprite = finalFrame;
                        }
                        else
                        {
                            ShowIdleFrame();
                        }
                    }
                    else
                    {
                        _spriteRenderer.sprite = _activeFrames[_frameIndex];
                    }
                }

                return;
            }

            if (_destroyed || _idleFrames == null || _idleFrames.Length < 2)
            {
                return;
            }

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= IdleFrameDuration)
            {
                _idleTimer = 0f;
                _idleIndex = (_idleIndex + 1) % _idleFrames.Length;
                _spriteRenderer.sprite = _idleFrames[_idleIndex];
            }
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (_spriteRenderer != null && camera != null)
            {
                _visual.rotation = camera.transform.rotation;
                Vector2 permanentCalibration = TowerVisualCalibration.GetPermanent(_type);
                Vector2 temporaryCalibration = TowerVisualCalibration.Get(_type);
                Vector2 calibration = permanentCalibration + temporaryCalibration;
                Vector3 commonGroundingCorrection = CalculateCommonGrounding(camera);
                Vector3 towerTypeCalibrationOffset =
                    camera.transform.right * calibration.x +
                    camera.transform.up * calibration.y;

                // This is the single final assignment controlling artwork position.
                // Gameplay placement remains exclusively on the tower root.
                _visual.position = transform.position
                    + commonGroundingCorrection
                    + towerTypeCalibrationOffset;
            }
        }

        private Vector3 CalculateCommonGrounding(Camera camera)
        {
            // Sprite pivots describe the near/bottom edge of the painted tower base,
            // while the gameplay root describes the centre of its ground footprint.
            // Project the existing TowerBase radius toward the camera onto the
            // billboard's vertical axis so that the painted footprint centre, rather
            // than its near edge, projects onto the gameplay root.
            Vector3 groundToCamera = Vector3.ProjectOnPlane(
                -camera.transform.forward,
                Vector3.up);
            if (_groundRadius <= 0f || groundToCamera.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 nearEdge = groundToCamera.normalized * _groundRadius;
            float verticalProjection = Vector3.Dot(nearEdge, camera.transform.up);
            return camera.transform.up * verticalProjection;
        }

        private void StartSequence(Sprite[] frames, float secondsPerFrame, bool holdFinalFrame)
        {
            _activeFrames = frames;
            _frameIndex = 0;
            _frameTimer = 0f;
            _frameDuration = secondsPerFrame;
            _holdFinalFrame = holdFinalFrame;
            _spriteRenderer.sprite = frames[0];
        }

        private void ShowIdleFrame()
        {
            if (_destroyed)
            {
                return;
            }

            if (_idleFrames == null || _idleFrames.Length == 0)
            {
                Debug.LogWarning($"No tower sprites were found for {name}.", this);
                return;
            }

            _idleIndex = 0;
            _idleTimer = 0f;
            _spriteRenderer.sprite = _idleFrames[0];
        }

        private static Sprite[] Load(params string[] resourcePaths)
        {
            List<Sprite> sprites = new List<Sprite>(resourcePaths.Length);
            foreach (string path in resourcePaths)
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
        }
    }
}
