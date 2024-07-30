using System;

namespace ET.Client
{
    /// <summary>
    /// YIUILoop渲染特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class YIUILoopRendererAttribute : BaseAttribute
    {
    }

    /*案例
    [YIUILoopRenderer]
    public class GMCommandItemComponentLoopRendererSystem : YIUILoopRendererSystem<GMCommandItemComponent,GMParamItemComponent,GMParamInfo>
    {
        protected override void Renderer(GMCommandItemComponent self, int index, GMParamItemComponent item, GMParamInfo data, bool select)
        {
            Log.Error("Renderer");
        }

        protected override void Click(GMCommandItemComponent self, int index, GMParamItemComponent item, GMParamInfo data, bool select)
        {
            Log.Error("Click");
        }
    }
    */
}