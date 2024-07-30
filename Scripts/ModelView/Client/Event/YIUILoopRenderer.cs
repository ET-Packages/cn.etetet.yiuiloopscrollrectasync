namespace ET.Client
{
    public interface IYIUILoopRenderer
    {
        /// <summary>
        /// 渲染数据项
        /// </summary>
        /// <param name="self">渲染器实体</param>
        /// <param name="index">数据的索引</param>
        /// <param name="data">数据项</param>
        /// <param name="item">显示对象</param>
        /// <param name="select">是否被选中</param>
        void Renderer(Entity self, int index, object data, Entity item, bool select);

        /// <summary>
        /// 点击事件
        /// 调用SetOnClick方法设置点击事件信息后生效
        /// </summary>
        void Click(Entity self, int index, object data, Entity item, bool select);
    }

    [EntitySystem]
    public abstract class YIUILoopRendererSystem<T1, T2, T3> : SystemObject, IYIUILoopRenderer
            where T1 : Entity, IYIUIBind, IYIUIInitialize
            where T2 : Entity, IYIUIBind, IYIUIInitialize
    {
        void IYIUILoopRenderer.Renderer(Entity self, int index, object data, Entity item, bool select)
        {
            Renderer((T1)self, index, (T2)item, (T3)data, select);
        }

        void IYIUILoopRenderer.Click(Entity self, int index, object data, Entity item, bool select)
        {
            Click((T1)self, index, (T2)item, (T3)data, select);
        }

        protected abstract void Renderer(T1 self, int index, T2 item, T3 data, bool select);

        protected virtual void Click(T1 self, int index, T2 item, T3 data, bool select)
        {
        }
    }
}