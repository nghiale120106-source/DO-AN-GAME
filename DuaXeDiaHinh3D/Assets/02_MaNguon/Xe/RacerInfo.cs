using UnityEngine;

namespace DuaXe3D
{
    public class RacerInfo : MonoBehaviour
    {
        public int CarIndex;
        public int CharacterIndex;
        public string DisplayName;
        public bool IsPlayer;
        public ArcadeCarController Controller;
        public CarVisual Visual;
        public RoadPath Path;

        public bool Finished;
        public float FinishTime;
        public int Place;
        public float Progress;
        public int ProgressIndex;
        public float LastProgressTime;

        public void ResetRaceState()
        {
            Finished = false;
            FinishTime = 0f;
            Place = 0;
            Progress = 0f;
            ProgressIndex = 0;
            LastProgressTime = Time.time;
        }
    }

    /// <summary>Xe máy tính bám theo đường đua bằng waypoint.</summary>
    public class AIDriver : MonoBehaviour
    {
        public RoadPath Path;
        public ArcadeCarController Controller;
        public float Skill = 0.94f;
        public float LaneOffset;
        public float RubberBand;
        public bool Active = true;

        private int _hint = -1;

        private void Start()
        {
            if (Controller == null) Controller = GetComponent<ArcadeCarController>();
            if (Path == null && Controller != null) Path = Controller.Path;
        }

        private void Update()
        {
            if (!Active || Controller == null || Path == null) return;

            if (Controller.ControlsLocked)
            {
                Controller.SetAIInput(0f, 0f, false);
                return;
            }

            int idx = Path.NearestIndex(transform.position, _hint);
            _hint = idx;

            float speed = Mathf.Abs(Controller.ForwardSpeed);
            float lookAhead = Mathf.Clamp(9f + speed * 0.85f, 11f, 42f);
            float dist = Path.DistanceFromIndex(idx) + lookAhead;

            Vector3 target = Path.GetLanePoint(dist, LaneOffset);
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.01f) toTarget = transform.forward;

            float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
            float steer = Mathf.Clamp(angle / 38f, -1f, 1f);

            // Nhìn xa hơn để giảm tốc trước khúc cua gắt
            Vector3 dirNow = Path.GetDirection(idx);
            Vector3 dirAhead = Path.GetDirectionAtDistance(Path.DistanceFromIndex(idx) + 28f);
            float curve = Vector3.Angle(dirNow, dirAhead);

            float throttle = 1f;
            if (Mathf.Abs(angle) > 55f) throttle = 0.35f;
            else if (Mathf.Abs(angle) > 30f) throttle = 0.62f;
            else if (curve > 45f) throttle = 0.72f;
            else if (curve > 25f) throttle = 0.86f;

            float skillFactor = Mathf.Clamp(Skill + RubberBand * 0.07f, 0.75f, 1.05f);
            throttle *= skillFactor;

            bool straight = Mathf.Abs(angle) < 9f && curve < 18f;
            bool nitro = straight && speed > 13f && Controller.Nitro01 > 0.28f;

