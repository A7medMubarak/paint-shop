using PaintShop.Application.DTOs.Customers;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class CustomerServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenCustomersExist_ReturnsAllCustomers()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Jane Smith", CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(customers: customers);
        var service = new CustomerService(ctx);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomerDetail()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", Phone = "123456789", CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(customers: customers);
        var service = new CustomerService(ctx);

        var result = await service.GetByIdAsync(1);

        result.Name.Should().Be("John Doe");
        result.Phone.Should().Be("123456789");
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesCustomer()
    {
        var ctx = MockDbContext.Create();
        var service = new CustomerService(ctx);

        var result = await service.CreateAsync(new CreateCustomerRequest { Name = "New Customer" });

        result.Name.Should().Be("New Customer");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesCustomer()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(customers: customers);
        var service = new CustomerService(ctx);

        var result = await service.UpdateAsync(1, new CreateCustomerRequest { Name = "Updated Name", Phone = "987654321" });

        result.Name.Should().Be("Updated Name");
        result.Phone.Should().Be("987654321");
    }

    [Fact]
    public async Task GetFilteredAsync_WithSearch_FiltersByName()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Jane Smith", CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(customers: customers);
        var service = new CustomerService(ctx);

        var result = await service.GetFilteredAsync(new CustomerFilterRequest { Search = "Jane", Page = 1, PageSize = 20 });

        result.Items.Should().ContainSingle();
        result.Items[0].Name.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetFilteredAsync_WithPagination_ReturnsPagedResults()
    {
        var customers = Enumerable.Range(1, 25).Select(i => new Customer
        {
            Id = i,
            Name = $"Customer {i}",
            CreatedAt = DateTime.Now
        }).ToList();
        var ctx = MockDbContext.Create(customers: customers);
        var service = new CustomerService(ctx);

        var result = await service.GetFilteredAsync(new CustomerFilterRequest { Page = 1, PageSize = 10 });

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }
}
