using SheepStrike.UseCases.Ports;

namespace SheepStrike.Infrastructure.Platform
{
    /// <summary>Picks the platform adapter from scripting define symbols set by each mini-game SDK.</summary>
    public static class PlatformServiceFactory
    {
        public static IPlatformService Create()
        {
#if WEIXINMINIGAME && !UNITY_EDITOR
            return new WeChatPlatformService();
#elif DOUYINMINIGAME && !UNITY_EDITOR
            return new DouyinPlatformService();
#elif UNITY_ANDROID && !UNITY_EDITOR && !DOUYINMINIGAME && !WEIXINMINIGAME
            return new AndroidPlatformService();
#else
            return new EditorPlatformService();
#endif
        }
    }
}
