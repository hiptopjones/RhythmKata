namespace Assets.Data
{
    public class Section : PositionElement
    {
        public string Name { get; set; }

        public Section(int position, string name) : base(position)
        {
            Name = name;
        }
    }
}
