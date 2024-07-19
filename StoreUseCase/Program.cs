using System;
using System.Collections.Generic;

namespace StoreUseCase
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            Good iPhone12 = new Good("IPhone 12");
            Good iPhone11 = new Good("IPhone 11");

            Warehouse warehouse = new Warehouse();

            Shop shop = new Shop(warehouse);

            warehouse.Delive(iPhone12, 10);
            warehouse.Delive(iPhone11, 1);

            //Вывод всех товаров на складе с их остатком
            warehouse.ShowGoodsInformation();

            Cart cart = shop.Cart();
            cart.Add(iPhone12, 4);
            cart.Add(iPhone11, 3); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

            //Вывод всех товаров в корзине
            cart.ShowGoodsInformation();

            Console.WriteLine(cart.Order().Paylink);

            cart.Add(iPhone12, 9); //Ошибка, после заказа со склада убираются заказанные товары

            Console.ReadKey();
        }
    }   

    class Shop
    {
        private Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            Utils.CompareObjectIsNotNull(warehouse, nameof(warehouse));
            _warehouse = warehouse;
        }

        public Cart Cart()
        {
            return new Cart(this);
        }

        public bool IsAvailableGood(Good good, int quantity)
        {
            return _warehouse.IsAvailableGood(good, quantity);
        }

        public void DecreaseGood(Good good, int quantity)
        {
            _warehouse.Decrease(good, quantity);
        }
    }

    class Good
    {
        public Good(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    class GoodsList
    {
        private List<Cell> _cells;

        public GoodsList()
        {
            _cells = new List<Cell>();
        }

        public IReadOnlyList<Cell> Cells => _cells;

        public void AddGood(Good good, int quantity)
        {
            VerifyGoodValue(good, quantity);

            Cell newCell = new Cell(good, quantity);
            Cell cell = GetCell(good);

            if (cell != null)
                cell.Merge(newCell);
            else
                _cells.Add(newCell);
        }

        public bool IsAvailableGood(Good good, int quantity)
        {
            VerifyGoodValue(good, quantity);

            Cell cell = GetCell(good);

            if (cell == null)
                throw new Exception("Product was not found in store");

            return cell.IsAvailableQuantity(quantity);
        }

        public void Decrease(Good good, int quantity)
        {
            if (IsAvailableGood(good, quantity))
                GetCell(good).Decrease(quantity);
        }

        public void ShowGoodsInformation()
        {
            foreach (Cell cell in Cells)            
                Console.WriteLine(cell.Information);            
        }

        public Cell GetCell(Good good)
        {
            Utils.CompareObjectIsNotNull(good, nameof(good));

            return _cells.Find(cell => cell.Good == good);
        }

        private void VerifyGoodValue(Good good, int quantity)
        {
            Utils.CompareObjectIsNotNull(good, nameof(good));
            Utils.CompareIntGreaterZero(quantity, nameof(quantity));
        }        
    }

    class Cart : GoodsList
    {
        private Shop _shop;

        public Cart(Shop shop)
        {
            Utils.CompareObjectIsNotNull(shop, nameof(shop));
            _shop = shop;
        }

        public void Add(Good good, int quantity)
        {
            if (_shop.IsAvailableGood(good, quantity))            
                AddGood(good, quantity);
        }

        public Order Order()
        {
            foreach (Cell cell in Cells)
                _shop.DecreaseGood(cell.Good, cell.Quantity);

            return new Order("paylink");
        }
    }    

    class Warehouse : GoodsList
    { 
        public void Delive(Good good, int quantity)
        {            
            AddGood(good, quantity);
        }
    }

    class Cell
    {   
        public Cell(Good good, int quantity)
        {
            Utils.CompareObjectIsNotNull(good, nameof(good));
            Utils.CompareIntGreaterZero(quantity, nameof(quantity));

            Good = good;
            Quantity = quantity;
        }

        public Good Good { get; private set; }
        public int Quantity { get; private set; }
        public string Information => $"{Good.Name}: {Quantity} шт.";

        public void Merge(Cell cell)
        {
            Utils.CompareObjectIsNotNull(cell, nameof(cell));

            if (Good != cell.Good)
                throw new Exception("Type of good does not match");

            Quantity += cell.Quantity;
        }

        public bool IsAvailableQuantity(int quantity)
        {
            Utils.CompareIntGreaterValue(Quantity, nameof(quantity), quantity, true);

            return true;
        }

        public void Decrease(int quantity)
        {
            if (IsAvailableQuantity(quantity))
                Quantity -= quantity;
        }
    }

    class Order
    {
        public Order(string paylink)
        {
            Paylink = paylink;
        }

        public string Paylink { get; private set; }
    }

    static class Utils
    {
        public static void CompareIntGreaterValue(int value, string name, int targetValue, bool canBeEqual = false)
        {
            if (value < targetValue || (!canBeEqual && value == targetValue))
                throw new ArgumentOutOfRangeException(name);
        }

        public static void CompareIntGreaterZero(int value, string name)
        {
            CompareIntGreaterValue(value, name, 0);
        }

        public static void CompareObjectIsNotNull(object instance, string name)
        {
            if (instance == null)
                throw new ArgumentNullException(name);
        }
    }
}
