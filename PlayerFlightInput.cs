using UnityEngine;

namespace PlaneGame
{
    /// <summary>Локальный ввод (старый Input Manager). В мультиплеере вместо прямой записи в контроллер
    /// собирается в структуру и отправляется на сервер каждый тик.</summary>
    [RequireComponent(typeof(ArcadeFlightController))]
    public class PlayerFlightInput : MonoBehaviour
    {
        ArcadeFlightController _fc;
        void Awake() => _fc = GetComponent<ArcadeFlightController>();

        void Update()
        {
            _fc.Pitch = Input.GetAxis("Vertical");
            _fc.Roll = Input.GetAxis("Horizontal");
            _fc.Yaw = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
            _fc.Throttle = Mathf.Clamp01(_fc.Throttle + Input.GetAxis("Mouse ScrollWheel") * 2f);
            _fc.Afterburner = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetButton("Fire1"))
                foreach (var w in GetComponentsInChildren<WeaponModule>()) w.TryFire();
        }
    }
}
