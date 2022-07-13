using System;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;

namespace OrderSaverFunc.Models;
internal class Order
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("BuyerId")]
    public string BuyerId { get; set; }

    [JsonProperty("OrderDate")]
    public DateTimeOffset OrderDate { get; set; }

    [JsonProperty("ShipToAddress")]
    public Address ShipToAddress { get; set; }

    [JsonProperty("OrderItems")]
    public OrderItem[] OrderItems { get; set; }

    [JsonProperty("FinalPrice")]
    public decimal FinalPrice { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public decimal Total()
    {
        var total = 0m;
        foreach (var item in OrderItems)
        {
            total += item.UnitPrice * item.Units;
        }
        return total;
    }

}
