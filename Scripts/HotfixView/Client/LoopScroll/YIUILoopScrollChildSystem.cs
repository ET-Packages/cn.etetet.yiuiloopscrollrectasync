using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 无限循环列表 (异步)
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/HPbwwkhsKi9aDik5VEXcqPhDnIh
    /// </summary>
    [FriendOf(typeof(YIUILoopScrollChild))]
    [EntitySystemOf(typeof(YIUILoopScrollChild))]
    public static partial class YIUILoopScrollChildSystem
    {
        #region ObjectSystem

        [EntitySystem]
        private static void Awake(this YIUILoopScrollChild self)
        {
            self.m_OwnerEntity = self.GetParent<Entity>();
        }

        [EntitySystem]
        private static void Awake(this YIUILoopScrollChild self, LoopScrollRect owner, Type itemType)
        {
            self.m_OwnerEntity = self.GetParent<Entity>();
            self.Initialize(owner, itemType);
        }

        [EntitySystem]
        private static void Awake(this YIUILoopScrollChild self, LoopScrollRect owner, Type itemType, string itemClickEventName)
        {
            self.m_OwnerEntity = self.GetParent<Entity>();
            self.Initialize(owner, itemType);
            self.SetOnClick(itemClickEventName);
        }

        [EntitySystem]
        private static void Destroy(this YIUILoopScrollChild self)
        {
            foreach (var code in self.m_BanLayerOptionForeverHashSet)
            {
                YIUIMgrComponent.Inst.RecoverLayerOptionForever(code);
            }
        }

        #endregion

        #region Private

        public static void Initialize(this YIUILoopScrollChild self, LoopScrollRect owner, Type itemType)
        {
            var data = YIUIBindHelper.GetBindVoByType(itemType);
            if (data == null) return;
            self.m_Owner    = owner;
            self.m_ItemType = itemType;
            self.m_ItemTransformDic.Clear();
            self.m_ItemTransformIndexDic.Clear();
            self.m_BindVo             = data.Value;
            self.m_ItemPool           = new(self.OnCreateItemRenderer);
            self.m_Owner.prefabSource = self;
            self.m_Owner.dataSource   = self;

            self.InitClearContent();
            self.InitCacheParent();
            self.m_InvokeLoadInstantiate = new YIUIInvokeLoadInstantiateByVo
            {
                BindVo          = self.m_BindVo,
                ParentEntity    = self,
                ParentTransform = self.CacheRect,
            };
        }

        private static void InitCacheParent(this YIUILoopScrollChild self)
        {
            if (self.m_Owner.u_CacheRect != null)
            {
                self.m_Owner.u_CacheRect.gameObject.SetActive(false);
            }
            else
            {
                var cacheObj  = new GameObject("Cache");
                var cacheRect = cacheObj.GetOrAddComponent<RectTransform>();
                self.m_Owner.u_CacheRect = cacheRect;
                cacheRect.SetParent(self.m_Owner.transform, false);
                cacheObj.SetActive(false);
            }
        }

        //不应该初始化时有内容 所有不管是什么全部摧毁
        private static void InitClearContent(this YIUILoopScrollChild self)
        {
            var count = self.Content.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = self.Content.GetChild(0);
                child.gameObject.SafeDestroySelf();
            }
        }

        private static Entity GetItemRendererByDic(this YIUILoopScrollChild self, Transform tsf)
        {
            if (self.m_ItemTransformDic.TryGetValue(tsf, out EntityRef<Entity> value))
            {
                return value;
            }

            Debug.LogError($"{tsf.name} 没找到这个关联对象 请检查错误");
            return null;
        }

        private static void AddItemRendererByDic(this YIUILoopScrollChild self, Transform tsf, Entity item)
        {
            self.m_ItemTransformDic.TryAdd(tsf, item);
        }

        private static int GetItemIndex(this YIUILoopScrollChild self, Transform tsf)
        {
            return self.m_ItemTransformIndexDic.GetValueOrDefault(tsf, -1);
        }

        private static void ResetItemIndex(this YIUILoopScrollChild self, Transform tsf, int index)
        {
            self.m_ItemTransformIndexDic[tsf] = index;
        }

        #endregion

        #region LoopScrollRect Interface

        private static async ETTask<EntityRef<Entity>> OnCreateItemRenderer(this YIUILoopScrollChild self)
        {
            var item = await EventSystem.Instance?.YIUIInvokeAsync<YIUIInvokeLoadInstantiateByVo, ETTask<Entity>>(self.m_InvokeLoadInstantiate);
            if (item == null)
            {
                Log.Error($"YIUILoopScroll 实例化失败 请检查 {self.m_BindVo.PkgName} {self.m_BindVo.ResName}");
                return null;
            }

            var ownerRectTransform = item.GetParent<YIUIChild>().OwnerRectTransform;

            #if UNITY_EDITOR
            if (!self.m_FirstCheckLayoutElement)
            {
                var layoutElement = ownerRectTransform.GetComponent<ILayoutElement>();
                if (layoutElement == null)
                {
                    var layoutGroup = ownerRectTransform.GetComponent<ILayoutGroup>();
                    if (layoutGroup == null)
                    {
                        Debug.LogError($"{ownerRectTransform.name} 没有LayoutElement组件 请检查错误");
                    }
                }

                self.m_FirstCheckLayoutElement = true;
            }
            #endif

            self.AddItemRendererByDic(ownerRectTransform, item);
            self.AddOnClickEvent(item);
            return item;
        }

        private static async ETTask<GameObject> GetObject(this YIUILoopScrollChild self, int index)
        {
            var item = await self.m_ItemPool.Get();
            return ((Entity)item)?.GetParent<YIUIChild>()?.OwnerGameObject;
        }

        private static void ReturnObject(this YIUILoopScrollChild self, Transform transform)
        {
            var item = self.GetItemRendererByDic(transform);
            if (item == null) return;
            self.m_ItemPool.Put(item);
            self.ResetItemIndex(transform, -1);
            transform.SetParent(self.m_Owner.u_CacheRect, false);
        }

        private static void ProvideData(this YIUILoopScrollChild self, Transform transform, int index)
        {
            var item = self.GetItemRendererByDic(transform);
            if (item == null) return;
            self.ResetItemIndex(transform, index);

            var select = self.m_OnClickItemHashSet.Contains(index);
            if (self.Data == null)
            {
                Debug.LogError($"{self.Parent.GetType().Name} {self.m_Owner.name}当前没有设定数据 m_Data == null");
                return;
            }

            YIUILoopHelper.Renderer(self.m_LoopRendererSystemType, self.OwnerEntity, item, self.Data[index], index, select);
        }

        #endregion

        #region EntitySystem

        [EntitySystem]
        public class YIUILoopScrollPrefabAsyncSource : YIUILoopScrollPrefabAsyncSourceSystem<YIUILoopScrollChild>
        {
            protected override async ETTask<GameObject> GetObject(YIUILoopScrollChild self, int index)
            {
                return await self.GetObject(index);
            }

            protected override void ReturnObject(YIUILoopScrollChild self, Transform trans)
            {
                self.ReturnObject(trans);
            }
        }

        [EntitySystem]
        public class YIUILoopScrollDataSource : YIUILoopScrollDataSourceSystem<YIUILoopScrollChild>
        {
            protected override void ProvideData(YIUILoopScrollChild self, Transform transform, int idx)
            {
                self.ProvideData(transform, idx);
            }
        }

        #endregion
    }
}