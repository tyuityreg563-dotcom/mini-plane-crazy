using System;
using System.Collections.Generic;

namespace PlaneGame
{
    [Serializable]
    public class PlacedModule
    {
        public string mountId;   // id точки крепления на фюзеляже
        public string moduleId;  // id из ModuleRegistry
    }

    /// <summary>Чертёж самолёта: это то, что хранится в профиле игрока и отправляется на сервер.</summary>
    [Serializable]
    public class AircraftBlueprint
    {
        public string fuselageId;
        public List<PlacedModule> modules = new();
    }
}
