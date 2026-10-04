using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneGame
{
    /// <summary>Собранный самолёт в бою: хранит модули, пересчитывает характеристики, обрабатывает потерю блоков.
    /// Корневой объект (фюзеляж) должен иметь Rigidbody и Collider.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AircraftRuntime : MonoBehaviour
    {
        public readonly List<AircraftModule> Modules = new();
        public AircraftStats Stats { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<AircraftRuntime> StatsChanged;
        public event Action<AircraftRuntime> Died;

        Rigidbody _rb;

        public void Initialize(IEnumerable<AircraftModule> modules)
        {
            _rb = GetComponent<Rigidbody>();
            foreach (var m in modules)
            {
                Modules.Add(m);
                m.Damaged += _ => Recalculate();
                m.Destroyed += OnModuleDestroyed;
            }
            Recalculate();
        }

        void OnModuleDestroyed(AircraftModule m)
        {
            if (m.Definition.type == ModuleType.Fuselage) { Die(); return; }
            Modules.Remove(m);
            m.Detach(_rb.velocity);
            Recalculate();
        }

        void Recalculate()
        {
            Stats = AircraftStatsCalculator.Calculate(Modules);
            StatsChanged?.Invoke(this);
        }

        void Die()
        {
            if (IsDead) return;
            IsDead = true;
            foreach (var m in Modules)
                if (m.Definition.type != ModuleType.Fuselage) m.Detach(_rb.velocity);
            Died?.Invoke(this);
            Destroy(gameObject, 0.1f); // сюда же повесить взрыв/респавн
        }
    }
}
