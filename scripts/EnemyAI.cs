using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAI : ObstacleMarker
{
    public float speed = 5f;
    public float attackRange = 10f;
    public float attackRate = 1f;
    public float damage = 10f;
    public float health = 200f;
    public float retargetCooldown = 2f;
    private float retargetTimer;
    private bool isAttacking;

    private Transform target;

    private Building targetBuilding;

    private float attackCooldown;

    void Start()
    {
        FindClosestTarget();
    }

    void Update()
    {
        // должно быть не тут
        if (MapModeManager.Instance.isMapMode) 
        {
            Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.enabled = false;
        }
        else
        {
            Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.enabled = true;
        }
        // должно быть не тут
        if (target == null || targetBuilding == null || targetBuilding.health <= 0)
        {
            isAttacking = false;
            FindClosestTarget();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
            MoveTowardsTarget();
        else
            Attack();

        if (targetBuilding != null && targetBuilding.health <= 0)
        {
            isAttacking = false;
            Destroy(targetBuilding.gameObject);
            FindClosestTarget();
        }
        else if (targetBuilding == null)
        {
            FindClosestTarget();
        }

        if (!isAttacking)
        {
            retargetTimer -= Time.deltaTime;
            if (retargetTimer <= 0)
            {
                FindClosestTarget();
                retargetTimer = retargetCooldown;
            }
        }     
    }

    public void Die()
    {
      Destroy(this.gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
    }

    void FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Building");
        float closestDistance = Mathf.Infinity;

        target = null;
        targetBuilding = null;

        foreach (var obj in targets)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);

            if (dist < closestDistance)
            {
                Building building = obj.GetComponent<Building>();
                if (building != null && building.health > 0)
                {
                    closestDistance = dist;
                    target = obj.transform;
                    targetBuilding = building;
                }
            }
        }
    }

    void MoveTowardsTarget()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(target);
    }

    void Attack()
    {
        if (attackCooldown <= 0f && targetBuilding != null)
        {
            isAttacking = true;
            targetBuilding.TakeDamage(damage);
            targetBuilding.isUnderAttack = true;
            attackCooldown = 1f / attackRate;
        }
        else
        {
            attackCooldown -= Time.deltaTime;
            targetBuilding.isUnderAttack = false;
        }
    }
}