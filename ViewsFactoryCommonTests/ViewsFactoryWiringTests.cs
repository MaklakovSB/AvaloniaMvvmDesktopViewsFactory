using AvaloniaAppWithCommunityToolkitNET8.ViewModels;
using AvaloniaAppWithCommunityToolkitNET8.Views;
using AvaloniaMvvmDesktopViewsFactory.Factories;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using AvaloniaMvvmDesktopViewsFactory.Service;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ViewsFactoryTests
{
    [TestFixture]
    public class ViewsFactoryWiringTests
    {
        class DummyVm : IUnique
        {
            public Guid Uid { get; set; }
        }

        private ViewsFactory _factory;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IGuidProvider, GuidProvider>();

            var viewAssembly = typeof(MainWindowView).Assembly;
            var viewModelAssembly = typeof(MainWindowViewModel).Assembly;
            _factory = new ViewsFactory(new GuidProvider(), viewAssembly, viewModelAssembly);
        }

        [TearDown]
        public void TearDown()
        {
            _factory?.Dispose();
        }

        [Test]
        public void GetViewType_ByConvention_ReturnsCorrectType()
        {
            var vm = new MainWindowViewModel(_factory!);
            var method = typeof(ViewsFactory)
                .GetMethod("GetViewType", BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Assert.Fail("Method GetViewType not found");
                return;
            }

            var genericMethod = method.MakeGenericMethod(typeof(MainWindowViewModel));
            var type = genericMethod.Invoke(_factory, new object[] { vm }) as Type;

            Assert.That(type, Is.EqualTo(typeof(MainWindowView)));
        }

        [Test]
        public void GetViewType_Throws_ForUnknownViewModel()
        {
            // 1. Arrange.
            var vm = new DummyVm();

            // 2. Act - получаем метод через рефлексию.
            var method = typeof(ViewsFactory)
                .GetMethod("GetViewType", BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Assert.Fail("Method GetViewType not found");
                return;
            }

            var genericMethod = method.MakeGenericMethod(vm.GetType());

            // 3. Assert - проверяем исключение.
            var ex = Assert.Throws<TargetInvocationException>(() =>
                genericMethod.Invoke(_factory, new object[] { vm }));

            // Проверяем тип внутреннего исключения.
            var innerEx = ex.InnerException;
            Assert.That(innerEx, Is.Not.Null);
            Assert.That(innerEx, Is.InstanceOf<InvalidOperationException>());

            // Проверяем содержание сообщения.
            Assert.That(innerEx.Message, Does.Contain("not from any registered ViewModel assembly")
                              .Or.Contains("Could not find View for"));
        }

        [Test]
        public void EnsureViewModelHasUid_AssignsUid()
        {
            var vm = new MainWindowViewModel(_factory!);
            var method = typeof(ViewsFactory)
                .GetMethod("EnsureViewModelHasUid", BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Assert.Fail("Method EnsureViewModelHasUid not found");
                return;
            }

            var genericMethod = method.MakeGenericMethod(typeof(MainWindowViewModel));
            genericMethod.Invoke(_factory, new object[] { vm });

            Assert.That(vm.Uid, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _factory.Dispose());
            Assert.DoesNotThrow(() => _factory.Dispose());
        }
    }
}