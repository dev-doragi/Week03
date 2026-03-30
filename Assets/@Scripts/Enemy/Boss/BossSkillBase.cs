using UnityEngine;

public abstract class BossSkillBase : MonoBehaviour
{
    protected EnemyBossBase Owner;

    public virtual void Initialize(EnemyBossBase owner)
    {
        Owner = owner;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}