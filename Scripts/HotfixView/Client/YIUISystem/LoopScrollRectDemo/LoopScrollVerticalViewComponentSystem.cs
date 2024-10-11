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
    [FriendOf(typeof(LoopScrollVerticalViewComponent))]
    public static partial class LoopScrollVerticalViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoopScrollVerticalViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoopScrollVerticalViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LoopScrollVerticalViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
