namespace Assets.Data
{
    public class TimeSignature : PositionElement
    {
        // How many beats to count within one measure
        public int Numerator { get; private set; }

        // What type of note gets the beat(quarter notes, eighth notes, etc)
        public int Denominator { get; private set; }

        public TimeSignature(int position, int numerator, int denominator) : base(position)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }
}
