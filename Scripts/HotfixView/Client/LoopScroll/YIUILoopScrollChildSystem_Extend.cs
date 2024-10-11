﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 无限循环列表 (异步)
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/HPbwwkhsKi9aDik5VEXcqPhDnIh
    /// </summary>
    [FriendOf(typeof(YIUILoopScrollChild))]
    public static partial class YIUILoopScrollChildSystem
    {
        //在开始时用startItem填充单元格，同时清除现有的单元格
        public static async ETTask RefillCells(this YIUILoopScrollChild self, int startItem = 0, float contentOffset = 0)
        {
            var code = self.BanLayerOptionForever();
            await self.m_Owner.RefillCells(startItem, contentOffset);
            self.RecoverLayerOptionForever(code);
        }

        //在结束时重新填充endItem中的单元格，同时清除现有的单元格
        public static async ETTask RefillCellsFromEnd(this YIUILoopScrollChild self, int endItem = 0, bool alignStart = false)
        {
            var code = self.BanLayerOptionForever();
            await self.m_Owner.RefillCellsFromEnd(endItem, alignStart);
            self.RecoverLayerOptionForever(code);
        }

        public static async ETTask RefreshCells(this YIUILoopScrollChild self)
        {
            var code = self.BanLayerOptionForever();
            await self.m_Owner.RefreshCells();
            self.RecoverLayerOptionForever(code);
        }

        public static void ClearCells(this YIUILoopScrollChild self)
        {
            self.m_Owner.ClearCells();
        }

        public static int GetFirstItem(this YIUILoopScrollChild self, out float offset)
        {
            return self.m_Owner.GetFirstItem(out offset);
        }

        public static int GetLastItem(this YIUILoopScrollChild self, out float offset)
        {
            return self.m_Owner.GetLastItem(out offset);
        }

        private static int GetValidIndex(this YIUILoopScrollChild self, int index)
        {
            return Mathf.Clamp(index, 0, self.TotalCount - 1);
        }

        public static async ETTask ScrollToCell(this YIUILoopScrollChild self, int index, float speed)
        {
            if (self.TotalCount <= 0) return;
            var code = self.BanLayerOptionForever();
            await self.m_Owner.ScrollToCell(self.GetValidIndex(index), speed);
            self.RecoverLayerOptionForever(code);
        }

        public static async ETTask ScrollToCellWithinTime(this YIUILoopScrollChild self, int index, float time)
        {
            if (self.TotalCount <= 0) return;
            var code = self.BanLayerOptionForever();
            await self.m_Owner.ScrollToCellWithinTime(self.GetValidIndex(index), time);
            self.RecoverLayerOptionForever(code);
        }

        public static void StopMovement(this YIUILoopScrollChild self)
        {
            self.m_Owner.StopMovement();
        }

        private static long BanLayerOptionForever(this YIUILoopScrollChild self)
        {
            var code = YIUIMgrComponent.Inst.BanLayerOptionForever();
            self.m_BanLayerOptionForeverHashSet.Add(code);
            return code;
        }

        private static void RecoverLayerOptionForever(this YIUILoopScrollChild self, long code)
        {
            self.m_BanLayerOptionForeverHashSet.Remove(code);
            YIUIMgrComponent.Inst.RecoverLayerOptionForever(code);
        }
    }
}