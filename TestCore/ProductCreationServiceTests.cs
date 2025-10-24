using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using Grocery.Core.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCore
{
    [TestFixture]
    public class ProductCreationServiceTests
    {
        private Client _adminClient;
        private Mock<IProductRepository> _mockRepository;
        private IProductCreationService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IProductRepository>();
            var productService = new ProductService(_mockRepository.Object);
            _service = new ProductCreationService(productService);

            _adminClient = new Client(1, "Admin", "admin@test.com", "pwd123")
            {
                Role = Role.Admin
            };
        }

        // --------------------------------------------------
        // TC19-1 – Happy Path: geldig product wordt toegevoegd
        // --------------------------------------------------
        [Test]
        public void TC19_1_AddValidProduct_ShouldSaveToRepository()
        {
            // Arrange
            var product = new Product(0, "Banaan", 100, new DateOnly(2026, 1, 1), 1.50m);
            _mockRepository.Setup(r => r.Add(It.IsAny<Product>())).Returns(product);

            // Act
            var result = _service.CreateProduct(_adminClient, product);

            // Assert
            Assert.That(result.Name, Is.EqualTo("Banaan"));
            _mockRepository.Verify(r => r.Add(It.Is<Product>(p => p.Name == "Banaan")), Times.Once);
        }

        // --------------------------------------------------
        // TC19-2 – Validatie: ongeldige invoer
        // --------------------------------------------------
        [Test]
        public void TC19_2_InvalidProduct_ShouldThrowValidationErrors()
        {
            var invalidProducts = new[]
            {
                new Product(0, "", 10, new DateOnly(2026, 1, 1), 1.50m),           // Naam leeg
                new Product(0, "Appel", -5, new DateOnly(2026, 1, 1), 1.50m),      // Negatieve voorraad
                new Product(0, "Peer", 10, new DateOnly(2026, 1, 1), 0m),          // Prijs 0
                new Product(0, "Melk", 10, new DateOnly(2020, 1, 1), 1.50m),       // Verlopen datum
            };

            foreach (var invalidProduct in invalidProducts)
            {
                var ex = Assert.Throws<ArgumentException>(() =>
                    _service.CreateProduct(_adminClient, invalidProduct));

                Assert.That(ex.Message, Is.Not.Empty);
            }

            // Verify Add() is never called when validation fails
            _mockRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        }

        // --------------------------------------------------
        // TC19-3 – Integratie: service roept repository aan
        // --------------------------------------------------
        [Test]
        public void TC19_3_Service_ShouldCallRepositoryAddMethod()
        {
            var product = new Product(0, "Chocolade", 20, new DateOnly(2026, 5, 10), 2.99m);
            _mockRepository.Setup(r => r.Add(It.IsAny<Product>())).Returns(product);

            var created = _service.CreateProduct(_adminClient, product);

            Assert.That(created.Price, Is.EqualTo(2.99m));
            _mockRepository.Verify(r => r.Add(It.Is<Product>(p => p.Name == "Chocolade")), Times.Once);
        }

        // --------------------------------------------------
        // TC19-4 – "Usability" check: geen crash bij toevoegen
        // --------------------------------------------------
        [Test]
        public void TC19_4_AddProduct_ShouldNotCrashOrThrow()
        {
            var product = new Product(0, "Koffie", 50, new DateOnly(2026, 12, 31), 3.25m);
            _mockRepository.Setup(r => r.Add(It.IsAny<Product>())).Returns(product);

            Assert.DoesNotThrow(() => _service.CreateProduct(_adminClient, product),
                "Het toevoegen van een geldig product mag geen crash veroorzaken.");

            _mockRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        }
    }
}
