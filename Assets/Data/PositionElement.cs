namespace Assets.Data
{
    public abstract class PositionElement
    {
        public int PositionInSteps { get; private set; }

        public PositionElement(int position)
        {
            PositionInSteps = position;
        }
    }
}
