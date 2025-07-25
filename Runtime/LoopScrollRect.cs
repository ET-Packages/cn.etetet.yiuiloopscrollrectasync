﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ET;
using YIUIFramework;

namespace UnityEngine.UI
{
    public abstract partial class LoopScrollRect : LoopScrollRectBase
    {
        [HideInInspector]
        [NonSerialized]
        public IYIUILoopScrollDataSource dataSource = null;

        protected override void ProvideData(Transform transform, int index)
        {
            dataSource.ProvideData(transform, index);
        }

        protected override async ETTask<RectTransform> GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem = null;
            if (deletedItemTypeStart > 0)
            {
                deletedItemTypeStart--;
                nextItem = m_Content.GetChild(0) as RectTransform;
                nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
            }
            else if (deletedItemTypeEnd > 0)
            {
                deletedItemTypeEnd--;
                nextItem = m_Content.GetChild(m_Content.childCount - 1) as RectTransform;
                nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
            }
            else
            {
                var prefabObj = await prefabSource.GetObject(itemIdx);
                if (prefabObj == null)
                {   
                    Debug.LogError($"预制异步加载错误,没有加载到预制资源 请检查");
                    return null;
                }
                nextItem = prefabObj.transform as RectTransform;
                nextItem.transform.SetParent(m_Content, false);
                nextItem.gameObject.SetActive(true);
            }

            ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            if (fromStart)
                deletedItemTypeStart += count;
            else
                deletedItemTypeEnd += count;
        }

        protected override void ClearTempPool()
        {
            Debug.Assert(m_Content.childCount >= deletedItemTypeStart + deletedItemTypeEnd);
            if (deletedItemTypeStart > 0)
            {
                for (int i = deletedItemTypeStart - 1; i >= 0; i--)
                {
                    prefabSource.ReturnObject(m_Content.GetChild(i));
                }

                deletedItemTypeStart = 0;
            }

            if (deletedItemTypeEnd > 0)
            {
                int t = m_Content.childCount - deletedItemTypeEnd;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    prefabSource.ReturnObject(m_Content.GetChild(i));
                }

                deletedItemTypeEnd = 0;
            }
        }
    }
}