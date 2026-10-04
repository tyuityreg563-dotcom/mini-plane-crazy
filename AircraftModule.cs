using System;
using UnityEngine;

namespace PlaneGame
{
    /// <summary>Живой экземпляр модуля на самолёте: здоровье, урон, отрыв при разрушении.
    /// На префабе модуля нужен Collider (Box/Capsule, не MeshCollider).</summary>
    public class AircraftModule : MonoBehaviour
    {
        public ModuleDefinition Definition { get; private set; }
        public MountSide Side { get; private set; }
        public float Health { get; private set; }

        public event Action<AircraftModule> Damaged;
        public event Action<AircraftModule> Destroyed;

        public bool IsAlive => Health > 0f;
        public float HealthFraction => Definition != null ? Mathf.Clamp01(Health / Definition.maxHealth) : 0f;
        /// <summary>Повреждённый модуль работает хуже (30%..100%), но не отключается сразу.</summary>
        public float Efficiency => Mathf.Lerp(0.3f, 1f, HealthFraction);

        public void Init(ModuleDefinition def, MountSide side)
        {
            Definition = def;
            Side = side;
            Health = def.maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            Health = Mathf.Max(0f, Health - amount);
            if (Health <= 0f) Destroyed?.Invoke(this);
            else Damaged?.Invoke(this);
        }

        /// <summary>Отрывает модуль от самолёта и превращает в обломок.</summary>
        public void Detach(Vector3 inheritedVelocity)
        {
            if (transform.parent == null) return;
            transform.SetParent(null, true);
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = Mathf.Max(1f, Definition.mass);
            rb.velocity = inheritedVelocity; // в Unity 6: rb.linearVelocity
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.VelocityChange);
            Destroy(gameObject, 6f);
        }
    }
}
