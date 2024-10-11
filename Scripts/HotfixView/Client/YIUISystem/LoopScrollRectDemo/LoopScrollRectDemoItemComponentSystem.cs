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
    [FriendOf(typeof(LoopScrollRectDemoItemComponent))]
    public static partial class LoopScrollRectDemoItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoopScrollRectDemoItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoopScrollRectDemoItemComponent self)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke]
        private static void OnEventSelectInvoke(this LoopScrollRectDemoItemComponent self)
        {

        }
        #endregion YIUIEvent结束
    }
}
