using UnityEngine;

namespace PlaneGame
{
    /// <summary>Аркадная модель полёта: скорость задаёт тяга/масса, вектор скорости следует за носом,
    /// подъёмная сила определяет, держится ли самолёт в воздухе. Асимметрия крыльев даёт самопроизвольный крен.
    /// Поля ввода заполняются локальным вводом или сетевой командой от клиента.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(AircraftRuntime))]
    public class ArcadeFlightController : MonoBehaviour
    {
        [Header("Ввод")]
        [Range(0, 1)] public float Throttle = 1f;
        [Range(-1, 1)] public float Pitch, Roll, Yaw;   // Pitch > 0 = нос вверх, Roll > 0 = крен вправо
        public bool Afterburner;

        [Header("Настройка (градусы/сек, м/с)")]
        public float minSpeed = 30f;
        public float basePitchRate = 70f, baseRollRate = 120f, baseYawRate = 25f;
        public float accelScale = 1f;
        public float rateSmoothing = 6f;
        public float damageRollDrift = 60f;   // насколько сильно кренит при потере крыла
        public float liftK = 0.15f;           // подъёмная сила ~ LiftTotal * v^2 * liftK / масса
        public float maxSink = 60f;
        public float afterburnerMul = 1.4f;

        Rigidbody _rb;
        AircraftRuntime _craft;
        float _speed, _sink;
        Vector3 _rates;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _craft = GetComponent<AircraftRuntime>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _speed = minSpeed * 2f;
        }

        void FixedUpdate()
        {
            if (_craft.IsDead) return;
            var s = _craft.Stats;
            float dt = Time.fixedDeltaTime;

            // 1. Скорость: тяга/масса определяет разгон, MaxSpeed — потолок.
            float maxSpeed = s.MaxSpeed * (Afterburner ? afterburnerMul : 1f);
            float target = Mathf.Lerp(Mathf.Min(minSpeed, maxSpeed), maxSpeed, Throttle);
            float accel = s.Thrust / s.Mass * accelScale;
            _speed = Mathf.MoveTowards(_speed, target, (target > _speed ? accel : accel * 0.7f) * dt);

            // 2. Подъёмная сила: на малой скорости/без крыльев самолёт проваливается.
            float liftFactor = Mathf.Clamp01(s.LiftTotal * _speed * _speed * liftK / s.Mass);
            _sink = Mathf.MoveTowards(_sink, (1f - liftFactor) * maxSink, 9.81f * dt);

            // 3. Повороты: маневренность зависит от крыльев, массы и скорости.
            float agility = s.Agility * Mathf.Lerp(0.3f, 1f, liftFactor);
            var desired = new Vector3(
                -Pitch * basePitchRate * agility + (1f - liftFactor) * 30f,        // x+ = нос вниз (сваливание)
                Yaw * baseYawRate * agility,
                -Roll * baseRollRate * agility + s.RollBias * damageRollDrift);    // z+ = крен влево
            _rates = Vector3.Lerp(_rates, desired, 1f - Mathf.Exp(-rateSmoothing * dt));

            Quaternion newRot = _rb.rotation * Quaternion.Euler(_rates * dt);
            _rb.MoveRotation(newRot);

            // 4. Скорость следует за носом.
            _rb.velocity = newRot * Vector3.forward * _speed + Vector3.down * _sink; // Unity 6: linearVelocity
        }
    }
}
