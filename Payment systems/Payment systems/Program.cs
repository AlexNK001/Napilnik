using System;
using System.Linq;

public class Program
{
    private static void Main()
    {
        Order order = new Order(id: 77, amount: 5);

        IPaymentSystem[] paymentSystems = new IPaymentSystem[]
        {
            new PaymentSystem(
                WebsiteStorage.System1, 
                LinkFabric.HashComponents.CreateMD5()),

            new PaymentSystem(
                WebsiteStorage.System2, 
                LinkFabric.HashComponents.CreateMD5(),
                LinkFabric.OrderBasedComponent.CreateSumBasedComponent()),

            new PaymentSystem(
                WebsiteStorage.System3,
                LinkFabric.HashComponents.CreateSHA1(),
                LinkFabric.OrderBasedComponent.CreateSumIDComponent(),
                LinkFabric.Security.KeySystem())
        };

        for (int i = 0; i < paymentSystems.Length; i++)
            Console.WriteLine(paymentSystems[i].GetPayingLink(order));
    }
}

public class Order
{
    public Order(int id, int amount)
    {
        if (id <= 0)
            throw new InvalidOperationException(nameof(id));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        Id = id;
        Amount = amount;
    }

    public int Id { get; }
    public int Amount { get; }
}

interface IPaymentSystem
{
    string GetPayingLink(Order order);
}

public interface ILinkComponent
{
    string GetPartLink(Order order);
}

public class PaymentSystem : IPaymentSystem
{
    private readonly ILinkComponent[] _linkComponents;
    private readonly string _website;

    public PaymentSystem(string website, params ILinkComponent[] linkComponents)
    {
        _website = website ?? throw new ArgumentNullException(nameof(website));

        if (linkComponents == null)
            throw new ArgumentNullException(nameof(linkComponents));

        if (linkComponents.Any(component => component == null))
            throw new ArgumentNullException($"{nameof(linkComponents)} contains an element equal to null");

        _linkComponents = linkComponents;
    }

    public string GetPayingLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        string link = _website;

        for (int i = 0; i < _linkComponents.Length; i++)
            link += _linkComponents[i].GetPartLink(order);

        return link;
    }
}

public class HashLinkComponent : ILinkComponent
{
    private readonly string _name;

    public HashLinkComponent(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string GetPartLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return $"{_name} {order.GetHashCode()}";
    }
}

public class SumBasedComponent : ILinkComponent
{
    private readonly string _name;

    public SumBasedComponent(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string GetPartLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return $"{_name} {order.Amount.GetHashCode()}";
    }
}

public class SumIDComponent : ILinkComponent
{
    private readonly string _name;

    public SumIDComponent(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string GetPartLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return $"{_name} {order.Id.GetHashCode()}";
    }
}

public class KeySystem : ILinkComponent
{
    private readonly string _name;
    private readonly int _key;

    public KeySystem(string name, int key)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));

        if (key == 0 && key == 1)
            throw new InvalidOperationException($"{key} does not fit as a key.");

        _key = key;
    }

    public string GetPartLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        int key = (order.Id + order.Amount) * _key;

        return $"{_name} {key}";
    }
}

public static class LinkFabric
{
    public static class HashComponents
    {
        public static ILinkComponent CreateMD5()
        {
            return new HashLinkComponent("MD5");
        }

        public static ILinkComponent CreateSHA1()
        {
            return new HashLinkComponent("SHA1");
        }
    }

    public static class OrderBasedComponent
    {
        public static ILinkComponent CreateSumBasedComponent()
        {
            return new SumBasedComponent("SumBasedComponent");
        }

        public static ILinkComponent CreateSumIDComponent()
        {
            return new SumIDComponent("SumIDComponent");
        }
    }

    public static class Security
    {
        public static ILinkComponent KeySystem()
        {
            return new KeySystem("KeySystem", key: 3);
        }
    }
}

public static class WebsiteStorage
{
    public const string System1 = "pay.system1.ru/order?amount=12000RUB&hash=";
    public const string System2 = "order.system2.ru/pay?hash=";
    public const string System3 = "system3.com/pay?amount=12000&curency=RUB&hash=";
}



