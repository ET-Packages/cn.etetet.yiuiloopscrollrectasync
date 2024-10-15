using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 无限循环列表 (异步)
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/HPbwwkhsKi9aDik5VEXcqPhDnIh
    /// </summary>
    [FriendOf(typeof(YIUILoopScrollChild))]
    public static partial class YIUILoopScrollChildSystem
    {
        public static void SetOnClick(this YIUILoopScrollChild self, string itemClickEventName)
        {
            if (self.m_OnClickInit)
            {
                Debug.LogError($"OnClick 相关只能初始化一次 且不能修改");
                return;
            }

            if (string.IsNullOrEmpty(itemClickEventName))
            {
                Debug.LogError($"必须有事件名称");
                return;
            }

            self.m_MaxClickCount      = Mathf.Max(1, self.m_Owner.u_MaxClickCount);
            self.m_ItemClickEventName = itemClickEventName;
            self.m_RepetitionCancel   = self.m_Owner.u_RepetitionCancel;
            self.m_OnClickInit        = true;
            self.m_AutoCancelLast     = self.m_Owner.u_AutoCancelLast;
            self.m_OnClickItemQueue.Clear();
            self.m_OnClickItemHashSet.Clear();
        }

        //动态改变 自动取消上一个选择的
        public static void ChangeAutoCancelLast(this YIUILoopScrollChild self, bool autoCancelLast)
        {
            self.m_AutoCancelLast = autoCancelLast;
        }

        //动态改变 重复选择 则取消选择
        public static void ChangeRepetitionCancel(this YIUILoopScrollChild self, bool repetitionCancel)
        {
            self.m_RepetitionCancel = repetitionCancel;
        }

        //动态改变 最大可选数量
        public static void ChangeMaxClickCount(this YIUILoopScrollChild self, int count, bool reset = true)
        {
            self.ClearSelect(reset);
            self.m_MaxClickCount = Mathf.Max(1, count);
        }

        //传入对象 选中目标
        public static void OnClickItem(this YIUILoopScrollChild self, Entity item)
        {
            var index = self.GetItemIndex(item);
            if (index < 0)
            {
                Debug.LogError($"无法选中一个不在显示中的对象");
                return;
            }

            var select = self.OnClickItemQueueEnqueue(index);
            self.OnClickItem(index, item, select);
        }

        //传入索引 选中目标
        public static void OnClickItem(this YIUILoopScrollChild self, int index)
        {
            if (index < 0 || index >= self.Data.Count)
            {
                Debug.LogError($"点击 索引越界:[ {index} ] 限制范围[0 - {self.Data.Count}]");
                return;
            }

            var item   = self.GetItemByIndex(index, false);
            var select = self.OnClickItemQueueEnqueue(index);
            if (item != null)
            {
                self.OnClickItem(index, item, select);
            }
        }

        private static bool OnClickItemQueueEnqueue(this YIUILoopScrollChild self, int index)
        {
            if (self.m_OnClickItemHashSet.Contains(index))
            {
                if (self.m_RepetitionCancel)
                {
                    self.RemoveSelectIndex(index);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (self.m_OnClickItemQueue.Count >= self.m_MaxClickCount)
            {
                if (self.m_AutoCancelLast)
                {
                    self.OnClickItemQueuePeek();
                }
                else
                {
                    return false;
                }
            }

            self.OnClickItemHashSetAdd(index);
            self.m_OnClickItemQueue.Enqueue(index);
            return true;
        }

        private static void SetDefaultSelect(this YIUILoopScrollChild self, int index)
        {
            self.OnClickItemQueueEnqueue(index);
        }

        private static void SetDefaultSelect(this YIUILoopScrollChild self, List<int> indexs)
        {
            foreach (var index in indexs)
            {
                self.SetDefaultSelect(index);
            }
        }

        private static void OnClickItem(this YIUILoopScrollChild self, int index, Entity item, bool select)
        {
            if (!self.m_OnClickInit) return;
            YIUILoopHelper.OnClick(self.m_LoopOnClickSystemType, self.OwnerEntity, item, self.Data[index], index, select);
        }

        private static void AddOnClickEvent(this YIUILoopScrollChild self, Entity item)
        {
            if (!self.m_OnClickInit) return;

            var eventTable = item.GetParent<YIUIChild>().EventTable;
            if (eventTable == null)
            {
                Debug.LogError($"目标item 没有 event表 请检查");
                return;
            }

            var uEventClickItem = eventTable.FindEvent<UIEventP0>(self.m_ItemClickEventName);
            if (uEventClickItem == null)
            {
                Debug.LogError($"当前监听的事件未找到 请检查 {self.m_BindVo.ComponentType?.Name} 中是否有这个事件 {self.m_ItemClickEventName}");
                self.m_OnClickInit = false;
            }
            else
            {
                uEventClickItem.Add(() => { self.OnClickItem(item); });
            }
        }

        private static void OnClickItemQueuePeek(this YIUILoopScrollChild self)
        {
            var index = self.m_OnClickItemQueue.Dequeue();
            self.OnClickItemHashSetRemove(index);
            if (index < self.ItemStart || index >= self.ItemEnd) return;
            var item = self.GetItemByIndex(index);
            if (item != null)
                self.OnClickItem(index, item, false);
        }

        private static void OnClickItemHashSetAdd(this YIUILoopScrollChild self, int index)
        {
            self.m_OnClickItemHashSet.Add(index);
        }

        private static void OnClickItemHashSetRemove(this YIUILoopScrollChild self, int index)
        {
            self.m_OnClickItemHashSet.Remove(index);
        }

        private static void RemoveSelectIndex(this YIUILoopScrollChild self, int index)
        {
            var list = self.m_OnClickItemQueue.ToList();
            list.Remove(index);
            self.m_OnClickItemQueue = new Queue<int>(list);
            self.OnClickItemHashSetRemove(index);
        }
    }
}