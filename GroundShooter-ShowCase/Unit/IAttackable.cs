public interface IAttackable
{
    float GetInstantKillChance();
    float GetCriticalHitChance();
    float GetCriticalDamageMultiplier();
    float GetAttackDamage();
}
public class HitAttack : IAttackable
{
    public HitAttack(float attackDamage, float criticalHitChance = 0f, float criticalDamageMultiplier = 2f, float instantKillChance = 0f)
    {
        this.attackDamage = attackDamage;
        this.criticalHitChance = criticalHitChance;
        this.criticalDamageMultiplier = criticalDamageMultiplier;
        this.instantKillChance = instantKillChance;
    }

    public float attackDamage { get; set; }
    public float criticalHitChance { get; set; }
    public float criticalDamageMultiplier { get; set; } = 2f;
    public float instantKillChance { get; set; }

    public float GetAttackDamage()
    {
        return attackDamage;
    }

    public float GetCriticalHitChance()
    {
        return criticalHitChance;
    }

    public float GetCriticalDamageMultiplier()
    {
        return criticalDamageMultiplier;
    }

    public float GetInstantKillChance()
    {
        return instantKillChance;
    }
}