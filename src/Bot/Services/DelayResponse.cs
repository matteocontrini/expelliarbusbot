namespace Bot.Services
{
    public class DelayResponse
    {
        public double Delay { get; set; }

        public int PreviousStopId { get; set; }

        public DelayResponse(double delay, int previousStopId)
        {
            this.Delay = delay;
            this.PreviousStopId = previousStopId;
        }

        public override string ToString()
        {
            return this.Delay.ToString();
        }
    }
}
