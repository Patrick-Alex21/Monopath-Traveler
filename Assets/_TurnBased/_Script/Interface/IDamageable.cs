public interface IDamageable
{
    void TakeDamage(int damage, DamageEffectiveness effectiveness = DamageEffectiveness.None);
}