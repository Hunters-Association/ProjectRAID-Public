using System;
using System.Collections.Generic;

namespace ProjectRaid.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// 리스트를 일정 크기만큼 잘라서 반환 (Unity 2022.3 환경에서도 안전하게 사용 가능)
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(this List<T> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "청크 크기는 0보다 커야합니다.");

            for (int i = 0; i < source.Count; i += size)
                yield return source.GetRange(i, Math.Min(size, source.Count - i));
        }

        /// <summary>
        /// IEnumerable 버전 지원 (메모리를 덜 쓰지만 내부적으로 버퍼링이 필요)
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "청크 크기는 0보다 커야합니다.");

            List<T> buffer = new List<T>(size);
            foreach (var item in source)
            {
                buffer.Add(item);
                if (buffer.Count >= size)
                {
                    yield return new List<T>(buffer);
                    buffer.Clear();
                }
            }
            if (buffer.Count > 0)
                yield return buffer;
        }
    }
}