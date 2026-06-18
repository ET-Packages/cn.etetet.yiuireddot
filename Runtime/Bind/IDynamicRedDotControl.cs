using UnityEngine;

namespace YIUIFramework
{
    //提供接口是为了 如果不想用RedDotBind
    //可以自己写mono, 尽量不要去修改 RedDotBind 因为修改过后不方便更新包
    public interface IDynamicRedDotControl
    {
        void BindKey(int key);

        void SetManualCount(int count);

        void ResetState();

        GameObject GetOwnerGameObject();
    }
}