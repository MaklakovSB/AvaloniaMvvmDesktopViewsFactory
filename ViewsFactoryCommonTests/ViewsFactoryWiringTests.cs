using System.Reflection;
using AvaloniaApplicationSample.ViewModels;
using AvaloniaApplicationSample.Views;
using AvaloniaMvvmDesktopViewsFactory.Factories;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using AvaloniaMvvmDesktopViewsFactory.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ViewsFactoryTests
{
    [TestFixture]
    public class ViewsFactoryWiringTests
    {
        private ViewsFactory _factory;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IGuidProvider, GuidProvider>();
            var viewAssembly = typeof(MainWindowView).Assembly;
            _factory = new ViewsFactory(new GuidProvider(), viewAssembly);
        }

        [Test]
        public void GetViewType_ByConvention_ReturnsCorrectType()
        {
            var vm = new MainWindowViewModel(_factory);
            var method = typeof(ViewsFactory)
                .GetMethod("GetViewType", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeof(MainWindowViewModel));
            var type = method.Invoke(_factory, new object[] { vm }) as Type;
            Assert.AreEqual(typeof(MainWindowView), type);
        }

        [Test]
        public void GetViewType_Throws_ForUnknownViewModel()
        {
            var vm = new DummyVm();
            var method = typeof(ViewsFactory)
                .GetMethod("GetViewType", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeof(DummyVm));

            // Ожидаем, что будет выброшено исключение
            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(_factory, new object[] { vm }));

            Assert.IsNotNull(ex);
            // Проверяем, что внутреннее исключение - то, что мы ожидаем
            Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            StringAssert.Contains("Could not find View for", ex.InnerException.Message);
        }

        [Test]
        public void EnsureViewModelHasUid_AssignsUid()
        {
            var vm = new MainWindowViewModel(_factory);
            typeof(ViewsFactory)
                .GetMethod("EnsureViewModelHasUid", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeof(MainWindowViewModel))
                .Invoke(_factory, new object[] { vm });
            Assert.AreNotEqual(Guid.Empty, vm.Uid);
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _factory.Dispose());
            Assert.DoesNotThrow(() => _factory.Dispose());
        }

        class DummyVm : IUnique
        {
            public Guid Uid { get; set; }
        }
    }
}