using UnityEngine;

public class EnemyStats : MonoBehaviour {

    private const float SixteenByNine = 0.5625F;
    private const float StandardSquare = 0.5F;

    public float health = 100;
    public float viewRange = 40f;
    public float viewSize = 2f;
    public float attackRange = 10f;
    public float attackDamage = 5f;
    public float attackRate = 0.2f;

    [Header("Detection")]
    [Range(0, 1)]
    public float aspect = SixteenByNine;
    [Range(2f, 10f)]
    public float frustumDistance = 4f;
    [Range(0, 1)]
    public float frustrumVisibility = 1f;

    [Header("Effects")]
    public GameObject hitParticles;
    public ParticleSystem shotImpactParticles;

    public void TakeDamage(float dmg, Vector3 point)
    {
        if (dmg > 0)
        {
            health -= dmg;
            Instantiate(hitParticles, point, Quaternion.identity);
        }
        if (health < 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //For now:
        Destroy(gameObject);
    }
}