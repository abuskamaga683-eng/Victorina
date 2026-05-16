namespace Victorina.Core.Models
{
    public class Question
    {
        public int    Id        { get; set; }
        public int    PoolIndex { get; set; } // 0=Общая база, 1=Блиц, 2=Черный ящик
        public string Text      { get; set; }
        public string Answer    { get; set; }
    }
}
