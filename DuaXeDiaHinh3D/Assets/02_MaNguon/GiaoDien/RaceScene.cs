using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuaXe3D
{
    /// <summary>Màn hình 05: dựng địa hình, sinh 4 tay đua và bắt đầu cuộc đua.</summary>
    public class RaceScene : MonoBehaviour
    {
        private void Start()
        {
            SceneSetup.Configure();
            GameSession.Load();
            StartCoroutine(BuildRoutine());
        }

        private IEnumerator BuildRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRaceMusic();

            GameObject world = new GameObject("World");
            world.transform.SetParent(transform, false);

            EnvironmentBuildResult env = EnvironmentBuilder.Build(GameSession.TerrainIndex, world.transform, false, true);
            TerrainTheme theme = env.Theme;

            List<RacerInfo> racers = new List<RacerInfo>();

            CarSpec playerCar = CarLibrary.Get(GameSession.CarIndex);
            CharacterSpec playerCharacter = CharacterLibrary.Get(GameSession.CharacterIndex);

            Vector3 pos;
            Quaternion rot;
            EnvironmentBuilder.GetGridSlot(env.Path, 0, out pos, out rot);

            RacerInfo player = CarFactory.SpawnRaceCar(playerCar, playerCharacter, pos, rot, true, "BẠN", env.Path);
            player.Controller.SurfaceGripMultiplier = theme.SurfaceGrip;
            player.Controller.SetDustColor(new Color(theme.DirtColor.r, theme.DirtColor.g, theme.DirtColor.b, 0.58f));
            racers.Add(player);

            int[] aiCars = { (GameSession.CarIndex + 1) % 3, (GameSession.CarIndex + 2) % 3, (GameSession.CarIndex + 1) % 3 };
            int[] aiChars = { (GameSession.CharacterIndex + 1) % 3, (GameSession.CharacterIndex + 2) % 3, GameSession.CharacterIndex };
            float[] skills = { 0.955f, 0.93f, 0.905f };
            float[] lanes = { -1.6f, 1.7f, 0.3f };

            for (int i = 0; i < 3; i++)
            {
                CarSpec car = CarLibrary.Get(aiCars[i]);
                CharacterSpec ch = CharacterLibrary.Get(aiChars[i]);
                EnvironmentBuilder.GetGridSlot(env.Path, i + 1, out pos, out rot);

                RacerInfo ai = CarFactory.SpawnRaceCar(car, ch, pos, rot, false, ch.Nickname, env.Path);
                ai.Controller.SurfaceGripMultiplier = theme.SurfaceGrip;
                ai.Controller.SetDustColor(new Color(theme.DirtColor.r, theme.DirtColor.g, theme.DirtColor.b, 0.52f));

                AIDriver driver = ai.gameObject.AddComponent<AIDriver>();
                driver.Path = env.Path;
                driver.Controller = ai.Controller;
                driver.Skill = skills[i];
                driver.LaneOffset = lanes[i];
                racers.Add(ai);
            }

            Vector3 camPos = player.transform.position - player.transform.forward * 9.2f + Vector3.up * 4.1f;
            Camera cam = SceneSetup.CreateMainCamera(camPos,
                Quaternion.LookRotation(player.transform.forward, Vector3.up), 60f);
            cam.farClipPlane = 1700f;
            cam.clearFlags = CameraClearFlags.Skybox;

            CameraPostFX fx = cam.GetComponent<CameraPostFX>();
            if (fx != null)
            {
                fx.vignette = 0.55f;
                fx.saturation = 1.08f;
                fx.contrast = 1.05f;
                fx.grain = 0.008f;
            }

            ChaseCamera chase = cam.gameObject.AddComponent<ChaseCamera>();
            chase.Target = player.transform;
            chase.Car = player.Controller;
            chase.IntroMode = true;

            RaceManager race = RaceManager.Create(transform);
            race.Autotest = AutoTestRunner.Enabled;

            if (AutoTestRunner.Enabled)
            {
                player.Controller.ExternalControl = true;
                AIDriver auto = player.gameObject.AddComponent<AIDriver>();
                auto.Path = env.Path;
                auto.Controller = player.Controller;
                auto.Skill = 0.99f;
                auto.LaneOffset = 0f;
            }

            race.Begin(env, player, racers, chase);

            yield return null;
            ScreenFade.FadeIn(0.7f);
            AutoTestRunner.MarkReady();
        }
    }
}
