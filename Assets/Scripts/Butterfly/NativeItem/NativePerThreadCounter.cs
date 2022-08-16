using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Butterfly.NativeItem
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public unsafe struct NativePerThreadCounter
    {
        // 指向已分配计数的实际指针需要放宽限制，以便可以使用此容器安排作业
        [NativeDisableUnsafePtrRestriction]
        internal int* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;

        // 处置哨兵跟踪内存泄漏。它是托管类型，因此在调度作业时将其清除为 null
        // 该作业无法处置容器，并且在作业运行之前没有其他人可以处置它，因此可以不传递它
        // 此属性是必需的，没有它，本机容器无法传递给作业，因为这将使作业访问托管对象
        [NativeSetClassTypeToNullOnSchedule]
        private DisposeSentinel m_DisposeSentinel;
#endif

        // 跟踪分配内存的位置
        private Allocator m_AllocatorLabel;

        public const int IntsPerCacheLine = JobsUtility.CacheLineSize / sizeof(int);

        public NativePerThreadCounter(Allocator label)
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

            // 每个潜在的工作索引 JobsUtility.MaxJobThreadCount 一个完整的缓存行（整数每个缓存行大小的整数）
            m_Counter = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>() * IntsPerCacheLine * JobsUtility.MaxJobThreadCount, 4, label);

            // 创建一个处置哨兵来跟踪内存泄漏。这也创建了 AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
#endif

            // 将Count初始化为0，避免数据未初始化
            Count = 0;
        }

        public void Increment()
        {
            // 验证调用方对此数据是否具有写入权限。
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
                var count = 0;
                for(var i = 0; i < JobsUtility.MaxJobThreadCount; i++)
                {
                    count += m_Counter[IntsPerCacheLine * i];
                }
                return count;
            }
            set
            {
                // 验证调用方对此数据是否具有写入权限。
                // 这是竞争条件保护，如果没有这些检查，AtomicSafetyHandle 将毫无用处
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

                // 清除所有本地缓存的计数，
                // 将第一个设置为所需的值
                for(var i = 0; i < JobsUtility.MaxJobThreadCount; i++)
                {
                    m_Counter[IntsPerCacheLine * i] = 0;
                }
                *m_Counter = value;
            }
        }

        public bool IsCreated => m_Counter != null;

        public void Dispose()
        {
            // 让 dispose sentinel 知道数据已被释放，因此它不会报告任何内存泄漏
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            UnsafeUtility.Free(m_Counter, m_AllocatorLabel);
            m_Counter = null;
        }
    }
}