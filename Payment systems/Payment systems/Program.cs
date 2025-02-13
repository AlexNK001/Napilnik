using System;
using System.Security.Cryptography;
using System.Text;

public class Program
{
    private static void Main()
    {
        Order order = new Order(id: 77, amount: 12000);

        IPaymentSystem[] paymentSystems = new IPaymentSystem[]
        {
            new PaySystem1(new HashGeneratorMD5()),
            new PaySystem2(new HashGeneratorMD5()),
            new PaySystem3(new HashGeneratorSHA1(), key: 111)
        };

        for (int i = 0; i < paymentSystems.Length; i++)
            Console.WriteLine(paymentSystems[i].GetPayingLink(order));
    }
}

public class Order
{
    public Order(int id, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        Id = id;
        Amount = amount;
    }

    public int Id { get; }
    public int Amount { get; }
}

public interface IPaymentSystem
{
    string GetPayingLink(Order order);
}

public interface IHashGenerator
{
    string GetHash(int value);
}

public class HashGeneratorMD5 : IHashGenerator
{
    private readonly MD5 _md5;

    public HashGeneratorMD5()
    {
        _md5 = MD5.Create();
    }

    public string GetHash(int value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());
        byte[] hash = _md5.ComputeHash(bytes);

        return Encoding.UTF8.GetString(hash);
    }
}

public class HashGeneratorSHA1 : IHashGenerator
{
    private readonly SHA1 _sha1;

    public HashGeneratorSHA1()
    {
        _sha1 = SHA1.Create();
    }

    public string GetHash(int value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());
        byte[] hash = _sha1.ComputeHash(bytes);

        return Encoding.UTF8.GetString(hash);
    }
}

public class PaySystem1 : IPaymentSystem
{
    private readonly IHashGenerator _hashGenerator;

    public PaySystem1(IHashGenerator hashGenerator)
    {
        _hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
    }

    public string GetPayingLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.Amount <= 0)
            throw new InvalidOperationException();

        return $"pay.system1.ru/order?amount={order.Amount}RUB&hash={_hashGenerator.GetHash(order.Id)}";
    }
}

public class PaySystem2 : IPaymentSystem
{
    private readonly IHashGenerator _hashGenerator;

    public PaySystem2(IHashGenerator hashGenerator)
    {
        _hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
    }

    public string GetPayingLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.Amount <= 0)
            throw new InvalidOperationException();

        return $"pay.system1.ru/order?amount={order.Amount}RUB&hash={_hashGenerator.GetHash(order.Id) + order.Amount}";
    }
}

public class PaySystem3 : IPaymentSystem
{
    private readonly IHashGenerator _hashGenerator;
    private readonly int _key;

    public PaySystem3(IHashGenerator hashGenerator, int key)
    {
        _hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
        _key = key;
    }

    public string GetPayingLink(Order order)
    {
        return $"system3.com/pay?amount={order.Amount}&curency=RUB&hash=" +
            $"{_hashGenerator.GetHash(order.Amount)}{order.Id}{_key}";
    }
}