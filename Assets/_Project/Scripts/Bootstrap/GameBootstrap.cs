using SheepStrike.Infrastructure.Levels;
using SheepStrike.Infrastructure.Persistence;
using SheepStrike.Infrastructure.Platform;
using SheepStrike.Presentation.Presenters;
using SheepStrike.Presentation.Theme;
using SheepStrike.Presentation.Views;
using SheepStrike.UseCases.Interactors;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Bootstrap
{
    /// <summary>
    /// Composition root: the only place that knows every concrete class. It builds the scene objects,
    /// wires infrastructure into use cases and hands them to the presenter.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int MaxSlots = 7;

        private GamePresenter _presenter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (FindObjectOfType<GameBootstrap>() == null) new GameObject(nameof(GameBootstrap)).AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            UnityEngine.Application.targetFrameRate = 60;

            var platform = PlatformServiceFactory.Create();
            platform.Initialize();
            var progress = new PlayerPrefsProgressStore();
            var catalog = new ProceduralLevelCatalog();
            var context = new PlayContext();

            var art = new ArtLibrary();
            var kit = new RenderKit();
            var camera = CreateCamera();
            CreateLight();
            CreateBackground(art, camera);

            var model = CreateChild<VoxelModelView>("VoxelModel");
            model.Init(kit);
            var projectiles = CreateChild<WoolProjectiles>("WoolProjectiles");
            projectiles.Init(kit);
            var debris = CreateChild<DebrisBurst>("Debris");
            debris.Init(kit);
            var rotator = CreateChild<ModelRotator>("ModelRotator");

            UiFactory.EnsureEventSystem();
            var hud = UiFactory.CreateCanvas("Hud", 10).gameObject.AddComponent<HudView>();
            hud.Build(art, MaxSlots);

            _presenter = new GamePresenter(
                context,
                new StartLevel(catalog, progress, context),
                new DeploySheep(context, platform),
                new UseBooster(context, platform),
                new FinishLevel(context, progress, platform),
                platform,
                new GamePresenter.ViewSet
                {
                    Hud = hud,
                    Model = model,
                    Rotator = rotator,
                    Projectiles = projectiles,
                    Debris = debris,
                    Camera = new CameraRig(camera)
                });
        }

        private void Start() => _presenter.BeginFromProgress();

        private void Update() => _presenter.Tick(Time.deltaTime);

        private T CreateChild<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        private static Camera CreateCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Background;
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.1f;
            return camera;
        }

        private static void CreateLight()
        {
            if (FindObjectOfType<Light>() != null) return;
            var light = new GameObject("Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            light.shadows = LightShadows.None;
        }

        private static void CreateBackground(ArtLibrary art, Camera camera)
        {
            if (art.Background == null) return;
            var canvas = UiFactory.CreateCanvas("Background", -10);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 90f;
            var image = UiFactory.AddImage(UiFactory.Stretch("Sky", canvas.transform), art.Background, Color.white);
            image.preserveAspect = false;
            Object.Destroy(canvas.GetComponent<GraphicRaycaster>());
        }
    }
}
