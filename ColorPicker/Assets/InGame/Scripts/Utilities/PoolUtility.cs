using System.Collections.Generic;

namespace ColorPicker.InGame
{
    /// <summary>
    /// Thread-local 컬렉션 풀 유틸리티 (GC 최소화)
    /// </summary>
    public static class PoolUtility
    {
        #region HashSet Pool
        [System.ThreadStatic]
        private static HashSet<MiniGameType> _missionTypeSet;

        [System.ThreadStatic]
        private static HashSet<int> _intSet;

        [System.ThreadStatic]
        private static HashSet<string> _stringSet;

        /// <summary>
        /// HashSet 풀에서 인스턴스 가져오기
        /// </summary>
        public static HashSet<T> GetHashSet<T>()
        {
            if (typeof(T) == typeof(MiniGameType))
            {
                if (_missionTypeSet == null)
                    _missionTypeSet = new HashSet<MiniGameType>();
                
                return _missionTypeSet as HashSet<T>;
            }

            if (typeof(T) == typeof(int))
            {
                if (_intSet == null)
                    _intSet = new HashSet<int>();
                
                return _intSet as HashSet<T>;
            }

            if (typeof(T) == typeof(string))
            {
                if (_stringSet == null)
                    _stringSet = new HashSet<string>();
                
                return _stringSet as HashSet<T>;
            }

            // 다른 타입은 새로 생성 (풀 미지원)
            return new HashSet<T>();
        }

        /// <summary>
        /// HashSet 풀로 반환 (내용 초기화)
        /// </summary>
        public static void ReturnHashSet<T>(HashSet<T> set)
        {
            if (set == null) return;

            set.Clear();
        }
        #endregion

        #region List Pool
        [System.ThreadStatic]
        private static List<MissionInstance> _missionInstanceList;

        [System.ThreadStatic]
        private static List<int> _intList;

        [System.ThreadStatic]
        private static List<string> _stringList;

        /// <summary>
        /// List 풀에서 인스턴스 가져오기
        /// </summary>
        public static List<T> GetList<T>()
        {
            if (typeof(T) == typeof(MissionInstance))
            {
                if (_missionInstanceList == null)
                    _missionInstanceList = new List<MissionInstance>();
                
                return _missionInstanceList as List<T>;
            }

            if (typeof(T) == typeof(int))
            {
                if (_intList == null)
                    _intList = new List<int>();
                
                return _intList as List<T>;
            }

            if (typeof(T) == typeof(string))
            {
                if (_stringList == null)
                    _stringList = new List<string>();
                
                return _stringList as List<T>;
            }

            // 다른 타입은 새로 생성 (풀 미지원)
            return new List<T>();
        }

        /// <summary>
        /// List 풀로 반환 (내용 초기화)
        /// </summary>
        public static void ReturnList<T>(List<T> list)
        {
            if (list == null) return;

            list.Clear();
        }
        #endregion

        #region Dictionary Pool
        [System.ThreadStatic]
        private static Dictionary<int, int> _intIntDict;

        [System.ThreadStatic]
        private static Dictionary<string, MissionInstance> _stringMissionDict;

        [System.ThreadStatic]
        private static Dictionary<MiniGameType, MiniGameTag> _missionTypeTagDict;

        /// <summary>
        /// Dictionary 풀에서 인스턴스 가져오기
        /// </summary>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
        {
            if (typeof(TKey) == typeof(int) && typeof(TValue) == typeof(int))
            {
                if (_intIntDict == null)
                    _intIntDict = new Dictionary<int, int>();
                
                return _intIntDict as Dictionary<TKey, TValue>;
            }

            if (typeof(TKey) == typeof(string) && typeof(TValue) == typeof(MissionInstance))
            {
                if (_stringMissionDict == null)
                    _stringMissionDict = new Dictionary<string, MissionInstance>();
                
                return _stringMissionDict as Dictionary<TKey, TValue>;
            }

            if (typeof(TKey) == typeof(MiniGameType) && typeof(TValue) == typeof(MiniGameTag))
            {
                if (_missionTypeTagDict == null)
                    _missionTypeTagDict = new Dictionary<MiniGameType, MiniGameTag>();
                
                return _missionTypeTagDict as Dictionary<TKey, TValue>;
            }

            // 다른 타입은 새로 생성 (풀 미지원)
            return new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Dictionary 풀로 반환 (내용 초기화)
        /// </summary>
        public static void ReturnDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict == null) return;

