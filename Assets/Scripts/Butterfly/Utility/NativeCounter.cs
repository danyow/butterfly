using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Butterfly.Utility
{
    [StructLayout(LayoutKind.Sequential)] // 顺序的
    [NativeContainer]
    public unsafe struct NativeCounter
    {
        // 指向已分配计数的实际指针需要放宽限制，以便可以使用此容器安排作业
        [NativeDisableUnsafePtrRestriction] // 原生禁用不安全指针限制
        private int* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS          // 启用Unity集合检查
        private AtomicSafetyHandle m_Safety; // 原子安全Handle

        // 处置哨兵跟踪内存泄漏。它是托管类型，因此在调度作业时将其清除为 null
        // 该作业无法处置容器，并且在作业运行之前没有其他人可以处置它，因此可以不传递它
        // 此属性是必需的，没有它，本机容器无法传递给作业，因为这将使作业访问托管对象
        [NativeSetClassTypeToNullOnSchedule] // 按计划将原生设置类类型为空
        private DisposeSentinel m_DisposeSentinel;
#endif

        // 跟踪分配内存的位置
        private Allocator m_AllocatorLabel;

        public NativeCounter(Allocator label)
        {
            // 这个检查是多余的，因为我们总是使用可 blittable 的 int。
            // 这里是一个示例，说明如何检查泛型类型的类型正确性。
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(!UnsafeUtility.IsBlittable<int>())
            {
                throw new ArgumentException($"NativeQueue<{typeof(int)}> 中使用的 {typeof(int)} 必须是 blittable");
            }
#endif
            m_AllocatorLabel = label;

            // 为单个整数分配本机内存
            m_Counter = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), 4, label);

            // 创建一个处置哨兵来跟踪内存泄漏。这也创建了 AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
#endif

            // 将 Count 初始化为0，避免数据未初始化
            Count = 0;
        }

        // 增量
        public void Increment()
        {
            //验证调用方对此数据是否具有写入权限。
            // 这是竞争条件保护，如果没有这些检查，AtomicSafetyHandle 将毫无用处
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            (*m_Counter)++;
        }

        public int Count
        {
            get
            {
                // 验证调用者是否具有对此数据的读取权限。
                // 这是竞争条件保护，如果没有这些检查，AtomicSafetyHandle 将毫无用处
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return *m_Counter;
            }
            set
            {
                // 验证调用方对此数据是否具有写入权限。
                // 这是竞争条件保护，如果没有这些检查，AtomicSafetyHandle 将毫无用处
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                *m_Counter = value;
            }
        }

        public bool IsCreated => m_Counter != null;

        /// <summary>
        /// 处置
        /// </summary>
        public void Dispose()
        {
            // 让 dispose sentinel 知道数据已被释放，因此它不会报告任何内存泄漏
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            UnsafeUtility.Free(m_Counter, m_AllocatorLabel);

            m_Counter = null;
        }

        [NativeContainer]

        // 这个属性使得在并行作业中使用 NativeCounter.Concurrent 成为可能
        [NativeContainerIsAtomicWriteOnly] // 原生容器是原子只写的
        public unsafe struct Concurrent
        {
            // 来自完整 NativeCounter 的指针副本
            [NativeDisableUnsafePtrRestriction] // 原生禁用不安全指针限制
            private int* m_Counter;

            // 来自完整 NativeCounter 的 AtomicSafetyHandle 副本。
            // dispose sentinel 没有被复制，因为这个内部结构不拥有内存并且不负责释放它
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            private AtomicSafetyHandle m_Safety;
#endif

            // implicit: 隐含的
            // operator: 操作员
            public static implicit operator Concurrent(NativeCounter cnt)
            {
                Concurrent concurrent;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(cnt.m_Safety);
                concurrent.m_Safety = cnt.m_Safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#endif
                concurrent.m_Counter = cnt.m_Counter;
                return concurrent;
            }

            public int Increment()
            {
                // 增量仍然需要检查写权限
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

                // 实际的增量是用原子实现的，因为它可以由多个线程同时递增
                return Interlocked.Increment(ref *m_Counter) - 1;
            }
        }
    }
}