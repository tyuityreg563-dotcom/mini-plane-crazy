using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlaneGame
{
    public static class AircraftAssembler
    {
        /// <summary>Проверка чертежа без создания объектов. ОБЯЗАТЕЛЬНО вызывать на сервере: клиенту верить нельзя.</summary>
        public static bool Validate(AircraftBlueprint bp, ModuleRegistry reg, out string error)
        {
            error = null;
            var fus = reg.Get(bp.fuselageId);
            if (fus == null || fus.type != ModuleType.Fuselage || fus.prefab == null)
            { error = "Не выбран корректный фюзеляж"; return false; }

            var mounts = fus.prefab.GetComponentsInChildren<MountPoint>(true).ToDictionary(m => m.id);
            var used = new HashSet<string>();
            float mass = fus.mass;
            int engines = 0;

            foreach (var p in bp.modules)
            {
                var def = reg.Get(p.moduleId);
                if (def == null) { error = $"Неизвестный модуль {p.moduleId}"; return false; }
                if (!mounts.TryGetValue(p.mountId, out var mount)) { error = $"Нет точки крепления {p.mountId}"; return false; }
                if (!used.Add(p.mountId)) { error = $"Точка {p.mountId} занята дважды"; return false; }
                if (!mount.Allows(def.type)) { error = $"{def.displayName} нельзя поставить в {p.mountId}"; return false; }
                mass += def.mass;
                if (def.type == ModuleType.Engine) engines++;
            }

            if (engines == 0) { error = "Нужен хотя бы один двигатель"; return false; }
            if (mass > fus.maxTotalMass) { error = $"Перевес: {mass:0} из {fus.maxTotalMass:0}"; return false; }
            return true;
        }

        /// <summary>Создаёт самолёт из чертежа. Возвращает null, если чертёж невалиден.</summary>
        public static AircraftRuntime Build(AircraftBlueprint bp, ModuleRegistry reg, Vector3 pos, Quaternion rot, out string error)
        {
            if (!Validate(bp, reg, out error)) return null;

            var fusDef = reg.Get(bp.fuselageId);
            var root = Object.Instantiate(fusDef.prefab, pos, rot);

            var modules = new List<AircraftModule>();
            var rootModule = root.GetComponent<AircraftModule>() ?? root.AddComponent<AircraftModule>();
            rootModule.Init(fusDef, MountSide.Center);
            modules.Add(rootModule);

            var mounts = root.GetComponentsInChildren<MountPoint>(true).ToDictionary(m => m.id);
            foreach (var p in bp.modules)
            {
                var def = reg.Get(p.moduleId);
                var mount = mounts[p.mountId];
                var go = Object.Instantiate(def.prefab, mount.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;

                var module = go.GetComponent<AircraftModule>() ?? go.AddComponent<AircraftModule>();
                module.Init(def, mount.side);
                modules.Add(module);
            }

            var runtime = root.GetComponent<AircraftRuntime>() ?? root.AddComponent<AircraftRuntime>();
            runtime.Initialize(modules);
            return runtime;
        }
    }
}
