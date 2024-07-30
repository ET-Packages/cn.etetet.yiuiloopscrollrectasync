//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using YIUIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/HPbwwkhsKi9aDik5VEXcqPhDnIh
    /// </summary>
    [EnableClass]
    public partial class YIUILoopScroll<TData> : LoopScrollPrefabAsyncSource, LoopScrollDataSource
    {
        private EntityRef<Entity> m_OwnerEntity;

        public Entity OwnerEntity => m_OwnerEntity;

        private YIUIBindVo   m_BindVo;
        private IList<TData> m_Data;

        private LoopScrollRect                           m_Owner;
        private ObjAsyncCache<EntityRef<Entity>>         m_ItemPool;
        private Dictionary<Transform, EntityRef<Entity>> m_ItemTransformDic      = new();
        private Dictionary<Transform, int>               m_ItemTransformIndexDic = new();

        private YIUIInvokeLoadInstantiateByVo m_InvokeLoadInstantiate;

        public YIUILoopScroll(Entity ownerEneity, LoopScrollRect owner, Type itemType)
        {
            Initialize(ownerEneity, owner, itemType);
        }

        public YIUILoopScroll(Entity ownerEneity, LoopScrollRect owner, Type itemType, string itemClickEventName)
        {
            Initialize(ownerEneity, owner, itemType);
            SetOnClick(itemClickEventName);
        }

        private void Initialize(Entity ownerEneity, LoopScrollRect owner, Type itemType)
        {
            var data = YIUIBindHelper.GetBindVoByType(itemType);
            if (data == null) return;
            m_ItemTransformDic.Clear();
            m_ItemTransformIndexDic.Clear();
            m_BindVo             = data.Value;
            m_ItemPool           = new(OnCreateItemRenderer);
            m_OwnerEntity        = ownerEneity;
            m_Owner              = owner;
            m_Owner.prefabSource = this;
            m_Owner.dataSource   = this;
            InitClearContent();
            InitCacheParent();
            m_InvokeLoadInstantiate = new YIUIInvokeLoadInstantiateByVo
            {
                BindVo          = m_BindVo,
                ParentEntity    = m_OwnerEntity,
                ParentTransform = CacheRect,
            };
        }

        #region Private

        private void InitCacheParent()
        {
            if (m_Owner.u_CacheRect != null)
            {
                m_Owner.u_CacheRect.gameObject.SetActive(false);
            }
            else
            {
                var cacheObj  = new GameObject("Cache");
                var cacheRect = cacheObj.GetOrAddComponent<RectTransform>();
                m_Owner.u_CacheRect = cacheRect;
                cacheRect.SetParent(m_Owner.transform, false);
                cacheObj.SetActive(false);
            }
        }

        //不应该初始化时有内容 所有不管是什么全部摧毁
        private void InitClearContent()
        {
            var count = Content.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = Content.GetChild(0);
                child.gameObject.SafeDestroySelf();
            }
        }

        private Entity GetItemRendererByDic(Transform tsf)
        {
            if (m_ItemTransformDic.TryGetValue(tsf, out EntityRef<Entity> value))
            {
                return value;
            }

            Debug.LogError($"{tsf.name} 没找到这个关联对象 请检查错误");
            return null;
        }

        private void AddItemRendererByDic(Transform tsf, Entity item)
        {
            m_ItemTransformDic.TryAdd(tsf, item);
        }

        private int GetItemIndex(Transform tsf)
        {
            return m_ItemTransformIndexDic.GetValueOrDefault(tsf, -1);
        }

        private void ResetItemIndex(Transform tsf, int index)
        {
            m_ItemTransformIndexDic[tsf] = index;
        }

        #endregion

        #region LoopScrollRect Interface

        private async ETTask<EntityRef<Entity>> OnCreateItemRenderer()
        {
            var item = await EventSystem.Instance?.YIUIInvokeAsync<YIUIInvokeLoadInstantiateByVo, ETTask<Entity>>(m_InvokeLoadInstantiate);
            if (item == null)
            {
                Log.Error($"YIUILoopScroll 实例化失败 请检查 {m_BindVo.PkgName} {m_BindVo.ResName}");
                return null;
            }

            AddItemRendererByDic(item.GetParent<YIUIChild>().OwnerRectTransform, item);
            AddOnClickEvent(item);
            return item;
        }

        public async ETTask<GameObject> GetObject(int index)
        {
            var item = await m_ItemPool.Get();
            return ((Entity)item)?.GetParent<YIUIChild>()?.OwnerGameObject;
        }

        public void ReturnObject(Transform transform)
        {
            var item = GetItemRendererByDic(transform);
            if (item == null) return;
            m_ItemPool.Put(item);
            ResetItemIndex(transform, -1);
            transform.SetParent(m_Owner.u_CacheRect, false);
        }

        public void ProvideData(Transform transform, int index)
        {
            var item = GetItemRendererByDic(transform);
            if (item == null) return;
            ResetItemIndex(transform, index);

            var select = m_OnClickItemHashSet.Contains(index);
            if (m_Data == null)
            {
                Debug.LogError($"当前没有设定数据 m_Data == null");
                return;
            }

            YIUILoopWatcher.Instance.Renderer(OwnerEntity, index, m_Data[index], item, select);
        }

        #endregion
    }
}