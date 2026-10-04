using System.Collections.Generic;
using UnityEngine;

namespace PlaneGame
{
    public enum ModuleType { Fuselage, Wing, Engine, Weapon, Armor, Utility }
    public enum MountSide { Center, Left, Right }

    /// <summary>Данные одного модуля (ScriptableObject). Весь баланс живёт здесь, а не в коде.</summary>
    [CreateAssetMenu(menuName = "PlaneGame/Module Definition")]
    public class ModuleDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public ModuleType type;
        public GameObject prefab;

        [Header("Общее")]
        public float mass = 50f;
        public float maxHealth = 100f;
        public float drag = 0.02f;
        public int cost = 100;

        [Header("Фюзеляж")]
        public float maxTotalMass = 2000f;   // лимит массы собранного самолёта

        [Header("Крыло (и фюзеляж: несущий корпус)")]
        public float lift = 1f;              // подъёмная сила
        public float controlAuthority = 1f;  // вклад в маневренность
        public float loadCapacity = 800f;    // какую массу крыло "держит"

        [Header("Двигатель")]
        public float thrust = 8000f;

        [Header("Оружие")]
        public float damage = 10f;
        public float fireRate = 10f;         // выстрелов в секунду
        public float range = 600f;
    }

    /// <summary>Реестр всех модулей: по id собираем самолёт из чертежа (одинаково на клиенте и сервере).</summary>
    [CreateAssetMenu(menuName = "PlaneGame/Module Registry")]
    public class ModuleRegistry : ScriptableObject
    {
        public List<ModuleDefinition> modules = new();
        Dictionary<string, ModuleDefinition> _map;

        public ModuleDefinition Get(string id)
        {
            if (_map == null)
            {
                _map = new Dictionary<string, ModuleDefinition>();
                foreach (var m in modules) if (m != null) _map[m.id] = m;
            }
            return id != null && _map.TryGetValue(id, out var def) ? def : null;
        }
    }
}
