using System.Collections.Generic;
using UnityEngine;

namespace PlaneGame
{
    public struct AircraftStats
    {
        public float Mass, Thrust, Drag, MaxSpeed;
        public float LiftLeft, LiftRight, LiftTotal;
        public float Agility;       // итоговая маневренность
        public float Overload;      // >1: крылья перегружены массой
        public float RollBias;      // >0: правое крыло сильнее -> крен влево
        public float ThrustToWeight;
    }

    /// <summary>Считает характеристики из ЖИВЫХ модулей. Вызывается при сборке и при каждом повреждении/потере блока.
    /// Все коэффициенты — заглушки для настройки на плейтестах.</summary>
    public static class AircraftStatsCalculator
    {
        public static AircraftStats Calculate(IReadOnlyList<AircraftModule> modules)
        {
            float mass = 0, thrust = 0, drag = 0.05f;
            float liftL = 0, liftR = 0, liftC = 0, authority = 0, capacity = 0;

            foreach (var m in modules)
            {
                if (!m.IsAlive) continue;
                var d = m.Definition;
                float eff = m.Efficiency;
                mass += d.mass;
                drag += d.drag;

                switch (d.type)
                {
                    case ModuleType.Engine:
                        thrust += d.thrust * eff;
                        break;
                    case ModuleType.Wing:
                    case ModuleType.Fuselage:
                        float l = d.lift * eff;
                        if (m.Side == MountSide.Left) liftL += l;
                        else if (m.Side == MountSide.Right) liftR += l;
                        else liftC += l;
                        authority += d.controlAuthority * eff;
                        capacity += d.loadCapacity * eff;
                        break;
                }
            }

            mass = Mathf.Max(mass, 1f);
            float overload = mass / Mathf.Max(capacity, 1f);
            float overloadPenalty = overload > 1f ? 1f / (overload * overload) : 1f;
            float liftSides = liftL + liftR;

            return new AircraftStats
            {
                Mass = mass,
                Thrust = thrust,
                Drag = drag,
                MaxSpeed = Mathf.Sqrt(thrust / drag) * 0.5f,
                LiftLeft = liftL + liftC * 0.5f,
                LiftRight = liftR + liftC * 0.5f,
                LiftTotal = liftSides + liftC,
                Agility = authority * Mathf.Clamp(500f / mass, 0.3f, 2f) * overloadPenalty,
                Overload = overload,
                RollBias = liftSides > 0.001f ? (liftR - liftL) / liftSides : 0f,
                ThrustToWeight = thrust / mass
            };
        }
    }
}
