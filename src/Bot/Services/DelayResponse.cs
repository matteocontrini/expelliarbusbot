namespace Bot.Services
{
    public class DelayResponse
    {
        public double Delay { get; set; }

        public int CurrentStopId { get; set; }

        public DelayResponse(double delay, int currentStopId)
        {
            this.Delay = delay;
            this.CurrentStopId = currentStopId;
        }

        public override string ToString()
        {
            return this.Delay.ToString();
        }
    }
}
