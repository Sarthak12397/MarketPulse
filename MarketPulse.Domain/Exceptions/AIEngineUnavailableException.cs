public sealed class AIEngineUnavailableException : Exception
{
    public AIEngineUnavailableException(
        int? statusCode,
        string detail)
        : base(
            $"AI Signal Engine unavailable. HTTP {statusCode}: {detail}")
    {
    }
}