﻿using System;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 红点 管理器
    /// </summary>
    public partial class RedDotMgr
    {
        /// <summary>
        /// 获取这个红点数据 
        /// </summary>
        public RedDotData GetData(int key)
        {
            m_AllRedDotData.TryGetValue(key, out var data);
            if (data == null)
            {
                if (!Disposed)
                {
                    Debug.LogError($"没有获取到这个红点数据 {key} Tips:如果是新增的注意需要ET编译过 否则就是没有 请检查");
                }
            }

            return data;
        }

        /// <summary>
        /// 添加变化监听
        /// </summary>
        public bool AddChanged(int key, Action<int> action)
        {
            var data = GetData(key);
            if (data == null)
            {
                return false;
            }

            data.AddOnChanged(action);
            return true;
        }

        /// <summary>
        /// 移除变化监听
        /// </summary>
        public bool RemoveChanged(int key, Action<int> action)
        {
            var data = GetData(key);
            if (data == null)
            {
                return false;
            }

            data.RemoveChanged(action);
            return true;
        }

        /// <summary>
        /// 设置对应红点的数量
        /// </summary>
        public bool SetCount(int key, int count)
        {
            var data = GetData(key);
            if (data == null) return false;

            if (SyncSetCount)
            {
                return data.TrySetCount(count);
            }
            else
            {
                return TryDirtySetCount(data, count);
            }
        }

        /// <summary>
        /// 获取某个红点的当前数量
        /// 如果他的tips被关闭 数量=0
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="isReal">真实数量</param>
        /// <returns></returns>
        public int GetCount(int key, bool isReal = false)
        {
            var data = GetData(key);
            if (data == null)
            {
                return 0;
            }

            return isReal ? data.RealCount : data.Count;
        }

        /// <summary>
        /// 设置此红点是否提示
        /// (可关闭红点 这样红点就不会一直提示了 给玩家设置的)
        /// </summary>
        public bool SetTips(int key, bool tips)
        {
            var data = GetData(key);
            return data != null && data.SetTips(tips);
        }

        public void DeletePlayerTipsPrefs(int key)
        {
            var data = GetData(key);
            data?.DeletePlayerTipsPrefs();
        }
    }
}