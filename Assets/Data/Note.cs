namespace Assets.Data
{
    public class Note : PositionElement
    {
        public int NoteType { get; set; }
        public int Duration { get; set; }

        public Note(int position, int noteType, int duration) : base(position)
        {
            NoteType = noteType;
            Duration = duration;
        }
    }
}
