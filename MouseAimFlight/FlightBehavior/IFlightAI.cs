namespace MouseAimFlight
{
    public interface IFlightAI
    {
        TargetData ComputeAI(TargetData targetData);
    }
}