            Controller.SetAIInput(throttle, steer, nitro, false);
        }
    }

    /// <summary>Camera bám theo xe, có nội suy mượt + rung nhẹ khi nitro (Yêu cầu 9).</summary>
    public class ChaseCamera : MonoBehaviour
    {
        public Transform Target;
        public ArcadeCarController Car;
        public bool IntroMode;
        public float Distance = 10.5f;
        public float Height = 4.3f;
        public float LookAhead = 4.5f;

        private Vector3 _velocity;
        private float _introT;

        private void LateUpdate()
        {
            if (Target == null) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            float speed01 = Car != null ? Car.Speed01 : 0f;

            if (IntroMode)
            {
                _introT += dt * 0.55f;
                // Góc máy 3/4 cố định phía sau - bên phải, cao và xa hơn để không cắt vào xe AI.
                float sway = Mathf.Sin(_introT) * 1.4f;
                Vector3 orbit = Target.position
                    - Target.forward * 9.5f
                    + Target.right * (7.5f + sway)
                    + Vector3.up * 5.8f;
                transform.position = Vector3.Lerp(transform.position, orbit, 1f - Mathf.Exp(-5f * dt));
                Vector3 look = Target.position + Vector3.up * 1.15f;
                Quaternion r = Quaternion.LookRotation(look - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, r, 1f - Mathf.Exp(-7f * dt));

                Camera camIntro = GetComponent<Camera>();
                if (camIntro != null) camIntro.fieldOfView = Mathf.Lerp(camIntro.fieldOfView, 48f, 1f - Mathf.Exp(-3f * dt));
                return;
            }

            float distance = Distance + speed01 * 2.2f;
            float height = Height + speed01 * 0.55f;
            Vector3 desired = Target.position - Target.forward * distance + Vector3.up * height;

            // Không để camera xuyên địa hình
            RaycastHit hit;
            Vector3 pivot = Target.position + Vector3.up * 1.6f;
            if (Physics.Linecast(pivot, desired, out hit, SceneSetup.GroundMask, QueryTriggerInteraction.Ignore))
            {
                desired = hit.point + hit.normal * 0.7f;
            }

            RaycastHit groundHit;
            if (Physics.Raycast(desired + Vector3.up * 18f, Vector3.down, out groundHit, 45f, SceneSetup.GroundMask, QueryTriggerInteraction.Ignore))
            {
                if (desired.y < groundHit.point.y + 0.8f)
                    desired.y = groundHit.point.y + 0.8f;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 0.13f);
            if (Car != null && Car.IsBoosting)
            {
                transform.position += Random.insideUnitSphere * 0.055f;
            }

            Vector3 lookAt = Target.position + Target.forward * (LookAhead + speed01 * 5f) + Vector3.up * 1.15f;
            Quaternion rot = Quaternion.LookRotation(lookAt - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-9f * dt));

            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                float targetFov = Mathf.Lerp(61f, 76f, speed01) + (Car != null && Car.IsBoosting ? 4f : 0f);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 1f - Mathf.Exp(-4f * dt));
            }
        }

        public void EndIntro()
        {
            IntroMode = false;
        }

        /// <summary>Đặt camera ngay sau xe (dùng khi teleport / bắt đầu lại).</summary>
        public void SnapBehind()
        {
            if (Target == null) return;
            Vector3 desired = Target.position - Target.forward * Distance + Vector3.up * Height;
            transform.position = desired;
            Vector3 look = Target.position + Target.forward * LookAhead + Vector3.up * 1.15f;
            transform.rotation = Quaternion.LookRotation(look - desired, Vector3.up);
            _velocity = Vector3.zero;
        }
    }

    /// <summary>Camera xoay quanh xe / sân trưng bày.</summary>
    public class OrbitCamera : MonoBehaviour
    {
        public Vector3 Target = new Vector3(0f, 1.2f, 0f);
        public float Distance = 11f;
        public float Pitch = 14f;
        public float Yaw = 195f;
        public float AutoOrbitSpeed = 2.5f;
        public bool AllowDrag = true;
        public float DragSpeed = 4f;

        private Vector3 _targetVelocity;
        private Vector3 _targetGoal;

        private void Start()
        {
            _targetGoal = Target;
        }

        public void SetTarget(Vector3 target, bool snap = false)
        {
            _targetGoal = target;
            if (snap)
            {
                Target = target;
                _targetVelocity = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            Target = Vector3.SmoothDamp(Target, _targetGoal, ref _targetVelocity, 0.4f);

            if (AllowDrag && Input.GetMouseButton(0))
            {
                Yaw += Input.GetAxis("Mouse X") * DragSpeed;
                Pitch = Mathf.Clamp(Pitch - Input.GetAxis("Mouse Y") * DragSpeed * 0.7f, 2f, 42f);
            }
            else
            {
                Yaw += AutoOrbitSpeed * dt;
            }

            Quaternion rot = Quaternion.Euler(Pitch, Yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -Distance);
            transform.position = Target + offset;
            transform.rotation = Quaternion.LookRotation(Target - transform.position, Vector3.up);
        }
    }

    /// <summary>Bình nitro nhặt trên đường.</summary>
    public class NitroPickup : MonoBehaviour
    {
        public float Cooldown = 12f;

        private float _timer;
        private Transform _visual;
        private SphereCollider _trigger;
        private Vector3 _basePos;

        public static NitroPickup Create(Transform parent, Vector3 position, Material glowMaterial)
        {
            GameObject root = new GameObject("NitroPickup");
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            NitroPickup pickup = root.AddComponent<NitroPickup>();
            pickup._basePos = position;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            pickup._visual = visual.transform;

            MeshKit.Sphere("Core", visual.transform, Vector3.zero, Vector3.one * 0.9f, glowMaterial);
            MeshKit.Sphere("Halo", visual.transform, Vector3.zero, Vector3.one * 1.6f,
                MaterialLibrary.Standard("nitro_halo", new Color(0.1f, 0.8f, 1f, 0.18f), 0f, 0.9f));

            GameObject ring = MeshKit.MeshObject("Ring", MeshKit.Ring(1.4f, 1.8f, 32),
                MaterialLibrary.Emissive("nitro_ring", new Color(0.2f, 0.9f, 1f), 2.5f),
                visual.transform, Vector3.zero, Quaternion.identity, Vector3.one);
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.radius = 3.2f;
            trigger.isTrigger = true;
            pickup._trigger = trigger;

            Light light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.15f, 0.85f, 1f);
            light.range = 9f;
            light.intensity = 2.2f;
            light.shadows = LightShadows.None;

            return pickup;
        }

        private void Update()
        {
            if (_visual != null)
            {
                _visual.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
                float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.08f;
                _visual.localScale = Vector3.one * pulse;
            }

            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                {
                    if (_visual != null) _visual.gameObject.SetActive(true);
                    if (_trigger != null) _trigger.enabled = true;
                }
            }
            else
            {
                transform.position = _basePos + Vector3.up * (Mathf.Sin(Time.time * 1.8f) * 0.22f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_timer > 0f) return;
            RacerInfo info = other.GetComponentInParent<RacerInfo>();
            if (info == null || !info.IsPlayer || info.Controller == null) return;

            info.Controller.AddNitro(0.55f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();

            _timer = Cooldown;
            if (_visual != null) _visual.gameObject.SetActive(false);
            if (_trigger != null) _trigger.enabled = false;
        }
    }

    public class FinishLine : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            RacerInfo info = other.GetComponentInParent<RacerInfo>();
            if (info != null && RaceManager.Instance != null)
                RaceManager.Instance.RacerCrossedFinish(info);
        }
    }
}
