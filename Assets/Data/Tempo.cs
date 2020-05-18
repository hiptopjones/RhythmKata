namespace Assets.Data
{
    public class Tempo : PositionElement
    {
        public double BeatsPerMinute { get; set; }

        public Tempo(int position, double beatsPerMinute) : base(position)
        {
            BeatsPerMinute = beatsPerMinute;
        }
    }
}
