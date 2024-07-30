using System;
using System.Collections.Generic;

namespace ET.Client
{
    public struct YIUILoopWatcherInfo
    {
        public Type SelfType { get; }
        public Type ItemType { get; }
        public Type DataType { get; }

        public IYIUILoopRenderer Renderer { get; }

        public YIUILoopWatcherInfo(Type selfType, Type itemType, Type dataType, IYIUILoopRenderer renderer)
        {
            SelfType = selfType;
            ItemType = itemType;
            DataType = dataType;
            Renderer = renderer;
        }
    }

    [CodeProcess]
    public class YIUILoopWatcher : Singleton<YIUILoopWatcher>, ISingletonAwake
    {
        private Dictionary<Type, Dictionary<Type, Dictionary<Type, YIUILoopWatcherInfo>>> m_AllWatchers;

        public void Awake()
        {
            m_AllWatchers = new();
            var types = CodeTypes.Instance.GetTypes(typeof(YIUILoopRendererAttribute));
            foreach (var type in types)
            {
                if (type.BaseType == null || type.BaseType.GenericTypeArguments.Length != 3)
                {
                    Log.Error($"没有找到 {type.Name} 的继承 YIUILoopRendererSystem 的<T1,T2,T3>");
                    continue;
                }

                Type selfType = type.BaseType.GenericTypeArguments[0];
                Type itemType = type.BaseType.GenericTypeArguments[1];
                Type dataType = type.BaseType.GenericTypeArguments[2];

                var obj         = (IYIUILoopRenderer)Activator.CreateInstance(type);
                var watcherInfo = new YIUILoopWatcherInfo(selfType, itemType, dataType, obj);

                if (!m_AllWatchers.TryGetValue(selfType, out var itemWatchers))
                {
                    itemWatchers = new();
                    m_AllWatchers.Add(selfType, itemWatchers);
                }

                if (!itemWatchers.TryGetValue(itemType, out var dataWatchers))
                {
                    dataWatchers = new();
                    itemWatchers.Add(itemType, dataWatchers);
                }

                if (!dataWatchers.TryAdd(dataType, watcherInfo))
                {
                    Log.Error($"已经存在相同的类型 {dataType} 的 {selfType} 与 {itemType} 的 {dataType} 的循环渲染器");
                }
            }
        }

        public void Renderer(Entity self, int index, object data, Entity item, bool select)
        {
            var renderer = GetRenderer(self, index, data, item, select);
            if (renderer == null) return;
            renderer.Renderer(self, index, data, item, select);
        }

        public void Click(Entity self, int index, object data, Entity item, bool select)
        {
            var renderer = GetRenderer(self, index, data, item, select);
            if (renderer == null) return;
            renderer.Click(self, index, data, item, select);
        }

        private IYIUILoopRenderer GetRenderer(Entity self, int index, object data, Entity item, bool select)
        {
            if (self == null || item == null || data == null)
            {
                Log.Error($"Renderer参数错误: self:{self}, index:{index}, data:{data}, item:{item}, select:{select}");
                return null;
            }

            m_AllWatchers.TryGetValue(self.GetType(), out var itemWatchers);
            if (itemWatchers == null)
            {
                Log.Error($"没有找到 [self:{self.GetType().Name}] 的循环渲染器");
                return null;
            }

            itemWatchers.TryGetValue(item.GetType(), out var dataWatchers);
            if (dataWatchers == null)
            {
                Log.Error($"没有找到 [self:{self.GetType().Name}] [item:{item.GetType().Name}] 的循环渲染器");
                return null;
            }

            dataWatchers.TryGetValue(data.GetType(), out var watcherInfo);
            if (watcherInfo.Renderer == null)
            {
                Log.Error($"没有找到 [self:{self.GetType().Name}] [item:{item.GetType().Name}] [data:{data.GetType().Name}] 的循环渲染器");
                return null;
            }

            return watcherInfo.Renderer;
        }
    }
}