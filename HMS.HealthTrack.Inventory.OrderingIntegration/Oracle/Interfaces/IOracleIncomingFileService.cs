namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal interface IOracleIncomingFileService
    {
        void ProcessFile(OracleInboundFile inboundFile);
    }
}