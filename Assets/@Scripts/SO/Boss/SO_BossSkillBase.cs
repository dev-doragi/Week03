using UnityEngine;

[CreateAssetMenu(fileName = "SO_BossSkill_", menuName = "Scriptable Objects/Boss/Skill Data")]
public class SO_BossSkillBase : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private float _skillCooldown;
    [SerializeField] private float _skillCastTime;
    [SerializeField] private int _skillRepeatCount = 1;

    public string SkillName => _skillName;
    public float SkillCooldown => _skillCooldown;
    public float SkillCastTime => _skillCastTime;
    public int SkillRepeatCount => _skillRepeatCount;
}