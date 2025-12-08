using System.Collections;
using UnityEngine;

public class BirdBlackController : BirdController
{
    public float explosionRadius = 2.5f;
    public float explosionForce = 350f;
    public float explosionDamage = 40f;
    public LayerMask affectedLayers;
    public GameObject explosionEffectPrefab;

    private bool _hasExploded = false;

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        if (isActive)
        {
            DetectAlive();
            DrawTrace();

            if (!_hasExploded && Input.GetKeyDown(KeyCode.Space))
            {
                TriggerExplosion();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!_hasExploded && isActive)
        {
            TriggerExplosion();
        }
    }

    void TriggerExplosion()
    {
        _hasExploded = true;

        ApplyExplosionDamage();
        ApplyExplosionPhysics();

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Rbody.linearVelocity = Vector2.zero;
        Rbody.bodyType = RigidbodyType2D.Static;

        StartCoroutine(DestroyAndReload());
    }

    IEnumerator DestroyAndReload()
    {
        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
        SlingshotController.instance.Reload();
    }

    void ApplyExplosionPhysics()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, affectedLayers);

        foreach (Collider2D hit in hits)
        {
            Rigidbody2D rb = hit.attachedRigidbody;

            if (rb != null)
            {
                Vector2 dir = hit.transform.position - transform.position;
                float falloff = Mathf.Clamp01(1 - dir.magnitude / explosionRadius);

                rb.AddForce(dir.normalized * explosionForce * falloff, ForceMode2D.Impulse);
            }
        }
    }

    void ApplyExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, affectedLayers);

        foreach (Collider2D hit in hits)
        {
            HealthController health = hit.GetComponent<HealthController>();

            if (health != null)
            {
                float distance = Vector2.Distance(hit.transform.position, transform.position);
                float falloff = Mathf.Clamp01(1 - (distance / explosionRadius));

                float finalDamage = explosionDamage * falloff;


                health.UpdateHealth(finalDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Debug visual radius
        Gizmos.color = new Color(1f, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }

}
