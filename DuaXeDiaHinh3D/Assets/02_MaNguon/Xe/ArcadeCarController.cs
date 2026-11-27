using UnityEngine;

namespace DuaXe3D
{
    /// <summary>
    /// Vật lý xe kiểu arcade: Rigidbody, bám địa hình, drift, nitro.
    /// Điều khiển: W/S/A/D, Space drift, Shift nitro (Yêu cầu 8).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ArcadeCarController : MonoBehaviour
    {
        public CarSpec Spec;
        public CarVisual Visual;
        public bool IsPlayer;
        public RoadPath Path;
        public bool ControlsLocked = true;
        public bool ExternalControl;

        public float ThrottleInput;
        public float SteerInput;
        public bool HandbrakeInput;
        public bool NitroInput;

        public float Nitro01 = 1f;
        public bool IsBoosting { get; private set; }
        public bool Grounded { get; private set; }
        public float ForwardSpeed { get; private set; }
        public float SpeedKmh { get { return Mathf.Abs(ForwardSpeed) * 3.6f; } }
        public float Speed01
        {
            get
            {
                float max = Spec != null ? Spec.MaxSpeedKmh / 3.6f : 40f;
                return Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / max);
            }
        }
        public bool OffRoad { get; private set; }
        public float DistanceToRoad { get; private set; }
        public int RoadIndex { get; private set; }
        public float SurfaceGripMultiplier = 1f;
        public Color DustColor = new Color(0.72f, 0.63f, 0.48f, 0.55f);

        private Rigidbody _rb;
        private int _groundMask;
        private Vector3 _groundNormal = Vector3.up;
        private float _wheelRadius = 0.6f;
        private float _wheelSpin;
        private float _steerVisual;
        private float _lateralSpeed;
        private float _flipTime;
        private float _stuckTime;
        private float _lastCrash;
        private bool _wasBoosting;
        private int _hint = -1;

        private ParticleSystem _dustL;
        private ParticleSystem _dustR;
        private ParticleSystem _flameL;
        private ParticleSystem _flameR;
        private TrailRenderer _skidL;
        private TrailRenderer _skidR;
        private Light _boostLight;

        private Vector3[] _wheelLocal;

        public void Configure(CarSpec spec, CarVisual visual, bool isPlayer, RoadPath path)
        {
            Spec = spec;
            Visual = visual;
            IsPlayer = isPlayer;
            Path = path;
            ExternalControl = !isPlayer;
            _wheelRadius = (visual.WheelRadii[0] + visual.WheelRadii[2]) * 0.5f;

            float halfTrack = spec.TrackWidth * 0.5f;
            float halfBase = spec.WheelBase * 0.5f;
            float y = spec.WheelRadius + 0.15f;
            _wheelLocal = new[]
            {
                new Vector3(-halfTrack, y, halfBase),
                new Vector3(halfTrack, y, halfBase),
                new Vector3(-halfTrack, y, -halfBase),
                new Vector3(halfTrack, y, -halfBase)
            };
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _groundMask = SceneSetup.GroundMask;
        }

        private void Start()
        {
            if (Spec == null)
            {
                Spec = CarLibrary.Get(0);
            }
            if (_wheelLocal == null)
            {
                float halfTrack = Spec.TrackWidth * 0.5f;
                float halfBase = Spec.WheelBase * 0.5f;
                float y = Spec.WheelRadius + 0.15f;
                _wheelLocal = new[]
                {
                    new Vector3(-halfTrack, y, halfBase),
                    new Vector3(halfTrack, y, halfBase),
                    new Vector3(-halfTrack, y, -halfBase),
                    new Vector3(halfTrack, y, -halfBase)
                };
            }

            _rb.isKinematic = ControlsLocked;
            CreateEffects();
        }

