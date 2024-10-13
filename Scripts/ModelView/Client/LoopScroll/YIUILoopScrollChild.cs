//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using YIUIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 无限循环列表 (异步)
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/HPbwwkhsKi9aDik5VEXcqPhDnIh
    /// </summary>
    [ChildOf]
    public partial class YIUILoopScrollChild : Entity, IAwake, IAwake<LoopScrollRect, Type>, IAwake<LoopScrollRect, Type, string>, IDestroy, IYIUILoopScrollPrefabAsyncSource, IYIUILoopScrollDataSource
    {
        public EntityRef<Entity> m_OwnerEntity;
        public Entity            OwnerEntity => m_OwnerEntity;

        public YIUIBindVo     m_BindVo;
        public Type           m_DataType;
        public LoopScrollRect m_Owner;
        public Type           m_ItemType;
        public Type           m_LoopRendererSystemType;
        public Type           m_LoopOnClickSystemType;

        public ObjAsyncCache<EntityRef<Entity>>         m_ItemPool;
        public Dictionary<Transform, EntityRef<Entity>> m_ItemTransformDic      = new();
        public Dictionary<Transform, int>               m_ItemTransformIndexDic = new();

        public YIUIInvokeLoadInstantiateByVo m_InvokeLoadInstantiate;
        public HashSet<long>                 m_BanLayerOptionForeverHashSet = new();

        private IList m_Data;

        public IList Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if (m_Data is { Count: > 0 })
                {
                    m_DataType               = m_Data[0].GetType();
                    m_LoopRendererSystemType = typeof(IYIUILoopRenderer<,,>).MakeGenericType(OwnerEntity?.GetType(), m_ItemType, m_DataType);
                    m_LoopOnClickSystemType  = typeof(IYIUILoopOnClick<,,>).MakeGenericType(OwnerEntity?.GetType(), m_ItemType, m_DataType);
                }
                else
                {
                    m_DataType               = null;
                    m_LoopRendererSystemType = null;
                    m_LoopOnClickSystemType  = null;
                }
            }
        }

        #if UNITY_EDITOR
        public bool m_FirstCheckLayoutElement;
        #endif
    }
}