using AutoMapper;
using Moq;
using OsDsII.api.Dtos;
using OsDsII.api.Dtos.Customers;
using OsDsII.api.Exceptions;
using OsDsII.api.Models;
using OsDsII.api.Repository;
using OsDsII.api.Repository.CustomersRepository;
using OsDsII.api.Services.Customers;


namespace CalculadoraSalario.Tests
{
    public class CustomersServiceTests
    {
        private readonly Mock<ICustomersRepository> _mockCustomersRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CustomersService _service;
        private List<CustomerDto> _lista;

        public CustomersServiceTests()
        {
            _mockCustomersRepository = new Mock<ICustomersRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new CustomersService(_mockCustomersRepository.Object, _mockMapper.Object);
            _lista = new List<CustomerDto>();

        }


        [Fact]
        public async Task GetCustomerAsync_CustomerExists_ReturnsCustomerDto()
        {
            // Arrange
            var customer = new Customer(1, "John Doe", "john.doe@example.com", "1234567890");
            var customerDto = new CustomerDto( "John Doe", "john.doe@example.com", "1234567890",null);
            
            _mockCustomersRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(customer);

            _mockMapper.Setup(mapper => mapper.Map<CustomerDto>(customer))
                .Returns(customerDto);

            // Act
            var result = await _service.GetCustomerAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerDto, result);
        }

        [Fact]
        public async Task GetCustomerAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _mockCustomersRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync((Customer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCustomerAsync(1));
        }

        [Fact]
        public async Task CreateAsync_CustomerDoesNotExist_AddsCustomer()
        {
            // Arrange
            var createCustomerDto = new CreateCustomerDto("John Doe", new object(), "john.doe@example.com", "1234567890");
            var customer = new Customer(0, "John Doe", "john.doe@example.com", "1234567890");

            _mockMapper.Setup(m => m.Map<Customer>(createCustomerDto)).Returns(customer);
            _mockCustomersRepository.Setup(repo => repo.FindUserByEmailAsync(createCustomerDto.Email))
                .ReturnsAsync((Customer)null);
            _mockCustomersRepository.Setup(repo => repo.AddCustomerAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.CreateAsync(createCustomerDto);

            // Assert
            _mockCustomersRepository.Verify(repo => repo.AddCustomerAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CustomerAlreadyExists_ThrowsConflictException()
        {
            // Arrange
            var createCustomerDto = new CreateCustomerDto("John Doe", new object(), "john.doe@example.com", "1234567890");
            var existingCustomer = new Customer(1, "John Doe", "john.doe@example.com", "1234567890");
            var customer = new Customer(0, "John Doe", "john.doe@example.com", "1234567890");

            _mockMapper.Setup(m => m.Map<Customer>(createCustomerDto)).Returns(customer);
            _mockCustomersRepository.Setup(repo => repo.FindUserByEmailAsync(createCustomerDto.Email))
                .ReturnsAsync(existingCustomer);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(createCustomerDto));
            Assert.Equal("Customer already exists", exception.Message);
        }


        [Fact]
        public async Task UpdateAsync_CustomerExists_UpdatesCustomer()
        {
            // Arrange
            var updateCustomerDto = new CreateCustomerDto("John Doe Updated", "John Doe Updated", "john.doe.updated@example.com", "0987654321");
            var existingCustomer = new Customer(1, "John Doe", "john.doe@example.com", "1234567890");
            _mockCustomersRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingCustomer);

            // Act
            await _service.UpdateAsync(1, updateCustomerDto);

            // Assert
            _mockCustomersRepository.Verify(repo => repo.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Once);
            Assert.Equal("John Doe Updated", existingCustomer.Name);
            Assert.Equal("john.doe.updated@example.com", existingCustomer.Email);
            Assert.Equal("0987654321", existingCustomer.Phone);
        }

        [Fact]
        public async Task UpdateAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var updateCustomerDto = new CreateCustomerDto("John Doe Updated", "John Doe Updated", "john.doe.updated@example.com", "0987654321");
            _mockCustomersRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync((Customer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(1, updateCustomerDto));
        }

        [Fact]
        public async Task DeleteAsync_CustomerExists_DeletesCustomer()
        {
            // Arrange
            var existingCustomer = new Customer(1, "John Doe", "john.doe@example.com", "1234567890");
            _mockCustomersRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingCustomer);

            // Act
            await _service.DeleteAsync(1);

            // Assert
            _mockCustomersRepository.Verify(repo => repo.DeleteCustomer(existingCustomer), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _mockCustomersRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync((Customer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(1));
        }
    }
}