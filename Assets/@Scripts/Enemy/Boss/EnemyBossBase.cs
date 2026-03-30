using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossBase : EnemyBase
{
    [System.Serializable]
    private class BossSkillSlot
    {
        public SO_BossSkillBase SkillData;
        public BossSkillBase SkillLogic;
        public int Weight = 1;
    }

    [Header("Boss Settings")]
    [SerializeField] private List<BossSkillSlot> _skillSlots = new();
    [SerializeField] private float _firstSkillDelay = 1.5f;

    public Rigidbody2D Rb => _rb;

    private readonly List<float> _nextSkillReadyTimes = new();
    private readonly List<int> _readySkillIndexes = new();

    private bool _isSkillInitialized;

    public bool IsCastingSkill { get; private set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        InitializeSkillsIfNeeded();
        ForceResetBossState();
        ResetSkillCooldowns();
    }

    protected override void Start()
    {
        base.Start();
        InitializeSkillsIfNeeded();
    }

    private void OnDisable()
    {
        ForceResetBossState();
    }

    protected override void TickCombat(float distance)
    {
        if (IsDead)
        {
            SetRunAnimation(false);
            return;
        }

        if (IsCastingSkill)
        {
            SetRunAnimation(false);
            return;
        }

        if (BuildReadySkillIndexes() > 0)
        {
            StopMovement();
            SetRunAnimation(false);

            if (CanStartAttack())
                StartAttack();

            return;
        }

        MoveTowardsTarget();
        SetRunAnimation(true);
    }

    protected override void FixedUpdate()
    {
        if (IsCastingSkill)
            return;

        base.FixedUpdate();
    }

    protected override IEnumerator AttackRoutine()
    {
        int skillIndex = PickWeightedRandomFromReady();
        if (skillIndex < 0)
            yield break;

        BossSkillSlot slot = _skillSlots[skillIndex];
        if (slot.SkillData == null || slot.SkillLogic == null)
            yield break;

        IsCastingSkill = true;

        slot.SkillLogic.Enter();

        int repeatCount = Mathf.Max(1, slot.SkillData.SkillRepeatCount);
        float castTime = Mathf.Max(0f, slot.SkillData.SkillCastTime);
        float interval = repeatCount > 1 ? castTime / (repeatCount - 1) : 0f;

        for (int i = 0; i < repeatCount; i++)
        {
            if (IsDead)
                break;

            slot.SkillLogic.Execute();

            if (repeatCount > 1 && i < repeatCount - 1 && interval > 0f)
                yield return new WaitForSeconds(interval);
        }

        if (repeatCount <= 1 && castTime > 0f)
            yield return new WaitForSeconds(castTime);

        slot.SkillLogic.Exit();
        _nextSkillReadyTimes[skillIndex] = Time.time + Mathf.Max(0f, slot.SkillData.SkillCooldown);
        IsCastingSkill = false;
    }

    public override void HandleDeath()
    {
        if (IsDead)
            return;

        ForceResetBossState();
        base.HandleDeath();
    }

    private void InitializeSkillsIfNeeded()
    {
        if (_isSkillInitialized)
            return;

        _nextSkillReadyTimes.Clear();

        for (int i = 0; i < _skillSlots.Count; i++)
        {
            BossSkillSlot slot = _skillSlots[i];

            if (slot.SkillLogic != null)
                slot.SkillLogic.Initialize(this);

            _nextSkillReadyTimes.Add(0f);
        }

        _isSkillInitialized = true;
    }

    private void ResetSkillCooldowns()
    {
        float firstReadyTime = Time.time + Mathf.Max(0f, _firstSkillDelay);

        for (int i = 0; i < _skillSlots.Count; i++)
        {
            if (i >= _nextSkillReadyTimes.Count)
                _nextSkillReadyTimes.Add(firstReadyTime);
            else
                _nextSkillReadyTimes[i] = firstReadyTime;
        }

        _readySkillIndexes.Clear();
    }

    private void ForceResetBossState()
    {
        IsCastingSkill = false;
        ExitAllSkills();
        _readySkillIndexes.Clear();

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    private void ExitAllSkills()
    {
        for (int i = 0; i < _skillSlots.Count; i++)
        {
            BossSkillSlot slot = _skillSlots[i];
            if (slot.SkillLogic != null)
                slot.SkillLogic.Exit();
        }
    }

    private int BuildReadySkillIndexes()
    {
        _readySkillIndexes.Clear();

        for (int i = 0; i < _skillSlots.Count; i++)
        {
            if (i >= _nextSkillReadyTimes.Count)
                continue;

            BossSkillSlot slot = _skillSlots[i];
            if (slot.SkillData == null || slot.SkillLogic == null)
                continue;

            if (Time.time >= _nextSkillReadyTimes[i])
                _readySkillIndexes.Add(i);
        }

        return _readySkillIndexes.Count;
    }

    private int PickWeightedRandomFromReady()
    {
        if (_readySkillIndexes.Count == 0)
            return -1;

        int totalWeight = 0;

        for (int i = 0; i < _readySkillIndexes.Count; i++)
        {
            int index = _readySkillIndexes[i];
            totalWeight += Mathf.Max(1, _skillSlots[index].Weight);
        }

        int pick = Random.Range(0, totalWeight);

        for (int i = 0; i < _readySkillIndexes.Count; i++)
        {
            int index = _readySkillIndexes[i];
            int weight = Mathf.Max(1, _skillSlots[index].Weight);

            if (pick < weight)
                return index;

            pick -= weight;
        }

        return _readySkillIndexes[0];
    }
}