namespace ET.Client
{
    [Event(SceneType.All)]
    public class On_Event_RedDot_Change_Handler : AEvent<Scene, Event_RedDot_Change>
    {
        protected override async ETTask Run(Scene currentScene, Event_RedDot_Change args)
        {
            var root = currentScene?.Root() ?? currentScene;
            var cache = root?.GetComponent<RedDotChangeCacheComponent>() ?? root?.AddComponent<RedDotChangeCacheComponent>();
            cache?.Cache(args.RedDotId, args.Count);
            cache?.FlushIfReady();
            await ETTask.CompletedTask;
        }
    }

    [Event(SceneType.Current)]
    public class On_YIUIEventInitializeAfter_FlushRedDotChange_Handler : AEvent<Scene, YIUIEventInitializeAfter>
    {
        protected override async ETTask Run(Scene currentScene, YIUIEventInitializeAfter args)
        {
            var root = currentScene?.Root() ?? currentScene;
            root?.GetComponent<RedDotChangeCacheComponent>()?.FlushIfReady();
            await ETTask.CompletedTask;
        }
    }
}
