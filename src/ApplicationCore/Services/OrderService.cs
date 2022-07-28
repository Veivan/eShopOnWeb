using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);

        Guard.Against.NullBasket(basketId, basket);
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);

        await _orderRepository.AddAsync(order);

        //await UploadOrderAsync(order);
        await SendOrderMessageAsync(order);
    }
    private async Task UploadOrderAsync(Order order)
    {
        var body = order.ToJson<Order>();
        HttpClient Client = new HttpClient();
        var jsonContent = new StringContent(body, Encoding.UTF8, "application/json");
        //var url = "http://localhost:7071/api/OrderUploader";
        //var url = "https://orderuploaderapp.azurewebsites.net/api/OrderUploader";
        //var url = "http://localhost:7062/api/OrderSaver";
        var url = "https://ordersaverfunc20220714012345.azurewebsites.net/api/OrderSaver";
        var response = await Client.PostAsync(url, jsonContent);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
    }

    private async Task SendOrderMessageAsync(Order order)
    {
        const string ServiceBusConnectionString = "Endpoint=sb://karafsbusns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IN7iq/kKgdTbZC9R8ag9t6HpckzTOAVUazQqnKO/v2U=";
        const string QueueName = "orders";

        await using var client = new ServiceBusClient(ServiceBusConnectionString);

        await using ServiceBusSender sender = client.CreateSender(QueueName);
        try
        {
            var body = order.ToJson<Order>();
            var message = new ServiceBusMessage(body);
            await sender.SendMessageAsync(message);
        }
        catch (System.Exception exception)
        {
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
