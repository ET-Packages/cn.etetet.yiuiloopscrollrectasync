using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2024.10.11
    /// Desc
    /// </summary>
    [FriendOf(typeof(LoopScrollHorizontalViewComponent))]
    public static partial class LoopScrollHorizontalViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoopScrollHorizontalViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoopScrollHorizontalViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LoopScrollHorizontalViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
