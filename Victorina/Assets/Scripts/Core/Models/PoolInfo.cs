namespace Victorina.Core.Models
{
    public static class PoolInfo
    {
        public const int Count = 3;

        public static readonly string[] Names =
        {
            "Общая база",
            "Блиц",
            "Черный ящик"
        };

        public static bool IsBlitz(int poolIndex)
            => poolIndex == 1;

        public static bool IsBlackBox(int poolIndex)
            => poolIndex == 2;

        public static int GetPoints(int poolIndex)
            => poolIndex == 2 ? 2 : 1;
    }
}