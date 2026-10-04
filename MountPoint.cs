using UnityEngine;

namespace PlaneGame
{
    /// <summary>Точка крепления. Расставляется на префабе фюзеляжа пустыми дочерними объектами.</summary>
    public class MountPoint : MonoBehaviour
    {
        public string id;                        // уникален в рамках фюзеляжа: "wing_L1", "engine_C"
        public MountSide side = MountSide.Center;
        public ModuleType[] allowed;             // какие типы модулей сюда можно ставить

        public bool Allows(ModuleType t) => allowed != null && System.Array.IndexOf(allowed, t) >= 0;
    }
}
