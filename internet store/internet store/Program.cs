using System;
using System.Collections.Generic;
using System.Linq;

namespace online_store
{
    public class Program
    {
        private static void Main()
        {
            Warehouse warehouse = new Warehouse();
            Product iPhone12 = new Product("iPhone12");
            Product iPhone11 = new Product("iPhone11");
            Shop shop = new Shop(warehouse);

            warehouse.Delive(iPhone12, 10);
            warehouse.Delive(iPhone11, 1);

            Console.WriteLine("Вывод всех товаров на складе с их остатком");
            Console.WriteLine(warehouse.GetInfo());

            Cart cart = shop.Cart();
            cart.AddProducts(iPhone12, 4);

            try
            {
                cart.AddProducts(iPhone11, 3);
            }
            catch
            {
                Console.WriteLine("Ошибка 1/3 так, как нет нужного количества товара на складе");
            }

            Console.WriteLine("Вывод всех товаров в корзине");
            Console.WriteLine(cart.GetInfo());

            Console.WriteLine(cart.PlaceOrder().Paylink);

            try
            {
                cart.AddProducts(iPhone12, 9);
            }
            catch
            {
                Console.WriteLine("Ошибка 2/3, после заказа со склада убираются заказанные товары");
            }

            BreakWarehouse(warehouse);
        }

        private static void BreakWarehouse(Warehouse warehouse)
        {
            Dictionary<string, int> nonExistentProducts = new Dictionary<string, int> { { "name1", 3 }, { "name2", 5 } };

            try
            {
                warehouse.DeleteProducts(nonExistentProducts);
            }
            catch
            {
                Console.WriteLine("Ошибка 3/3, склад не может удалить несуществующие товары");
            }
        }
    }

    public class Shop
    {
        private readonly Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            _warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));
        }

        public Cart Cart()
        {
            if (_warehouse == null)
                throw new ArgumentNullException(nameof(_warehouse));

            return new Cart(_warehouse);
        }
    }

    public class Warehouse : IWarehouse
    {
        private readonly Dictionary<string, int> _products;

        public Warehouse()
        {
            _products = new Dictionary<string, int>();
        }

        public void Delive(Product product, int count)
        {
            PreventIncorrectProduct(product);
            PreventIncorrectProductCount(count);

            string name = product.Name;

            if (_products.ContainsKey(name))
                _products[name] += count;
            else
                _products.Add(name, count);
        }

        public bool CanReserve(Product product, int count)
        {
            PreventIncorrectProduct(product);
            PreventIncorrectProductCount(count);
            PreventMissingProduct(_products, product);
            PreventProductShortages(_products, product, count);

            return _products[product.Name] >= count;
        }

        public void DeleteProducts(IReadOnlyDictionary<string, int> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            if (products.Count == 0)
                throw new InvalidOperationException();

            if (products.Any(product => product.Value <= 0))
                throw new InvalidOperationException();

            foreach (var name in products.Keys)
            {
                if ((_products.ContainsKey(name) && _products[name] >= products[name]) == false)
                    throw new InvalidOperationException();
            }

            foreach (var name in products.Keys)
            {
                if (_products.ContainsKey(name) && _products[name] >= products[name])
                {
                    _products[name] -= products[name];

                    if (_products[name] == 0)
                        _products.Remove(name);
                }
            }
        }

        private void PreventIncorrectProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.Name == null)
                throw new ArgumentNullException(nameof(product.Name));
        }

        private void PreventIncorrectProductCount(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                throw new InvalidOperationException();
        }

        private void PreventMissingProduct(Dictionary<string, int> products, Product product)
        {
            if (products.ContainsKey(product.Name) == false)
                throw new InvalidOperationException();
        }

        private void PreventProductShortages(Dictionary<string, int> products, Product product, int count)
        {
            if (products[product.Name] <= count)
                throw new InvalidOperationException();
        }

        public string GetInfo()
        {
            string info = "Информация о складе:";
            info += CollectInformation(_products);

            return info;
        }

        private string CollectInformation(Dictionary<string, int> collection)
        {
            string info = string.Empty;

            if (collection.Count > 0)
            {
                foreach (var pair in collection)
                    info += $"{pair.Key} {pair.Value}\n";
            }
            else
            {
                info += "Коллекция пуста.\n";
            }

            return info;
        }
    }

    public interface IWarehouse
    {
        bool CanReserve(Product product, int count);

        void DeleteProducts(IReadOnlyDictionary<string, int> products);
    }

    public class Cart
    {
        private readonly IWarehouse _warehouse;
        private readonly Dictionary<string, int> _products;

        public Cart(IWarehouse warehouse)
        {
            _warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));
            _products = new Dictionary<string, int>();
        }

        public void AddProducts(Product product, int count)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (_warehouse.CanReserve(product, count))
                AddProduct(product, count);
        }

        public string GetInfo()
        {
            string info = "Информация о корзине.\n";

            if (_products.Count == 0)
            {
                info += "Корзина пуста.";
            }
            else
            {
                foreach (var pair in _products)
                    info += $"{pair.Key} {pair.Value}\n";
            }

            return info;
        }

        public Order PlaceOrder()
        {
            if (_warehouse == null)
                throw new ArgumentNullException(nameof(_warehouse));

            if (_products.Count == 0)
                throw new InvalidOperationException();

            _warehouse.DeleteProducts(_products);
            var products = _products;
            _products.Clear();

            return new Order(products);
        }

        private void AddProduct(Product product, int count)
        {
            string name = product.Name;

            if (_products.ContainsKey(name))
                _products[name] += count;
            else
                _products.Add(name, count);
        }
    }

    public class Product
    {
        public Product(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }

    public class Order
    {
        private readonly IReadOnlyDictionary<string, int> _products;

        public Order(IReadOnlyDictionary<string, int> products)
        {
            _products = products ?? throw new ArgumentNullException(nameof(products));

            foreach (int count in products.Values)
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        public string Paylink => "fdsfsdf767678";
    }
}
