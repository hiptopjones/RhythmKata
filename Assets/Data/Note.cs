namespace Assets.Data
{
    public class Note : PositionElement
    {
        public MidiNote MidiNote { get; set; }
        public int Duration { get; set; }

        public Note(int position, MidiNote midiNote, int duration) : base(position)
        {
            MidiNote = midiNote;
            Duration = duration;
        }
    }
}
