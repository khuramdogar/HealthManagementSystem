using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration
{
    [TestFixture]
    public class OrderingIntegrationUnityConfigTests
    {
        [Test, Explicit]
        public void RegisterOrderingIntegrationComponents_CanResolveServices()
        {
            var container = new UnityContainer();

            container.RegisterComponents();

            var orderProcessingService= container.Resolve<IOrderProcessingService>();

            Assert.IsInstanceOf<OracleOrderProcessingService>(orderProcessingService);

            var watcher = container.Resolve<IInboundOrderReceiptWatcher>();

            Assert.IsInstanceOf<OracleInboundFtpWatcher>(watcher);
        }
    }
}
