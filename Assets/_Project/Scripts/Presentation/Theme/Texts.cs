using SheepStrike.Domain.Session;
using UnityEngine;

namespace SheepStrike.Presentation.Theme
{
    /// <summary>All player-facing Chinese copy in one place.</summary>
    public static class Texts
    {
        public const string GameTitle = "羊羊大反击";
        public const string Next = "下一关";
        public const string Retry = "再来一次";
        public const string Share = "叫朋友来帮忙";
        public const string GiveUp = "放弃";
        public const string StuckTitle = "羊圈挤满啦！";
        public const string StuckBody = "场上的羊都打不到目标\n扩建羊圈继续战斗？";
        public const string Rescue = "看视频扩羊圈";
        public const string NotNow = "现在用不了哦";
        public const string AdNotFinished = "看完视频才能获得奖励";
        public const string PadFull = "羊圈满了，先等等";
        public const string FrontOnly = "只能派出最前排的羊";
        public const string DogReady = "牧羊犬就位：可以派出任意一只羊";
        public const string RotateHint = "拖动旋转";
        public const string RemainingLabel = "剩余数量";
        public const string AdBadge = "视频";
        public const string Unlock = "解锁";
        public const string Assisted = "羊群士气大增：这次一定能赢！";
        public const string Settings = "设置";
        public const string Music = "音乐";
        public const string Sound = "音效";
        public const string Vibration = "震动";
        public const string ComboDisplay = "连击显示";
        public const string Home = "主界面";
        public const string Restart = "重新开始";
        public const string SkipLevel = "跳过关卡";
        public const string Continue = "继续游戏";
        public const string Collection = "图鉴";
        public const string Replay = "重玩";
        public const string PreviousPage = "上一页";
        public const string NextPage = "下一页";
        public const string StartGame = "开始游戏";
        public const string Leaderboard = "排行榜";
        public const string AddToDesktop = "添加桌面";
        public const string DesktopUnavailable = "请在抖音或微信中添加到桌面";
        public const string Close = "×";
        public const string Locked = "?";
        public const string FriendsTab = "好友榜";
        public const string GlobalTab = "总榜";
        public const string Me = "我";
        public const string MyBest = "我的最佳纪录";
        public const string LeaderboardOffline = "发布到抖音后可查看好友和全国排行";

        public static string Level(int number) => $"第 {number} 关";
        public static string Unlocked(int count) => $"已解锁({count})";
        public static string LevelsReached(int level) => $"{level}关";
        public static string PageNumber(int page) => $"第 {page} 页";
        public static string Version(string version) => $"V{version}";
        public static string Remaining(int count) => count.ToString();
        public static string Combo(int combo) => $"连击 x {combo}";

        /// <summary>Banner shown when the combo reaches tier 1..ComboTier.Count.</summary>
        public static string ComboBanner(int tier)
        {
            switch (tier)
            {
                case 1: return "不错哦！";
                case 2: return "太棒啦！";
                case 3: return "势不可挡！";
                default: return "羊神降临！";
            }
        }
        public static string Timer(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }

        public static string Won(string animal, int bestCombo) => $"胜利！\n{animal}被羊群赶跑了\n最高连击 {bestCombo}";
        public static string Lost(string animal) => $"失败了…\n{animal}还在虎视眈眈";

        public static string Animal(string targetId)
        {
            switch (targetId)
            {
                case "wolf": return "大灰狼";
                case "fox": return "狡猾狐狸";
                case "bear": return "贪吃棕熊";
                case "boar": return "野猪大王";
                case "alpha_wolf": return "狼王";
                default: return targetId;
            }
        }

        public static string Booster(BoosterType booster)
        {
            switch (booster)
            {
                case BoosterType.RamCharge: return "羊角冲锋";
                case BoosterType.ExtraPen: return "扩羊圈";
                case BoosterType.ShepherdDog: return "牧羊犬";
                case BoosterType.Shuffle: return "赶羊群";
                default: return booster.ToString();
            }
        }
    }
}
