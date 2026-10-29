using System;
using System.Collections.Generic;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>One line of the leaderboard as the presenter wants it shown.</summary>
    public readonly struct LeaderboardRow
    {
        public LeaderboardRow(int rank, string name, int level, bool isPlayer)
        {
            Rank = rank;
            Name = name;
            Level = level;
            IsPlayer = isPlayer;
        }

        public int Rank { get; }

        public string Name { get; }

        public int Level { get; }

        public bool IsPlayer { get; }
    }

    /// <summary>
    /// The rank sheet (排行榜), styled like the native WeChat / Douyin one: a white bottom sheet over a dimmed
    /// backdrop with a blue header and title, a round close button, friends / global tabs, up to six rows
    /// (medal or number, avatar disc, name and levels reached) and a note line at the bottom.
    /// </summary>
    public sealed class LeaderboardPanelView
    {
        public const int MaxRows = 6;

        private const float SheetWidth = 1080f;
        private const float SheetHeight = 1160f;
        /// <summary>The sheet runs on below the screen edge so only its top corners look rounded.</summary>
        private const float BottomBleed = 200f;
        /// <summary>Halves the pixels per unit of the 9-slice so corners get twice the default radius.</summary>
        private const float CornerScale = 0.5f;
        private const float HeaderHeight = 280f;
        private const float TitleInset = 70f;
        private const int TitleFontSize = 96;
        private const float CloseSize = 88f;
        private const float CloseInset = 70f;
        private const int CloseFontSize = 72;

        private const float TabHeight = 110f;
        private const float TabWidth = 220f;
        private const float FirstTabX = 60f;
        private const int TabFontSize = 48;
        private static readonly Vector2 UnderlineSize = new Vector2(56f, 8f);
        private const float UnderlineBottom = 12f;
        /// <summary>Shrinks the 9-slice corners to half the bar height so the thin underline is a pill.</summary>
        private const float UnderlineCornerScale = 7.5f;
        private const float SeparatorHeight = 2f;

        private const float RowHeight = 112f;
        private const float NoteHeight = 90f;
        private const int NoteFontSize = 30;
        private const float NoteSideInset = 60f;

        private static readonly Color HeaderBlue = new Color32(0x5B, 0x8C, 0xF5, 0xFF);
        private static readonly Color CloseDisc = new Color(0f, 0f, 0f, 0.25f);
        private static readonly Color Separator = new Color32(0xEE, 0xEF, 0xF2, 0xFF);

        private readonly RectTransform _root;
        private readonly RectTransform _sheet;
        private readonly Tab _friendsTab;
        private readonly Tab _globalTab;
        private readonly List<LeaderboardRowView> _rows = new List<LeaderboardRowView>();
        private readonly Text _note;

        public Action CloseTapped;
        /// <summary>True for the friends tab, false for the global tab.</summary>
        public Action<bool> TabTapped;

        public LeaderboardPanelView(RectTransform parent, ArtLibrary art)
        {
            _root = UiFactory.Stretch("Leaderboard", parent);
            UiFactory.AddImage(_root, null, Palette.Dim, true);
            _sheet = BuildSheet(art);
            BuildHeader(art);

            _friendsTab = BuildTab(art, Texts.FriendsTab, 0, true);
            _globalTab = BuildTab(art, Texts.GlobalTab, 1, false);
            BuildSeparator();

            float listTop = -(HeaderHeight + TabHeight);
            for (int i = 0; i < MaxRows; i++) _rows.Add(new LeaderboardRowView(_sheet, art, listTop - i * RowHeight, RowHeight));
            _note = BuildNote(art);

            SelectTab(true);
            _root.gameObject.SetActive(false);
            _backdrop = UiTransition.On(_root, UiTransition.Motion.Fade);
            _sheetMotion = UiTransition.On(_sheet, UiTransition.Motion.FromBottom);
        }

        private readonly UiTransition _backdrop;
        private readonly UiTransition _sheetMotion;

        public bool IsOpen => _backdrop.IsShown;

        public void Show()
        {
            _root.SetAsLastSibling();
            _backdrop.Show();
            _sheetMotion.Show();
        }

        public void Hide() => _backdrop.Hide();

        /// <param name="rows">Shown in order; anything past <see cref="MaxRows"/> is ignored.</param>
        public void Render(bool friendsTab, IReadOnlyList<LeaderboardRow> rows, string note)
        {
            SelectTab(friendsTab);
            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < rows.Count;
                _rows[i].SetVisible(used);
                if (used) _rows[i].Fill(rows[i]);
            }
            _note.text = note ?? string.Empty;
        }

        private RectTransform BuildSheet(ArtLibrary art)
        {
            var sheet = UiFactory.Rect("Sheet", _root, new Vector2(0.5f, 0f), new Vector2(0f, -BottomBleed), new Vector2(SheetWidth, SheetHeight + BottomBleed));
            sheet.pivot = new Vector2(0.5f, 0f);
            UiFactory.AddBox(sheet, art.FlatBox, Color.white, true).pixelsPerUnitMultiplier = CornerScale;
            return sheet;
        }

        private void BuildHeader(ArtLibrary art)
        {
            var header = UiFactory.Rect("Header", _sheet, new Vector2(0.5f, 1f), new Vector2(0f, -HeaderHeight * 0.5f), new Vector2(SheetWidth, HeaderHeight));
            UiFactory.AddBox(header, art.FlatBox, HeaderBlue).pixelsPerUnitMultiplier = CornerScale;
            var squareBottom = UiFactory.Rect("SquareBottom", header, new Vector2(0.5f, 0f), new Vector2(0f, HeaderHeight * 0.25f), new Vector2(SheetWidth, HeaderHeight * 0.5f));
            UiFactory.AddImage(squareBottom, null, HeaderBlue);

            var titleRect = UiFactory.Stretch("Title", header);
            titleRect.offsetMin = new Vector2(TitleInset, 0f);
            var title = UiFactory.AddLabel(titleRect, art.Font, TitleFontSize, TextAnchor.MiddleLeft);
            title.text = Texts.Leaderboard;
            RemoveOutlines(title);

            BuildCloseButton(art, header);
        }

        private void BuildCloseButton(ArtLibrary art, RectTransform header)
        {
            var close = UiFactory.Rect("Close", header, new Vector2(1f, 1f), new Vector2(-CloseInset, -CloseInset), Vector2.one * CloseSize);
            UiFactory.AddImage(close, art.Disc, CloseDisc);
            var cross = UiFactory.AddLabel(UiFactory.Stretch("Cross", close), art.Font, CloseFontSize, TextAnchor.MiddleCenter);
            cross.text = Texts.Close;
            RemoveOutlines(cross);
            UiFactory.AddButton(close, () => CloseTapped?.Invoke());
        }

        private Tab BuildTab(ArtLibrary art, string label, int index, bool isFriends)
        {
            var position = new Vector2(FirstTabX + TabWidth * (index + 0.5f), -HeaderHeight - TabHeight * 0.5f);
            var rect = UiFactory.Rect(isFriends ? "FriendsTab" : "GlobalTab", _sheet, new Vector2(0f, 1f), position, new Vector2(TabWidth, TabHeight));
            var text = UiFactory.AddLabel(UiFactory.Stretch("Label", rect), art.Font, TabFontSize, TextAnchor.MiddleCenter);
            text.text = label;
            RemoveOutlines(text);

            var underline = UiFactory.Rect("Underline", rect, new Vector2(0.5f, 0f), new Vector2(0f, UnderlineBottom), UnderlineSize);
            UiFactory.AddBox(underline, art.FlatBox, HeaderBlue).pixelsPerUnitMultiplier = UnderlineCornerScale;
            UiFactory.AddButton(rect, () => OnTabTapped(isFriends));
            return new Tab(text, underline.gameObject);
        }

        private void BuildSeparator()
        {
            var line = UiFactory.Rect("Separator", _sheet, new Vector2(0.5f, 1f), new Vector2(0f, -HeaderHeight - TabHeight), new Vector2(SheetWidth, SeparatorHeight));
            UiFactory.AddImage(line, null, Separator);
        }

        private Text BuildNote(ArtLibrary art)
        {
            var rect = UiFactory.Rect("Note", _sheet, new Vector2(0.5f, 0f), new Vector2(0f, BottomBleed + NoteHeight * 0.5f), new Vector2(SheetWidth - NoteSideInset * 2f, NoteHeight));
            var note = UiFactory.AddLabel(rect, art.Font, NoteFontSize, TextAnchor.MiddleCenter, LeaderboardRowView.GreyText);
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            RemoveOutlines(note);
            return note;
        }

        private void OnTabTapped(bool friends)
        {
            SelectTab(friends);
            TabTapped?.Invoke(friends);
        }

        private void SelectTab(bool friends)
        {
            _friendsTab.SetSelected(friends);
            _globalTab.SetSelected(!friends);
        }

        internal static void RemoveOutlines(Text text)
        {
            foreach (var outline in text.GetComponents<Outline>()) UnityEngine.Object.Destroy(outline);
        }

        private sealed class Tab
        {
            private readonly Text _label;
            private readonly GameObject _underline;

            public Tab(Text label, GameObject underline)
            {
                _label = label;
                _underline = underline;
            }

            public void SetSelected(bool selected)
            {
                _label.color = selected ? LeaderboardRowView.DarkText : LeaderboardRowView.GreyText;
                _underline.SetActive(selected);
            }
        }
    }

    /// <summary>One leaderboard line: medal or rank number, avatar disc with an initial, name and levels reached.</summary>
    internal sealed class LeaderboardRowView
    {
        public static readonly Color DarkText = new Color32(0x1F, 0x23, 0x29, 0xFF);
        public static readonly Color GreyText = new Color32(0x8A, 0x8F, 0x99, 0xFF);

        private const int MedalCount = 3;
        private const float RankX = -440f;
        private const float MedalSize = 68f;
        private const int MedalFontSize = 38;
        private const int RankFontSize = 44;
        private const float AvatarX = -330f;
        private const float AvatarSize = 88f;
        private const int AvatarFontSize = 44;
        private const float NameLeft = 270f;
        private const float NameWidth = 560f;
        private const float NameOffsetY = 20f;
        private const float LevelOffsetY = -26f;
        private const int NameFontSize = 44;
        private const int LevelFontSize = 32;
        private const float MeX = 430f;
        private static readonly Vector2 MeSize = new Vector2(76f, 48f);
        private const int MeFontSize = 30;
        private const float MeCornerScale = 1.5f;

        private static readonly Color PlayerHighlight = new Color32(0xFF, 0xF6, 0xD6, 0xFF);
        private static readonly Color MeBlue = new Color32(0x5B, 0x8C, 0xF5, 0xFF);
        private static readonly Color[] MedalColors =
        {
            new Color32(0xF5, 0xB7, 0x1E, 0xFF),
            new Color32(0xB4, 0xBC, 0xC8, 0xFF),
            new Color32(0xD2, 0x8A, 0x4E, 0xFF)
        };
        private static readonly Color[] AvatarColors =
        {
            new Color32(0xF7, 0xA8, 0x8B, 0xFF),
            new Color32(0x8B, 0xC8, 0xF7, 0xFF),
            new Color32(0xA8, 0xDB, 0x8F, 0xFF),
            new Color32(0xC9, 0xA8, 0xF0, 0xFF),
            new Color32(0xF7, 0xD0, 0x7A, 0xFF),
            new Color32(0x8F, 0xD8, 0xCF, 0xFF)
        };

        private readonly RectTransform _root;
        private readonly Image _highlight;
        private readonly Image _medal;
        private readonly Text _rank;
        private readonly Image _avatar;
        private readonly Text _initial;
        private readonly Text _name;
        private readonly Text _level;
        private readonly GameObject _me;

        public LeaderboardRowView(RectTransform sheet, ArtLibrary art, float top, float height)
        {
            _root = UiFactory.Rect("Row", sheet, new Vector2(0.5f, 1f), new Vector2(0f, top - height * 0.5f), new Vector2(sheet.sizeDelta.x, height));
            _highlight = UiFactory.AddImage(_root, null, PlayerHighlight);

            _medal = UiFactory.AddImage(UiFactory.Rect("Medal", _root, new Vector2(0.5f, 0.5f), new Vector2(RankX, 0f), Vector2.one * MedalSize), art.Disc, Color.white);
            _rank = Label(art, UiFactory.Rect("Rank", _root, new Vector2(0.5f, 0.5f), new Vector2(RankX, 0f), Vector2.one * MedalSize), RankFontSize, TextAnchor.MiddleCenter, GreyText);

            var avatarRect = UiFactory.Rect("Avatar", _root, new Vector2(0.5f, 0.5f), new Vector2(AvatarX, 0f), Vector2.one * AvatarSize);
            _avatar = UiFactory.AddImage(avatarRect, art.Disc, Color.white);
            _initial = Label(art, UiFactory.Stretch("Initial", avatarRect), AvatarFontSize, TextAnchor.MiddleCenter, Color.white);

            _name = Label(art, TextRect("Name", NameOffsetY), NameFontSize, TextAnchor.MiddleLeft, DarkText);
            _level = Label(art, TextRect("Level", LevelOffsetY), LevelFontSize, TextAnchor.MiddleLeft, GreyText);
            _me = BuildMeTag(art);
        }

        public void SetVisible(bool visible) => _root.gameObject.SetActive(visible);

        public void Fill(LeaderboardRow row)
        {
            _highlight.enabled = row.IsPlayer;
            _me.SetActive(row.IsPlayer);
            FillRank(row.Rank);

            string name = string.IsNullOrEmpty(row.Name) ? (row.IsPlayer ? Texts.Me : string.Empty) : row.Name;
            _name.text = name;
            _level.text = Texts.LevelsReached(row.Level);
            _initial.text = Initial(name);
            _avatar.color = AvatarColors[(name.GetHashCode() & int.MaxValue) % AvatarColors.Length];
        }

        /// <summary>First visible character; keeps a surrogate pair (emoji, rare CJK) whole.</summary>
        private static string Initial(string name)
        {
            if (name.Length == 0) return string.Empty;
            int length = char.IsHighSurrogate(name[0]) && name.Length > 1 ? 2 : 1;
            return name.Substring(0, length);
        }

        private void FillRank(int rank)
        {
            bool isMedal = rank >= 1 && rank <= MedalCount;
            _medal.enabled = isMedal;
            if (isMedal) _medal.color = MedalColors[rank - 1];
            _rank.text = rank.ToString();
            _rank.fontSize = isMedal ? MedalFontSize : RankFontSize;
            _rank.color = isMedal ? Color.white : GreyText;
        }

        private RectTransform TextRect(string name, float offsetY)
        {
            float x = -_root.sizeDelta.x * 0.5f + NameLeft + NameWidth * 0.5f;
            return UiFactory.Rect(name, _root, new Vector2(0.5f, 0.5f), new Vector2(x, offsetY), new Vector2(NameWidth, _root.sizeDelta.y * 0.5f));
        }

        private GameObject BuildMeTag(ArtLibrary art)
        {
            var tag = UiFactory.Rect("Me", _root, new Vector2(0.5f, 0.5f), new Vector2(MeX, 0f), MeSize);
            UiFactory.AddBox(tag, art.FlatBox, MeBlue).pixelsPerUnitMultiplier = MeCornerScale;
            Label(art, UiFactory.Stretch("Text", tag), MeFontSize, TextAnchor.MiddleCenter, Color.white).text = Texts.Me;
            return tag.gameObject;
        }

        private static Text Label(ArtLibrary art, RectTransform rect, int size, TextAnchor alignment, Color color)
        {
            var text = UiFactory.AddLabel(rect, art.Font, size, alignment, color);
            LeaderboardPanelView.RemoveOutlines(text);
            return text;
        }
    }
}
