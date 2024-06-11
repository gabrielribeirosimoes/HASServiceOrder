using AutoMapper;
using Microsoft.AspNetCore.Routing;
using Moq;
using OsDsII.api.Dtos;
using OsDsII.api.Dtos.ServiceOrders;
using OsDsII.api.Exceptions;
using OsDsII.api.Models;
using OsDsII.api.Repository.CustomersRepository;
using OsDsII.api.Repository.ServiceOrderRepository;
using OsDsII.api.Services.ServiceOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Has_Service_Order.Tests.Services
{
    public class ServiceOrderServiceTest
    {
        private readonly Mock<IServiceOrderRepository> _serviceOrderRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICustomersRepository> _serviceCustomerMock;
        private readonly ServiceOrderService _service;

        public ServiceOrderServiceTest()
        {
            _serviceOrderRepositoryMock = new Mock<IServiceOrderRepository>();
            _mapperMock = new Mock<IMapper>();
            _serviceCustomerMock = new Mock<ICustomersRepository>();
            _service = new ServiceOrderService(_serviceOrderRepositoryMock.Object, _serviceCustomerMock.Object, _mapperMock.Object);
        }
        [Fact]
        public async Task GetAllAsync_ShouldReturnServiceOrders()
        {
            // Arrange
            var serviceOrders = new List<ServiceOrder>
            {
                new ServiceOrder { Id = 1, Description = "Service 1", Price = 100, Status = StatusServiceOrder.OPEN },
                new ServiceOrder { Id = 2, Description = "Service 2", Price = 200, Status = StatusServiceOrder. OPEN }
            };

            var serviceOrderDtos = new List<ServiceOrderDto>
            {
                new ServiceOrderDto(1, "Service 1", 100, StatusServiceOrder.OPEN, DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), new List<CommentDto>()),
                new ServiceOrderDto(2, "Service 2", 200, StatusServiceOrder.OPEN, DateTimeOffset.Now, DateTimeOffset.Now.AddHours(2), new List<CommentDto>())
            };

            _serviceOrderRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(serviceOrders);
            _mapperMock.Setup(mapper => mapper.Map<List<ServiceOrderDto>>(serviceOrders)).Returns(serviceOrderDtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Equal(serviceOrderDtos, result);
        }

        [Fact]
        public async Task GetServiceOrderAsync_ShouldReturnServiceOrderDto_WhenServiceOrderExists()
        {
            // Arrange
            var serviceOrder = new ServiceOrder { Id = 1, Description = "Service 1", Price = 100, Status = StatusServiceOrder.OPEN };
            var serviceOrderDto = new ServiceOrderDto(1, "Service 1", 100, StatusServiceOrder.OPEN, DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), new List<CommentDto>());

            _serviceOrderRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(serviceOrder);
            _mapperMock.Setup(mapper => mapper.Map<ServiceOrderDto>(serviceOrder)).Returns(serviceOrderDto);

            // Act
            var result = await _service.GetServiceOrderAsync(1);

            // Assert
            Assert.Equal(serviceOrderDto, result);
        }

        [Fact]
        public async Task GetServiceOrderAsync_ShouldThrowNotFoundException_WhenServiceOrderDoesNotExist()
        {
            // Arrange
            _serviceOrderRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((ServiceOrder)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetServiceOrderAsync(1));
        }

        [Fact]
        public async Task CreateServiceOrderAsync_ShouldCreateAndReturnNewServiceOrderDto()
        {
            // Arrange
            var createServiceOrderDto = new CreateServiceOrderDto("New Service", 300, 1);
            var customer = new Customer { Id = 1 };
            var serviceOrder = new ServiceOrder { Id = 1, Description = "New Service", Price = 300, Status = StatusServiceOrder.OPEN, OpeningDate = DateTimeOffset.Now };
            var newServiceOrderDto = new NewServiceOrderDto(1, "New Service", 300, StatusServiceOrder.OPEN, DateTimeOffset.Now, null, 1);

            _serviceCustomerMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(customer);
            _mapperMock.Setup(mapper => mapper.Map<ServiceOrder>(createServiceOrderDto)).Returns(serviceOrder);
            _serviceOrderRepositoryMock.Setup(repo => repo.AddAsync(serviceOrder)).Returns(Task.CompletedTask);
            _mapperMock.Setup(mapper => mapper.Map<NewServiceOrderDto>(serviceOrder)).Returns(newServiceOrderDto);

            // Act
            var result = await _service.CreateServiceOrderAsync(createServiceOrderDto);

            // Assert
            Assert.Equal(newServiceOrderDto, result);
        }

        [Fact]
        public async Task CreateServiceOrderAsync_ShouldThrowBadRequest_WhenCustomerDoesNotExist()
        {
            // Arrange
            var createServiceOrderDto = new CreateServiceOrderDto("New Service", 300, 1);

            _serviceCustomerMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Customer)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequest>(() => _service.CreateServiceOrderAsync(createServiceOrderDto));
        }

        [Fact]
        public async Task FinishServiceOrderAsync_ShouldFinishServiceOrder_WhenServiceOrderExists()
        {
            // Arrange
            var serviceOrder = new ServiceOrder { Id = 1, Description = "Service 1", Price = 100, Status = StatusServiceOrder.OPEN };

            _serviceOrderRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(serviceOrder);
            _serviceOrderRepositoryMock.Setup(repo => repo.FinishAsync(serviceOrder)).Returns(Task.CompletedTask);

            // Act
            await _service.FinishServiceOrderAsync(1);

            // Assert
            _serviceOrderRepositoryMock.Verify(repo => repo.FinishAsync(serviceOrder), Times.Once);
        }

        [Fact]
        public async Task FinishServiceOrderAsync_ShouldThrowNotFoundException_WhenServiceOrderDoesNotExist()
        {
            // Arrange
            _serviceOrderRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((ServiceOrder)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.FinishServiceOrderAsync(1));
        }

        [Fact]
        public async Task CancelServiceOrderAsync_ShouldCancelServiceOrder_WhenServiceOrderExists()
        {
            // Arrange
            var serviceOrder = new ServiceOrder { Id = 1, Description = "Service 1", Price = 100, Status = StatusServiceOrder.OPEN };

            _serviceOrderRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(serviceOrder);
            _serviceOrderRepositoryMock.Setup(repo => repo.CancelAsync(serviceOrder)).Returns(Task.CompletedTask);

            // Act
            await _service.CancelServiceOrderAsync(1);

            // Assert
            _serviceOrderRepositoryMock.Verify(repo => repo.CancelAsync(serviceOrder), Times.Once);
        }

        [Fact]
        public async Task CancelServiceOrderAsync_ShouldThrowNotFoundException_WhenServiceOrderDoesNotExist()
        {
            // Arrange
            _serviceOrderRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((ServiceOrder)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CancelServiceOrderAsync(1));
        }


    }
}