        private void Update()
        {
            if (IsPlayer && !ExternalControl)
            {
                if (ControlsLocked)
                {
                    ThrottleInput = 0f;
                    SteerInput = 0f;
                    HandbrakeInput = false;
                    NitroInput = false;
                }
                else
                {
                    float throttle = 0f;
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) throttle += 1f;
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) throttle -= 1f;
                    ThrottleInput = throttle;

                    float steer = 0f;
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) steer -= 1f;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steer += 1f;
                    SteerInput = steer;

                    HandbrakeInput = Input.GetKey(KeyCode.Space);
                    NitroInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                }
            }

            if (ControlsLocked)
            {
                ThrottleInput = 0f;
                SteerInput = 0f;
                HandbrakeInput = false;
            }

            bool wantBoost = NitroInput && Nitro01 > 0.015f && !ControlsLocked && Mathf.Abs(ThrottleInput) > 0.1f;
            IsBoosting = wantBoost;
            if (IsBoosting && !_wasBoosting && IsPlayer && AudioManager.Instance != null)
                AudioManager.Instance.PlayBoost();
            _wasBoosting = IsBoosting;

            if (IsBoosting) Nitro01 = Mathf.Max(0f, Nitro01 - Time.deltaTime * 0.30f);
            else Nitro01 = Mathf.Min(1f, Nitro01 + Time.deltaTime * 0.045f);

            UpdateVisuals();
            UpdateEffects();
        }

        private void FixedUpdate()
        {
            if (_rb == null || Spec == null) return;

            if (ControlsLocked)
            {
                if (!_rb.isKinematic)
                {
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                }
                return;
            }

            if (Path != null)
            {
                DistanceToRoad = Path.DistanceToRoad(transform.position, _hint, out int idx);
                RoadIndex = idx;
                _hint = idx;
                OffRoad = DistanceToRoad > Path.RoadWidth * 0.62f + 2.2f;
            }

            float dt = Time.fixedDeltaTime;
            Vector3 origin = transform.position;
            Vector3 avgNormal = Vector3.zero;
            int hits = 0;

            for (int i = 0; i < _wheelLocal.Length; i++)
            {
                Vector3 o = transform.TransformPoint(_wheelLocal[i] + Vector3.up * 0.35f);
                float rayLength = Spec.WheelRadius * 2.4f + 1.0f;
                RaycastHit hit;
                if (Physics.Raycast(o, Vector3.down, out hit, rayLength, _groundMask, QueryTriggerInteraction.Ignore))
                {
                    hits++;
                    avgNormal += hit.normal;
                }
            }

            Grounded = hits > 0;
            if (hits >= 2)
            {
                _groundNormal = avgNormal.normalized;
                if (_groundNormal.y < 0.35f) _groundNormal = Vector3.Lerp(Vector3.up, _groundNormal, 0.4f).normalized;
            }
            else if (hits == 1)
            {
                _groundNormal = Vector3.Lerp(_groundNormal, avgNormal.normalized, 0.35f).normalized;
            }
            else
            {
                _groundNormal = Vector3.Lerp(_groundNormal, Vector3.up, 0.08f).normalized;
            }

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal);
            if (forward.sqrMagnitude < 0.05f) forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            forward.Normalize();

            ForwardSpeed = Vector3.Dot(_rb.linearVelocity, forward);
            _lateralSpeed = Vector3.Dot(_rb.linearVelocity, Vector3.Cross(_groundNormal, forward).normalized);

            float maxSpeed = Spec.MaxSpeedKmh / 3.6f;
            if (OffRoad) maxSpeed *= 0.72f;
            if (IsBoosting) maxSpeed += 12f;

            float accel = Spec.AccelForce * 1.55f;
            if (OffRoad) accel *= 0.68f;
            if (IsBoosting) accel *= 1.85f;

            float throttle = ThrottleInput;
            if (throttle > 0.02f)
            {
                if (ForwardSpeed < maxSpeed)
                {
                    // Bù lực leo dốc để xe vượt đồi mạnh mẽ, mượt mà
                    float slopeBoost = Mathf.Max(0f, forward.y) * 14f;
                    _rb.AddForce(forward * ((accel + slopeBoost) * throttle), ForceMode.Acceleration);
                }
            }
            else if (throttle < -0.02f)
            {
                float brake = Mathf.Abs(throttle);
                if (ForwardSpeed > 1f)
                    _rb.AddForce(-forward * (accel * 1.55f * brake), ForceMode.Acceleration);
                else if (ForwardSpeed > -10f)
                    _rb.AddForce(-forward * (accel * 0.65f * brake), ForceMode.Acceleration);
            }
            else if (Grounded && Mathf.Abs(ForwardSpeed) > 0.1f)
            {
                _rb.AddForce(-forward * (Mathf.Sign(ForwardSpeed) * 2.2f), ForceMode.Acceleration);
            }

            // Bám đường + chống trượt ngang
            if (Grounded)
            {
                _rb.AddForce(-_groundNormal * 6f, ForceMode.Acceleration);

                float grip = Spec.Grip * Mathf.Clamp(SurfaceGripMultiplier, 0.55f, 1.25f);
                if (OffRoad) grip *= 0.58f;
                if (HandbrakeInput) grip *= 0.18f;
                _rb.AddForce(-Vector3.Cross(_groundNormal, forward).normalized * (_lateralSpeed * grip), ForceMode.Acceleration);

                if (HandbrakeInput && Mathf.Abs(ForwardSpeed) > 1f)
                    _rb.AddForce(-forward * (Mathf.Sign(ForwardSpeed) * 7f), ForceMode.Acceleration);

                float speedRatio = Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / Mathf.Max(1f, maxSpeed));
                float steerFactor = Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / 3.0f);
                float highSpeedScale = 1f - 0.28f * speedRatio;
                float driftTurnBonus = HandbrakeInput ? 1.28f : 1f;
                float yawRateDeg = SteerInput * Spec.SteerAngle * 2.45f * steerFactor * highSpeedScale * driftTurnBonus;
                if (ForwardSpeed < -0.5f) yawRateDeg = -yawRateDeg;

                // Áp dụng góc rẽ trực tiếp để tay lái nhạy và bám cua chuẩn, chỉ nội suy độ nghiêng theo địa hình
                Quaternion yaw = Quaternion.AngleAxis(yawRateDeg * dt, _groundNormal);
                Quaternion turned = yaw * _rb.rotation;
                Quaternion align = Quaternion.FromToRotation(turned * Vector3.up, _groundNormal) * turned;
                _rb.MoveRotation(Quaternion.Slerp(turned, align, Mathf.Clamp01(10f * dt)));
                _rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // Cho phép chỉnh lái nhẹ khi vừa qua đỉnh dốc để không bị mất lái
                if (Mathf.Abs(SteerInput) > 0.05f && Mathf.Abs(ForwardSpeed) > 3f)
                {
                    float airYaw = SteerInput * Spec.SteerAngle * 0.65f * dt;
                    _rb.MoveRotation(Quaternion.AngleAxis(airYaw, Vector3.up) * _rb.rotation);
                }
                _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, 0.6f * dt);
            }

            // Giới hạn tốc độ
            Vector3 velocity = _rb.linearVelocity;
            float fwd = Vector3.Dot(velocity, forward);
            if (fwd > maxSpeed)
                _rb.linearVelocity = velocity - forward * (fwd - maxSpeed);
            if (fwd < -10f)
                _rb.linearVelocity = velocity - forward * (fwd + 10f);

            // Chống kẹt / lật / rơi
            if (transform.position.y < -20f)
            {
                ResetToTrack();
                return;
            }

            if (Grounded && Vector3.Dot(transform.up, Vector3.up) < 0.1f)
            {
                _flipTime += dt;
                if (_flipTime > 2.2f) { ResetToTrack(); return; }
            }
            else _flipTime = 0f;

            if (Grounded && Mathf.Abs(ForwardSpeed) < 0.8f && Mathf.Abs(throttle) > 0.5f)
            {
                _stuckTime += dt;
                if (_stuckTime > 4f) { ResetToTrack(); return; }
            }
            else _stuckTime = 0f;
        }

        private void UpdateVisuals()
        {
            if (Visual == null) return;

            float spin = 0f;
            if (!ControlsLocked && Grounded)
                _wheelSpin += (ForwardSpeed / Mathf.Max(0.15f, _wheelRadius)) * Mathf.Rad2Deg * Time.deltaTime;
            _wheelSpin = Mathf.Repeat(_wheelSpin, 360f);

            _steerVisual = Mathf.Lerp(_steerVisual, SteerInput * 28f, 1f - Mathf.Exp(-10f * Time.deltaTime));
            Visual.ApplyWheels(_wheelSpin, _steerVisual);
        }

        private void UpdateEffects()
        {
            if (_dustL == null) return;

            float speedAbs = Mathf.Abs(ForwardSpeed);
            float slip = Mathf.Abs(_lateralSpeed);
            float dust = 0f;
            if (Grounded && !ControlsLocked)
            {
                float speedDust = Mathf.Clamp01((speedAbs - 3.5f) / 26f) * 0.65f;
                dust = Mathf.Max(speedDust, Mathf.Clamp01((slip - 1.2f) / 7f));
                if (OffRoad) dust = Mathf.Max(dust, Mathf.Clamp01(speedAbs / 18f) * 0.95f);
            }

            SetEmission(_dustL, dust * 65f);
            SetEmission(_dustR, dust * 65f);

            bool drift = Grounded && !ControlsLocked &&
                         ((slip > 4.2f && speedAbs > 3f) || (HandbrakeInput && speedAbs > 2f));
            if (_skidL != null) _skidL.emitting = drift;
            if (_skidR != null) _skidR.emitting = drift;

            if (_flameL != null) SetEmission(_flameL, IsBoosting ? 70f : 0f);
            if (_flameR != null) SetEmission(_flameR, IsBoosting ? 70f : 0f);

            if (_boostLight != null)
            {
                float target = IsBoosting ? 2.6f : 0f;
                _boostLight.intensity = Mathf.Lerp(_boostLight.intensity, target, 1f - Mathf.Exp(-10f * Time.deltaTime));
            }
        }

        private static void SetEmission(ParticleSystem ps, float rate)
        {
            if (ps == null) return;
            ParticleSystem.EmissionModule e = ps.emission;
            e.rateOverTime = rate;
        }

        private void CreateEffects()
        {
            if (Spec == null) return;
            float halfTrack = Spec.TrackWidth * 0.5f;
            float rearZ = -Spec.WheelBase * 0.5f;
            float dustY = 0.12f;

            _dustL = CreateDust(new Vector3(-halfTrack, dustY, rearZ), "DustL");
            _dustR = CreateDust(new Vector3(halfTrack, dustY, rearZ), "DustR");

            _flameL = CreateFlame(new Vector3(-0.28f, Spec.WheelRadius * 0.55f, -Spec.BodyLength * 0.5f - 0.08f), "FlameL");
            _flameR = CreateFlame(new Vector3(0.28f, Spec.WheelRadius * 0.55f, -Spec.BodyLength * 0.5f - 0.08f), "FlameR");

            _skidL = CreateSkid(new Vector3(-halfTrack, 0.045f, rearZ), "SkidL");
            _skidR = CreateSkid(new Vector3(halfTrack, 0.045f, rearZ), "SkidR");

            GameObject lightGo = new GameObject("BoostLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, Spec.WheelRadius * 0.7f, -Spec.BodyLength * 0.55f);
            _boostLight = lightGo.AddComponent<Light>();
            _boostLight.type = LightType.Point;
            _boostLight.color = new Color(1f, 0.45f, 0.1f);
            _boostLight.range = 7f;
            _boostLight.intensity = 0f;
            _boostLight.shadows = LightShadows.None;

            SetDustColor(DustColor);
        }

        private ParticleSystem CreateDust(Vector3 localPos, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.95f);
            main.startColor = DustColor;
            main.gravityModifier = 0.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 140;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.7f, 0.15f, 0.7f);

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialLibrary.Particle("dust_particle", false);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Play();
            return ps;
        }

        private ParticleSystem CreateFlame(Vector3 localPos, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.34f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 7.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
            main.startColor = new Color(1f, 0.65f, 0.15f, 0.95f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 90;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.08f;
            shape.rotation = new Vector3(0f, 180f, 0f);

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f),
                    new GradientColorKey(new Color(1f, 0.45f, 0.08f), 0.45f),
                    new GradientColorKey(new Color(0.25f, 0.35f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0.6f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = g;

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialLibrary.Particle("nitro_particle", true);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Play();
            return ps;
        }

        private TrailRenderer CreateSkid(Vector3 localPos, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;

            TrailRenderer trail = go.AddComponent<TrailRenderer>();
            trail.time = 3.2f;
            trail.minVertexDistance = 0.35f;
            trail.startWidth = 0.24f;
            trail.endWidth = 0.03f;
            trail.material = MaterialLibrary.Trail();
            trail.startColor = new Color(0.035f, 0.035f, 0.04f, 0.6f);
            trail.endColor = new Color(0.035f, 0.035f, 0.04f, 0f);
            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.alignment = LineAlignment.View;
            trail.textureMode = LineTextureMode.Stretch;
            return trail;
        }

        public void SetDustColor(Color color)
        {
            DustColor = color;
            if (_dustL != null)
            {
                ParticleSystem.MainModule m = _dustL.main;
                m.startColor = color;
            }
            if (_dustR != null)
            {
                ParticleSystem.MainModule m = _dustR.main;
                m.startColor = color;
            }
            if (_skidL != null) _skidL.startColor = new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, 0.55f);
            if (_skidR != null) _skidR.startColor = new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, 0.55f);
        }

        public void SetAIInput(float throttle, float steer, bool nitro, bool handbrake = false)
        {
            ThrottleInput = Mathf.Clamp(throttle, -1f, 1f);
            SteerInput = Mathf.Clamp(steer, -1f, 1f);
            NitroInput = nitro;
            HandbrakeInput = handbrake;
        }

        public void AddNitro(float amount)
        {
            Nitro01 = Mathf.Clamp01(Nitro01 + amount);
        }

        public void SetLocked(bool locked)
        {
            ControlsLocked = locked;
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                if (locked)
                {
                    if (!_rb.isKinematic)
                    {
                        _rb.linearVelocity = Vector3.zero;
                        _rb.angularVelocity = Vector3.zero;
                    }
                    _rb.isKinematic = true;
                }
                else
                {
                    _rb.isKinematic = false;
                }
            }
            if (locked)
            {
                ThrottleInput = 0f;
                SteerInput = 0f;
                HandbrakeInput = false;
                NitroInput = false;
                IsBoosting = false;
            }
        }

        public void ResetToTrack()
        {
            if (Path == null || _rb == null) return;
            int idx = Path.NearestIndex(transform.position, _hint);
            _hint = idx;
            Vector3 p = Path.GetPoint(idx) + Vector3.up * 1.6f;
            Quaternion r = Quaternion.LookRotation(Path.GetDirection(idx), Vector3.up);
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(p, r);
            _rb.position = p;
            _rb.rotation = r;
            _flipTime = 0f;
            _stuckTime = 0f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - _lastCrash < 1.2f) return;
            float v = collision.relativeVelocity.magnitude;
            if (v > 7f)
            {
                _lastCrash = Time.time;
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCrash(v / 30f);
            }
        }
    }
}
