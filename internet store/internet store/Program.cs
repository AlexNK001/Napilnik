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
            cart.AcceptProducts(iPhone12, 4);

            try
            {
                cart.AcceptProducts(iPhone11, 3);
            }
            catch
            {
                Console.WriteLine("Ошибка 1/2 так, как нет нужного количества товара на складе");
            }

            Console.WriteLine("Вывод всех товаров в корзине");
            Console.WriteLine(cart.GetInfo());

            Console.WriteLine(cart.PlaceOrder().Paylink);

            try
            {
                cart.AcceptProducts(iPhone12, 9);
            }
            catch
            {
                Console.WriteLine("Ошибка 2/2, после заказа со склада убираются заказанные товары");
            }
        }
    }

    public class Shop
    {
        private readonly Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            _warehouse = warehouse??throw new ArgumentNullException(nameof(warehouse));
        }

        public Cart Cart()
        {
            if(_warehouse == null)
                throw new ArgumentNullException(nameof(_warehouse));

            return new Cart(_warehouse);
        }
    }

    public class Warehouse : IWarehouse
    {
        private readonly Dictionary<string, int> _productsToOrder;
        private readonly Dictionary<string, int> _orderedProducts;

        public Warehouse()
        {
            _productsToOrder = new Dictionary<string, int>();
            _orderedProducts = new Dictionary<string, int>();
        }

        public void Delive(Product product, int count)
        {
            PreventIncorrectProduct(product);
            PreventIncorrectProductCount(count);

            string name = product.Name;

            if (_productsToOrder.ContainsKey(name))
                _productsToOrder[name] += count;
            else
                _productsToOrder.Add(name, count);
        }

        public bool TryReserve(Product product, int count)
        {
            PreventIncorrectProduct(product);
            PreventIncorrectProductCount(count);
            PreventMissingProduct(_productsToOrder, product);

            return _productsToOrder[product.Name] >= count;
        }

        public void Reserve(Product product, int count)
        {
            PreventIncorrectProduct(product);
            PreventIncorrectProductCount(count);
            PreventMissingProduct(_productsToOrder, product);
            PreventProductShortages(_productsToOrder, product, count);

            Swap(_productsToOrder, _orderedProducts, product, count);
        }

        public void CancelReservation(Product product, int count)
        {
            PreventIncorrectProduct(product);
            PreventIncorrectProductCount(count);
            PreventMissingProduct(_orderedProducts, product);
            PreventProductShortages(_orderedProducts, product, count);

            Swap(_orderedProducts, _productsToOrder, product, count);
        }

        public void DeleteProducts(IReadOnlyDictionary<string, int> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            if (products.Count == 0)
                throw new InvalidOperationException();

            if (products.Any(product => product.Value <= 0))
                throw new InvalidOperationException();

            foreach (var item in products.Keys)
            {
                if (_orderedProducts.ContainsKey(item) && _orderedProducts[item] >= products[item])
                {
                    _orderedProducts[item] -= products[item];

                    if (_orderedProducts[item] == 0)
                    {
                        _orderedProducts.Remove(item);
                    }
                }
                else
                {
                    throw new InvalidOperationException();
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
            string info = "Информация о складе:\nТовары на заказ:\n";
            info += CollectInformation(_productsToOrder);
            info += "Заказанные товары:\n";
            info += CollectInformation(_orderedProducts);

            return info;
        }

        private void Swap(Dictionary<string, int> first, Dictionary<string, int> second, Product product, int count)
        {
            string name = product.Name;

            first[name] -= count;

            if (first[name] == 0)
                first.Remove(name);

            if (second.ContainsKey(name))
                second[name] += count;
            else
                second.Add(name, count);
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
        bool TryReserve(Product product, int count);
        void Reserve(Product product, int count);
        void CancelReservation(Product product, int count);
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

        public void AcceptProducts(Product product, int count)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (_products.ContainsKey(product.Name))
                count -= _products[product.Name];

            if (count > 0)
            {
                if (_warehouse.TryReserve(product, count))
                {
                    _warehouse.Reserve(product, count);
                    AddProduct(product, count);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (count < 0)
            {
                count = Math.Abs(count);
                _warehouse.CancelReservation(product, count);
                RemoveProducts(product, count);
            }
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

        private void RemoveProducts(Product product, int count)
        {
            _products[product.Name] -= count;

            if (_products[product.Name] == 0)
                _products.Remove(product.Name);
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