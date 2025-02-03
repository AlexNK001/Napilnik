using System;
using System.Collections.Generic;

namespace online_store
{
    public class Program
    {
        private static void Main()
        {
            Good iPhone12 = new Good("IPhone 12");
            Good iPhone11 = new Good("IPhone 11");

            Warehouse warehouse = new Warehouse();

            Shop shop = new Shop(warehouse);

            warehouse.Delive(iPhone12, 10);
            warehouse.Delive(iPhone11, 1);

            Console.WriteLine("Вывод всех товаров на складе с их остатком");
            Console.WriteLine(warehouse.GetInfo());

            Cart cart = shop.Cart();
            cart.TakeFromWarehouse(iPhone12, 4);

            try
            {
                cart.TakeFromWarehouse(iPhone11, 3);
            }
            catch
            {
                Console.WriteLine("Ошибка 1/2 так, как нет нужного количества товара на складе");
            }

            Console.WriteLine("Вывод всех товаров в корзине");
            Console.WriteLine(cart.GetInfo());

            Console.WriteLine(cart.Order().Paylink);

            try
            {
                cart.TakeFromWarehouse(iPhone12, 9);
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
            _warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));
        }

        public Cart Cart()
        {
            return new Cart(_warehouse);
        }
    }

    public class Warehouse
    {
        private readonly Dictionary<string, ProductCell> _cells;

        public Warehouse()
        {
            _cells = new Dictionary<string, ProductCell>();
        }

        public void Delive(Good product, int productCount)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (productCount < 0)
                throw new ArgumentOutOfRangeException(nameof(productCount));

            string productName = product.Name;

            if (_cells.ContainsKey(productName))
                _cells[productName].AddProduct(productCount);
            else
                _cells.Add(productName, new ProductCell(productName, productCount));
        }

        public void Get(string productName, int productCount)
        {
            if (productName == null)
                throw new ArgumentNullException(nameof(productName));

            if (productCount < 0)
                throw new ArgumentOutOfRangeException(nameof(productCount));

            if (_cells.ContainsKey(productName))
            {
                if (_cells[productName].Count >= productCount)
                {
                    _cells[productName].GetProduct(productCount);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(productCount));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(productName));
            }
        }

        public string GetInfo()
        {
            string info = "Информация о складе\n";

            if (_cells.Count > 0)
            {
                foreach (string name in _cells.Keys)
                    info += $"{name} {_cells[name].Count}\n";
            }
            else
            {
                info += "Склад пуст";
            }

            return info;
        }
    }

    public class Good
    {
        public Good(string productName)
        {
            Name = productName;
        }

        public string Name { get; }
    }

    public class Cart : IReadOnlyCart
    {
        private readonly Dictionary<string, ProductCell> _products;
        private readonly Warehouse _warehouse;

        public Cart(Warehouse warehouse)
        {
            _warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));
            _products = new Dictionary<string, ProductCell>();
            Paylink = "Paylink/jhfdkgs6854378";
        }

        public string Paylink { get; }

        public void TakeFromWarehouse(Good product, int productCount)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (productCount < 0)
                throw new ArgumentOutOfRangeException(nameof(productCount));

            string productName = product.Name;
            ProductCell currentCell;

            if (_products.ContainsKey(productName))
                currentCell = _products[productName];
            else
                currentCell = new ProductCell(productName, productCount);

            _warehouse.Get(productName, productCount);
            _products.Add(productName, currentCell);
        }

        public IReadOnlyCart Order()
        {
            return this;
        }

        public string GetInfo()
        {
            string info = $"Информация о продуктовой карте\n";

            foreach (string name in _products.Keys)
                info += $"{name} {_products[name].Count}\n";

            return info;
        }
    }

    public class ProductCell
    {
        private int _count;

        public ProductCell(string productName, int productCount)
        {
            Name = productName ?? throw new ArgumentNullException(nameof(productName));
            _count = productCount > 0 ? productCount : throw new ArgumentOutOfRangeException(nameof(productCount));
        }

        public string Name { get; }
        public int Count => _count;

        public void AddProduct(int productCount)
        {
            if (productCount < 0)
                throw new ArgumentOutOfRangeException(nameof(productCount));

            _count += productCount;
        }

        public void GetProduct(int productCount)
        {
            if (_count - productCount < 0)
                throw new ArgumentOutOfRangeException(nameof(productCount));

            _count -= productCount;
        }
    }

    public interface IReadOnlyCart
    {
        string Paylink { get; }
    }
}
