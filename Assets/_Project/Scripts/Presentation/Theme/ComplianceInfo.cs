using System.Collections.Generic;

namespace SheepStrike.Presentation.Theme
{
    /// <summary>
    /// Registration numbers the platform requires on the start screen (Douyin review "信息标识").
    /// Fill each one in exactly as issued; empty ones are not shown, so nothing made-up ever reaches players.
    /// </summary>
    public static class ComplianceInfo
    {
        /// <summary>游戏批准文号 (e.g. 国新出审[20xx]xxxx号).</summary>
        public const string ApprovalNumber = "";

        /// <summary>网络游戏出版物号 (ISBN).</summary>
        public const string PublicationNumber = "";

        /// <summary>著作权人.</summary>
        public const string CopyrightOwner = "";

        /// <summary>软件著作权登记号.</summary>
        public const string CopyrightRegistration = "";

        /// <summary>The age band shown on the badge; keep it identical to what was declared to the platform.</summary>
        public const int MinimumAge = 8;

        public static IEnumerable<string> Lines()
        {
            if (ApprovalNumber.Length > 0) yield return "批准文号：" + ApprovalNumber;
            if (PublicationNumber.Length > 0) yield return "出版物号：" + PublicationNumber;
            if (CopyrightOwner.Length > 0) yield return "著作权人：" + CopyrightOwner;
            if (CopyrightRegistration.Length > 0) yield return "登记号：" + CopyrightRegistration;
        }
    }
}
