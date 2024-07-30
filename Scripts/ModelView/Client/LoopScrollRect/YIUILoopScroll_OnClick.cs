﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 额外点击相关
    /// </summary>
    public partial class YIUILoopScroll<TData>
    {
        private bool             m_OnClickInit;                //是否已初始化
        private string           m_ItemClickEventName;         //ui中的点击UIEventP0
        private Queue<int>       m_OnClickItemQueue   = new(); //当前所有已选择 遵循先进先出 有序
        private HashSet<int>     m_OnClickItemHashSet = new(); //当前所有已选择 无序 为了更快查找
        private int              m_MaxClickCount      = 1;     //可选最大数量 >=2 就是复选 最小1
        private bool             m_RepetitionCancel   = true;  //重复选择 则取消选择
        private bool             m_AutoCancelLast     = true;  //当选择操作最大数量过后 自动取消第一个选择的 否则选择无效

        public YIUILoopScroll<TData> SetOnClick(string itemClickEventName)
        {
            if (m_OnClickInit)
            {
                Debug.LogError($"OnClick 相关只能初始化一次 且不能修改");
                return this;
            }

            if (string.IsNullOrEmpty(itemClickEventName))
            {
                Debug.LogError($"必须有事件名称");
                return this;
            }
            
            m_MaxClickCount      = Mathf.Max(1, m_Owner.u_MaxClickCount);
            m_ItemClickEventName = itemClickEventName;
            m_RepetitionCancel   = m_Owner.u_RepetitionCancel;
            m_OnClickInit        = true;
            m_AutoCancelLast     = m_Owner.u_AutoCancelLast;
            m_OnClickItemQueue.Clear();
            m_OnClickItemHashSet.Clear();
            return this;
        }

        //reset=吧之前选择的都取消掉 讲道理应该都是true
        //false出问题自己查
        public void ClearSelect(bool reset = true)
        {
            if (reset)
            {
                var selectCount = m_OnClickItemHashSet.Count;
                for (var i = 0; i < selectCount; i++)
                {
                    OnClickItemQueuePeek();
                }
            }

            m_OnClickItemQueue.Clear();
            m_OnClickItemHashSet.Clear();
        }

        //动态改变 自动取消上一个选择的
        public void ChangeAutoCancelLast(bool autoCancelLast)
        {
            m_AutoCancelLast = autoCancelLast;
        }

        //动态改变 重复选择 则取消选择
        public void ChangeRepetitionCancel(bool repetitionCancel)
        {
            m_RepetitionCancel = repetitionCancel;
        }

        //动态改变 最大可选数量
        public void ChangeMaxClickCount(int count, bool reset = true)
        {
            ClearSelect(reset);
            m_MaxClickCount = Mathf.Max(1, count);
        }

        //传入对象 选中目标
        public void OnClickItem(Entity item)
        {
            var index = GetItemIndex(item);
            if (index < 0)
            {
                Debug.LogError($"无法选中一个不在显示中的对象");
                return;
            }

            var select = OnClickItemQueueEnqueue(index);
            OnClickItem(index, item, select);
        }

        //传入索引 选中目标
        public void OnClickItem(int index)
        {
            if (index < 0 || index >= m_Data.Count)
            {
                Debug.LogError($"索引越界{index}  0 - {m_Data.Count}");
                return;
            }

            var item   = GetItemByIndex(index, false);
            var select = OnClickItemQueueEnqueue(index);
            if (item != null)
            {
                OnClickItem(index, item, select);
            }
        }

        private bool OnClickItemQueueEnqueue(int index)
        {
            if (m_OnClickItemHashSet.Contains(index))
            {
                if (m_RepetitionCancel)
                {
                    RemoveSelectIndex(index);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (m_OnClickItemQueue.Count >= m_MaxClickCount)
            {
                if (m_AutoCancelLast)
                {
                    OnClickItemQueuePeek();
                }
                else
                {
                    return false;
                }
            }

            OnClickItemHashSetAdd(index);
            m_OnClickItemQueue.Enqueue(index);
            return true;
        }

        private void SetDefaultSelect(int index)
        {
            OnClickItemQueueEnqueue(index);
        }

        private void SetDefaultSelect(List<int> indexs)
        {
            foreach (var index in indexs)
            {
                SetDefaultSelect(index);
            }
        }

        private void OnClickItem(int index, Entity item, bool select)
        {
            if (!m_OnClickInit) return;
            YIUILoopWatcher.Instance.Click(OwnerEntity, index, m_Data[index], item, select);
        }

        private void AddOnClickEvent(Entity item)
        {
            if (!m_OnClickInit) return;

            var eventTable = item.GetParent<YIUIChild>().EventTable;
            if (eventTable == null)
            {
                Debug.LogError($"目标item 没有 event表 请检查");
                return;
            }

            var uEventClickItem = eventTable.FindEvent<UIEventP0>(m_ItemClickEventName);
            if (uEventClickItem == null)
            {
                Debug.LogError($"当前监听的事件未找到 请检查 {m_BindVo.ComponentType?.Name} 中是否有这个事件 {m_ItemClickEventName}");
                m_OnClickInit = false;
            }
            else
            {
                uEventClickItem.Add(() => { OnClickItem(item); });
            }
        }

        private void OnClickItemQueuePeek()
        {
            var index = m_OnClickItemQueue.Dequeue();
            OnClickItemHashSetRemove(index);
            if (index < ItemStart || index >= ItemEnd) return;
            var item = GetItemByIndex(index);
            if (item != null)
                OnClickItem(index, item, false);
        }

        private void OnClickItemHashSetAdd(int index)
        {
            m_OnClickItemHashSet.Add(index);
        }

        private void OnClickItemHashSetRemove(int index)
        {
            m_OnClickItemHashSet.Remove(index);
        }

        private void RemoveSelectIndex(int index)
        {
            var list = m_OnClickItemQueue.ToList();
            list.Remove(index);
            m_OnClickItemQueue = new Queue<int>(list);
            OnClickItemHashSetRemove(index);
        }
    }
}