            dict.Clear();
        }
        #endregion

        #region Array Pool
        [System.ThreadStatic]
        private static int[] _intArray;

        [System.ThreadStatic]
        private static float[] _floatArray;

        [System.ThreadStatic]
        private static string[] _stringArray;

        /// <summary>
        /// 배열 풀에서 인스턴스 가져오기 (크기 지정)
        /// </summary>
        public static T[] GetArray<T>(int size)
        {
            if (typeof(T) == typeof(int))
            {
                if (_intArray == null || _intArray.Length < size)
                    _intArray = new int[size];
                
                return _intArray as T[];
            }

            if (typeof(T) == typeof(float))
            {
                if (_floatArray == null || _floatArray.Length < size)
                    _floatArray = new float[size];
                
                return _floatArray as T[];
            }

            if (typeof(T) == typeof(string))
            {
                if (_stringArray == null || _stringArray.Length < size)
                    _stringArray = new string[size];
                
                return _stringArray as T[];
            }

            // 다른 타입은 새로 생성 (풀 미지원)
            return new T[size];
        }

        /// <summary>
        /// 배열 풀로 반환 (Thread-local이므로 실제 반환 작업 불필요)
        /// </summary>
        public static void ReturnArray<T>(T[] array)
        {
            // Thread-local storage이므로 별도 반환 작업 불필요
            // 다음 GetArray 호출 시 자동으로 재사용됨
        }
        #endregion

        #region StringBuilder Pool
        [System.ThreadStatic]
        private static System.Text.StringBuilder _stringBuilder;

        /// <summary>
        /// StringBuilder 풀에서 인스턴스 가져오기
        /// </summary>
        public static System.Text.StringBuilder GetStringBuilder()
        {
            if (_stringBuilder == null)
                _stringBuilder = new System.Text.StringBuilder();
            
            return _stringBuilder;
        }

        /// <summary>
        /// StringBuilder 풀로 반환 (내용 초기화)
        /// </summary>
        public static void ReturnStringBuilder(System.Text.StringBuilder sb)
        {
            if (sb == null) return;

            sb.Clear();
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 모든 풀 초기화 (메모리 절약 필요 시 호출)
        /// </summary>
        public static void ClearAllPools()
        {
            // HashSet 풀
            _missionTypeSet?.Clear();
            _intSet?.Clear();
            _stringSet?.Clear();

            // List 풀
            _missionInstanceList?.Clear();
            _intList?.Clear();
            _stringList?.Clear();

            // Dictionary 풀
            _intIntDict?.Clear();
            _stringMissionDict?.Clear();
            _missionTypeTagDict?.Clear();

            // StringBuilder 풀
            _stringBuilder?.Clear();

            // 배열은 초기화 불필요 (재사용 가능)
        }

        /// <summary>
        /// 모든 풀 해제 (메모리 완전 해제 필요 시 호출)
        /// </summary>
        public static void ReleaseAllPools()
        {
            // HashSet 풀
            _missionTypeSet = null;
            _intSet = null;
            _stringSet = null;

            // List 풀
            _missionInstanceList = null;
            _intList = null;
            _stringList = null;

            // Dictionary 풀
            _intIntDict = null;
            _stringMissionDict = null;
            _missionTypeTagDict = null;

            // StringBuilder 풀
            _stringBuilder = null;

            // 배열 풀
            _intArray = null;
            _floatArray = null;
            _stringArray = null;
        }
        #endregion
    }
}