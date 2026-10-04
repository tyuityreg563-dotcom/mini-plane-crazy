using UnityEngine;

namespace PlaneGame
{
    /// <summary>Пока хитскан (рейкаст). Для ракет и медленных снарядов заменить на Projectile с проверкой на сервере.
    /// Вешается на префаб оружия рядом с AircraftModule; muzzle — точка выстрела.</summary>
    [RequireComponent(typeof(AircraftModule))]
    public class WeaponModule : MonoBehaviour
    {
        public Transform muzzle;
        AircraftModule _module;
        AircraftRuntime _owner;
        float _nextFire;

        void Start()
        {
            _module = GetComponent<AircraftModule>();
            _owner = GetComponentInParent<AircraftRuntime>();
        }

        public void TryFire()
        {
            if (_module == null || !_module.IsAlive || Time.time < _nextFire) return;
            var d = _module.Definition;
            _nextFire = Time.time + 1f / d.fireRate;

            Transform origin = muzzle != null ? muzzle : transform;
            if (!Physics.Raycast(origin.position, origin.forward, out var hit, d.range)) return;

            // Попадание в конкретный блок: берём ближайший AircraftModule вверх по иерархии от коллайдера.
            var target = hit.collider.GetComponentInParent<AircraftModule>();
            if (target == null) return;
            var targetOwner = target.GetComponentInParent<AircraftRuntime>();
            if (targetOwner != null && targetOwner == _owner) return; // свои блоки не бьём
            target.TakeDamage(d.damage);
        }
    }
}
