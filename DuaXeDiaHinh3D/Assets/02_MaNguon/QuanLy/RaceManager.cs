using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuaXe3D
{
    public enum RacePhase
    {
        Intro,
        Countdown,
        Racing,
        Finished
    }

    /// <summary>
    /// Điều khiển một cuộc đua: đếm ngược, thời gian, xếp hạng, về đích,
    /// chuyển sang màn hình kết quả và lưu kỷ lục.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        public RacePhase Phase = RacePhase.Intro;
        public RaceHUD Hud;
        public RacerInfo Player;
        public List<RacerInfo> Racers = new List<RacerInfo>();
        public RoadPath Path;
        public EnvironmentBuildResult Environment;
        public float RaceTime;
        public float BestTime;
        public int PlayerPlace = 1;
        public bool Autotest;

        private ChaseCamera _chase;
        private float _rankTimer;
        private float _autoTimer;
        private bool _finished;
        private bool _paused;
        private bool _resultsShown;
        private readonly List<RacerInfo> _finishOrder = new List<RacerInfo>();

        public bool Paused { get { return _paused; } }

        public static RaceManager Create(Transform parent)
        {
            GameObject go = new GameObject("RaceManager");
            if (parent != null) go.transform.SetParent(parent, false);
            return go.AddComponent<RaceManager>();
        }

        public void Begin(EnvironmentBuildResult environment, RacerInfo player, List<RacerInfo> racers, ChaseCamera chase)
        {
            Instance = this;
            Environment = environment;
            Path = environment.Path;
            Player = player;
            Racers = racers;
            _chase = chase;
            _finishOrder.Clear();
            _finished = false;
            _paused = false;
            _resultsShown = false;
            RaceTime = 0f;
            Phase = RacePhase.Intro;
            PlayerPlace = 1;

            RaceResults.Clear();
            BestTime = GameSession.GetBestTime(environment.Theme.Index);

            for (int i = 0; i < Racers.Count; i++)
            {
                RacerInfo r = Racers[i];
                if (r == null) continue;
                r.ResetRaceState();
                r.Place = i + 1;
                if (r.Controller != null) r.Controller.SetLocked(true);
            }

            Hud = RaceHUD.Create(this);
            Hud.SetBest(BestTime);
            Hud.SetCarName(CarLibrary.Get(player.CarIndex).Name, CharacterLibrary.Get(player.CharacterIndex).Name);
            Hud.ShowIntro(environment.Theme.Name, CarLibrary.Get(player.CarIndex).Name);
            ComputeRanks();

            StartCoroutine(IntroRoutine());
        }

        private IEnumerator IntroRoutine()
        {
            Phase = RacePhase.Intro;
            yield return new WaitForSeconds(2.3f);
            if (Hud != null) Hud.HideIntro();
            if (_chase != null) _chase.EndIntro();
            yield return new WaitForSeconds(0.25f);
            yield return CountdownRoutine();
        }

        private IEnumerator CountdownRoutine()
        {
            Phase = RacePhase.Countdown;
            for (int i = 3; i >= 1; i--)
            {
                if (Hud != null) Hud.ShowCountdown(i.ToString(), i == 1 ? UITheme.Gold : UITheme.Text, i);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCountdownBeep(false);
                yield return new WaitForSeconds(0.92f);
            }

            if (Hud != null) Hud.ShowCountdown("GO!", UITheme.Good, 0);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCountdownBeep(true);

            Phase = RacePhase.Racing;
            RaceTime = 0f;
            for (int i = 0; i < Racers.Count; i++)
            {
                if (Racers[i] != null && Racers[i].Controller != null)
                    Racers[i].Controller.SetLocked(false);
            }

            yield return new WaitForSeconds(1.05f);
            if (Hud != null) Hud.HideCountdown();
        }

        private void Update()
        {
            if (Phase == RacePhase.Finished || Phase == RacePhase.Racing)
                RaceTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape) && Phase != RacePhase.Finished)
                TogglePause(!_paused);

            if (_paused) return;

            if (Phase == RacePhase.Racing)
            {
                if (Input.GetKeyDown(KeyCode.R) && Player != null && Player.Controller != null)
                    Player.Controller.ResetToTrack();

                _rankTimer -= Time.deltaTime;
                if (_rankTimer <= 0f)
                {
                    _rankTimer = 0.2f;
                    ComputeRanks();
                }

                if (Hud != null && Player != null && Player.Controller != null)
                {
                    ArcadeCarController c = Player.Controller;
                    float slip = Mathf.Abs(Vector3.Dot(c.GetComponent<Rigidbody>() != null ? c.GetComponent<Rigidbody>().linearVelocity : Vector3.zero, c.transform.right));
                    bool drifting = c.Grounded && !c.ControlsLocked && (c.HandbrakeInput && Mathf.Abs(c.ForwardSpeed) > 2f || slip > 4.2f);
                    Hud.UpdateHud(RaceTime, BestTime, c.SpeedKmh, c.Nitro01, PlayerPlace,
                        Racers.Count, c.IsBoosting, drifting, c.OffRoad, c.ThrottleInput);
                }

                // Nếu người chơi vượt qua vạch đích mà trigger bị lỡ
                if (!_finished && Player != null && Path != null && Path.Count > 2)
                {
                    float p = Path.Progress01(Player.transform.position, Player.ProgressIndex, out int idx);
                    Player.ProgressIndex = idx;
                    if (p > 0.994f) RacerCrossedFinish(Player);
                }

                if (Autotest && !_finished)
                {
                    _autoTimer += Time.deltaTime;
                    if (_autoTimer > 11.5f)
                    {
                        TeleportPlayerToFinish();
                        RacerCrossedFinish(Player);
                    }
                }
            }
        }

        public void ComputeRanks()
        {
            if (Racers == null || Path == null) return;

            for (int i = 0; i < Racers.Count; i++)
            {
                RacerInfo r = Racers[i];
                if (r == null) continue;
                if (r.Finished)
                {
                    r.Progress = 1f;
                    continue;
                }
                int idx = Path.NearestIndex(r.transform.position, r.ProgressIndex);
                r.ProgressIndex = idx;
                r.Progress = Path.Count > 1 ? idx / (float)(Path.Count - 1) : 0f;
            }

            List<RacerInfo> sorted = new List<RacerInfo>(Racers);
            sorted.Sort(CompareRacers);
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i] != null) sorted[i].Place = i + 1;
            }
            if (Player != null) PlayerPlace = Mathf.Max(1, Player.Place);
        }

        private static int CompareRacers(RacerInfo a, RacerInfo b)
        {
            if (a == null) return 1;
            if (b == null) return -1;
            if (a.Finished && b.Finished) return a.FinishTime.CompareTo(b.FinishTime);
            if (a.Finished) return -1;
            if (b.Finished) return 1;
            return b.Progress.CompareTo(a.Progress);
        }

        public void RacerCrossedFinish(RacerInfo info)
        {
            if (info == null || info.Finished) return;
            if (Phase != RacePhase.Racing && Phase != RacePhase.Finished) return;

            info.Finished = true;
            info.FinishTime = RaceTime;
            _finishOrder.Add(info);
            ComputeRanks();

            if (info.IsPlayer && !_finished)
            {
                FinishPlayer();
            }
        }

        private void FinishPlayer()
        {
            _finished = true;
            Phase = RacePhase.Finished;
            Player.Controller.SetLocked(true);

            bool newRecord = !Autotest && GameSession.SubmitTime(Environment.Theme.Index, Player.FinishTime);
            BestTime = GameSession.GetBestTime(Environment.Theme.Index);

            if (Hud != null)
                Hud.ShowFinish(Player.Place == 1, Player.Place, Player.FinishTime, BestTime, newRecord);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayVictory();

            StartCoroutine(FinishDelayRoutine(6.2f));
        }

        /// <summary>Đưa xe về sát vạch đích trong chế độ tự kiểm thử để ảnh chụp có cổng đích.</summary>
        private void TeleportPlayerToFinish()
        {
            if (Player == null || Path == null || Path.Count < 4) return;

            float d = Mathf.Max(4f, Path.Length - 22f);
            Vector3 p = Path.GetLanePoint(d, 0f) + Vector3.up * 1.2f;
            Quaternion r = Quaternion.LookRotation(Path.GetDirectionAtDistance(d), Vector3.up);

            Rigidbody rb = Player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = p;
                rb.rotation = r;
            }
            Player.transform.SetPositionAndRotation(p, r);
            Player.ProgressIndex = Path.NearestIndex(p, Player.ProgressIndex);
            if (_chase != null) _chase.SnapBehind();
        }

        private IEnumerator FinishDelayRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            GoToResults();
        }

        public void GoToResults()
        {
            if (_resultsShown) return;
            _resultsShown = true;
            PopulateResults();
            SceneFlow.Load(SceneFlow.Results);
        }

        private void PopulateResults()
        {
            ComputeRanks();
            RaceResults.Clear();
            RaceResults.HasData = true;
            RaceResults.TerrainIndex = Environment != null ? Environment.Theme.Index : 0;
            RaceResults.PlayerPlace = Player != null ? Player.Place : 1;
            RaceResults.PlayerTime = Player != null ? Player.FinishTime : 0f;

            // Khi người chơi về đích, ước lượng thời gian cho các AI còn trên đường
            // để bảng xếp hạng đầy đủ thay vì hiển thị dấu "—".
            float baseTime = RaceResults.PlayerTime;
            float[] gaps = { 1.3f, 2.7f, 4.1f, 5.5f };
            int gapIndex = 0;
            for (int i = 0; i < Racers.Count; i++)
            {
                RacerInfo r = Racers[i];
                if (r == null || r.Finished) continue;
                r.Finished = true;
                r.FinishTime = baseTime + gaps[Mathf.Min(gapIndex, gaps.Length - 1)];
                gapIndex++;
            }
            ComputeRanks();

            RaceResults.PlayerPlace = Player != null ? Player.Place : 1;
            RaceResults.NewRecord = Player != null && Player.Finished &&
                Mathf.Approximately(RaceResults.PlayerTime, GameSession.GetBestTime(RaceResults.TerrainIndex));

            List<RacerInfo> sorted = new List<RacerInfo>(Racers);
            sorted.Sort((a, b) =>
            {
                if (a == null) return 1;
                if (b == null) return -1;
                return a.Place.CompareTo(b.Place);
            });

            for (int i = 0; i < sorted.Count; i++)
            {
                RacerInfo r = sorted[i];
                if (r == null) continue;
                CarSpec car = CarLibrary.Get(r.CarIndex);
                CharacterSpec ch = CharacterLibrary.Get(r.CharacterIndex);
                RaceResults.Entries.Add(new RaceResultEntry
                {
                    Name = r.IsPlayer ? "BẠN" : r.DisplayName,
                    CarName = car.Name,
                    CharacterName = ch.Name,
                    CarIndex = r.CarIndex,
                    CharacterIndex = r.CharacterIndex,
                    Time = r.Finished ? r.FinishTime : 0f,
                    IsPlayer = r.IsPlayer,
                    Finished = r.Finished,
                    Place = r.Place
                });
            }
        }

        public void TogglePause(bool pause)
        {
            if (Phase == RacePhase.Finished) return;
            _paused = pause;
            Time.timeScale = pause ? 0f : 1f;
            AudioListener.pause = pause;
            if (Hud != null) Hud.ShowPause(pause);
        }

        public void RestartRace()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneFlow.Load(SceneFlow.Race);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
