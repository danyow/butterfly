using Unity.Collections.LowLevel.Unsafe;

namespace Butterfly.NativeItem
{
    [NativeContainer]
    [NativeContainerIsAtomicWriteOnly] // 原生容器是原子只写的
    // 让 JobSystem 知道它应该将当前的 worker 索引注入到这个容器中
    public unsafe struct Concurrent
    {
        [NativeDisableUnsafePtrRestriction] // 原生禁用不安全指针限制
        private int* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
#endif

        // 当前工作线程索引，它必须使用这个确切的名称，因为它是注入的
        [NativeSetThreadIndex]
        int m_ThreadIndex;

        public static implicit operator Concurrent(NativePerThreadCounter cnt)
        {
            Concurrent concurrent;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(cnt.m_Safety);
            concurrent.m_Safety = cnt.m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#endif
            concurrent.m_Counter = cnt.m_Counter;
            concurrent.m_ThreadIndex = 0;
            return concurrent;
        }

        public void Increment()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

            // 不再需要原子，因为我们只是增加本地计数
            ++m_Counter[NativePerThreadCounter.IntsPerCacheLine * m_ThreadIndex];
        }
    }
}