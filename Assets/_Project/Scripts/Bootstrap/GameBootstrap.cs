using SheepStrike.Infrastructure.Levels;
using SheepStrike.Infrastructure.Persistence;
using SheepStrike.Infrastructure.Platform;
using SheepStrike.Presentation.Audio;
using SheepStrike.Presentation.Presenters;
using SheepStrike.Presentation.Theme;
using SheepStrike.Presentation.Views;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;
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
        private const int MenuSortingOrder = 20;
        private const int SplashSortingOrder = 50;

        private GamePresenter _presenter;
        private MenuPresenter _menu;

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
            var sound = CreateChild<SoundBoard>("Sound");
            sound.Init();

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
                    Camera = new CameraRig(camera),
                    Sound = sound
                });

            _menu = CreateMenu(art, hud, sound, progress, catalog, platform);
            _splash = UiFactory.CreateCanvas("Splash", SplashSortingOrder).gameObject.AddComponent<SplashView>();
            _splash.Build(art);
        }

        private SplashView _splash;

        private void Start() => _splash.Play(_menu.ShowHome);

        private void Update() => _presenter.Tick(Time.deltaTime);

        private MenuPresenter CreateMenu(ArtLibrary art, HudView hud, SoundBoard sound, PlayerPrefsProgressStore progress, ProceduralLevelCatalog catalog, IPlatformService platform)
        {
            var root = (RectTransform)UiFactory.CreateCanvas("Menu", MenuSortingOrder).transform;
            MenuPresenter menu = null;
            var views = new MenuPresenter.ViewSet
            {
                Home = new HomeView(root, art),
                Settings = new SettingsPanelView(root, art, UnityEngine.Application.version),
                Collection = new CollectionPanelView(root, art,
                    level => menu.OnCollectionReplay(level),
                    page => menu.OnCollectionPageRequested(page),
                    () => menu.OnCollectionClosed()),
                Leaderboard = new LeaderboardPanelView(root, art),
                AgeTips = new AgeTipsPanelView(root, art, () => menu.CloseAgeTips()),
                Hud = hud,
                Sound = sound
            };
            menu = new MenuPresenter(
                _presenter,
                new ChangeSettings(new PlayerPrefsSettingsStore()),
                new SkipLevel(progress, platform),
                new BrowseCollection(progress, catalog),
                platform,
                views);
            return menu;
        }

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
            if (camera.GetComponent<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();
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
            var canvas = UiFactory.CreateCanvas("Background", -10);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 90f;
            canvas.gameObject.AddComponent<SkyView>().Build(art);
            Object.Destroy(canvas.GetComponent<GraphicRaycaster>());
        }
    }
}
