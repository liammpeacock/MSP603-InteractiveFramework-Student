using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP603.TowerDefense
{
    /// <summary>Displays the enemy artwork without owning movement, damage, rewards, or audio.</summary>
    public sealed class EnemyPresentation : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Sprite[] _walkFrames;
        private Sprite[] _hitFrames;
        private Sprite[] _deathFrames;
        private Sprite[] _activeFrames;
        private Action _sequenceCompleted;
        private int _walkIndex;
        private int _frameIndex;
        private float _walkTimer;
        private float _frameTimer;
        private float _walkFrameDuration;
        private float _sequenceFrameDuration;
        private bool _dead;

        public void Configure(EnemyType type)
        {
            GameObject visual = new GameObject("AnimatedEnemyVisual");
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = Vector3.one * 0.5f;
            _spriteRenderer = visual.AddComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 12;

            switch (type)
            {
                case EnemyType.GoblinRaider:
                    _walkFrameDuration = 0.14f;
                    _walkFrames = Load(
                        "Enemies/GoblinRaider/Master/GoblinRaider_Master",
                        "Enemies/GoblinRaider/Walk/GoblinRaider_Walk01",
                        "Enemies/GoblinRaider/Walk/GoblinRaider_Walk02",
                        "Enemies/GoblinRaider/Walk/GoblinRaider_Walk03");
                    _hitFrames = Load("Enemies/GoblinRaider/Attack/GoblinRaider_Hit");
                    _deathFrames = Load(
                        "Enemies/GoblinRaider/Death/GoblinRaider_Death02",
                        "Enemies/GoblinRaider/Death/GoblinRaider_Death03",
                        "Enemies/GoblinRaider/Death/GoblinRaider_Dead");
                    break;
                case EnemyType.RuinKnight:
                    _walkFrameDuration = 0.24f;
                    _walkFrames = Load(
                        "Enemies/RuinKnight/Walk/Ruinknight_Walk_01",
                        "Enemies/RuinKnight/Walk/Ruinknight_Walk_02",
                        "Enemies/RuinKnight/Walk/Ruinknight_Walk_03");
                    _hitFrames = Load("Enemies/RuinKnight/Hit/RuinKnight_Hit");
                    _deathFrames = Load(
                        "Enemies/RuinKnight/Death/RuinKnight_Death01",
                        "Enemies/RuinKnight/Death/RuinKnight_Death02",
                        "Enemies/RuinKnight/Death/RuinKnight_Death03",
                        "Enemies/RuinKnight/Death/RuinKnight_Dead");
                    break;
                default:
                    _walkFrameDuration = 0.20f;
                    _walkFrames = Load(
                        "Enemies/GolbinBrute/Master/GoblinBrute_Master",
                        "Enemies/GolbinBrute/Walk/GoblinBrute_Walk01",
                        "Enemies/GolbinBrute/Walk/GoblinBrute_Walk02",
                        "Enemies/GolbinBrute/Walk/GoblinBrute_Walk03",
                        "Enemies/GolbinBrute/Walk/GoblinBrute_Walk04");
                    _hitFrames = Load("Enemies/GolbinBrute/Hit/GoblinBrute_Hit");
                    _deathFrames = Load(
                        "Enemies/GolbinBrute/Death/GoblinBrute_Death01",
                        "Enemies/GolbinBrute/Death/GoblinBrute_Death02",
                        "Enemies/GolbinBrute/Death/GoblinBrute_Death03");
                    break;
            }

            if (_walkFrames.Length == 0)
            {
                Destroy(visual);
                _spriteRenderer = null;
                Debug.LogWarning($"No enemy sprites were found for {name}; retaining the fallback mesh.", this);
                return;
            }

            Renderer fallbackRenderer = GetComponent<Renderer>();
            if (fallbackRenderer != null)
            {
                fallbackRenderer.enabled = false;
            }

            _spriteRenderer.sprite = _walkFrames[0];
        }

        public void SetMoveDirection(Vector3 direction)
        {
            if (_spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
            {
                _spriteRenderer.flipX = direction.x < 0f;
            }
        }

        public void PlayHit()
        {
            if (!_dead && _hitFrames.Length > 0)
            {
                StartSequence(_hitFrames, 0.12f, null);
            }
        }

        public bool PlayDeath(Action completed)
        {
            if (_dead || _spriteRenderer == null || _deathFrames.Length == 0)
            {
                return false;
            }

            _dead = true;
            StartSequence(_deathFrames, 0.18f, completed);
            return true;
        }

        private void Update()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            if (_activeFrames != null)
            {
                _frameTimer += Time.deltaTime;
                if (_frameTimer < _sequenceFrameDuration)
                {
                    return;
                }

                _frameTimer = 0f;
                _frameIndex++;
                if (_frameIndex < _activeFrames.Length)
                {
                    _spriteRenderer.sprite = _activeFrames[_frameIndex];
                    return;
                }

                Action completed = _sequenceCompleted;
                _activeFrames = null;
                _sequenceCompleted = null;
                if (!_dead)
                {
                    ShowWalkFrame();
                }
                completed?.Invoke();
                return;
            }

            if (_dead || _walkFrames.Length < 2)
            {
                return;
            }

            _walkTimer += Time.deltaTime;
            if (_walkTimer >= _walkFrameDuration)
            {
                _walkTimer = 0f;
                _walkIndex = (_walkIndex + 1) % _walkFrames.Length;
                ShowWalkFrame();
            }
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (_spriteRenderer != null && camera != null)
            {
                _spriteRenderer.transform.rotation = camera.transform.rotation;
            }
        }

        private void StartSequence(Sprite[] frames, float secondsPerFrame, Action completed)
        {
            _activeFrames = frames;
            _frameIndex = 0;
            _frameTimer = 0f;
            _sequenceFrameDuration = secondsPerFrame;
            _sequenceCompleted = completed;
            _spriteRenderer.sprite = frames[0];
        }

        private void ShowWalkFrame()
        {
            _spriteRenderer.sprite = _walkFrames[_walkIndex];
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
